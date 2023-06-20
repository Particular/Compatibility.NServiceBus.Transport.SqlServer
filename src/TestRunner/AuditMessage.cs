namespace NServiceBus.Compatibility.TestRunner;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents the audit message
/// </summary>
public class AuditMessage
{
    /// <summary>
    /// Ctor
    /// </summary>
    public AuditMessage(string id, Dictionary<string, string> headers, ReadOnlyMemory<byte> body)
    {
        Id = id;
        Headers = headers;
        Body = body;
    }

    /// <summary>
    /// Body of the message
    /// </summary>
    public ReadOnlyMemory<byte> Body { get; }
    /// <summary>
    /// Headers of the message
    /// </summary>
    public Dictionary<string, string> Headers { get; }

    /// <summary>
    /// Id of the message
    /// </summary>
    public string Id { get; }
}