using BepInEx;
using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using PavonisInteractive.TerraInvicta.Systems;
using System;
using System.Linq;

namespace Hyperion
{
  [HarmonyPatch]
  public static class TIFactionStateHooks
  {
    [HarmonyPrefix]
    [HarmonyPatch(typeof(TIFactionState), "OnProjectComplete")]
    static void OnProjectComplete_Prefix(TIFactionState __instance, TIProjectTemplate project, int slot, bool suppressLogging = false, bool startup = false)
    {
      Output.Debug($"TIFactionState.OnProjectComplete called ({__instance}, {slot})");

      
      if (__instance == null)
      {
        Output.Debug($"TIFactionState of the project completer could not be retrieved");
        return;
      }

      if (project == null)
      {
        Output.Debug($"Project was null");
        return;
      }

      Output.Info($"Project [{project?.displayName}]({project?.researchCost} XP) completed by [{__instance.displayName}, {__instance?.ideology.ideology.ToString()}]");

      DataCollector.Instance.ProjectIsFinishing(__instance, slot, project);
    }
  }
}