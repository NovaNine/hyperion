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
      Output.Info($"[Hyperion] Scene loaded: {sceneName}");

      if (scene != null) {
        foreach (var root in scene.GetRootGameObjects())
          Output.Info($"  Root object: {root.name}");

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
      Output.Info($"[Hyperion] Scene unloaded: {sceneName}");
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(SceneManager.LoadSceneAsync), new[] { typeof(string), typeof(LoadSceneMode) })]
    public static void LoadSceneAsync_Prefix(string sceneName, LoadSceneMode mode)
    {
      Output.Debug($"[Harmony] Loading scene asynchronously: {sceneName} with mode: {mode}");
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(SceneManager.LoadScene), new[] { typeof(string), typeof(LoadSceneMode) })]
    public static void LoadScene_Prefix(string sceneName, LoadSceneMode mode)
    {
      Output.Debug($"[Harmony] Loading scene: {sceneName} with mode: {mode}");
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(SceneManager.UnloadSceneAsync), new[] { typeof(string) })]
    public static void UnloadSceneAsync_Prefix(string sceneName)
    {
      Output.Debug($"[Harmony] Unloading scene: {sceneName}");
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(SceneManager.SetActiveScene))]
    public static void SetActiveScene_Prefix(Scene scene)
    {
      Output.Debug($"[Harmony] Setting active scene: {scene.name}");
    }
  }
}