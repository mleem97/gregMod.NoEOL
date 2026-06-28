using System;
using System.Collections;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(GregModNoEOL.GregModNoEOLMod), "gregMod.NoEOL", "1.0.0", "TeamGreg Modding (Neox & mleem97)")]
[assembly: MelonGame()]

namespace GregModNoEOL;

/// <summary>
/// Prevents servers and switches from reaching end-of-life and auto-repairs broken devices.
/// Uses MelonPreferences for configuration (F5 menu).
/// </summary>
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

    public override void OnInitializeMelon()
    {
        _prefs = MelonPreferences.CreateCategory("gregMod_NoEOL", "gregMod.NoEOL");
        _prefDisableSwitchEol = _prefs.CreateEntry("DisableSwitchesEOL", true, "Disable Switches EOL");
        _prefDisableServerEol = _prefs.CreateEntry("DisableServersEOL", true, "Disable Servers EOL");
        _prefAutoRepairSwitches = _prefs.CreateEntry("AutoRepairSwitches", true, "Auto Repair Broken Switches");
        _prefAutoRepairServers = _prefs.CreateEntry("AutoRepairServers", true, "Auto Repair Broken Servers");

        LoggerInstance.Msg("gregMod.NoEOL v1.0.0 loaded. Config via F5 → Mods → gregMod.NoEOL.");
    }

    public override void OnUpdate()
    {
        if (_readyToRun)
        {
            if (_prefAutoRepairSwitches.Value) RepairSwitches();
            if (_prefAutoRepairServers.Value) RepairServers();
            if (_prefDisableSwitchEol.Value) HandleSwitchesEol();
            if (_prefDisableServerEol.Value) HandleServersEol();
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
            LoggerInstance.Msg("gregMod.NoEOL: scene ready, EOL management active.");
        }
        catch (Exception ex)
        {
            LoggerInstance.Warning($"gregMod.NoEOL: init failed: {ex.Message}");
        }
    }

    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        if (buildIndex == MainMenuSceneBuildIndex)
        {
            _readyToRun = false;
            _gameManager = null;
            _networkMap = null;
            _switchTypeDefaultEol.Clear();
            _serverTypeDefaultEol.Clear();
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
        catch
        {
            return DefaultEOL;
        }
    }

    private void RepairSwitches()
    {
        if (!_readyToRun) return;
        try
        {
            var broken = _networkMap.GetAllBrokenSwitches() as IEnumerable;
            if (broken == null) return;
            var en = broken.GetEnumerator();
            try
            {
                while (en.MoveNext())
                {
                    var sw = en.Current as NetworkSwitch;
                    if (sw != null) sw.RepairDevice();
                }
            }
            finally
            {
                (en as IDisposable)?.Dispose();
            }
        }
        catch { }
    }

    private void RepairServers()
    {
        if (!_readyToRun) return;
        try
        {
            var broken = _networkMap.GetAllBrokenServers() as IEnumerable;
            if (broken == null) return;
            var en = broken.GetEnumerator();
            try
            {
                while (en.MoveNext())
                {
                    var srv = en.Current as Server;
                    if (srv != null) srv.RepairDevice();
                }
            }
            finally
            {
                (en as IDisposable)?.Dispose();
            }
        }
        catch { }
    }

    private void HandleSwitchesEol()
    {
        if (!_readyToRun) return;
        try
        {
            var switches = _networkMap.switches as IEnumerable;
            if (switches == null) return;
            var en = switches.GetEnumerator();
            try
            {
                while (en.MoveNext())
                {
                    var entry = en.Current;
                    if (entry == null) continue;
                    var swProp = entry.GetType().GetProperty("value");
                    var sw = swProp?.GetValue(entry) as NetworkSwitch;
                    if (sw != null)
                        sw.eolTime = GetDefaultEol(true, sw.switchType);
                }
            }
            finally
            {
                (en as IDisposable)?.Dispose();
            }
        }
        catch { }
    }

    private void HandleServersEol()
    {
        if (!_readyToRun) return;
        try
        {
            var servers = _networkMap.servers as IEnumerable;
            if (servers == null) return;
            var en = servers.GetEnumerator();
            try
            {
                while (en.MoveNext())
                {
                    var entry = en.Current;
                    if (entry == null) continue;
                    var srvProp = entry.GetType().GetProperty("value");
                    var srv = srvProp?.GetValue(entry) as Server;
                    if (srv != null)
                        srv.eolTime = GetDefaultEol(false, srv.serverType);
                }
            }
            finally
            {
                (en as IDisposable)?.Dispose();
            }
        }
        catch { }
    }
}
