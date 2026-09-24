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

        gregCore.Core.Mods.GregPatches.TryPatchPrefix(harmony, typeof(StaticUIElements),
            "InstantiateErrorWarningSign", typeof(EolHider), nameof(SkipInstantiate), "NoEOL");

        _initialized = true;

        if (!_prefEnabled.Value)
            ApplyVisibility(false);

        ModReleaseLog.Info($"[EolHider] Initialized, enabled={_prefEnabled.Value}");
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
