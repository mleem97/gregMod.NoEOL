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

namespace NoMoreEOL.Options
{
    /// <summary>
    /// Base metadata container for a configuration option exposed by the mod.
    /// </summary>
    public abstract class ConfigOption
    {
        /// <summary>
        /// Internal key used to identify the option in the configuration system.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// User-facing name shown in the mod configuration UI.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Default value used when the option has not been explicitly configured.
        /// </summary>
        public object DefaultValue { get; protected init; }

        /// <summary>
        /// User-facing description explaining what the option does.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Initializes the shared metadata for a configuration option.
        /// </summary>
        /// <param name="key">Internal key used to store and retrieve the option.</param>
        /// <param name="displayName">Name shown to the player in the configuration UI.</param>
        /// <param name="defaultValue">Default value assigned to the option.</param>
        /// <param name="description">Description shown alongside the option in the UI.</param>
        protected ConfigOption(string key, string displayName, object defaultValue, string description)
        {
            Key = key;
            DisplayName = displayName;
            DefaultValue = defaultValue;
            Description = description;
        }
    }

    /// <summary>
    /// Strongly typed configuration option that registers itself with the mod configuration system.
    /// </summary>
    /// <typeparam name="T">Supported value type for the option.</typeparam>
    public class ConfigOption<T> : ConfigOption where T : struct
    {
        /// <summary>
        /// Creates and registers a typed configuration option.
        /// </summary>
        /// <param name="key">Internal key used to store and retrieve the option.</param>
        /// <param name="displayName">Name shown to the player in the configuration UI.</param>
        /// <param name="defaultValue">Default value assigned to the option.</param>
        /// <param name="description">Description shown alongside the option in the UI.</param>
        public ConfigOption(string key, string displayName, T defaultValue, string description)
            : base(key, displayName, defaultValue, description)
        {
            DefaultValue = defaultValue;

            // Avoid double-registration if this option has already been tracked by the local manager.
            if (!OptionsManager.Instance.AddConfigOption(this))
                return;

            // The external config API is type-specific, so map the generic option to the matching registration call.
            switch (typeof(T))
            {
                case Type t when t == typeof(bool):
                {
                    ModConfigSystem.RegisterBoolOption(Core.ModName, key, displayName, (bool)DefaultValue, description);
                    break;
                }
                case Type t when t == typeof(int):
                {
                    ModConfigSystem.RegisterIntOption(Core.ModName, key, displayName, (int)DefaultValue, int.MinValue, int.MaxValue, description);
                    break;
                }
                case Type t when t == typeof(float):
                {
                    ModConfigSystem.RegisterFloatOption(Core.ModName, key, displayName, (float)DefaultValue, float.MinValue, float.MaxValue, description);
                    break;
                }
            }
        }
    }
}
