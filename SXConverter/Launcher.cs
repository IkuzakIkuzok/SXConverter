
// (c) 2024 Kazuki Kohzuki

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace SXConverter;

internal static class Launcher
{
    private const string ProviderName = "Ultrafast Systems";
    private const string AppName = "SurfaceXplorer.exe";

    private static readonly string ProviderFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), ProviderName);

    private static string? appPath = null;

    internal static bool IsInstalled
    {
        get
        {
            if (appPath is null) SearchApp();
            return !string.IsNullOrEmpty(appPath);
        }
    }

    internal static string AppPath
    {
        get
        {
            if (appPath is null) SearchApp();
            return appPath;
        }
    }

    internal static void LaunchSX(string filename)
    {
        if (!IsInstalled) return;
        var psi = new ProcessStartInfo(AppPath, filename);
        Process.Start(psi);
    } // internal static void LaunchSX (string)

    [MemberNotNull(nameof(appPath))]
    private static void SearchApp()
    {
        if (appPath is not null) return;

        List<(string Path, double Version)> candidates = [];
        var folders = Directory.EnumerateDirectories(ProviderFolder, "Surface Xplorer v*", SearchOption.TopDirectoryOnly);
        foreach (var folder in folders)
        {
            var versionStr = folder[(folder.LastIndexOf('v') + 1)..];
            if (!double.TryParse(versionStr, out var version)) continue;
            var path = Path.Combine(folder, AppName);
            if (!File.Exists(path)) continue;
            candidates.Add((path, version));
        }

        if (candidates.Count == 0)
        {
            appPath = string.Empty;
            return;
        }

        appPath = candidates.OrderByDescending(c => c.Version).First().Path;
    } // private static void SearchApp ()
} // internal static class Launcher
