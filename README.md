# Compatibility.NServiceBus.Transport.SqlServer

Compatibility.NServiceBus.Transport.SqlServerTests consists of tests to ensure wire compatibility between versions of a specific transport configuration.

It is part of the toolset used internally by Particular Software to maintain high quality standards in the [Particular Service Platform](https://particular.net/service-platform), which includes [NServiceBus](https://particular.net/nservicebus) and tools to build, monitor, and debug distributed systems.

## Scenarios

Scenarios for SQL transport:

- PubSub - Message driven (until version 6)
- PubSub - Native (version 5+)
- Request Response - Single shared SQL schema (version 4+)
- Request Response - Multiple SQL schemas (version 6+)
- Request Response - Multiple SQL catalogs (version 6+)
