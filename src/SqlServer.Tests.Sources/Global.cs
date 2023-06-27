using System;

static class Global
{
    public static string ConnectionString { get; } = Environment.GetEnvironmentVariable("SqlServerTransportConnectionString") ?? @"Data Source=.\SQLEXPRESS;Initial Catalog=nservicebus;Integrated Security=True";
}

