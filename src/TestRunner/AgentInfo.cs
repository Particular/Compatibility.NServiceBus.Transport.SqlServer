namespace NServiceBus.Compatibility.TestRunner;

using NuGet.Versioning;

/// <summary>
/// Represents the test agent
/// </summary>
public class AgentInfo
{
    /// <summary>
    /// Version to run
    /// </summary>
    public SemanticVersion Version { get; set; }

    /// <summary>
    /// Type name of the behavior
    /// </summary>
    public string Behavior { get; set; }

    /// <summary>
    /// Run parameters
    /// </summary>
    public PluginOptions BehaviorParameters { get; set; }

    /// <summary>
    /// Creates a new instance
    /// </summary>
    public static AgentInfo Create(
        string behavior,
        SemanticVersion version,
        PluginOptions opts
        )
    {
        return new AgentInfo
        {
            Behavior = behavior,
            Version = version,
            BehaviorParameters = opts
        };
    }
}