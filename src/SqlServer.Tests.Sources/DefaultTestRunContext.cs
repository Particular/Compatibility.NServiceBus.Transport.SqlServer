using NuGet.Versioning;

public partial class TestRunContext
{
    public bool UsePackageReferences { get; }
    public bool RunAgainstSpecificVersion { get; }
    public SemanticVersion VersionUnderTest { get; }
}
