using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Hyperion
{
  public static class TraceLogger
  {
    private static readonly string TraceLogPath = Path.Combine(
      Path.GetFullPath(Path.Combine(Application.dataPath, "..", "BepInEx")),
      "hyperion-call-trace.log"
    );

    private const bool OUTPUT_INVALID = false;

    private const bool OUTPUT_EXCLUDED_CLASS = false;
    private const bool OUTPUT_EXCLUDED_METHOD = false;

    private const int FlushInterval = 100;

    private static readonly Regex REGEX_ANY = new Regex(".*");

    private static readonly Regex[] ExcludeClassPatterns = {
      new Regex("^.*\\.Error$"),
      new Regex("^.*\\.Log$"),
      new Regex("^.*\\.UI\\..*$"),
      new Regex("^.*\\+<>c.*$"),
      new Regex("^.*\\.Audio\\..*$"),
      new Regex("^.*\\.Mood$"),
      new Regex("^.*\\.Loc$"),    // Crash?
      new Regex("^.*\\.TIUtilities$"),  // Crash?
      new Regex("^.*\\.TI[^.]Template$"), // Crash?

      new Regex("^.*\\.OptionsScreenController.*$"),
      new Regex("^.*\\.SavingFailedDialog.*$"),
      new Regex("^.*\\.[^.]*ListAdapter.*$"),
      new Regex("^.*\\.GamePlayScript\\..*$"),
      new Regex("^.*\\.Actions\\..*$"),

      new Regex("^.*\\.UI[^.]*Feedback$"),
      new Regex("^.*\\.UI[^.]*Style$"),
      new Regex("^.*\\.UI[^.]*Controller$"),
      new Regex("^.*\\.[^.]*UIController$"),
      new Regex("^.*\\.[^.]*UIElementController$"),
      new Regex("^.*\\.[^.]*MenuController$"),
      new Regex("^.*\\.RightClickHandler$"),
      new Regex("^.*\\.TabbedPaneController$"),

      new Regex("^.*\\.SpaceCombat\\..*$"),

      new Regex("^.*\\.TIVFXManager$"),

      new Regex("^.*\\.FactionGoal_.*$"),

      new Regex("^.*\\.[^.]*ModelController$"),

      new Regex("^.*\\.Comet.*$"),

      new Regex("^.*\\.TerrestrialUnitModel$"),
      new Regex("^.*\\..*MarkerController$"),
      new Regex("^.*\\..*ItemController$"),
      new Regex("^.*\\..*ItemViewsHolder$"),

      new Regex("^.*\\.StrategyShipContoller$"),

      new Regex("^.*\\..*Vector3.*$"),

      new Regex("^.*\\.Alien.*Controller$"),

      new Regex("^.*\\.TIRegionAlien.*$"),
      new Regex("^.*\\.TINotificationQueueState\\..*$"),
      new Regex("^.*\\.TIRegionUFO.*$"),
      new Regex("^.*\\.TIRegionXenoforming.*$"),
    };

    private static readonly (Regex ClassPattern, Regex MethodPattern)[] ExcludePatterns = new (Regex, Regex)[] {

      (new Regex("^.*$"), new Regex("^<[A-Za-z0-9_]*>b__.*$")),
      (new Regex("^.*$"), new Regex("^.*Mesh.*$")),
      (new Regex("^.*\\.TemplateManager$"), new Regex("^Add$")),
      (new Regex("^.*\\.TemplateManager$"), new Regex("^ClearSkirmishModeTemplates$")),
      (new Regex("^.*\\.TemplateManager$"), new Regex("^RegisterFileBasedTemplate$")),
      (new Regex("^.*\\.TemplateManager$"), new Regex("^RegisterFileBasedTemplates$")),
      (new Regex("^.*\\.TemplateManager$"), new Regex("^RegisterClassBasedTemplates$")),
      (new Regex("^.*\\.TemplateManager$"), new Regex("^FindDataTemplateType$")),
      (new Regex("^.*\\.GlobalInstaller$"), new Regex("^HandleException$")),
      (new Regex("^.*\\.UnityConsoleAppender$"), new Regex("^Append$")),
    };

    private static int _lineCounter = 0;

    private static StreamWriter writer;
    private static readonly object lockObj = new object();

    public static void Init(Harmony harmonyInstance)
    {
      try
      {
        writer = new StreamWriter(TraceLogPath, append: false) { AutoFlush = true };

        Write(">>> TraceLogger.Init() triggered");

        var assembly = typeof(PavonisInteractive.TerraInvicta.TIGameState).Assembly;
        foreach (var type in assembly.GetTypes().Where(t => t.Namespace != null && t.Namespace.StartsWith("PavonisInteractive.TerraInvicta")))
        {
          if (ShouldExcludeClass(type?.FullName)) {
            if (OUTPUT_EXCLUDED_CLASS)
              Write($"x  {type?.FullName}");

            continue;
          }

          Write($"+  {type?.FullName}");

          foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
          {
            // Skip special or compiler-generated methods
            if (method == null || method.IsAbstract || method.IsGenericMethod || method.IsConstructor || method.IsSpecialName) {
              if (OUTPUT_INVALID)
                Write($"     -  {type.FullName}.{method?.Name}()");
              
              continue;
            }

            if (ShouldExcludeMethod(method))
            {
              if (OUTPUT_EXCLUDED_METHOD)
                Write($"     x  {type.FullName}.{method.Name}()");

              continue;
            }

            try
            {
              var postfix = new HarmonyMethod(typeof(TraceLogger).GetMethod(nameof(LogMethodCall), BindingFlags.Static | BindingFlags.NonPublic));
              Write($"     +  {method.Name}()");
              harmonyInstance.Patch(method, postfix: postfix);
            }
            catch (Exception ex)
            {
              Write($"          Err: {ex.Message}");
            }
          }
        }

        Write("=== Trace Started ===");
      }
      catch (Exception ex)
      {
        Write($"TraceLogger Init failed: {ex.Message}");
      }
    }

    private static bool ShouldExcludeClass(string className) {
      foreach(var classRegex in ExcludeClassPatterns) {
        if (classRegex.IsMatch(className))
          return true;
      }

      return false;
    }

    private static bool ShouldExcludeMethod(MethodInfo method)
    {
      var className = method.DeclaringType?.FullName ?? "";
      var methodName = method.Name;

      foreach (var (classRegex, methodRegex) in ExcludePatterns)
      {
        if (classRegex.IsMatch(className) && methodRegex.IsMatch(methodName))
          return true;
      }

      return false;
    }

    private static void LogMethodCall(MethodBase __originalMethod)
    {
      try {
        if (__originalMethod == null)
          return;

        var reflectedType = __originalMethod.ReflectedType; // less likely to trigger static init
        string reflectedTypeName = reflectedType?.FullName ?? "<unknown>";

        var methodName = __originalMethod.Name ?? "<null method>";

        Write($"{reflectedTypeName}.{methodName}()");
      } catch (Exception ex) {
        File.AppendAllText("hyperion-trace-errors.log", $"[Log Crash] {ex.GetType().Name}: {ex.Message}\n");
        Write($"!  Exception in log method: {ex.Message}");
      }
    }

    private static void Write(string line)
    {
      lock (lockObj)
      {
        if (writer == null)
          return;
        
        try {
          writer.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {line}");
          _lineCounter++;
          if (_lineCounter >= FlushInterval)
          {
            writer?.Flush();
            _lineCounter = 0;
          }
        } catch (IOException ioEx) {
          File.AppendAllText("hyperion-trace-errors.log", $"[I/O Error] {ioEx.Message}\n");
        } catch (ObjectDisposedException) {

        } catch (Exception ex) {
          File.AppendAllText("hyperion-trace-errors.log", $"[Other Error] {ex.Message}\n");
        }
      }
    }

    public static void Dispose()
    {
      lock (lockObj)
      {
        writer?.Flush();
        writer?.Close();
        writer = null;
      }
    }
  }
}
