# dotnet-steps

A small step for man kind, one giant leap for build scripts.

> Note: Most of the stuff described here is not implemented yet. This is what I like to call MDD (Markdown Driven Development) 👍

## What is it?

A super simple way of composing "steps" in a C# script. No strings attached (pun intended 😀).

## How to use

Simply grab the `steps.csx`file from this repo and we are good to go.

If you are on `dotnet-script` we also have a script package 

```c#
#load "nuget: dotnet-steps, [version]"
```



## Example

```c#
#load "steps.csx"

Step build = () => WriteLine(nameof(build));

Step publish = () => {
    build();
    WriteLine(nameof(publish));
}

return await ExecuteSteps(Args);
```

The `Args` is used to determine which step(s) to execute. In the case of an empty arguments list, we can specify the default step like this. The following example specifies the `build` step as the `default` step. 

```C#
Step default = () => build();
```

We can now execute this script like

```shell
csi myscript.csx publish
```

Or with dotnet-script like this

```shell
dotnet script myscript.csx publish
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



