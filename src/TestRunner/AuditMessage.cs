namespace NServiceBus.Compatibility.TestRunner;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents the audit message
/// </summary>
/// <remarks>
/// Ctor
/// </remarks>
public class AuditMessage(string id, Dictionary<string, string> headers, ReadOnlyMemory<byte> body)
{

    /// <summary>
    /// Body of the message
    /// </summary>
    public ReadOnlyMemory<byte> Body { get; } = body;
    /// <summary>
    /// Headers of the message
    /// </summary>
    public Dictionary<string, string> Headers { get; } = headers;

    /// <summary>
    /// Id of the message
    /// </summary>
    public string Id { get; } = id;
}