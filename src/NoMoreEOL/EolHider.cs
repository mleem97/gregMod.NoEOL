using System;
using System.Reflection;
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

        var targetType = typeof(StaticUIElements);
        var prefix = new HarmonyMethod(typeof(EolHider).GetMethod(nameof(SkipInstantiate), BindingFlags.Static | BindingFlags.NonPublic));

        var m = targetType.GetMethod("InstantiateErrorWarningSign",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (m != null)
        {
            harmony.Patch(m, prefix);
            ModReleaseLog.Info("[EolHider] Patched InstantiateErrorWarningSign");
        }
        else
        {
            ModReleaseLog.Warning("[EolHider] Could not find InstantiateErrorWarningSign");
        }

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
