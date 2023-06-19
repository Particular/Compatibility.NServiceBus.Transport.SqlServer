namespace NServiceBus.Compatibility.TestRunner;

using System.Collections.Generic;

/// <summary>
/// Results of the test
/// </summary>
public class TestExecutionResult
{
    /// <summary>
    /// Holds values of variables defined in the test description (either bool or int)
    /// </summary>
    public List<AuditMessage> AuditedMessages { get; set; }

    /// <summary>
    /// Has the test succeeded?
    /// </summary>
    public bool Succeeded { get; set; }
}