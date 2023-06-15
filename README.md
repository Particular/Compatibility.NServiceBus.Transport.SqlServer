# Wire Compatibility

## NServiceBus.WireCompatibility

Tests to ensure wire compatibility between versions of a specific transport configuration.

## Scenarios

For SQL transport these scenarios are:

- PubSub - Message driven (until version 6)
- PubSub - Native (version 5+)
- Request Response - Single shared SQL schema (version 4+)
- Request Response - Multiple SQL schemas (version 6+)
- Request Response - Multiple SQL catalogs (version 6+)

## Why isn't this part of the NServiceBus.SqlServer repository?

See [RFCed design change recommendation](https://github.com/Particular/PlatformInternals/pull/379).
