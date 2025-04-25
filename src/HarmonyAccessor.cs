using System.Collections;
using UnityEngine;

namespace Hyperion
{
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
