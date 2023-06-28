namespace NServiceBus.Compatibility;

/// <summary>
/// Describes parameters for the test run
/// </summary>
public class PluginOptions
{
    /// <summary>
    /// Name of the audit queue
    /// </summary>
    public string? AuditQueue { get; set; }

    /// <summary>
    /// Should the test runner emit projects that use package references?
    /// </summary>
    public bool RunningInTransportRepo { get; set; }

    /// <summary>
    /// Transport connection strings
    /// </summary>
    public Dictionary<string, string>? ConnectionStrings { get; set; }

    /// <summary>
    /// Id of the test run
    /// </summary>
    public string? TestRunId { get; set; }

    /// <summary>
    /// Run count?
    /// </summary>
    public long? RunCount { get; set; }

    /// <summary>
    /// Version that is being developed and needs to be replaced with a project reference
    /// </summary>
    public string? VersionBeingDeveloped { get; set; }

    /// <summary>
    /// Generates a unique prefix for the test run.
    /// </summary>
    public string ApplyUniqueRunPrefix(string text)
    {
        return $"{RunCount:D3}.{text}";
    }
}