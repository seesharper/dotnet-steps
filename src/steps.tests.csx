#!/usr/bin/env dotnet-script
#load "nuget: ScriptUnit, 0.1.4"
#load "steps.csx"
#r "nuget: FluentAssertions, 5.5.3"
using static ScriptUnit;
using FluentAssertions;


return await new TestRunner().AddTestsFrom<StepsTests>().Execute();
public class StepsTests
{
    public async Task ShouldExecuteDefaultStep()
    {
        await StepRunner.Execute(Args, step1);
        TestContext.StandardOut.Should().Contain("step1");
    }
}




Step step1 = () => WriteLine("step1");