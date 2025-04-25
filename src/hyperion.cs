// This project is licensed under CC BY-NC 4.0
// You may not use this work for commercial purposes.
// Attribution: © 2025 Nova9

using BepInEx;
using HarmonyLib;
using UnityEngine;
using PavonisInteractive.TerraInvicta;
using System;

namespace Hyperion
{
  [BepInPlugin("games.alekki.hyperion", "Hyperion", "0.1.0")]
  public class HyperionPlugin : BaseUnityPlugin
  {
    private const bool EnableTraceLogging = true;

    private const bool DELAYED_TRACE_LOGGING = true;
    private Harmony harmony;

    void Awake()
    {
      Output.Debug("Hyperion data collection plugin loaded.");
      harmony = new Harmony("games.alekki.hyperion");
      HarmonyAccessor.Set(harmony);

      UnityEngine_SceneManagement_SceneManager_Hook.Init();

      harmony.PatchAll(typeof(UnityEngine_SceneManagement_SceneManager_Hook));
      //harmony.PatchAll(typeof(StartMenuController_Hook));
      harmony.PatchAll(typeof(TIGlobalResearchStateHooks));
    }
  }
}
