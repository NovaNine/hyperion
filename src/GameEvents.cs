// This project is licensed under CC BY-NC 4.0
// You may not use this work for commercial purposes.
// © 2025 Nova9

using PavonisInteractive.TerraInvicta;

namespace Hyperion
{
  public static class GameEvents
  {
    public static void Register_StartupComplete_Listener()
    {
      Output.Info("[Hyperion] Registering StartupComplete listener");
      GameControl.eventManager.AddListener<StartupComplete>(OnStartupComplete);
    }

    private static void OnStartupComplete(StartupComplete evt)
    {
      Output.Info("[Hyperion] StartupComplete event received!");
      DataCollector.RecreateInstance();
      DataCollector.Instance.GameHasStarted();
    }
  }
}
