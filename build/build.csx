#!/usr/bin/env dotnet-script
#load "nuget: Dotnet.Build, 0.3.9"
#load "nuget:github-changelog, 0.1.5"
#load "build-context.csx"
#load "../src/steps.csx"
using static FileUtils;
using static DotNet;
using static ChangeLog;
using static ReleaseManagement;


Step test = () =>
{
    Test(stepsTests);
    Test(asyncTests);
};

Step pack = () =>
{
    Copy(pathToSourceFile, pathToPackageScriptFile);
    Copy(pathToNuGetMetadataSource, pathToNuGetMetadataTarget);
    NuGet.Pack(tempFolder, NuGetArtifactsFolder);
};

AsyncStep changelog = async () =>
{
    Logger.Log("Creating release notes");
    var generator = ChangeLogFrom(Owner, ProjectName, BuildEnvironment.GitHubAccessToken).SinceLatestTag();
    if (!Git.Default.IsTagCommit())
    {
        generator = generator.IncludeUnreleased();
    }
    await generator.Generate(PathToReleaseNotes);
};

[DefaultStep]
AsyncStep deploy = async () =>
{
    test();
    if (BuildEnvironment.IsSecure)
    {
        await changelog();
        if (Git.Default.IsTagCommit())
        {
            Git.Default.RequreCleanWorkingTree();
            await ReleaseManagerFor(Owner, ProjectName,BuildEnvironment.GitHubAccessToken)
            .CreateRelease(Git.Default.GetLatestTag(), PathToReleaseNotes, Array.Empty<ReleaseAsset>());
            NuGet.TryPush(NuGetArtifactsFolder);
        }
    }
};


await StepRunner.Execute(Args);