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

The versions are extracted from MyGet for a configured version range for a specific package where the range is expressed as a NuGet version range expression.

```c#
[TestCaseSourcePackageSupportedVersions("NServiceBus.SqlServer", "[5,)")]
```

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
- Request Response - Multiple SQL catalogs (version 6+)

## Restrictions

The wire compatibility tests have the following restrictions:

- It only tests transport MINORS against each other for wire compatibility and will not test different Core minors.
  - The likely hood of breaking wire compatibility in a core minor is small enough to not have to test minors
  - Core majors are still indirectly tested as major version of transports can be dependant on different major versions of Core.
- The test runner is dependant on AUDIT data and it is assumed that all versions of a given transport will send message to the audit queue in the same way.
- Due to the way the resources are loaded via a plugin model the tests can only run with transports that are capable to run in .NET and .NET Framework is not supported.
- Transport versions are not tested between Linux and Windows
- Transport versions are not tested between framework targets (currently only targets 7.0)

## Why isn't this part of the NServiceBus.SqlServer repository?

The first draft was added to NServiceBus.SqlServer repository@master. The tests require:

1. a conforming configuration project/package for each transport MAJOR that contains configuration that complies to the scenario requirements
2. a conforming behavior implementation project/package for each MAJOR of NServiceBus Core

The first are transport specific, the second core specific and could be shared among compatibility tests for different transports.

We considered:

- Transport repo - Maintaining everything on a single branch for all MAJORS
- Transport repo - Maintaining everything for the scope of that MINOR into each release branch
- Unique Transport compatibility repo
- Mono repo for all transport compatibility tests

### Everything on the main branch

Cons:

- Everything on the main/master with dependencies on many out of support versions
- Eventually will be on every future release branch
- Hard to align all branches when scenarios are extended, bugfixes are made or configurations are added.

It would not make sense to store all of this as part of the main branch. Especially as this means that this eventually make it into every release branch and would need to be maintained.

It does not make sense to having confirming configuration/behavior for versions that the main branch isn't representing.

### Per release branch

Only have main/master contain the conforming configuration/behavior for the minor it represents and maintain previous minor implementations in each release branch.

This has issues:

- Many implementations are confirming configuration/behavior implementations for versions that are no longer supported and which do not have a `release-*` branch. Somewhere this code needs to be stored.

- Lets assume a scenario needs configuration of behavior to be added of fixed. Now all branches need to be updated separately. Requiring seperate PR's.

### Separate repo

Having a separate repo with a single branch resolves the maintenance and also the setup limitations previously mentioned.

Additional benefits:

- Testing compatibility between different transports. For example between `NServiceBus.Transport.AzureServiceBus` and `NServiceBus.AzureServiceBus`.

- Changes on minors would not need to be backported test against newer minors, only against previous minors.

A conforming project is based on a minimum minor within a major version. Lets assume its 1.0, and over its lifetime we have 8 minors which means each minor has a conforming project that needs to be maintained and extended when new scenarios are added.

Who is then going to do the actual test run?

Changes on minors would not test against newer minors, only against previous minors.


 nor make sense to have each branch have its own conforming project as this is conforming to a specific transport configuration that is compatible with a minimum major/minor version. If such a project requires updating each branch needs to be updated.


## More flexibility

Allows for testing cross transport compatibility. For example testing between `NServiceBus.Transport.AzureServiceBus` (ASBS) and `NServiceBus.AzureServiceBus` (ASBL). It would not make sense to add all conforming code for ASBL in the ASBS repo.



## Prevent a explosion of package on myget

It would only make sense to have shared test infrastructure on myget.


Extend release checklist to review compatibility test results before releasing.



Optionally: CI step "compatibility" that tests current minor on branch against all released latest minors/



When a new major is added the CI fail as it would not capable to run the test permutations referencing the new major thus not compile and fail.


Testing transports resource leaks by starving connections either by limiting connections on the resource






https://github.com/Particular/Platform/blob/main/documentation/transports/wire-compatibility.md
