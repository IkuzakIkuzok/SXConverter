﻿
// (c) 2024 Kazuki Kohzuki

using Microsoft.Win32;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace SXConverter.Launchers;

/// <summary>
/// Provides methods to launch an application with the specified file extension.
/// </summary>
internal class AppLauncher
{
    private const string OpenCommand = @"shell\open\command";

    private readonly string extension;
    protected string appCommand;

    /// <summary>
    /// Gets the file extension associated to this instance.
    /// </summary>
    internal string Extension
        => this.extension;

    /// <summary>
    /// Gets a value indicating whether the associated application is registered.
    /// </summary>
    internal bool IsRegistered
        => !string.IsNullOrEmpty(this.appCommand);

    /// <summary>
    /// Gets the command to run the associated application with the specified file extension.
    /// </summary>
    internal string AppCommand
        => this.appCommand;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppLauncher"/> class with the specified file extension.
    /// </summary>
    /// <param name="extension">The file extension associated to this instance.</param>
    internal AppLauncher(string extension)
    {
        this.extension = extension;
        this.appCommand = SearchApp(extension);
    } // ctor (string)

    /// <summary>
    /// Searches the associated application with the specified file extension.
    /// </summary>
    /// <param name="extension">The file extension to search.</param>
    /// <returns>The command to run the associated application with the specified file extension.
    /// If the associated application is not found, returns an empty string.</returns>
    [MemberNotNull(nameof(appCommand))]
    protected virtual string SearchApp(string extension)
    {
        if (this.appCommand is not null) return this.appCommand;

        var keyExt = Registry.ClassesRoot.OpenSubKey(extension);
        if (keyExt?.OpenSubKey(OpenCommand) is RegistryKey openKey)
        {
            if (openKey.GetValue(string.Empty) is string command)
                return this.appCommand = command;
        }

        if (keyExt?.GetValue(string.Empty) is not string appName)
            return this.appCommand = string.Empty;

        var appKey = Registry.ClassesRoot.OpenSubKey(appName);
        var appOpenKey = appKey?.OpenSubKey(OpenCommand);
        return this.appCommand = appOpenKey?.GetValue(string.Empty) as string ?? string.Empty;
    } // protected virtual string SearchApp (string)

    /// <summary>
    /// Gets the command to run the associated application with the specified file.
    /// </summary>
    /// <param name="filename">The name of the file to open.</param>
    /// <returns>The command to run the associated application with the specified file.</returns>
    protected virtual string GetRunCommand(string filename)
        => this.appCommand.Replace("%1", filename);

    /// <summary>
    /// Opens the specified file with the associated application.
    /// </summary>
    /// <param name="filename">The name of the file to open.</param>
    /// <returns><see langword="true"/> if the file is opened successfully; otherwise, <see langword="false"/>.</returns>
    internal bool OpenFile(string filename)
    {
        if (!this.IsRegistered) return false;
        var psi = new ProcessStartInfo(GetRunCommand(filename));
        return Process.Start(psi) is not null;
    } // internal bool OpenFile (string)
} // internal class AppLauncher
