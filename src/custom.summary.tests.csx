#!/usr/bin/env dotnet-script
#r "nuget: FluentAssertions, 5.5.3"
#load "nuget: ScriptUnit, 0.2.0"
#load "steps.csx"


using FluentAssertions;
using static ScriptUnit;

[DefaultStep]
Step step = () => WriteLine("TEST");

SummaryStep summaryStep = results => WriteLine("Custom Summary");

await new TestRunner().AddTopLevelTests().AddFilter(m => m.Name.StartsWith("Should")).Execute();


public async Task ShouldUseCustomSummary()
{
    await ExecuteSteps(new List<string>());
    TestContext.StandardOut.Should().Contain("Custom Summary");
}