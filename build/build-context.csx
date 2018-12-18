#load "nuget: Dotnet.Build, 0.3.9"
using static FileUtils;

var Owner = "seesharper";

var ProjectName = "dotnet-steps";

var ScriptFolder = GetScriptFolder();
var TempFolder = CreateDirectory(ScriptFolder, "tmp");
var ContentFolder = CreateDirectory(TempFolder,"contentFiles", "csx", "any");
var PathToSourceFile = Path.Combine(ScriptFolder, "..", "src", "steps.csx");
var PathToPackageScriptFile = Path.Combine(ContentFolder, "main.csx");

var PathToNuGetMetadataSource = Path.Combine(ScriptFolder,"dotnet-steps.nuspec");
var PathToNuGetMetadataTarget = Path.Combine(TempFolder,"dotnet-steps.nuspec");

var StepsTests = Path.Combine(ScriptFolder,"..","src","steps.tests.csx");

var AsyncTests = Path.Combine(ScriptFolder,"..","src","async.steps.tests.csx");

var DefaultTests = Path.Combine(ScriptFolder,"..","src","default.tests.csx");

var DurationTests = Path.Combine(ScriptFolder,"..","src","duration.tests.csx");

var SummaryTests = Path.Combine(ScriptFolder,"..","src","custom.summary.tests.csx");

var PathToArtifactsFolders = CreateDirectory(ScriptFolder, "Artifacts");

var NuGetArtifactsFolder = CreateDirectory(PathToArtifactsFolders, "Artifacts", "NuGet");
var GitHubArtifactsFolder = CreateDirectory(PathToArtifactsFolders, "Artifacts", "GitHub");

var PathToReleaseNotes = Path.Combine(GitHubArtifactsFolder, "ReleaseNotes.md");

