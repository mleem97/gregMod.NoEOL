using System;
using System.Collections;
using Il2Cpp;
using MelonLoader;
using UnityEngine;
using UnityEngine.InputSystem;

[assembly: MelonInfo(typeof(GregModNoEOL.GregModNoEOLMod), "gregMod.NoEOL", "1.8.0", "TeamGreg Modding (Neox & mleem97)")]
[assembly: MelonGame()]

namespace GregModNoEOL;

public class GregModNoEOLMod : MelonMod
{
    private const int DefaultEOL = 14401;
    private const int MainMenuSceneBuildIndex = 0;
    private const int BaseSceneBuildIndex = 1;

    private static MelonPreferences_Category _prefs;
    private static MelonPreferences_Entry<bool> _prefDisableSwitchEol;
    private static MelonPreferences_Entry<bool> _prefDisableServerEol;
    private static MelonPreferences_Entry<bool> _prefAutoRepairSwitches;
    private static MelonPreferences_Entry<bool> _prefAutoRepairServers;
    private static MelonPreferences_Entry<bool> _prefHideWarningTriangles;

    private readonly System.Collections.Generic.Dictionary<int, int> _switchTypeDefaultEol = new();
    private readonly System.Collections.Generic.Dictionary<int, int> _serverTypeDefaultEol = new();
    private bool _readyToRun;
    private NetworkMap _networkMap;
    private MainGameManager _gameManager;
    private int _frameCount;

    // Rising-edge Escape detection (wasPressedThisFrame can be stripped by Harmony)
    private bool _prevEscapeIsPressed;

    // Double-press Escape: first press closes overlay, second press opens pause menu
    private static float _overlayEscapeCloseTime;
    private const float EscapeDoublePressWindow = 0.6f;

    public override void OnInitializeMelon()
    {
        ModReleaseLog.Bootstrap();

        _prefs = MelonPreferences.CreateCategory("gregMod_NoEOL", "gregMod.NoEOL");
        _prefDisableSwitchEol = _prefs.CreateEntry("DisableSwitchesEOL", true, "Disable Switches EOL");
        _prefDisableServerEol = _prefs.CreateEntry("DisableServersEOL", true, "Disable Servers EOL");
        _prefAutoRepairSwitches = _prefs.CreateEntry("AutoRepairSwitches", true, "Auto Repair Broken Switches");
        _prefAutoRepairServers = _prefs.CreateEntry("AutoRepairServers", true, "Auto Repair Broken Servers");
        _prefHideWarningTriangles = _prefs.CreateEntry("HideWarningTriangles", false, "Hide EOL Warning Triangles");

        NoEolOverlay.Init(_prefDisableSwitchEol, _prefDisableServerEol, _prefAutoRepairSwitches, _prefAutoRepairServers, _prefHideWarningTriangles);
        EolHider.Init(_prefHideWarningTriangles);

        ModReleaseLog.ConfigEvent($"DisableSwitchesEOL = {_prefDisableSwitchEol.Value}");
        ModReleaseLog.ConfigEvent($"DisableServersEOL = {_prefDisableServerEol.Value}");
        ModReleaseLog.ConfigEvent($"AutoRepairSwitches = {_prefAutoRepairSwitches.Value}");
        ModReleaseLog.ConfigEvent($"AutoRepairServers = {_prefAutoRepairServers.Value}");
        ModReleaseLog.ConfigEvent($"HideWarningTriangles = {_prefHideWarningTriangles.Value}");

        LoggerInstance.Msg("gregMod.NoEOL v1.8.0 loaded. Press F5 for configuration.");
        ModReleaseLog.Info("gregMod.NoEOL v1.8.0 initialized successfully");
        ModReleaseLog.Info($"Release log: {ModReleaseLog.LogPath}");
    }

    public override void OnUpdate()
    {
        HandleInput();

        // Keep input suppression active for a short window after overlay closes via Escape
        // so the game does not see the same Escape press and open the pause menu.
        if (!NoEolOverlay.IsVisible && IsInEscapeCooldown())
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (!GameInputSuppression.IsActive)
                GameInputSuppression.SetSuppressed(true);
        }
        else if (!NoEolOverlay.IsVisible && GameInputSuppression.IsActive)
        {
            GameInputSuppression.SetSuppressed(false);
        }

        // Keep cursor + input suppression in sync every frame while overlay is open
        if (NoEolOverlay.IsVisible)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (!GameInputSuppression.IsActive)
                GameInputSuppression.SetSuppressed(true);
        }

        if (_readyToRun)
        {
            _frameCount++;
            var repairedSw = 0;
            var repairedSrv = 0;
            var resetSw = 0;
            var resetSrv = 0;

            if (_prefAutoRepairSwitches.Value) repairedSw = RepairSwitches();
            if (_prefAutoRepairServers.Value) repairedSrv = RepairServers();
            if (_prefDisableSwitchEol.Value) resetSw = HandleSwitchesEol();
            if (_prefDisableServerEol.Value) resetSrv = HandleServersEol();

            if (repairedSw > 0) ModReleaseLog.RepairEvent($"Repaired {repairedSw} broken switch(es)");
            if (repairedSrv > 0) ModReleaseLog.RepairEvent($"Repaired {repairedSrv} broken server(s)");
            if (resetSw > 0 && _frameCount % 300 == 0) ModReleaseLog.EolEvent($"EOL reset applied to {resetSw} switch(es)");
            if (resetSrv > 0 && _frameCount % 300 == 0) ModReleaseLog.EolEvent($"EOL reset applied to {resetSrv} server(s)");
            return;
        }

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex != BaseSceneBuildIndex)
            return;

        try
        {
            var networkMap = NetworkMap.instance;
            var gameManager = MainGameManager.instance;
            if (networkMap == null || gameManager == null) return;

            _readyToRun = true;
            _networkMap = networkMap;
            _gameManager = gameManager;
            _frameCount = 0;
            LoggerInstance.Msg("gregMod.NoEOL: scene ready, EOL management active.");
            ModReleaseLog.SceneEvent("Gameplay scene ready — EOL management active");
        }
        catch (Exception ex)
        {
            LoggerInstance.Warning($"gregMod.NoEOL: init failed: {ex.Message}");
            ModReleaseLog.Error("Scene init failed", ex);
        }
    }

    public override void OnGUI()
    {
        NoEolOverlay.Draw();
    }

    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        ModReleaseLog.SceneEvent($"Scene loaded: {sceneName} (buildIndex={buildIndex})");
        if (buildIndex == MainMenuSceneBuildIndex)
        {
            _readyToRun = false;
            _gameManager = null;
            _networkMap = null;
            _switchTypeDefaultEol.Clear();
            _serverTypeDefaultEol.Clear();
            ModReleaseLog.SceneEvent("Main menu — EOL management paused");
        }

        EolHider.OnSceneLoaded();
    }

    private void HandleInput()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        // Do not open overlay while pause menu is active
        if (!NoEolOverlay.IsVisible && IsPauseMenuActive())
            return;

        if (!NoEolOverlay.IsVisible)
        {
            // F5 opens overlay (only when not already open)
            if (kb.f5Key.wasPressedThisFrame)
            {
                NoEolOverlay.IsVisible = true;
                _prevEscapeIsPressed = false; // reset latch
                ModReleaseLog.ConfigEvent("Overlay opened (F5)");
            }
            return;
        }

        // Rising-edge Escape detection (wasPressedThisFrame can be stripped by Harmony)
        var escapeDown = kb.escapeKey.isPressed;
        var escapeThisFrame = escapeDown && !_prevEscapeIsPressed;
        _prevEscapeIsPressed = escapeDown;

        if (escapeThisFrame)
        {
            NoEolOverlay.IsVisible = false;
            _overlayEscapeCloseTime = Time.unscaledTime;
            ModReleaseLog.ConfigEvent("Overlay closed (Escape)");
        }
    }

    /// <summary>
    /// Returns true if a game pause/settings menu canvas is currently active.
    /// Used to prevent NoEOL overlay from opening on top of the pause menu.
    /// </summary>
    internal static bool IsPauseMenuActive()
    {
        try
        {
            var all = Resources.FindObjectsOfTypeAll<Canvas>();
            if (all == null) return false;
            foreach (var c in all)
            {
                if (c == null || !c.isActiveAndEnabled) continue;
                var go = c.gameObject;
                if (go == null) continue;
                if (!go.scene.IsValid() || !go.scene.isLoaded) continue;
                if (c.renderMode != RenderMode.ScreenSpaceOverlay) continue;

                var n = go.name ?? "";
                if (n.IndexOf("Pause", StringComparison.OrdinalIgnoreCase) >= 0
                    || n.IndexOf("PauseMenu", StringComparison.OrdinalIgnoreCase) >= 0
                    || n.IndexOf("EscapeMenu", StringComparison.OrdinalIgnoreCase) >= 0
                    || n.IndexOf("InGameMenu", StringComparison.OrdinalIgnoreCase) >= 0
                    || n.IndexOf("SystemMenu", StringComparison.OrdinalIgnoreCase) >= 0
                    || n.IndexOf("OptionsMenu", StringComparison.OrdinalIgnoreCase) >= 0
                    || n.IndexOf("SettingsMenu", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }
        }
        catch { }
        return false;
    }

    /// <summary>
    /// Should the game's Escape handler skip this press?
    /// Returns true if NoEOL just closed within the double-press window.
    /// First Escape = close overlay (consumed). Second Escape = open pause menu.
    /// </summary>
    internal static bool ShouldConsumeEscape()
    {
        if (Time.unscaledTime - _overlayEscapeCloseTime < EscapeDoublePressWindow)
        {
            _overlayEscapeCloseTime = 0f; // consume once, next Escape goes through
            return true;
        }
        return false;
    }

    /// <summary>Returns true if the escape cooldown is still active (overlay just closed via Escape).</summary>
    private static bool IsInEscapeCooldown()
    {
        return _overlayEscapeCloseTime > 0f && Time.unscaledTime - _overlayEscapeCloseTime < EscapeDoublePressWindow;
    }

    private int GetDefaultEol(bool isSwitch, int type)
    {
        var dict = isSwitch ? _switchTypeDefaultEol : _serverTypeDefaultEol;
        if (dict.TryGetValue(type, out var cached)) return cached;
        if (!_readyToRun) return DefaultEOL;

        try
        {
            var prefab = isSwitch ? _gameManager.GetSwitchPrefab(type) : _gameManager.GetServerPrefab(type);
            if (prefab == null) return DefaultEOL;

            if (isSwitch)
            {
                var sw = prefab.GetComponent<NetworkSwitch>();
                if (sw == null) return DefaultEOL;
                var eol = sw.eolTime;
                if (eol < int.MaxValue) eol++;
                dict[type] = eol;
                return eol;
            }
            else
            {
                var srv = prefab.GetComponent<Server>();
                if (srv == null) return DefaultEOL;
                var eol = srv.eolTime;
                if (eol < int.MaxValue) eol++;
                dict[type] = eol;
                return eol;
            }
        }
        catch { return DefaultEOL; }
    }

    private int RepairSwitches()
    {
        if (!_readyToRun) return 0;
        var count = 0;
        try
        {
            var broken = _networkMap.GetAllBrokenSwitches() as IEnumerable;
            if (broken == null) return 0;
            var en = broken.GetEnumerator();
            try
            {
                while (en.MoveNext())
                {
                    var sw = en.Current as NetworkSwitch;
                    if (sw != null) { sw.RepairDevice(); count++; }
                }
            }
            finally { (en as IDisposable)?.Dispose(); }
        }
        catch { }
        return count;
    }

    private int RepairServers()
    {
        if (!_readyToRun) return 0;
        var count = 0;
        try
        {
            var broken = _networkMap.GetAllBrokenServers() as IEnumerable;
            if (broken == null) return 0;
            var en = broken.GetEnumerator();
            try
            {
                while (en.MoveNext())
                {
                    var srv = en.Current as Server;
                    if (srv != null) { srv.RepairDevice(); count++; }
                }
            }
            finally { (en as IDisposable)?.Dispose(); }
        }
        catch { }
        return count;
    }

    private int HandleSwitchesEol()
    {
        if (!_readyToRun) return 0;
        var count = 0;
        try
        {
            var switches = _networkMap.switches as IEnumerable;
            if (switches == null) return 0;
            var en = switches.GetEnumerator();
            try
            {
                while (en.MoveNext())
                {
                    var entry = en.Current;
                    if (entry == null) continue;
                    var sw = entry.GetType().GetProperty("value")?.GetValue(entry) as NetworkSwitch;
                    if (sw != null) { sw.eolTime = GetDefaultEol(true, sw.switchType); count++; }
                }
            }
            finally { (en as IDisposable)?.Dispose(); }
        }
        catch { }
        return count;
    }

    private int HandleServersEol()
    {
        if (!_readyToRun) return 0;
        var count = 0;
        try
        {
            var servers = _networkMap.servers as IEnumerable;
            if (servers == null) return 0;
            var en = servers.GetEnumerator();
            try
            {
                while (en.MoveNext())
                {
                    var entry = en.Current;
                    if (entry == null) continue;
                    var srv = entry.GetType().GetProperty("value")?.GetValue(entry) as Server;
                    if (srv != null) { srv.eolTime = GetDefaultEol(false, srv.serverType); count++; }
                }
            }
            finally { (en as IDisposable)?.Dispose(); }
        }
        catch { }
        return count;
    }
}
