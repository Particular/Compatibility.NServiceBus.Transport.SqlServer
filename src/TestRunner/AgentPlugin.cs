namespace NServiceBus.Compatibility.TestRunner;

using System;
using System.IO;
using System.Threading.Tasks;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
using NuGet.Versioning;
using System.Xml.Linq;

class AgentPlugin
{
    static readonly AsyncDuplicateLock Locks = new AsyncDuplicateLock();

    readonly string projectName;
    readonly string behaviorTypeName;
    readonly string generatedProjectFolder;
    readonly PluginOptions opts;
    IPlugin plugin;
    bool started;
    readonly string behaviorPackageName;
    readonly SemanticVersion versionToTest;
    readonly string transportPackageName;

    PluginLoadContext pluginLoadContext;

#if NET7_0
    const string TargetFramework = "net7.0";
#endif

    public AgentPlugin(
        SemanticVersion versionToTest,
        string behaviorTypeName,
        string generatedProjectFolder,
        PluginOptions opts)
    {
        this.versionToTest = versionToTest;
        behaviorPackageName = $"Compatibility.NServiceBus.Transport.SqlServer.V{versionToTest.Major}"; //behaviors depend only on downstream major
        this.behaviorTypeName = behaviorTypeName; //$"{behaviorTypeName}, NServiceBus.Compatibility.SqlServer.V{versionToTest.Major}";
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

            var fileVersionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            var version = fileVersionInfo.ProductVersion;

            var projectFilePath = Path.Combine(projectFolder, $"{projectName}.csproj");
            if (!File.Exists(projectFilePath))
            {
                // Private=""false"" ExcludeAssets=""runtime"" prevent files from copied to the bin folder
                //
                // https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files#controlling-dependency-assets
                // https://github.com/dotnet/docs/issues/15811
                //
                var commonReference =
                    $@"<PackageReference Include=""Compatibility.NServiceBus.Common"" Version=""{version}"" Private=""false"" ExcludeAssets=""runtime"" />";

                var behaviorReference =
                    $@"<PackageReference Include=""{behaviorPackageName}"" Version=""{version}"" />";

                var isDevelopment = opts.VersionBeingDeveloped != null && SemanticVersion.Parse(opts.VersionBeingDeveloped).Equals(versionToTest);
                var transportReference = isDevelopment
                    ? $@"<ProjectReference Include=""..\..\{transportPackageName}\{transportPackageName}.csproj"" />"
                    : $@"<PackageReference Include=""{transportPackageName}"" Version=""{versionToTest.ToNormalizedString()}"" />";

                var xml = $@"
                        <Project Sdk=""Microsoft.NET.Sdk"">
                          <PropertyGroup>
                            <TargetFramework>{TargetFramework}</TargetFramework>
                            <RootNamespace>TestAgent</RootNamespace>
                            <EnableDynamicLoading>true</EnableDynamicLoading>
                            <AssemblyName>NServiceBus.Compatibility.{projectName}</AssemblyName>
                          </PropertyGroup>
                          <ItemGroup>
                            {commonReference}
                            {behaviorReference}
                            {transportReference}
                          </ItemGroup>
                        </Project>
                        ";

                var xCsProj = XDocument.Parse(xml);
                xCsProj.Save(projectFilePath);
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

            var assemblyFileName = $"Compatibility.NServiceBus.Transport.SqlServer.V{versionToTest.Major}.dll";

#if DEBUG
            var path = $"{folder}/bin/Debug/{TargetFramework}/";
#else
            var path = $"{folder}/bin/Release/{TargetFramework}/";
#endif
            var agentDllPath = Path.Combine(path, assemblyFileName);

            if (!File.Exists(agentDllPath))
            {
                throw new FileNotFoundException("Expected agent assembly not present after compilation", agentDllPath);
            }

            var pluginAssembly = LoadPlugin(agentDllPath);
            var behaviorType = pluginAssembly.GetType(behaviorTypeName, true);
            plugin = (IPlugin)Activator.CreateInstance(behaviorType);
        }
    }

    public async Task StartEndpoint(CancellationToken cancellationToken = default)
    {
        await plugin.StartEndpoint(opts, cancellationToken).ConfigureAwait(false);
        started = true;
    }

    public Task StartTest(CancellationToken cancellationToken = default) => plugin.StartTest(cancellationToken);

    public async Task Stop(CancellationToken cancellationToken = default)
    {
        if (!started)
        {
            return;
        }

        await plugin.Stop(cancellationToken).ConfigureAwait(false);

        //pluginLoadContext.Unload();
        //pluginLoadContext = null;
    }

    Assembly LoadPlugin(string pluginLocation)
    {
        pluginLoadContext = new PluginLoadContext(pluginLocation);
        var assemblyName = AssemblyName.GetAssemblyName(pluginLocation);
        return pluginLoadContext.LoadFromAssemblyName(assemblyName);
    }
}