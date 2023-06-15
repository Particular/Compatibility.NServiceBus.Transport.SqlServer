using NUnit.Framework;

public class TestCaseSourcePackageSupportedVersionsAttribute : TestCaseSourceAttribute
{
    public TestCaseSourcePackageSupportedVersionsAttribute(string packageId, string rangeValue) : base(typeof(GeneratedVersionsSet), nameof(GeneratedVersionsSet.GetLatestMinors), new object[] { packageId, rangeValue })
    {
    }
}