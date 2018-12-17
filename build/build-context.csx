#load "nuget: Dotnet.Build, 0.3.9"
using static FileUtils;

var Owner = "seesharper";

var ProjectName = "dotnet-steps";

var scriptFolder = GetScriptFolder();
var tempFolder = CreateDirectory(scriptFolder, "tmp");
var contentFolder = CreateDirectory(tempFolder,"contentFiles", "csx", "any");
var pathToSourceFile = Path.Combine(scriptFolder, "..", "src", "steps.csx");
var pathToPackageScriptFile = Path.Combine(contentFolder, "main.csx");

var pathToNuGetMetadataSource = Path.Combine(scriptFolder,"dotnet-steps.nuspec");
var pathToNuGetMetadataTarget = Path.Combine(tempFolder,"dotnet-steps.nuspec");

var stepsTests = Path.Combine(scriptFolder,"..","src","steps.tests.csx");

var asyncTests = Path.Combine(scriptFolder,"..","src","async.steps.tests.csx");

var PathToArtifactsFolders = CreateDirectory(scriptFolder, "Artifacts");

var NuGetArtifactsFolder = CreateDirectory(PathToArtifactsFolders, "Artifacts", "NuGet");
var GitHubArtifactsFolder = CreateDirectory(PathToArtifactsFolders, "Artifacts", "GitHub");

var PathToReleaseNotes = Path.Combine(GitHubArtifactsFolder, "ReleaseNotes.md");

