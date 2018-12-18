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

private IEnumerable<StepResult> _results;

SummaryStep summaryStep = (results) => _results = results;



await new TestRunner().AddTopLevelTests().AddFilter(m => m.Name.StartsWith("Should")).Execute();

public async Task ShouldReportIndividualStepDurations()
{
    await StepRunner.Execute(new List<string>(){"step3"});
    _results.Last().Duration.Should().BeCloseTo(TimeSpan.FromMilliseconds(300));
}


