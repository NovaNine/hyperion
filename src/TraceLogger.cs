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
      new Regex("^.*\\+<[^>]*>d__.*$"),
      new Regex("^.*\\+<[^>]*>o__.*$"),
      new Regex("^.*\\.Audio\\..*$"),
      new Regex("^.*\\.Mood$"),
      new Regex("^.*\\.Loc$"),    // Crash?
      new Regex("^.*\\.TIUtilities$"),  // Crash?
      new Regex("^.*\\.TI[^.]Template$"), // Crash?

      new Regex("^.*\\.IName.*$"),
      new Regex("^.*\\.IEvent.*$"),
      new Regex("^.*\\..*Parser$"),

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
      new Regex("^.*\\.TabbedPaneManager$"),
      new Regex("^.*\\..*GridItem$"),
      new Regex("^.*\\..*PanelOpened$"),
      new Regex("^.*\\..*PanelClosed$"),
      new Regex("^.*\\..*ScreenOpened$"),
      new Regex("^.*\\..*ScreenClosed$"),
      new Regex("^.*\\..*UIRequested$"),
      new Regex("^.*\\.TargetSelectionTool$"),
      new Regex("^.*\\.LocalizationManager$"),
      new Regex("^.*\\.AssetCacheManager$"),
      new Regex("^.*\\..*CanvasController.*$"),
      new Regex("^.*\\..*IconController.*$"),
      new Regex("^.*\\.GeneralControlsController$"),

      new Regex("^.*\\.FloatExtensions$"),

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
      new Regex("^.*\\.Ship.ComponentMap$"),
      new Regex("^.*\\.Ship.I(Armor|Weapon|Component|BaseComponent|Hull|FireMode|HullSection)$"),

      new Regex("^.*\\..*Vector3.*$"),

      new Regex("^.*\\.HabGridCell$"),
      new Regex("^.*\\.CouncilorAppearanceGridItem$"),

      new Regex("^.*\\.Alien.*Controller$"),

      new Regex("^.*\\.TIRegionAlien.*$"),
      new Regex("^.*\\.TINotificationQueueState\\..*$"),
      new Regex("^.*\\.TIRegionUFO.*$"),
      new Regex("^.*\\.TIRegionXenoforming.*$"),

      new Regex("^.*\\.Tasks\\.AI.*$"),

      // May be worth revisiting
      new Regex("^.*\\.TIHabSiteState$"),
    };

    private static readonly (Regex ClassPattern, Regex MethodPattern)[] ExcludePatterns = new (Regex, Regex)[] {

      (new Regex("^.*$"), new Regex("^<[A-Za-z0-9_]*>b__.*$")),
      (new Regex("^.*$"), new Regex("^<[A-Za-z0-9_]*>g__.*$")),
      (new Regex("^.*$"), new Regex("^.*Mesh.*$")),
      (new Regex("^.*$"), new Regex("^Equals$")),
      (new Regex("^.*$"), new Regex("^Compare$")),
      (new Regex("^.*$"), new Regex("^Add(Sorted|Event|Listener(s)?|Cost)$")),
      (new Regex("^.*$"), new Regex("^GetHashCode$")),
      (new Regex("^.*$"), new Regex("^Tick$")),
      (new Regex("^.*$"), new Regex("^Update$")),
      (new Regex("^.*$"), new Regex("^Nothing$")),
      (new Regex("^.*$"), new Regex("^GetString$")),
      (new Regex("^.*$"), new Regex("^To(String|Percent|Global)$")),
      (new Regex("^.*$"), new Regex("^ToResource(s)?Cost$")),
      (new Regex("^.*$"), new Regex("^.*ToolTip.*$")),
      (new Regex("^.*$"), new Regex("^SetColor.*$")),
      (new Regex("^.*$"), new Regex("^Cache.*$")),
      (new Regex("^.*$"), new Regex("^SetPreviewer$")),
      (new Regex("^.*$"), new Regex("^.*GlobalCartesian.*$")),
      (new Regex("^.*$"), new Regex("^.*PositionAtTime.*$")),
      (new Regex("^.*$"), new Regex("^.*ToCartesian.*$")),
      (new Regex("^.*\\.TemplateManager$"), new Regex("^Add$")),
      (new Regex("^.*\\.TemplateManager$"), new Regex("^ClearSkirmishModeTemplates$")),
      (new Regex("^.*\\.TemplateManager$"), new Regex("^RegisterFileBasedTemplate$")),
      (new Regex("^.*\\.TemplateManager$"), new Regex("^RegisterFileBasedTemplates$")),
      (new Regex("^.*\\.TemplateManager$"), new Regex("^RegisterClassBasedTemplates$")),
      (new Regex("^.*\\.TemplateManager$"), new Regex("^Find.*$")),
      (new Regex("^.*\\.GlobalInstaller$"), new Regex("^HandleException$")),
      (new Regex("^.*\\.UnityConsoleAppender$"), new Regex("^Append$")),

      (new Regex("^.*\\.TICouncilorState$"), new Regex("^GetMonthlyIncomeFrom.*$")),
      (new Regex("^.*\\.TICouncilorState$"), new Regex("^GetAttribute$")),

      (new Regex("^.*\\.TIHabSiteState$"), new Regex("^Modify.*$")),
      (new Regex("^.*\\.TIHabModuleState$"), new Regex("^PostGlobalGameStateCreateInit_2$")),
      (new Regex("^.*\\.TIHabModuleState$"), new Regex("^SetSolarPowerOutput$")),

      (new Regex("^.*\\.TIGlobalValuesState$"), new Regex("^GetGlobalMineProductivityModifier.*$")),
      (new Regex("^.*\\.TIGlobalValuesState$"), new Regex("^TryDeserialize$")),
      (new Regex("^.*\\.TIGlobalValuesState$"), new Regex("^CreateInstance$")),
      (new Regex("^.*\\.TIGlobalValuesState$"), new Regex("^DeserializeGameStateFromID$")),

      (new Regex("^.*\\.TIGameState$"), new Regex("^Valid$")),
      (new Regex("^.*\\.TIGameState$"), new Regex("^PostGlobalGameStateCreateInit_2$")),
      (new Regex("^.*\\.TIGameStateConverter$"), new Regex("^TryDeserialize$")),
      (new Regex("^.*\\.TIGameStateConverter$"), new Regex("^CreateInstance$")),
      (new Regex("^.*\\.TIGameStateConverter$"), new Regex("^DeserializeGameStateFromID$")),

      (new Regex("^.*\\.TIGlobalValuesState$"), new Regex("^NarrativeEventTemplate$")),

      (new Regex("^.*\\.TIRegionState$"), new Regex("^AddClaim$")),
      (new Regex("^.*\\.TIControlPoint$"), new Regex("^PostGlobalGameStateCreateInit_2$")),
      (new Regex("^.*\\.TIOrgState$"), new Regex("^PostGlobalGameStateCreateInit_2$")),
      (new Regex("^.*\\.TIOrgState$"), new Regex("^InitRunTimeValues$")),
      (new Regex("^.*\\.TIOrgState$"), new Regex("^GetStatBonus$")),
      (new Regex("^.*\\.TIRegionState$"), new Regex("^IsFullyOccupied$")),
      (new Regex("^.*\\.TINationState$"), new Regex("^GetFactionMissionControlFromNation$")),
      (new Regex("^.*\\.TINationState$"), new Regex("^GetMonthlyCouncilResourceShare$")),
      (new Regex("^.*\\.TINationState$"), new Regex("^GetPublicOpinionOfFaction$")),
      (new Regex("^.*\\.TINationState$"), new Regex("^AddToMaxMilitaryTechLevel$")),
      (new Regex("^.*\\.TINationState$"), new Regex("^GetIdeologicalDistance$")),
      (new Regex("^.*\\.TINationState$"), new Regex("^GetInlinePriorityIcon$")),
      (new Regex("^.*\\.TI(Region|Nation)State$"), new Regex("^PostGlobalGameStateCreateInit_2$")),
      (new Regex("^.*\\.TI(Region|Nation)State$"), new Regex("^InitializeAllTrackers$")),
      (new Regex("^.*\\.TI(Region|Nation)State$"), new Regex("^SetDisplayNameAndFlag$")),

      (new Regex("^.*\\.TIFactionState$"), new Regex("^UnlockedShipPart$")),
      (new Regex("^.*\\.TIFactionState$"), new Regex("^LogAI$")),
      (new Regex("^.*\\.TIFactionState$"), new Regex("^IsASpaceResource$")),
      (new Regex("^.*\\.TIFactionState$"), new Regex("^Get.*IncomeFrom.*$")),
      (new Regex("^.*\\.TIFactionState$"), new Regex("^.*Intel.*$")),
      (new Regex("^.*\\.GameStateManager$"), new Regex("^AllFactions$")),
      (new Regex("^.*\\.TICouncilorState$"), new Regex("^PostGlobalGameStateCreateInit_2$")),
      (new Regex("^.*\\.TICouncilorState$"), new Regex("^Get.*Income$")),
      (new Regex("^.*\\.TICouncilorState$"), new Regex("^GetResource.*$")),
      (new Regex("^.*\\.TICouncilorState$"), new Regex("^Set(LearnedMissions|Traits)$")),

      (new Regex("^.*\\.TIOrbitState$"), new Regex("^PostGlobalGameStateCreateInit_2$")),
      (new Regex("^.*\\.TISpaceBodyState$"), new Regex("^(innerSystemAsteroid|innerMainBeltAsteroid|midMainBeltAsteroid|outerMainBeltAsteroid|centaur|kuiperBeltObject)$")),
      (new Regex("^.*\\.TISpaceBodyState$"), new Regex("^PostGlobalGameStateCreateInit_2$")),
      (new Regex("^.*\\.TISpaceBodyState$"), new Regex("^GetRotationPeriod_Hours$")),
      (new Regex("^.*\\.TISpaceBodyState$"), new Regex("^GetPolarRadius_m$")),
      (new Regex("^.*\\.TISpaceBodyState$"), new Regex("^GetMeanRadius_km$")),
      (new Regex("^.*\\.GameStateManager$"), new Regex("^(Sol|InnerSystemAsteroids|Mars|InnerAsteroidBelt|MidAsteroidBelt|OuterAsteroidBelt|Jupiter|Neptune|Centaurs|KuiperBeltObjects)$")),
      (new Regex("^.*\\.TISpaceObjectState$"), new Regex("^To(Local|Global).*AtTime$")),
      (new Regex("^.*\\.TISpaceObjectState$"), new Regex("^ExactDistance.*$")),
      (new Regex("^.*\\.TISpaceObjectState$"), new Regex("^GetSunOrbitingRelatedObject_static$")),
      (new Regex("^.*\\.TISpaceObjectState$"), new Regex("^OrbitalPeriod$")),
      (new Regex("^.*\\.TISpaceObjectState$"), new Regex("^ToCartesian.*$")),
      (new Regex("^.*\\.TISpaceObjectState$"), new Regex("^ExactDistance.*$")),
      (new Regex("^.*\\.TINaturalSpaceObjectState$"), new Regex("^SetHillRadius_m.*$")),
      (new Regex("^.*\\.OrbitalElementsState$"), new Regex("^Get.*Anomaly$")),
      (new Regex("^.*\\.TILagrangePointState$"), new Regex("^PostGlobalGameStateCreateInit_2$")),

      (new Regex("^.*\\.TIResourcesCost$"), new Regex("^SumCosts_NoDuration$")),

      (new Regex("^.*\\.TITimeState$"), new Regex("^Now$")),

      (new Regex("^.*\\.TIArmyState$"), new Regex("^PostGlobalGameStateCreateInit_2$")),

      (new Regex("^.*\\.GameStateManager$"), new Regex("^GlobalValues$")),
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
