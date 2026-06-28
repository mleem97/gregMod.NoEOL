using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace GregModNoEOL;

/// <summary>
/// Writes a verbose log (<c>noeol.latest.log</c>) next to MelonLoader's <c>Latest.log</c>.
/// Recreated on each game launch.
/// </summary>
internal static class ModReleaseLog
{
    private static string _path;
    private static bool _initTried;
    private static readonly object Sync = new();

    internal static string LogPath => _path;

    internal static void Bootstrap()
    {
        lock (Sync)
        {
            if (_initTried) return;
            _initTried = true;
            try
            {
                var dir = Path.GetDirectoryName(Application.dataPath);
                if (string.IsNullOrEmpty(dir)) return;
                _path = Path.Combine(dir, "noeol.latest.log");
                WriteHeader();
            }
            catch { _path = null; }
        }
    }

    private static void WriteHeader()
    {
        var asm = Assembly.GetExecutingAssembly();
        var version = asm.GetName().Version?.ToString() ?? "unknown";
        var buildDate = File.GetLastWriteTimeUtc(asm.Location);

        Append($"============================================================");
        Append($"  gregMod.NoEOL — Verbose Release Log");
        Append($"============================================================");
        Append($"");
        Append($"Version:       {version}");
        Append($"Build Date:    {buildDate:yyyy-MM-dd HH:mm:ss} UTC");
        Append($"Log Started:   {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        Append($"");
        Append($"------------------------------------------------------------");
        Append($"  Environment");
        Append($"------------------------------------------------------------");
        Append($"OS:            {System.Runtime.InteropServices.RuntimeInformation.OSDescription}");
        Append($".NET:          {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}");
        Append($"Unity:         {Application.unityVersion}");
        Append($"Game:          {Application.productName} {Application.version}");
        Append($"Data Path:     {Application.dataPath}");
        Append($"Platform:      {Application.platform}");
        Append($"Screen:        {Screen.width}x{Screen.height} @ {Screen.currentResolution.refreshRateRatio}Hz");
        Append($"");
        Append($"------------------------------------------------------------");
        Append($"  Mod Info");
        Append($"------------------------------------------------------------");
        Append($"Mod GUID:      com.gregmod.noeol");
        Append($"Mod Name:      gregMod.NoEOL");
        Append($"Author:        TeamGreg Modding (Neox & mleem97)");
        Append($"License:       Apache 2.0");
        Append($"Namespace:     GregModNoEOL");
        Append($"Assembly:      {Path.GetFileName(asm.Location)}");
        Append($"");
        Append($"============================================================");
        Append($"");
    }

    internal static void Info(string message) { Append($"[INFO]  {Ts()} {message}"); }
    internal static void Warning(string message) { Append($"[WARN]  {Ts()} {message}"); }
    internal static void Error(string message) { Append($"[ERROR] {Ts()} {message}"); }
    internal static void Error(string message, Exception ex)
    {
        Append($"[ERROR] {Ts()} {message}");
        if (ex != null)
        {
            Append($"        Exception: {ex.GetType().FullName}");
            Append($"        Message:   {ex.Message}");
        }
    }

    internal static void EolEvent(string message) { Append($"[EOL]   {Ts()} {message}"); }
    internal static void RepairEvent(string message) { Append($"[FIX]   {Ts()} {message}"); }
    internal static void SceneEvent(string message) { Append($"[SCENE] {Ts()} {message}"); }
    internal static void ConfigEvent(string message) { Append($"[CFG]   {Ts()} {message}"); }

    private static string Ts() => $"{DateTime.UtcNow:HH:mm:ss.fff}";

    private static void Append(string line)
    {
        if (string.IsNullOrEmpty(_path)) return;
        try { File.AppendAllText(_path, line + Environment.NewLine); }
        catch { }
    }
}
