#!/usr/bin/env dotnet-script
#load "nuget: ScriptUnit, 0.2.0"
#load "steps.csx"
#r "nuget: FluentAssertions, 5.5.3"
using static ScriptUnit;
using FluentAssertions;


Step step1 = () => WriteLine(nameof(step1));

Step step2 = () => WriteLine(nameof(step2));

await new TestRunner().AddTopLevelTests().AddFilter(m => m.Name.StartsWith("Should")).Execute();

public async Task ShouldShowHelpWhenThereIsNoDefaultStep()
{
    await ExecuteSteps(new List<string>());
    TestContext.StandardOut.Should().Contain("Available Steps");
    TestContext.StandardOut.Should().Contain("step1");
    TestContext.StandardOut.Should().Contain("step2");
    TestContext.StandardOut.Should().NotContain("Summary");
}