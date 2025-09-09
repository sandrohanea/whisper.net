// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Xunit;
using Xunit.Extensions.AssemblyFixture;

[assembly: CollectionBehavior(MaxParallelThreads = 2, DisableTestParallelization = false)]
[assembly: TestFramework(AssemblyFixtureFramework.TypeName, AssemblyFixtureFramework.AssemblyName)]
