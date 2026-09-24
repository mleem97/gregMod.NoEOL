using System;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace GregModNoEOL;

/// <summary>
/// Hides EOL warning triangles on devices. Based on EolHider by tindolt.
/// https://github.com/tindolt
/// </summary>
internal static class EolHider
{
    private static MelonPreferences_Entry<bool> _prefEnabled;
    private static bool _initialized;

    internal static void Init(MelonPreferences_Entry<bool> prefEnabled)
    {
        _prefEnabled = prefEnabled;

        var harmony = new HarmonyLib.Harmony("com.gregmod.noeol.eolhider");

        // Soft dependency: with gregCore use the shared patch helper,
        // otherwise fall back to manual reflection (same target, no core).
        if (NoEolGregHost.HasCore)
            CorePatchInit(harmony);
        else
            ManualPatchInit(harmony);

        _initialized = true;

        if (!_prefEnabled.Value)
            ApplyVisibility(false);

        ModReleaseLog.Info($"[EolHider] Initialized, enabled={_prefEnabled.Value}");
    }

    // Separate method (JIT separation): touches gregCore types.
    private static void CorePatchInit(HarmonyLib.Harmony harmony)
    {
        gregCore.Core.Mods.GregPatches.TryPatchPrefix(harmony, typeof(StaticUIElements),
            "InstantiateErrorWarningSign", typeof(EolHider), nameof(SkipInstantiate), "NoEOL");
    }

    // Standalone fallback (no gregCore): manual prefix patch, same target.
    private static void ManualPatchInit(HarmonyLib.Harmony harmony)
    {
        var targetType = typeof(StaticUIElements);
        var prefix = new HarmonyLib.HarmonyMethod(typeof(EolHider).GetMethod(nameof(SkipInstantiate),
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic));

        var m = targetType.GetMethod("InstantiateErrorWarningSign",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);

        if (m != null)
        {
            harmony.Patch(m, prefix);
            ModReleaseLog.Info("[EolHider] Patched InstantiateErrorWarningSign");
        }
        else
        {
            ModReleaseLog.Warning("[EolHider] Could not find InstantiateErrorWarningSign");
        }
    }

    internal static void OnSceneLoaded()
    {
        if (!_initialized || _prefEnabled.Value) return;
        ApplyVisibility(false);
    }

    internal static void ApplyVisibility(bool visible)
    {
        try
        {
            foreach (var pi in UnityEngine.Object.FindObjectsOfType<PositionIndicator>())
            {
                if (pi != null && pi.gameObject != null)
                    pi.gameObject.SetActive(visible);
            }
        }
        catch (Exception ex)
        {
            ModReleaseLog.Error("[EolHider] ApplyVisibility error", ex);
        }
    }

    private static bool SkipInstantiate(ref int __result)
    {
        if (_prefEnabled == null || _prefEnabled.Value) return true;
        __result = -1;
        return false;
    }
}
