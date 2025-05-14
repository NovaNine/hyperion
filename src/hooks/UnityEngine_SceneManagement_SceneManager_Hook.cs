// This project is licensed under CC BY-NC 4.0
// You may not use this work for commercial purposes.
// © 2025 Nova9

using UnityEngine;
using UnityEngine.SceneManagement;

using BepInEx;
using HarmonyLib;

namespace Hyperion
{
  [HarmonyPatch(typeof(SceneManager))]
  public static class UnityEngine_SceneManagement_SceneManager_Hook

  {

    public static void Init() {
      SceneManager.sceneLoaded += OnSceneLoaded;
      SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
      var sceneName = scene.name ?? "<NULL Scene>";
      Output.Debug($"Scene loaded: {sceneName}");

      if (scene != null) {
        foreach (var root in scene.GetRootGameObjects())
          Output.Debug($"   * Root object: {root.name}");

        // If we're entering the in-game scene, add further hooks to detect load finishing
        if (scene.name == "SolarSystemScene")
        {
          GameEvents.Register_StartupComplete_Listener();
        }
      }
    }

    private static void OnSceneUnloaded(Scene scene)
    {
      var sceneName = scene.name ?? "<NULL Scene>";
      Output.Debug($"Scene unloaded: {sceneName}");
    }
  }
}