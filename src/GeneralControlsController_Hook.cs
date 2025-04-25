using HarmonyLib;
using PavonisInteractive.TerraInvicta;

namespace Hyperion
{
  public static class GeneralControlsController_Hook
  {
    private static bool gameReadyRanOnce = false;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GeneralControlsController), "Initialize")]
    public static void GeneralControlsController_Initialize_Postfix()
    {
      if (gameReadyRanOnce)
        return;

      if (GameStateManager.IsValid())
      {
        Output.Debug("[Hyperion] Game is ready.");
        gameReadyRanOnce = true;
      }
      else
      {
        Output.Debug("[Hyperion] GeneralControlsController.Initialize() ran but GameStateManager is not valid.");
      }
    }
  }
}
