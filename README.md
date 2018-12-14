# dotnet-steps

A small step for man kind, one giant leap for build scripts.

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

Step build = () => WriteLine("building");

Step publish = () => {
    build();
    WriteLine("publishing");
}

return await ExecuteSteps(Args);
```

The `Args` is used to determine which step(s) to execute. In the case of an empty arguments list, we can specify the default step like this.

```C#
return await ExecuteSteps(Args, build);
```

We can now execute this script like

```shell
csx myscript.csx publish
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





