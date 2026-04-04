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

using DataCenterModLoader;
using NoMoreEOL.Enums;

namespace NoMoreEOL.Options
{
    /// <summary>
    /// Central registry for mod configuration options and their runtime values.
    /// </summary>
    public class OptionsManager
    {
        /// <summary>
        /// Backing instance for the lazy-loaded singleton.
        /// </summary>
        private static OptionsManager _instance;

        /// <summary>
        /// Stores registered configuration options keyed by their enum identifier.
        /// </summary>
        private static Dictionary<OptionType, ConfigOption> _optionsDict = new();

        /// <summary>
        /// Gets the shared options manager instance.
        /// </summary>
        public static OptionsManager Instance 
        {
            get
            {
                if (_instance is null)
                    _instance = new OptionsManager();

                return _instance;
            }
        }

        /// <summary>
        /// Returns the registered metadata for a specific option.
        /// </summary>
        /// <param name="optionType">The option to look up.</param>
        /// <returns>The registered option metadata, or <see langword="null"/> if it has not been registered.</returns>
        public ConfigOption GetConfigOption(OptionType optionType)
        {
            if (_optionsDict.ContainsKey(optionType))
                return _optionsDict[optionType];

            return null;
        }

        /// <summary>
        /// Reads the current value of a registered option from the external mod configuration system.
        /// </summary>
        /// <typeparam name="T">Expected value type for the option.</typeparam>
        /// <param name="optionType">The option whose value should be read.</param>
        /// <returns>The current option value, or the type default if the option is missing or unsupported.</returns>
        public T GetConfigOptionValue<T> (OptionType optionType) where T : struct
        {
            var option = GetConfigOption(optionType);

            if (option == null)
                return default(T);

            // The config API exposes separate getters for each primitive type the mod supports.
            switch (typeof(T))
            {
                case Type t when t == typeof(bool):
                    return (T)(object)ModConfigSystem.GetBoolValue(Core.ModName, optionType.ToString());
                case Type t when t == typeof(int):
                    return (T)(object)ModConfigSystem.GetIntValue(Core.ModName, optionType.ToString());
                case Type t when t == typeof(float):
                    return (T)(object)ModConfigSystem.GetFloatValue(Core.ModName, optionType.ToString());
                default:
                    return default(T);
            }
        }

        /// <summary>
        /// Registers a configuration option with the local option dictionary.
        /// </summary>
        /// <param name="configOption">The option metadata to add.</param>
        /// <returns><see langword="true"/> if the option was added; otherwise, <see langword="false"/>.</returns>
        public bool AddConfigOption(ConfigOption configOption)
        {
            // Option keys are expected to match the enum names so lookup stays consistent everywhere else.
            if (!Enum.TryParse<OptionType>(configOption.Key, out var optionType))
                return false;

            if (_optionsDict.ContainsKey(optionType))
                return false;

            _optionsDict.Add(optionType, configOption);

            return true;
        }

        /// <summary>
        /// Indicates whether the default option set has already been registered.
        /// </summary>
        public bool Initialized { get; private set;  }

        /// <summary>
        /// Prevents external construction; use <see cref="Instance"/> instead.
        /// </summary>
        private OptionsManager() { }

        /// <summary>
        /// Registers the full set of options exposed by this mod.
        /// </summary>
        public void InitializeOptions()
        {
            if (Initialized)
                return;

            // Register each supported option once during startup so it becomes available in the config UI.
            new ConfigOption<bool>
            (
                key: nameof(OptionType.DisableSwitchesEOL),
                displayName: "Disable Switches End-of-Life",
                defaultValue: true,
                description: "Prevents switches from reaching End-of-Life, keeping them operational indefinitely."
            );

            new ConfigOption<bool>
            (
                key: nameof(OptionType.DisableServersEOL),
                displayName: "Disable Servers End-of-Life",
                defaultValue: true,
                description: "Prevents servers from reaching End-of-Life, keeping them operational indefinitely."
            );

            new ConfigOption<bool>
            (
                key: nameof(OptionType.AutoRepairBrokenSwitches),
                displayName: "Auto Repair Broken Switches",
                defaultValue: true,
                description: "Automatically repairs switches when they break, eliminating the need for manual maintenance."
            );

            new ConfigOption<bool>
            (
                key: nameof(OptionType.AutoRepairBrokenServers),
                displayName: "Auto Repair Broken Servers",
                defaultValue: true,
                description: "Automatically repairs servers when they break, eliminating the need for manual maintenance."
            );

            Initialized = true;
        }
    }
}
