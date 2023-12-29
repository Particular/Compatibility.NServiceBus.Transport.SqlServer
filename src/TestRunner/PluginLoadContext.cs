﻿using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

/// <summary>
/// Based on:
/// 
/// - https://learn.microsoft.com/en-us/dotnet/core/tutorials/creating-app-with-plugin-support
/// - https://github.com/dotnet/samples/tree/main/core/extensions/AppWithPlugin
/// </summary>
sealed class PluginLoadContext : AssemblyLoadContext
{
    readonly string platformPath;
    readonly string libPath;
    readonly AssemblyDependencyResolver resolver;

    // https://learn.microsoft.com/en-us/dotnet/standard/net-standard
    static readonly string[] Frameworks =
    [
#if NET8_0_OR_GREATER
        "net8.0",
#endif
#if NET7_0_OR_GREATER
        "net7.0",
#endif
#if NET6_0_OR_GREATER
        "net6.0",
#endif
#if NET5_0_OR_GREATER
        "net5.0",
#endif
#if NETCOREAPP3_1_OR_GREATER
        "netcoreapp3.1",
#endif
#if NETCOREAPP3_0_OR_GREATER
        "netcoreapp3.0",
        "netstandard2.1",
#endif
#if NETCOREAPP2_1_OR_GREATER
        "netcoreapp2.1",
#endif
#if NETCOREAPP2_0_OR_GREATER
        "netcoreapp2.0",
        "netstandard2.0",
#endif        
    ];

    public PluginLoadContext(string pluginPath)
    {
        var path = Path.GetDirectoryName(pluginPath);
        resolver = new AssemblyDependencyResolver(pluginPath);
        platformPath = Path.Combine(path, "runtimes", $"win-{RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant()}", "native");

        var os = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "win" : "unix";
        libPath = Path.Combine(path, "runtimes", os, "lib");
    }

    protected override Assembly Load(AssemblyName assemblyName)
    {
        string assemblyPath = ResolveManagedDllPath(assemblyName);
        if (assemblyPath != null)
        {
            return LoadFromAssemblyPath(assemblyPath);
        }
        return null;
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        string libraryPath = ResolveUnmanagedDllPath(unmanagedDllName);
        if (libraryPath != null)
        {
            return LoadUnmanagedDllFromPath(libraryPath);
        }

        return IntPtr.Zero;
    }

    string ResolveManagedDllPath(AssemblyName assemblyName)
    {
        var name = assemblyName.Name;

        foreach (var framework in Frameworks)
        {
            var dllPath = Path.Combine(libPath, framework, $"{name}.dll");
            if (File.Exists(dllPath))
            {
                return dllPath;
            }
        }
        return resolver.ResolveAssemblyToPath(assemblyName);
    }

    string ResolveUnmanagedDllPath(string unmanagedDllName)
    {
        string dllPath = resolver.ResolveUnmanagedDllToPath(unmanagedDllName);

        if (dllPath != null)
        {
            return dllPath;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            dllPath = Path.Combine(platformPath, unmanagedDllName);
            if (File.Exists(dllPath))
            {
                return dllPath;
            }
        }
        return null;
    }
}