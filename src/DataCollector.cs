// This project is licensed under CC BY-NC 4.0
// You may not use this work for commercial purposes.
// © 2025 Nova9

using PavonisInteractive.TerraInvicta;
using System;
using System.Linq;
using System.Collections.Generic;
using Hyperion.Writer;

namespace Hyperion
{
  /// <summary>
  /// Singleton class to collect data from the game and send it to the WriterManager
  /// </summary>
  public class DataCollector : IDisposable
  {
    /// <summary>
    /// Format in which the game date will be sent to the writers
    /// </summary>
    static string GAME_DATE_FORMAT = "yyyy-MM-dd HH:mm";

    /// <summary>
    /// Lazy helper for the singleton instance
    /// </summary>
    private static Lazy<DataCollector> lazyInstance = new Lazy<DataCollector>(() => new DataCollector());

    /// <summary>
    /// Singleton instance of the DataCollector
    /// </summary>
    public static DataCollector Instance => lazyInstance.Value;
    
    private WriterManager writerManager;
    /// <summary>
    /// Track if we already disposed this instance
    /// </summary>
    private bool disposed = false;

    /// <summary>
    /// Private constructor to prevent outside creation
    /// </summary>
    private DataCollector() { }

    /// <summary>
    /// Recreate the singleton instance of the DataCollector
    /// </summary>
    public static void RecreateInstance()
    {
      DisposeInstance();
      lazyInstance = new Lazy<DataCollector>(() => new DataCollector());
    }

    /// <summary>
    /// Dispose the singleton instance of the DataCollector
    /// </summary>
    public static void DisposeInstance()
    {
      if (lazyInstance.IsValueCreated)
      {
        lazyInstance.Value.Dispose();
      }
    }


    /// <summary>
    /// Called when the game has entered playable state (after load or new game)
    /// </summary>
    /// <remarks
    /// TODO: Verify that this is called after a new game is created
    /// </remarks>
    public void GameHasStarted()
    {
      Output.Debug("Collecting data...");

      // activePlayer is TIFactionState
      var playerIdeology = GameControl.control.activePlayer.ideology.ideology.ToString();
      var playerFaction = GameControl.control.activePlayer.displayName.ToString();
      var gameDate = TITimeState.Now();

      Output.Info($"Playing as {playerFaction} ({playerIdeology})");
      Output.Info($"It is {gameDate:ToString(GAME_DATE_FORMAT)} in the game.");

      writerManager = new WriterManager($"{playerFaction}-{playerIdeology}");
      writerManager.Set("meta.playerFaction", playerFaction);
      writerManager.Set("meta.playerIdeology", playerIdeology);
      writerManager.Set("meta.initialGameDate", gameDate.ToString("yyyy-MM-dd HH:mm"));
      writerManager.Flush();
    }

    /// <summary>
    /// Called when a research technology is finished (slots 0-2 at the top)
    /// </summary>
    /// <param name="grs"></param>
    /// <param name="techProgress"></param>
    /// <param name="slot"></param>
    /// <param name="tiFactionState"></param>
    public void TechIsFinishing(TIGlobalResearchState grs, TechProgress techProgress, int slot, TIFactionState tiFactionState)
    {
      var gameDate = TITimeState.Now();

      var contributions = Obj();
      foreach (var rec in techProgress.factionContributions)
      {
        var ideology = rec.Key.ideology.ToString();
        var contribution = rec.Value;
        contributions[ideology] = contribution;
      }

      var eventRecord = Obj(
        ("type", "RESEARCH"),
        ("subtype", "TECH"),
        ("ev", "DONE"),
        ("date", gameDate.ToString(GAME_DATE_FORMAT)),
        ("title", techProgress.techTemplate?.displayName),
        ("dataName", techProgress.techTemplate?.dataName),
        ("winner", tiFactionState.ideology.ideology.ToString()),
        ("slot", slot),
        ("totalCost", techProgress.techTemplate?.researchCost),
        ("contribution", contributions)
      );

      writerManager.Insert("history", eventRecord);
      writerManager.Flush();
    }

    /// <summary>
    /// Called when a research project is finished (slots 3-5 at the bottom)
    /// </summary>
    /// <param name="tiFactionState"></param>
    /// <param name="slot"></param>
    /// <param name="project"></param>
    public void ProjectIsFinishing(TIFactionState tiFactionState, int slot, TIProjectTemplate project)
    {
      var gameDate = TITimeState.Now();

      var eventRecord = Obj(
        ("type", "RESEARCH"),
        ("subtype", "PROJECT"),
        ("ev", "DONE"),
        ("date", gameDate.ToString(GAME_DATE_FORMAT)),
        ("title", project.displayName),
        ("dataName", project.dataName),
        ("owner", tiFactionState?.ideology?.ideology.ToString() ?? "NULL"),
        ("slot", slot),
        ("totalCost", project.researchCost)
      );

      writerManager.Insert("history", eventRecord);
      writerManager.Flush();
    }

    /// <summary>
    /// Helper function to create a dictionary from a list of key-value pairs
    /// </summary>
    /// <param name="entries"></param>
    /// <returns></returns>
    Dictionary<string, object> Obj(params (string key, object value)[] entries)
    {
      return entries.ToDictionary(e => e.key, e => e.value);
    }

    /// <summary>
    /// Disposes the DataCollector and its resources
    /// </summary>
    public void Dispose()
    {
      if (disposed)
        return;

      disposed = true;

      writerManager?.Dispose();
      writerManager = null;
    }
  }
}