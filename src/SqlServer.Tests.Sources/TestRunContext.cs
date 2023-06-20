using NuGet.Versioning;

public interface ITestRunContext
{
    bool UsePackageReferences { get; }
    bool RunAgainstSpecificVersion { get; }
    SemanticVersion VersionUnderTest { get; }
}

public partial class TestRunContext : ITestRunContext
{
}

