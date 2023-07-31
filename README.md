# Wire Compatibility

## NServiceBus.WireCompatibility

Tests to ensure wire compatibility between versions of a specific transport configuration.

## How it works

There are a couple of message patterns that are tested for wire compatibility:

- Publish / Subscribe
- Request / Response

These are tested with various configurations of the transport against all a specific range of versions. These configurations are coupled to specific transport features. Some features are only available on certain version ranges.

For each scenario we need behaviors. For simplicity behaviors are bound to a specific major version of the transport.

A behavior has two stages:

- Configure
- Execute

The configure stage initialized the endpoint configuration to conform to the transport configuration for that scenarios.

The execute stage will run any task required to start the wire compatibility test.

## Test project

The test project defines unit tests for the transport specific scenarios. These scenarios are defined as NUnit test. It uses the NUnit TestCase feature to invoke the test with the  versions the test will test for compatibility.

### Version permutation generator

The versions are extracted from Feedz.io for a configured version range for a specific package where the range is expressed as a NuGet version range expression.

    [TestCaseSourcePackageSupportedVersions("NServiceBus.SqlServer", "[5,)")]

The test will only return all latest minors that match the range and only match alpha, beta or rc pre release packages.

### Test scenario plugin runner


The unit tests invokes the scenario runner with which versions to tests and for each which behavior must be invoked. The runner initializes an agent for each version. The agent is responsible for loading and managing the lifetime of the resources of that version.

The scenario runner initializes an audit queue monitor and instructs all the agents to "compile" (TODO: Setup?), then to StartEndpoint, then to Start the test. During the test it waits until either the fetched state from the audit queue represents to scenario to be completed or the test to be timed out.

The fetched audit messages are returned to the test for validation.

## NServiceBus.WireCompatibility.SqlServer

Tests to ensure wire compatibility between versions of [NServiceBus.SqlServer](https://github.com/Particular/NServiceBus.SqlServer).

### SqlServer specific scenarios

For SQL transport these scenarios are:

- PubSub - Message driven (until version 6)
- PubSub - Native (version 5+)
- Request Response - Single shared SQL schema (version 4+)
- Request Response - Multiple SQL schemas (version 6+)

## Restrictions

The wire compatibility tests have the following restrictions:

- It only tests transport MINORS against each other for wire compatibility and will not test different Core minors.
  - The likely hood of breaking wire compatibility in a core minor is small enough to not have to test minors
  - Core majors are still indirectly tested as major version of transports can be dependant on different major versions of Core.
- The test runner is dependant on AUDIT data and it is assumed that all versions of a given transport will send message to the audit queue in the same way.
- Due to the way the resources are loaded via a plugin model the tests can only run with transports that are capable to run in .NET and .NET Framework is not supported.
- Transport versions are not tested between Linux and Windows
- Transport versions are not tested between framework targets (currently only targets 7.0)
