using BepInEx;
using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using PavonisInteractive.TerraInvicta.Systems;
using System;
using System.Linq;

namespace Hyperion {
  /// <summary>
  /// Hooks for the TIGlobalResearchState class
  /// </summary>
  [HarmonyPatch]
  public static class TIGlobalResearchStateHooks
  {
    /// <summary>
    /// Prefix hook for the OnTechFinished method of the TIGlobalResearchState class
    /// This method is called when a technology is finished researching.
    /// </summary>
    /// <param name="__instance"></param>
    /// <param name="slot"></param>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(TIGlobalResearchState), "OnTechFinished")]
    static void OnTechFinished_Prefix(TIGlobalResearchState __instance, int slot)
    {
      Output.Debug($"TIGlobalResearchState.OnTechFinished called ({__instance}, {slot})");

      var techProgressList = Util.GetPrivateField<TechProgress[]>(__instance, "techProgress");
      if (techProgressList == null || slot >= techProgressList.Length)
      {
        Output.Debug("Failed to access techProgress");
        return;
      }

      var techProgress = techProgressList[slot] as TechProgress;
      if (techProgress == null)
      {
        Output.Debug($"Tech progress in slot {slot} was null");
        return;
      }

      TIFactionState tifactionState = __instance.Leader(slot);
      if (tifactionState == null)
      {
        Output.Debug($"TIFactionState of the slot leader could not be retrieved");
        return;
      }

      Output.Info(techProgress.techTemplate?.displayName);
      Output.Info($"Research completed by [{tifactionState.displayName}, {tifactionState.displayNameCapitalized}]");
      foreach (var rec in __instance.GetTechProgress(slot).factionContributions)
      {
        Output.Info($"   * {rec.Key.ideology.displayName} contributed {rec.Value}");
      }

      DataCollector.Instance.TechIsFinishing(__instance, techProgress, slot, tifactionState);
    }
  }
}