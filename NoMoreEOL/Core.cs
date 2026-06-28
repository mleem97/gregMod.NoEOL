using System;
using System.Collections;
using Il2Cpp;
using MelonLoader;
using UnityEngine;
using UnityEngine.InputSystem;

[assembly: MelonInfo(typeof(GregModNoEOL.GregModNoEOLMod), "gregMod.NoEOL", "1.6.5", "TeamGreg Modding (Neox & mleem97)")]
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

    private readonly System.Collections.Generic.Dictionary<int, int> _switchTypeDefaultEol = new();
    private readonly System.Collections.Generic.Dictionary<int, int> _serverTypeDefaultEol = new();
    private bool _readyToRun;
    private NetworkMap _networkMap;
    private MainGameManager _gameManager;
    private int _frameCount;

    public override void OnInitializeMelon()
    {
        ModReleaseLog.Bootstrap();

        _prefs = MelonPreferences.CreateCategory("gregMod_NoEOL", "gregMod.NoEOL");
        _prefDisableSwitchEol = _prefs.CreateEntry("DisableSwitchesEOL", true, "Disable Switches EOL");
        _prefDisableServerEol = _prefs.CreateEntry("DisableServersEOL", true, "Disable Servers EOL");
        _prefAutoRepairSwitches = _prefs.CreateEntry("AutoRepairSwitches", true, "Auto Repair Broken Switches");
        _prefAutoRepairServers = _prefs.CreateEntry("AutoRepairServers", true, "Auto Repair Broken Servers");

        NoEolOverlay.Init(_prefDisableSwitchEol, _prefDisableServerEol, _prefAutoRepairSwitches, _prefAutoRepairServers);

        ModReleaseLog.ConfigEvent($"DisableSwitchesEOL = {_prefDisableSwitchEol.Value}");
        ModReleaseLog.ConfigEvent($"DisableServersEOL = {_prefDisableServerEol.Value}");
        ModReleaseLog.ConfigEvent($"AutoRepairSwitches = {_prefAutoRepairSwitches.Value}");
        ModReleaseLog.ConfigEvent($"AutoRepairServers = {_prefAutoRepairServers.Value}");

        LoggerInstance.Msg("gregMod.NoEOL v1.6.5 loaded. Press F5 for configuration.");
        ModReleaseLog.Info("gregMod.NoEOL v1.6.5 initialized successfully");
        ModReleaseLog.Info($"Release log: {ModReleaseLog.LogPath}");
    }

    public override void OnUpdate()
    {
        HandleInput();

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
    }

    private void HandleInput()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.f5Key.wasPressedThisFrame)
        {
            NoEolOverlay.IsVisible = !NoEolOverlay.IsVisible;
            if (NoEolOverlay.IsVisible)
                ModReleaseLog.ConfigEvent("Overlay opened");
            else
                ModReleaseLog.ConfigEvent("Overlay closed");
        }
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
