#!/usr/bin/env dotnet-script
#load "nuget: ScriptUnit, 0.2.0"
#load "steps.csx"
#r "nuget: FluentAssertions, 5.5.3"
using static ScriptUnit;
using FluentAssertions;

AsyncStep step1 = async () => WriteLine(nameof(step1));

await new TestRunner().AddTopLevelTests().AddFilter(m => m.Name.StartsWith("Should")).Execute();

public async Task ShouldExecuteAsyncStep()
{
    await StepRunner.Execute(new List<string>(){"step1"});
    TestContext.StandardOut.Should().Contain("step1");
}