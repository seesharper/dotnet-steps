#!/usr/bin/env dotnet-script
#load "nuget: ScriptUnit, 0.2.0"
#load "steps.csx"
#r "nuget: FluentAssertions, 5.5.3"
using static ScriptUnit;
using FluentAssertions;


return await new TestRunner().AddTopLevelTests().AddFilter(m => m.Name.StartsWith("Should")).Execute();

static  List<string> EmptyArgs = new List<string>();

public async Task ShouldExecuteDefaultStep()
{
    await StepRunner.Execute(EmptyArgs, step1);
    TestContext.StandardOut.Should().Contain("step1");
}

public async Task ShouldExecuteSingleStep()
{
    await StepRunner.Execute("step1");
    TestContext.StandardOut.Should().Contain("step1");
}





static Step step1 = () => WriteLine(nameof(step1));