// This project is licensed under CC BY-NC 4.0
// You may not use this work for commercial purposes.
// © 2025 Nova9

using PavonisInteractive.TerraInvicta;
using System;
using System.Collections.Generic;
using Hyperion.Writer;

namespace Hyperion
{
  public class DataCollector : IDisposable
  {
    private static Lazy<DataCollector> lazyInstance = new Lazy<DataCollector>(() => new DataCollector());
    public static DataCollector Instance => lazyInstance.Value;
    
    private WriterManager writerManager;
    /// <summary>
    /// Track if we already disposed this instance
    /// </summary>
    private bool disposed = false;

    // Private constructor to prevent outside creation
    private DataCollector() { }

    // Force a fresh instance
    public static void RecreateInstance()
    {
      DisposeInstance();
      lazyInstance = new Lazy<DataCollector>(() => new DataCollector());
    }

    public static void DisposeInstance()
    {
      if (lazyInstance.IsValueCreated)
      {
        lazyInstance.Value.Dispose();
      }
    }


    public void GameHasStarted()
    {
      Output.Debug("Collecting data...");

      // activePlayer is TIFactionState
      var playerIdeology = GameControl.control.activePlayer.ideology.ideology.ToString();
      var playerFaction = GameControl.control.activePlayer.displayName.ToString();
      var gameDate = TITimeState.Now();

      Output.Info($"Playing as {playerFaction} ({playerIdeology})");
      Output.Info($"It is {gameDate:ToString(\"yyyy-MM-dd HH:mm\")} in the game.");

      writerManager = new WriterManager($"{playerFaction}-{playerIdeology}");
      writerManager.Set("meta.playerFaction", playerFaction);
      writerManager.Set("meta.playerIdeology", playerIdeology);
      writerManager.Set("meta.initialGameDate", gameDate.ToString("yyyy-MM-dd HH:mm"));
      writerManager.Flush();
    }

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