# dotnet-steps

A small step for man kind, one giant leap for build scripts.

## What is it?

A super simple way of composing "steps" in a C# script. No strings attached (pun intended 😀).

## Installing

Simply grab the `steps.csx`file from this repo and we are good to go.

If you are on `dotnet-script` we also have a script package 

```c#
#load "nuget: dotnet-steps, [version]"
```



## Getting started

```c#
#load "steps.csx"

Step step1 = () => WriteLine(nameof(step1));

await ExecuteSteps(Args);
```

We can now execute this script like

```shell
csi main.csx step1
```

Or with dotnet-script like this

```shell
dotnet script main.csx step1
```

> Note: The arguments passed to the script is used to determine which step(s) to execute.

At the end of execution we will output a summary report.

```
---------------------------------------------------------------------
Steps Summary
---------------------------------------------------------------------
Step                Duration           Total
-----               ----------------   ----------------
step1               00:00:00.0006126   00:00:00.0006126
---------------------------------------------------------------------
Total               00:00:00.0006126
```

### Multiple Steps

```c#
Step step1 = () => WriteLine(nameof(step1));
Step step2 = () => WriteLine(nameof(step2));
await ExecuteSteps(Args);
```

We can now execute both steps like this

```shell
csi main.csx step1 step2 
```

Which will generate a report showing the duration of each step.

```
---------------------------------------------------------------------
Steps Summary
---------------------------------------------------------------------
Step                Duration           Total
-----               ----------------   ----------------
step2               00:00:00.0000528   00:00:00.0000528
step1               00:00:00.0006086   00:00:00.0006086
---------------------------------------------------------------------
Total               00:00:00.0006614
```

### Nested Steps

Nesting steps is as simple as calling the step within another step. 
Notice that there is no  `DependsOn` or any other funky DSL, just plain C# 👍

```c#
Step step1 = () => WriteLine(nameof(step1));
Step step2 = () =>
{
    step1();
    WriteLine(nameof(step2));
};

await ExecuteSteps(Args);
```

Looking at the summary we will see that we get a full report of executed steps even if just called `step1` from within `step2`

```
---------------------------------------------------------------------
Steps Summary
---------------------------------------------------------------------
Step                Duration           Total
-----               ----------------   ----------------
step1               00:00:00.0007010   00:00:00.0007010
step2               00:00:00.0009654   00:00:00.0016664
---------------------------------------------------------------------
Total               00:00:00.0016664
```

The `Duration` column shows the time spent in the step excluding the time spent calling other steps, while the `Total` column show the time spent in the step including the time spent calling other steps.

The `Total` in the end of the summary is just a sum of the `Duration` column.

### Default Step

When we have multiple steps in a script, we can mark a step with the `DefaultStep` attribute so that we can invoke the script without any arguments. 

```c#
Step step1 = () => WriteLine(nameof(step1));

[DefaultStep]
Step step2 = () =>
{
    step1();
    WriteLine(nameof(step2));
};

await ExecuteSteps(Args);
```



### Async Steps

If we need to call an `async` method from within a step  we can do that easily by declaring an `AsyncStep` 

```c#
AsyncStep step1 = async () =>
{
    await Task.CompletedTask;
    WriteLine(nameof(step1));
};

await ExecuteSteps(Args);
```

We can of course call another steps from within an `AsyncStep`, but we should try to avoid calling an `AsyncStep` from within a `Step` as that would be a blocking operation. The general rules of `async/await` applies here as well.

## Help

We can get a list of available steps by passing `help` when executing our script.

```shell
csi main.csx help
```

Witch gives us a nice list of available steps.

```c#
Available steps
---------------------------------------------------------------------
build
test
publish
```

The step name might be descriptive enough as it is, but we can also provide a step description like this. 

```c#
[StepDescription("Builds all projects")]
Step build = () => WriteLine(nameof(build));
```

This information will be included in the help step like this. 

```
---------------------------------------------------------------------
Available steps
---------------------------------------------------------------------
Step					Description
------					-----------
build					Builds all projects
test
publish
```

Explain HelpStep here ……....



## Summary

By default, `dotnet-step` will create a summary at the end of execution like this.

```shell
---------------------------------------------------------------------
Summary Time Report
---------------------------------------------------------------------
Step							Duration
------							--------
build							(1.2sec)
test							(5.6sec)
------							--------
Total							(6.9sec)
```

If we for some reason should want to remove the summary report, we can do that with a `SummaryStep` that does nothing.

```c#
SummaryStep summary = (results) => {};
```

Or if we want to format the output differently

```C#
SummaryStep summary = (results) => results.Dump(); 
```

> Note: The `Dump` method is just an `IEnumerable<StepResult>`extension method witch also is an excellent way to to implement a custom summary report. 



