#!/usr/bin/env dotnet-script
#load "nuget: ScriptUnit, 0.2.0"
#load "steps.csx"
#r "nuget: FluentAssertions, 5.5.3"
using static ScriptUnit;
using FluentAssertions;

AsyncStep step1 = async () => WriteLine(nameof(step1));

AsyncStep step2 = async () => WriteLine(nameof(step2));

Step step3 = () => WriteLine(nameof(step3));

await new TestRunner().AddTopLevelTests().AddFilter(m => m.Name.StartsWith("ShouldShowHelpWhenThereIsNoDefaultStep")).Execute();

public async Task ShouldExecuteAsyncStep()
{
    await ExecuteSteps(new List<string>(){"step1"});
    TestContext.StandardOut.Should().Contain("step1");
}

public async Task ShouldShowHelpWhenThereIsNoDefaultStep()
{
    await ExecuteSteps(new List<string>());
    TestContext.StandardOut.Should().Contain("Available Steps");
    TestContext.StandardOut.Should().Contain("step1");
    TestContext.StandardOut.Should().Contain("step2");
    TestContext.StandardOut.Should().Contain("step3");
}