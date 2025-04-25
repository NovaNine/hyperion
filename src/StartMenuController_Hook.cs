using HarmonyLib;

namespace Hyperion
{

  [HarmonyPatch]
  public static class StartMenuController_Hook
  {
    private static bool triggered = false;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(StartMenuController), "Initialize")]
    static void Initialize_Postfix()
    {
      if (triggered)
        return;
      
      triggered = true;

      Output.Info("[Hyperion] StartMenuController.Initialize() — triggering TraceLogger");
      TraceLogger.Init(HarmonyAccessor.Instance);
    }
  }
}