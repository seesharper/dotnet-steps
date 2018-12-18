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
Step                Duration
-----               ------------------
step1               00:00:00.0007891
---------------------------------------------------------------------
Total               00:00:00.0007891
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





The `Args` is used to determine which step(s) to execute. In the case of an empty arguments list, we can specify the default step like this. The following example specifies the `build` step as the `default` step. 

```C#
Step defaultStep = () => build();
```



Show the easiest example possible before diving into the details.

Another way of specifying the default step is to specify the `DefaultStepAttribute`.

```
[DefaultStep]
Step build = () => WriteLine(nameof(build));
```





## What about async steps?

We got you covered with an `AsyncStep`

```c#
AsyncStep generateApiDocs = async () => await SomeMethodThatGeneratesApiDocs() 
```

## Help

We can get a list of available steps by passing `-h.|--help` when executing our script.

```shell
csi myscript.csx --help
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



