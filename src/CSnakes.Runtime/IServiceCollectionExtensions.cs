﻿using CSnakes.Runtime.Locators;
using CSnakes.Runtime.PackageManagement;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CSnakes.Runtime;
/// <summary>
/// Extension methods for <see cref="IServiceCollection"/> to configure Python-related services.
/// </summary>
public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Adds Python-related services to the service collection with the specified Python home directory.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="home">The Python home directory.</param>
    /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
    public static IPythonEnvironmentBuilder WithPython(this IServiceCollection services)
    {
        var pythonBuilder = new PythonEnvironmentBuilder(services);

        services.AddSingleton<IPythonEnvironmentBuilder>(pythonBuilder);
        services.AddSingleton<IPythonEnvironment>(sp =>
        {
            var envBuilder = sp.GetRequiredService<IPythonEnvironmentBuilder>();
            var locators = sp.GetServices<PythonLocator>();
            var installers = sp.GetServices<IPythonPackageInstaller>();
            var logger = sp.GetRequiredService<ILogger<IPythonEnvironment>>();

            var options = envBuilder.GetOptions();

            return PythonEnvironment.GetPythonEnvironment(locators, installers, options, logger);
        });

        return pythonBuilder;
    }

    public static Version ParsePythonVersion(string version)
    {
        // Remove non -numeric characters except .
        version = new string(version.Where(c => char.IsDigit(c) || c == '.').ToArray());

        var v = Version.Parse(version);
        if (v.Minor == -1)
            return new Version(v.Major, 0, 0);
        if (v.Build == -1)
            return new Version(v.Major, v.Minor, 0);
        return v;
    }

    /// <summary>
    /// Adds a Python locator using Python from a NuGet packages to the service collection with the specified version.
    /// </summary>
    /// <param name="builder">The <see cref="IPythonEnvironmentBuilder"/> to add the locator to.</param>
    /// <param name="version">The version of the NuGet package.</param>
    /// <returns>The modified <see cref="IPythonEnvironmentBuilder"/>.</returns>
    public static IPythonEnvironmentBuilder FromNuGet(this IPythonEnvironmentBuilder builder, string version)
    {
        builder.Services.AddSingleton<PythonLocator>(new NuGetLocator(ParsePythonVersion(version)));
        return builder;
    }

    /// <summary>
    /// Adds a Python locator using Python from the Windows Store packages to the service collection with the specified version.
    /// </summary>
    /// <param name="builder">The <see cref="IPythonEnvironmentBuilder"/> to add the locator to.</param>
    /// <param name="version">The version of the Windows Store package.</param>
    /// <returns>The modified <see cref="IPythonEnvironmentBuilder"/>.</returns>
    public static IPythonEnvironmentBuilder FromWindowsStore(this IPythonEnvironmentBuilder builder, string version)
    {
        builder.Services.AddSingleton<PythonLocator>(new WindowsStoreLocator(ParsePythonVersion(version)));
        return builder;
    }

    /// <summary>
    /// Adds a Python locator using Python from the Windows Installer packages to the service collection with the specified version.
    /// </summary>
    /// <param name="builder">The <see cref="IPythonEnvironmentBuilder"/> to add the locator to.</param>
    /// <param name="version">The version of the Windows Installer package.</param>
    /// <returns>The modified <see cref="IPythonEnvironmentBuilder"/>.</returns>
    public static IPythonEnvironmentBuilder FromWindowsInstaller(this IPythonEnvironmentBuilder builder, string version)
    {
        builder.Services.AddSingleton<PythonLocator>(new WindowsInstallerLocator(ParsePythonVersion(version)));
        return builder;
    }

    /// <summary>
    /// Adds a Python locator using Python from the Official macOS Installer packages to the service collection with the specified version.
    /// </summary>
    /// <param name="builder">The <see cref="IPythonEnvironmentBuilder"/> to add the locator to.</param>
    /// <param name="version">The version of the Windows Installer package.</param>
    /// <returns>The modified <see cref="IPythonEnvironmentBuilder"/>.</returns>
    public static IPythonEnvironmentBuilder FromMacOSInstallerLocator(this IPythonEnvironmentBuilder builder, string version)
    {
        builder.Services.AddSingleton<PythonLocator>(new MacOSInstallerLocator(ParsePythonVersion(version)));
        return builder;
    }

    public static IPythonEnvironmentBuilder FromSource(this IPythonEnvironmentBuilder builder, string folder, string version, bool debug = true, bool freeThreaded = false)
    {
        builder.Services.AddSingleton<PythonLocator>(new SourceLocator(folder, ParsePythonVersion(version), debug, freeThreaded));
        return builder;
    }

    /// <summary>
    /// Adds a Python locator using Python from an environment variable to the service collection with the specified environment variable name and version.
    /// </summary>
    /// <param name="builder">The <see cref="IPythonEnvironmentBuilder"/> to add the locator to.</param>
    /// <param name="environmentVariable">The name of the environment variable.</param>
    /// <param name="version">The version of the Python installation.</param>
    /// <returns>The modified <see cref="IPythonEnvironmentBuilder"/>.</returns>
    public static IPythonEnvironmentBuilder FromEnvironmentVariable(this IPythonEnvironmentBuilder builder, string environmentVariable, string version)
    {
        builder.Services.AddSingleton<PythonLocator>(new EnvironmentVariableLocator(environmentVariable, ParsePythonVersion(version)));
        return builder;
    }

    /// <summary>
    /// Adds a Python locator using Python from a specific folder to the service collection with the specified folder path and version.
    /// </summary>
    /// <param name="builder">The <see cref="IPythonEnvironmentBuilder"/> to add the locator to.</param>
    /// <param name="folder">The path to the folder.</param>
    /// <param name="version">The version of the Python installation.</param>
    /// <returns>The modified <see cref="IPythonEnvironmentBuilder"/>.</returns>
    public static IPythonEnvironmentBuilder FromFolder(this IPythonEnvironmentBuilder builder, string folder, string version)
    {
        builder.Services.AddSingleton<PythonLocator>(new FolderLocator(folder, ParsePythonVersion(version)));
        return builder;
    }

    /// <summary>
    /// Adds a Python locator using Python from the system PATH to the service collection with the specified version.
    /// </summary>
    /// <param name="builder">The <see cref="IPythonEnvironmentBuilder"/> to add the locator to.</param>
    /// <param name="version">The version of the Python installation.</param>
    /// <returns>The modified <see cref="IPythonEnvironmentBuilder"/>.</returns>
    public static IPythonEnvironmentBuilder FromPath(this IPythonEnvironmentBuilder builder, string version)
    {
        builder.Services.AddSingleton<PythonLocator>(new PathLocator(ParsePythonVersion(version)));
        return builder;
    }

    /// <summary>
    /// Adds a pip package installer to the service collection.
    /// </summary>
    /// <param name="builder">The <see cref="IPythonEnvironmentBuilder"/> to add the installer to.</param>
    /// <returns>The modified <see cref="IPythonEnvironmentBuilder"/>.</returns>
    public static IPythonEnvironmentBuilder WithPipInstaller(this IPythonEnvironmentBuilder builder)
    {
        builder.Services.AddSingleton<IPythonPackageInstaller, PipInstaller>();
        return builder;
    }
}
