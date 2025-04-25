using UnityEngine;

namespace Hyperion
{
  public class DelayedTraceStarter : MonoBehaviour
  {
    // Adjust this delay as needed to safely reach the main menu or game load
    private const float DELAY = 30f;
    private const float DISPLAY_INTERVAL = 5f;

    private float timer = 0f;

    private float lastDisplayTimer = 0f;
    private bool initialized = false;

    private bool firstRun = true;

    void Start()
    {
      Output.Debug("DelayedTraceStarter.Start() fired");
    }

    void Update()
    {
      if (initialized)
        return;

      if (firstRun) {
        Output.Info("Started delayed trace start countdown");
        firstRun = false;
      }

      timer += Time.deltaTime;

      if (timer > lastDisplayTimer + DISPLAY_INTERVAL) {
        Output.Info($"Delayed trace start waited {timer} seconds");
        lastDisplayTimer = timer;
      }

      if (timer > DELAY)
      {
        initialized = true;
        Output.Debug("Triggering delayed TraceLogger.Init()");
        TraceLogger.Init(HarmonyAccessor.Instance);
      }
    }
  }

  // Singleton pattern to share Harmony instance across files
  public static class HarmonyAccessor
  {
    public static HarmonyLib.Harmony Instance { get; private set; }

    public static void Set(HarmonyLib.Harmony harmony)
    {
      Instance = harmony;
    }
  }
}
