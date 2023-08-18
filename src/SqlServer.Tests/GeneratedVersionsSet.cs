﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

static class GeneratedVersionsSet
{
    static readonly SourceCacheContext cache = new() { NoCache = true };
    static readonly string[] sources;

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
            .Where(v => (!v.IsPrerelease || v.Release.StartsWith("rc.") || v.Release.StartsWith("beta.") || v.Release.StartsWith("alpha.")) && versionRange.Satisfies(v))
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

        foreach (var a in latestMinors)
        {
            foreach (var b in latestMinors)
            {
                yield return new object[] { a, b };
            }
        }
    }
}