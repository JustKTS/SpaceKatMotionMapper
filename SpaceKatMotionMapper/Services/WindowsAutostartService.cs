#if WINDOWS
using System;
using Microsoft.Win32;
using PlatformAbstractions;

namespace SpaceKatMotionMapper.Services;

public class WindowsAutostartService : IPlatformAutostartService
{
    private const string AppName = "SpaceKatMotionMapper";
    private const string RunKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string MinimizedArg = "--minimized";

    public bool IsAutostartEnabled
    {
        get
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath);
                return key?.GetValue(AppName) != null;
            }
            catch
            {
                return false;
            }
        }
        set
        {
            if (value)
                Enable();
            else
                Disable();
        }
    }

    public bool IsAvailable => true;

    private static void Enable()
    {
        try
        {
            var exePath = Environment.ProcessPath;
            if (string.IsNullOrEmpty(exePath)) return;

            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true);
            key?.SetValue(AppName, $"\"{exePath}\" {MinimizedArg}");
        }
        catch
        {
        }
    }

    private static void Disable()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true);
            key?.DeleteValue(AppName, throwOnMissingValue: false);
        }
        catch
        {
        }
    }
}
#endif
