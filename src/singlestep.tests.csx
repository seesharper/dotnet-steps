#!/usr/bin/env dotnet-script
#load "nuget: ScriptUnit, 0.2.0"
#load "steps.csx"
#r "nuget: FluentAssertions, 5.5.3"
using static ScriptUnit;
using FluentAssertions;
using System.Threading;

Step step1 = () => WriteLine(nameof(step1));

await new TestRunner().AddTopLevelTests().AddFilter(m => m.Name.StartsWith("Should")).Execute();

public async Task ShouldUseSingleStepAsDefault()
{
    await ExecuteSteps(new List<string>());
    TestContext.StandardOut.Should().Contain("Summary");
    TestContext.StandardOut.Should().Contain("step1");
}