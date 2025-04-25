// This project is licensed under CC BY-NC 4.0
// You may not use this work for commercial purposes.
// Attribution: © 2025 Nova9

using System;
using System.IO;
using UnityEngine;

namespace Hyperion
{
  public static class Output
  {
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
      UnityEngine.Debug.Log($"[Hyperion] [INFO] {msg}");
      WriteToFile($"[INFO] {GetTimestamp()} {msg}");
    }

    public static void Debug(string msg)
    {
      UnityEngine.Debug.Log($"[Hyperion] [DBUG] {msg}");
      WriteToFile($"[DBUG] {GetTimestamp()} {msg}");
    }

    public static string GetTimestamp()
    {
      return $"{DateTime.Now:u}";
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
