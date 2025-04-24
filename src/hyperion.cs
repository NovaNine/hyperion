using BepInEx;
using HarmonyLib;
using UnityEngine;
using PavonisInteractive.TerraInvicta;
using System;
using System.Linq;

namespace Hyperion {
  [BepInPlugin("games.alekki.hyperion", "Hyperion", "0.1.0")]
  public class HyperionPlugin : BaseUnityPlugin
  {
    void Awake()
    {
      Output.Debug("Hyperion data collection plugin loaded.");
      Harmony.CreateAndPatchAll(typeof(TIGlobalResearchStateHooks));
    }
  }

  [HarmonyPatch]
  public static class TIGlobalResearchStateHooks
  {
    [HarmonyPostfix]
    [HarmonyPatch(typeof(TIGlobalResearchState), "OnTechFinished")]
    static void OnTechFinished_Postfix(TIGlobalResearchState __instance, int slot)
    {
      Output.Debug($"TIGlobalResearchState.OnTechFinished called ({__instance}, {slot})");

      var techProgressList = Util.GetPrivateField<TechProgress[]>(__instance, "techProgress");
      if(techProgressList == null || slot >= techProgressList.Count()) {
        Output.Debug("Failed to access techProgress");
        return;
      }

      var techProgress = techProgressList[slot] as TechProgress;
      if (techProgress == null) {
        Output.Debug($"Tech progress in slot {slot} was null");
        return;
      }

      TIFactionState tifactionState = __instance.Leader(slot);
      if (tifactionState == null) {
        Output.Debug($"TIFactionState of the slot leader could not be retrieved");
        return;
      }

      Output.Info(techProgress.techTemplate?.displayName);
      Output.Info($"Research completed by [{tifactionState.displayName}, {tifactionState.displayNameCapitalized}]");
    }

    /* [HarmonyPostfix]
    [HarmonyPatch(typeof(TIGlobalResearchState), "TechCompletionDate")]
    static void TechCompletionDate_Postfix(TIGlobalResearchState __instance, int slot)
    {
      Output.Debug($"TIGlobalResearchState.TechCompletionDate called ({__instance}, {slot})");
    } */

    [HarmonyPostfix]
    [HarmonyPatch(typeof(TIGlobalResearchState), "AddFinishedTech", new Type[] { typeof(TITechTemplate) })]
    static void AddFinishedTech_Postfix(TIGlobalResearchState __instance, TITechTemplate finishedTech)
    {
      Output.Debug($"TIGlobalResearchState.AddFinishedTech called ({__instance}, {finishedTech})");
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(TIGlobalResearchState), "AddFinishedTech", new Type[] { typeof(string) })]
    static void AddFinishedTech_Postfix(TIGlobalResearchState __instance, string templateName)
    {
      Output.Debug($"TIGlobalResearchState.AddFinishedTech called ({__instance}, (string) {templateName})");
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(TIGlobalResearchState), "AddFinishedOneTimeOnlyProject")]
    static void AddFinishedOneTimeOnlyProject_Postfix(TIGlobalResearchState __instance, TIProjectTemplate finishedProject)
    {
      Output.Debug($"TIGlobalResearchState.AddFinishedOneTimeOnlyProject called ({__instance}, {finishedProject})");
    }
  }
}