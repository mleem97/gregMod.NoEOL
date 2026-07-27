using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GregModNoEOL;

/// <summary>
/// While NoEOL overlay is open, disables PlayerInput devices so camera rotation
/// and game input do not fire behind the overlay. Same pattern as gregMod.IPAM GameInputSuppression.
/// </summary>
internal static class GameInputSuppression
{
    private static readonly List<PlayerInput> Suspended = new();
    private static readonly HashSet<InputActionAsset> DisabledAssets = new();
    private static bool _active;

    internal static bool IsActive => _active;

    internal static void SetSuppressed(bool suppress)
    {
        if (suppress == _active) return;
        _active = suppress;

        if (suppress)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            Suspended.Clear();
            DisabledAssets.Clear();

            var all = Resources.FindObjectsOfTypeAll<PlayerInput>();
            if (all == null) return;

            foreach (var pi in all)
            {
                if (pi == null) continue;
                var go = pi.gameObject;
                if (go == null || !go.scene.IsValid() || !go.scene.isLoaded) continue;

                try
                {
                    TryDisableActions(pi);
                    pi.DeactivateInput();
                    Suspended.Add(pi);
                }
                catch { }
            }
        }
        else
        {
            foreach (var pi in Suspended)
            {
                if (pi == null) continue;
                try { pi.ActivateInput(); } catch { }
            }

            foreach (var asset in DisabledAssets)
            {
                if (asset == null) continue;
                try { asset.Enable(); } catch { }
            }

            Suspended.Clear();
            DisabledAssets.Clear();
        }
    }

    private static void TryDisableActions(PlayerInput pi)
    {
        try
        {
            var asset = pi.actions;
            if (asset == null || !asset.enabled) return;
            asset.Disable();
            DisabledAssets.Add(asset);
        }
        catch { }
    }
}
