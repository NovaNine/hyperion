using BepInEx;
using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using PavonisInteractive.TerraInvicta.Systems;
using System;
using System.Linq;

namespace Hyperion
{
  [HarmonyPatch]
  public static class TIControlPointHooks
  {
    [HarmonyPrefix]
    [HarmonyPatch(typeof(TIControlPoint), "SetFaction")]
    static void SetFaction_Prefix(TIControlPoint __instance, TIFactionState newFaction, bool newCampaign = false)
    {
      Output.Trace($"TIControlPointHooks.SetFaction_Prefix called ({__instance}, {newFaction}, {newCampaign})");

      if (__instance == null)
      {
        Output.Debug($"The control point being acquired is null");
        return;
      }

      if (newFaction == null)
      {
        Output.Debug($"New faction acquiring the control point was null");
      }

      DataCollector.Instance.ControlPointOwnerChanging(__instance, newFaction, newCampaign);
    }
  }
}