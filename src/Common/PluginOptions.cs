namespace NServiceBus.Compatibility;

public class PluginOptions
{
    public string? AuditQueue { get; set; }

    public string? ConnectionString { get; set; }
    public string? TestRunId { get; set; }
    public long? RunCount { get; set; }
}