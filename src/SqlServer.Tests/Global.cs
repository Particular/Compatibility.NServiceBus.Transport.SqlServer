using System;

static class Global
{
    public static string ConnectionString { get; } = Environment.GetEnvironmentVariable("SQLSERVERTRANSPORTCONNECTIONSTRING") ?? "Data source = (local); Initial catalog = nservicebus; Integrated Security = true; Encrypt=false";
}