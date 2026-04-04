/*
MIT License

Copyright (c) 2026 Neox

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to do so, subject to the
following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using Il2Cpp;
using Il2CppSystem.Linq;
using MelonLoader;
using NoMoreEOL.Enums;
using DataCenterModLoader;
using NoMoreEOL.Options;

[assembly: MelonInfo(typeof(NoMoreEOL.Core), "NoMoreEOL", "1.0.0", "Neox", null)]
[assembly: MelonGame("Waseku", "Data Center")]

namespace NoMoreEOL
{
    /// <summary>
    /// Main MelonLoader entry point for the mod. Keeps devices from reaching end-of-life
    /// and optionally repairs broken equipment while the main gameplay scene is active.
    /// </summary>
    public class Core : MelonMod
    {
        /// <summary>
        /// Public name used when registering the mod with the shared mod configuration system.
        /// </summary>
        public const string ModName = "NoMoreEOL";

        /// <summary>
        /// Mod author displayed in metadata.
        /// </summary>
        private const string Author = "Neox";

        /// <summary>
        /// Current mod version displayed in metadata.
        /// </summary>
        private const string Version = "1.0.0";

        #region Scene Constants

        /// <summary>
        /// Build index for the main menu scene, used to clear scene-specific references.
        /// </summary>
        private const int MainMenuSceneBuildIndex = 0;

        /// <summary>
        /// Build index for the main gameplay scene, used to know when game systems are available.
        /// </summary>
        private const int BaseSceneBuildIndex = 1;

        #endregion
        #region Dictionaries

        /// <summary>
        /// Cached original EOL values for switch prefabs keyed by switch type.
        /// </summary>
        private Dictionary<int, int> _switchTypeDefaultEOL = new();

        /// <summary>
        /// Cached original EOL values for server prefabs keyed by server type.
        /// </summary>
        private Dictionary<int, int> _serverTypeDefaultEOL = new();

        #endregion

        /// <summary>
        /// Fallback EOL value used before prefab data is available or when a prefab lookup fails.
        /// </summary>
        private const int DefaultEOL = 14401;

        /// <summary>
        /// Tracks whether the mod has resolved the scene objects it needs to operate safely.
        /// </summary>
        private bool _readyToRun;

        /// <summary>
        /// Cached map instance for querying and updating devices in the active game scene.
        /// </summary>
        private NetworkMap _networkMap = null;

        /// <summary>
        /// Cached game manager used to resolve device prefabs and their original data.
        /// </summary>
        private MainGameManager _gameManager = null;

        /// <summary>
        /// Registers mod metadata and initializes the configuration options exposed to players.
        /// </summary>
        public override void OnInitializeMelon()
        {
            ModConfigSystem.SetModInfo(ModName, Author, Version);
            OptionsManager.Instance.InitializeOptions();
        }

        /// <summary>
        /// Returns the default EOL value for a server based on its prefab type.
        /// </summary>
        /// <param name="server">The live server instance whose type should be inspected.</param>
        /// <returns>The cached or resolved default EOL value for that server type.</returns>
        public int GetEOLDeviceDefaultEOL(Server server) => GetEOLDeviceDefaultEOL(EOLDeviceType.Server, server.serverType);

        /// <summary>
        /// Returns the default EOL value for a switch based on its prefab type.
        /// </summary>
        /// <param name="networkSwitch">The live switch instance whose type should be inspected.</param>
        /// <returns>The cached or resolved default EOL value for that switch type.</returns>
        public int GetEOLDeviceDefaultEOL(NetworkSwitch networkSwitch) => GetEOLDeviceDefaultEOL(EOLDeviceType.Switch, networkSwitch.switchType);

        /// <summary>
        /// Resolves the original EOL value from the device prefab and caches it by device type.
        /// </summary>
        /// <param name="deviceType">Whether the lookup should target a switch or a server prefab.</param>
        /// <param name="type">The concrete game-specific type identifier for the device.</param>
        /// <returns>The original EOL value for the requested device type, or a fallback value if unavailable.</returns>
        public int GetEOLDeviceDefaultEOL(EOLDeviceType deviceType, int type)
        {
            var isSwitch = deviceType == EOLDeviceType.Switch;

            // Keep switch and server caches separate because their type IDs come from different systems.
            var dict = isSwitch ? _switchTypeDefaultEOL : _serverTypeDefaultEOL;

            if (dict.ContainsKey(type))
                return dict[type];

            if (!_readyToRun)
                return DefaultEOL;

            var prefab = isSwitch ? _gameManager.GetSwitchPrefab(type) : _gameManager.GetServerPrefab(type);

            if (prefab is null)
                return DefaultEOL;

            object device = isSwitch ? prefab.GetComponent<NetworkSwitch>() : prefab.GetComponent<Server>();

            if (device is null)
                return DefaultEOL;

            var defaultEOL = isSwitch ? ((NetworkSwitch)device).eolTime : ((Server)device).eolTime;

            if (defaultEOL < int.MaxValue)
                // The in-game value can end up one second below the real cap, so nudge it upward once.
                defaultEOL++;

            dict.Add(type, defaultEOL);

            return defaultEOL;
        }

        /// <summary>
        /// Repairs all broken switches when the corresponding option is enabled.
        /// </summary>
        private void RepairSwitches()
        {
            if (!_readyToRun || !OptionsManager.Instance.GetConfigOptionValue<bool>(OptionType.AutoRepairBrokenSwitches))
                return;

            var brokenSwitches = _networkMap.GetAllBrokenSwitches().ToArray();

            foreach (var brokenSwitch in brokenSwitches)
                brokenSwitch.RepairDevice();
        }

        /// <summary>
        /// Repairs all broken servers when the corresponding option is enabled.
        /// </summary>
        private void RepairServers()
        {
            if (!_readyToRun || !OptionsManager.Instance.GetConfigOptionValue<bool>(OptionType.AutoRepairBrokenServers))
                return;

            var brokenServers = _networkMap.GetAllBrokenServers().ToArray();

            foreach (var brokenServer in brokenServers)
                brokenServer.RepairDevice();
        }

        /// <summary>
        /// Restores all switches to their default EOL value when switch EOL is disabled.
        /// </summary>
        private void HandleSwitchesEOL()
        {
            if (!_readyToRun || !OptionsManager.Instance.GetConfigOptionValue<bool>(OptionType.DisableSwitchesEOL))
                return;

            var switches = _networkMap.switches.Values;

            foreach (var networkSwitch in switches)
                networkSwitch.eolTime = GetEOLDeviceDefaultEOL(networkSwitch);
        }

        /// <summary>
        /// Restores all servers to their default EOL value when server EOL is disabled.
        /// </summary>
        private void HandleServersEOL()
        {
            if (!_readyToRun || !OptionsManager.Instance.GetConfigOptionValue<bool>(OptionType.DisableServersEOL))
                return;

            var servers = _networkMap.servers.Values;

            foreach (var server in servers)
                server.eolTime = GetEOLDeviceDefaultEOL(server);
        }

        /// <summary>
        /// Waits until the main gameplay scene is ready, then continuously applies the selected mod behavior.
        /// </summary>
        public override void OnUpdate()
        {
            if (_readyToRun)
            {
                // Re-apply the configured behavior continuously so newly created or changed devices
                // are handled without needing scene reloads or extra hooks.
                RepairSwitches();
                RepairServers();
                HandleSwitchesEOL();
                HandleServersEOL();

                return;
            }

            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex != BaseSceneBuildIndex)
                return;

            try
            {
                // These singleton instances may not exist on the first few frames after the scene loads.
                var networkMap = NetworkMap.instance;
                var mainGameManager = MainGameManager.instance;
                
                if (networkMap is null || mainGameManager is null)
                    return;

                // Cache the references once so the rest of the update loop can avoid repeated singleton lookups.
                _readyToRun = true;
                _networkMap = networkMap;
                _gameManager = mainGameManager;
            }
            catch (Exception ex)
            {
                LoggerInstance.Msg("OnUpdate -> Exception -> " + ex);
            }
        }

        /// <summary>
        /// Resets cached scene references when the player leaves gameplay and returns to the main menu.
        /// </summary>
        /// <param name="buildIndex">Build index of the scene that was loaded.</param>
        /// <param name="sceneName">Name of the loaded scene provided by MelonLoader.</param>
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (buildIndex == MainMenuSceneBuildIndex)
            {
                // Clear scene-specific references when returning to the main menu.
                _readyToRun = false;
                _gameManager = null;
                _networkMap = null;
            }
        }
    }
}
