using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

static partial class GeneratedVersionsSet
{
    static readonly SourceCacheContext cache = new() { NoCache = true };
    static readonly string[] sources;
    internal static NuGetVersion VersionFilter;

    [ModuleInitializer]
    public static void SetVersionFilter()
    {
        const string DefaultVersionTextWithoutCommitInfo = "1.0.0";

        var versionText = Assembly
            .GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            .InformationalVersion;

        if (versionText != DefaultVersionTextWithoutCommitInfo)
        {
            var version = NuGetVersion.Parse(versionText);
            VersionFilter = version;
        }
    }

    static GeneratedVersionsSet()
    {
        var settings = Settings.LoadDefaultSettings(Directory.GetCurrentDirectory());
        var packageSourceProvider = new PackageSourceProvider(settings);
        var packageSources = packageSourceProvider.LoadPackageSources();

        sources = packageSources
            .Where(x => x.IsEnabled)
            .Select(x => x.Source)
            .ToArray();
    }

    public static IEnumerable<object[]> GetLatestMinors(string packageId, string range)
    {
        var versionRange = VersionRange.Parse(range);

        HashSet<NuGetVersion> versionSet;

        try
        {
            var versionsFromAllSources = sources.AsParallel().SelectMany(source =>
            {
                var nuget = Repository.Factory.GetCoreV3(source);
                var resources = nuget.GetResource<FindPackageByIdResource>();
                return resources.GetAllVersionsAsync(packageId, cache, NullLogger.Instance, CancellationToken.None).GetAwaiter().GetResult();
            });
            versionSet = new HashSet<NuGetVersion>(versionsFromAllSources);
        }
        catch (Exception e)
        {
            throw new Exception("Error querying package sources. Review enabled sources and ensure inactive sources are disabled or removed.", e);
        }

        // Get all minors
        var versions = versionSet
            .Where(v => !v.IsPrerelease && versionRange.Satisfies(v))
            .OrderBy(v => v)
            .ToArray();

        NuGetVersion last = null;

        var latestMinors = new HashSet<NuGetVersion>();

        foreach (var v in versions)
        {
            if (last == null)
            {
                last = v;
                continue;
            }

            if (last.Major != v.Major)
            {
                latestMinors.Add(last);
            }
            else if (last.Minor != v.Minor)
            {
                latestMinors.Add(last);
            }

            last = v;
        }

        latestMinors.Add(last);

        if (VersionFilter != null)
        {
            if (versionRange.Satisfies(VersionFilter))
            {
                foreach (var a in latestMinors)
                {
                    yield return new object[] { VersionFilter, a };
                }
            }
        }
        else
        {
            foreach (var a in latestMinors)
            {
                foreach (var b in latestMinors)
                {
                    var isMatch = VersionFilter is null || IsMinorMatch(VersionFilter, a) || IsMinorMatch(VersionFilter, b);
                    if (isMatch)
                    {
                        yield return new object[] { a, b };
                    }
                }
            }
        }
    }

    static bool IsMinorMatch(NuGetVersion a, NuGetVersion b)
    {
        return a.Major == b.Major && a.Minor == b.Minor;
    }

}