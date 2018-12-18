#!/usr/bin/env dotnet-script
#load "nuget: ScriptUnit, 0.2.0"
#load "steps.csx"
#r "nuget: FluentAssertions, 5.5.3"
using static ScriptUnit;
using FluentAssertions;
using System.Threading;

[StepDescription("This is step one")]
[DefaultStep]
Step step1 = () => WriteLine("nameof(step1)");

Step step2 = () => {
        step1();
        WriteLine("nameof(step2)");
    };


await new TestRunner().AddTopLevelTests().AddFilter(m => m.Name.StartsWith("Should")).Execute();

static  List<string> EmptyArgs = new List<string>();

public async Task ShouldExecuteStep()
{
    await StepRunner.Execute(new List<string>(){"step1"});
    TestContext.StandardOut.Should().Contain("step1");
}

public async Task ShouldExecuteDefaultStep()
{
    await StepRunner.Execute(new List<string>());
    TestContext.StandardOut.Should().Contain("step1");
}

public async Task ShouldShowHelp()
{
    await StepRunner.Execute(new List<string>(){"help"});
    TestContext.StandardOut.Should().Contain("Available Steps");
    TestContext.StandardOut.Should().Contain("This is step one");
    TestContext.StandardOut.Should().Contain("step1 (default)");
}

public async Task ShouldReportNestedStep()
{
    await StepRunner.Execute(new List<string>(){"step2"});
    TestContext.StandardOut.Should().Contain("step1");
    TestContext.StandardOut.Should().Contain("step2");
}

public async Task ShouldMarkDefaultStepInHelp()
{
    await StepRunner.Execute(new List<string>(){"help"});
    TestContext.StandardOut.Should().Contain("step1 (default)");
}



