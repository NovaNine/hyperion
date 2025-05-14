// This project is licensed under CC BY-NC 4.0
// You may not use this work for commercial purposes.
// © 2025 Nova9

using PavonisInteractive.TerraInvicta;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Hyperion.Writer;
using PavonisInteractive.TerraInvicta.Systems;

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
    /// Format in which the game date will be sent to the writers (without time)
    /// </summary>
    static string GAME_DATE_ONLY_FORMAT = "yyyy-MM-dd";

    /// <summary>
    /// Lazy helper for the singleton instance
    /// </summary>
    private static Lazy<DataCollector> lazyInstance = new Lazy<DataCollector>(() => new DataCollector());

    /// <summary>
    /// Singleton instance of the DataCollector
    /// </summary>
    public static DataCollector Instance
    {
      get { return lazyInstance.Value; }
    }

    private WriterManager writerManager;
    /// <summary>
    /// Track if we already disposed this instance
    /// </summary>
    private bool disposed = false;

    /// <summary>
    /// Private constructor to prevent outside creation
    /// </summary>
    private DataCollector()
    {
      Output.Trace("DataCollector constructor");
      writerManager = new WriterManager();
    }

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
      if (lazyInstance == null)
        return;

      if (lazyInstance.IsValueCreated)
      {
        lazyInstance.Value.Dispose();
      }

      lazyInstance = null;
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
      if (GameControl.control == null || GameControl.control.activePlayer == null)
      {
        Output.Error("GameControl.control or GameControl.control.activePlayer is null. Cannot collect data.");
        return;
      }

      var activePlayer = GameControl.control.activePlayer;
      if (activePlayer.ideology == null)
      {
        Output.Error("activePlayer.ideology is null. Cannot collect data.");
        return;
      }
      var playerIdeology = activePlayer.ideology.ideology.ToString();
      var playerFaction = activePlayer.displayName.ToString();
      var gameDateStr = GetGameDateString();

      Output.Info($"Playing as {playerFaction} ({playerIdeology})");
      Output.Info($"It is {gameDateStr} in the game.");

      var now = DateTime.Now;
      var fileName = new StringBuilder(playerIdeology) + "_" + now.ToString("yyyyMMdd_HHmmss") + (now.ToString("zzz").Replace(":", ""));
      writerManager.SetFilename(fileName.ToString());
      writerManager.Set("meta.playerFaction", playerFaction);
      writerManager.Set("meta.playerIdeology", playerIdeology);
      writerManager.Set("meta.initialGameDate", gameDateStr);
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
        ("date", GetGameDateString()),
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
      try
      {
        var sbIdeology = new StringBuilder();
        var ideologyTemplate = tiFactionState?.ideology;
        if (ideologyTemplate == null)
        {
          sbIdeology.Append("NULL Ideology Template");
        }
        else
        {
          sbIdeology.Append(ideologyTemplate.ideology.ToString());
        }

        var eventRecord = Obj(
          ("type", "RESEARCH"),
          ("subtype", "PROJECT"),
          ("ev", "DONE"),
          ("date", GetGameDateString()),
          ("title", project.displayName),
          ("dataName", project.dataName),
          ("owner", sbIdeology.ToString()),
          ("slot", slot),
          ("totalCost", project.researchCost)
        );

        writerManager.Insert("history", eventRecord);
        writerManager.Flush();
      }
      catch (Exception ex)
      {
        Output.Error($"Error in ProjectIsFinishing: {ex.Message}");
      }
    }

    /// <summary>
    /// Called when a control point is about to change ownership
    /// </summary>
    /// <param name="controlPoint"></param>
    /// <param name="nation"></param>
    /// <param name="newCampaign"></param>
    public void ControlPointOwnerChanging(TIControlPoint controlPoint, TIFactionState newFaction, bool newCampaign)
    {
      var gameDateStr = GetGameDateString();
      var nation = controlPoint?.nation;
      var nationName = nation?.displayName ?? "NULL";
      var oldOwner = controlPoint.faction;
      var oldOwnerIdeology = oldOwner?.ideology?.ideology.ToString() ?? "NULL";
      var newOwnerIdeology = newFaction?.ideology?.ideology.ToString() ?? "NULL";

      Output.Info($"[{gameDateStr}] Control point in {nationName} ({controlPoint.positionInNation}) changing from {oldOwnerIdeology} to {newOwnerIdeology}");

      var eventRecord = Obj(
        ("type", "TERRITORY"),
        ("subtype", "CONTROL_POINT"),
        ("ev", "CHANGE"),
        ("date", gameDateStr),
        ("controlPoint", controlPoint?.displayName),
        ("nation", nationName),
        ("nationId", nation?.ID.ToString()),
        ("oldOwnerIdeology", oldOwnerIdeology),
        ("oldOwnerFaction", oldOwner?.displayName),
        ("newOwnerIdeology", newOwnerIdeology),
        ("newOwnerFaction", newFaction?.displayName),
        ("isNewCampaign", newCampaign.ToString())
      );

      writerManager.Insert("history", eventRecord);
      writerManager.Flush();
    }

    /// <summary>
    /// Helper function to get the current game date as a formatted string
    /// </summary>
    /// <returns></returns>
    public string GetGameDateString()
    {
      var gameDate = TITimeState.Now();

      var sbGameDate = new StringBuilder();
      if (gameDate == null)
      {
        sbGameDate.Append("0000-00-00 00:00");
      }
      else
      {
        sbGameDate.Append(gameDate.ToString(GAME_DATE_FORMAT));
      }

      return sbGameDate.ToString();
    }

    /// <summary>
    /// Helper function to get the current game date as a formatted string (without time)
    /// </summary>
    /// <returns></returns>
    public string GetGameDateOnlyString()
    {
      var gameDate = TITimeState.Now();

      var sbGameDate = new StringBuilder();
      if (gameDate == null)
      {
        sbGameDate.Append("0000-00-00");
      }
      else
      {
        sbGameDate.Append(gameDate.ToString(GAME_DATE_ONLY_FORMAT));
      }

      return sbGameDate.ToString();
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
    /// 
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