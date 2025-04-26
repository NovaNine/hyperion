// This project is licensed under CC BY-NC 4.0
// You may not use this work for commercial purposes.
// © 2025 Nova9

using System;
using System.IO;
using UnityEngine;

namespace Hyperion
{
  public enum LogLevel
  {
    ERROR,  // Error
    WARN,  // Warning
    INFO,  // Info
    DEBUG,  // Debug
    TRACE   // Trace (very detailed)
  }

  public static class Output
  {
    static string TIMESTAMP_FORMAT = "yyyy-MM-ddTHH:mm:ss.fffzzz";

    /// <summary>
    /// The full path where Hyperion outputs its main log
    /// </summary>
    private static readonly string LogPath = Path.Combine(
      Path.GetFullPath(Path.Combine(Application.dataPath, "..", "BepInEx")),
      "hyperion.log"
    );

    /// <summary>
    /// File lock for the log file
    /// </summary>
    private static readonly object LogFileLock = new object();

    public static void Info(string msg)
    {
      Out(LogLevel.INFO, msg);
    }

    public static void Debug(string msg)
    {
      Out(LogLevel.DEBUG, msg);
    }

    public static void Error(string msg)
    {
      Out(LogLevel.ERROR, msg);
    }

    public static void Warn(string msg)
    {
      Out(LogLevel.WARN, msg);
    }

    public static void Trace(string msg)
    {
      Out(LogLevel.TRACE, msg);
    }

    public static string LevelToCode(LogLevel level)
    {
      switch (level)
      {
        case LogLevel.ERROR: return "ERR";
        case LogLevel.WARN: return "WRN";
        case LogLevel.INFO: return "INF";
        case LogLevel.DEBUG: return "DBG";
        case LogLevel.TRACE: return "TRC";
        default: return level.ToString().ToUpper();
      }
    }

    public static void Out(LogLevel level, string msg)
    {
      var code = LevelToCode(level);
      UnityEngine.Debug.LogWarning($"[Hyperion] [{code}] {msg}");
      WriteToFile($"[{code}] {GetTimestamp()} {msg}");
    }

    public static string GetTimestamp()
    {
      return DateTime.Now.ToString("");
    }

    private static void WriteToFile(string line)
    {
      try
      {
        lock (LogFileLock)
        {
          File.AppendAllText(LogPath, line + Environment.NewLine);
        }
      }
      catch (Exception ex)
      {
        UnityEngine.Debug.LogError($"[Hyperion] Failed to write to log file: {ex.Message}");
      }
    }
  }
}
