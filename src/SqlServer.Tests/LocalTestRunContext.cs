using NuGet.Versioning;

public partial class TestRunContext
{
    public bool UsePackageReferences => false;
    public bool RunAgainstSpecificVersion => false;
    public SemanticVersion VersionUnderTest => null;
}
