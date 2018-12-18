#!/usr/bin/env dotnet-script
#load "nuget: ScriptUnit, 0.2.0"
#load "steps.csx"
#r "nuget: FluentAssertions, 5.5.3"
using static ScriptUnit;
using FluentAssertions;
using System.Threading;


Step step1 = () => Thread.Sleep(100);


Step step2 = () =>
{
    Thread.Sleep(100);
};

Step step3 = () =>
{
    step1();
    step2();
    Thread.Sleep(100);
};

Step step4 = () =>
{
    step3();
    Thread.Sleep(100);
};

AsyncStep asyncStep1 = async () => await Task.Delay(100);


AsyncStep asyncStep2 = async () =>
{
    await Task.Delay(100);
};

AsyncStep asyncStep3 = async () =>
{
    await asyncStep1();
    await asyncStep2();
    await Task.Delay(100);
};

AsyncStep asyncStep4 = async () =>
{
    await asyncStep3();
    await Task.Delay(100);
};

private StepResult[] _results;

SummaryStep summaryStep = (results) => _results = results.ToArray();



await new TestRunner().AddTopLevelTests().AddFilter(m => m.Name.StartsWith("Should")).Execute();

public async Task ShouldReportIndividualStepDurations()
{
    await ExecuteSteps(new List<string>(){"step3"});
    TimeSpan.FromTicks(_results.Sum(r => r.Duration.Ticks)).Should().BeCloseTo(TimeSpan.FromMilliseconds(300), 50);
}

public async Task ShouldReportIndividualAsyncStepDurations()
{
    await ExecuteSteps(new List<string>(){"asyncStep3"});
    TimeSpan.FromTicks(_results.Sum(r => r.Duration.Ticks)).Should().BeCloseTo(TimeSpan.FromMilliseconds(300), 50);
}


public async Task ShouldReportIndividualStepDurationsForNestedSteps()
{
    await StepRunner.Execute(new List<string>(){"step4"});
    TimeSpan.FromTicks(_results.Sum(r => r.Duration.Ticks)).Should().BeCloseTo(TimeSpan.FromMilliseconds(400), 50);
}

public async Task ShouldReportIndividualAsyncStepDurationsForNestedSteps()
{
    await StepRunner.Execute(new List<string>(){"asyncStep4"});
    TimeSpan.FromTicks(_results.Sum(r => r.Duration.Ticks)).Should().BeCloseTo(TimeSpan.FromMilliseconds(400), 50);
}
