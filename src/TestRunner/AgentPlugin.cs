namespace NServiceBus.Compatibility.TestRunner;

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
using NuGet.Versioning;

class AgentPlugin
{
    static readonly AsyncDuplicateLock Locks = new AsyncDuplicateLock();

    readonly string projectName;
    readonly string behaviorType;
    readonly Dictionary<string, string> platformSpecificAssemblies;
    readonly string generatedProjectFolder;
    readonly PluginOptions opts;
    IPlugin plugin;
    bool started;
    readonly string behaviorCsProjFileName;
    readonly SemanticVersion versionToTest;
    readonly string transportPackageName;

#if NET7_0
    const string TargetFramework = "net7.0";
#endif

    static readonly List<string> projects = new List<string>();
    public AgentPlugin(
        Dictionary<string, string> platformSpecificAssemblies,
        SemanticVersion versionToTest,
        string behaviorType,
        string generatedProjectFolder,
        PluginOptions opts)
    {
        this.versionToTest = versionToTest;
        behaviorCsProjFileName = $"SqlServer.V{versionToTest.Major}"; //behaviors depend only on downstream major
        this.behaviorType = $"{behaviorType}, NServiceBus.Compatibility.SqlServer.V{versionToTest.Major}";
        this.platformSpecificAssemblies = platformSpecificAssemblies;
        this.generatedProjectFolder = generatedProjectFolder;
        this.opts = opts;
        transportPackageName = versionToTest.Major > 5 ? "NServiceBus.Transport.SqlServer" : "NServiceBus.SqlServer";
        projectName = $"Agent.{transportPackageName}.{versionToTest.ToNormalizedString()}"; //generated project depends on downstream minor
    }


    public async Task Compile(CancellationToken cancellationToken = default)
    {
        var disposable = await Locks.LockAsync(projectName, cancellationToken).ConfigureAwait(false);

        using (disposable)
        {
            var projectFolder = Path.Combine(generatedProjectFolder, projectName);
            if (!Directory.Exists(projectFolder))
            {
                Directory.CreateDirectory(projectFolder);
            }

            var projectFilePath = Path.Combine(projectFolder, $"{projectName}.csproj");
            if (!File.Exists(projectFilePath))
            {
                await File.WriteAllTextAsync(projectFilePath, @$"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFramework>{TargetFramework}</TargetFramework>
    <RootNamespace>TestAgent</RootNamespace>
    <EnableDynamicLoading>true</EnableDynamicLoading>
    <AssemblyName>NServiceBus.Compatibility.{projectName}</AssemblyName>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include=""..\..\Common\Common.csproj"" >
      <Private>false</Private>
      <ExcludeAssets>runtime</ExcludeAssets>
    </ProjectReference>

    <ProjectReference Include=""..\..\{behaviorCsProjFileName}\{behaviorCsProjFileName}.csproj"" />

    <PackageReference Include=""{transportPackageName}"" Version=""{versionToTest.ToNormalizedString()}"" />

  </ItemGroup>

</Project>
", cancellationToken).ConfigureAwait(false);
            }

            var buildProcess = new Process();
            buildProcess.StartInfo.FileName = @"dotnet";
            buildProcess.StartInfo.Arguments = $"build \"{projectFilePath}\"";
#if !DEBUG
            buildProcess.StartInfo.Arguments += " --configuration Release";
#endif
            buildProcess.StartInfo.UseShellExecute = false;
            buildProcess.StartInfo.RedirectStandardOutput = true;
            buildProcess.StartInfo.RedirectStandardError = true;
            buildProcess.StartInfo.RedirectStandardInput = true;
            buildProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            buildProcess.StartInfo.CreateNoWindow = true;

            buildProcess.Start();

            await buildProcess.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

            if (buildProcess.ExitCode != 0)
            {
                var buildOutput = await buildProcess.StandardOutput.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
                await Console.Out.WriteLineAsync(buildOutput).ConfigureAwait(false);
                throw new Exception("Build failed");
            }

            var folder = Path.GetDirectoryName(projectFilePath);

            var coreAssemblyFilePattern = "NServiceBus.Compatibility.Core.V*.dll";
#if DEBUG
            var scanPath = $"{folder}/bin/Debug/{TargetFramework}/";
#else
            var scanPath = $"{folder}/bin/Release/{TargetFramework}/";
#endif
            var agentDllPath = Directory.EnumerateFiles(scanPath, coreAssemblyFilePattern).SingleOrDefault();

            if (default == agentDllPath)
            {
                throw new FileNotFoundException("Expected agent assembly not present after compilation", Path.Combine(scanPath, coreAssemblyFilePattern));
            }

            var pluginAssembly = LoadPlugin(agentDllPath);
            plugin = CreateCommands(pluginAssembly).Single();
            projects.Add(projectName);
        }
    }

    public async Task StartEndpoint(CancellationToken cancellationToken = default)
    {
        await plugin.StartEndpoint(behaviorType, opts, cancellationToken).ConfigureAwait(false);
        started = true;
    }

    public Task StartTest(CancellationToken cancellationToken = default) => plugin.StartTest(cancellationToken);

    public Task Stop(CancellationToken cancellationToken = default)
    {
        if (!started)
        {
            return Task.CompletedTask;
        }

        return plugin.Stop(cancellationToken);
    }

    Assembly LoadPlugin(string pluginLocation)
    {
        var loadContext = new PluginLoadContext(pluginLocation, platformSpecificAssemblies);

        return loadContext.LoadFromAssemblyName(AssemblyName.GetAssemblyName(pluginLocation));
    }

    static IEnumerable<IPlugin> CreateCommands(Assembly assembly)
    {
        int count = 0;

        foreach (Type type in assembly.GetTypes())
        {
            if (typeof(IPlugin).IsAssignableFrom(type))
            {
                if (Activator.CreateInstance(type) is IPlugin result)
                {
                    count++;
                    yield return result;
                }
            }
        }

        if (count == 0)
        {
            string availableTypes = string.Join(",", assembly.GetTypes().Select(t => t.FullName));
            throw new ApplicationException($"Can't find any type which implements '{typeof(IPlugin)}' in '{assembly}' from '{assembly.Location}'.\nAvailable types: {availableTypes}");
        }
    }
}