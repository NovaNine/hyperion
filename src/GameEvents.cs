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
    }
  }
}
