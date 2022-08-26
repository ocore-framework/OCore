// Variables
#tool "dotnet:?package=GitVersion.Tool&version=5.10.3"

var solution = "./src/OCore/OCore.sln";
var buildConfiguration = "Release";
var packageOutputDir = "./packages/";
var target = Argument("target", "Build");
var apiKey = EnvironmentVariable("OCORE_NUGET_API_KEY");

var gitVersion = GitVersion();

Console.WriteLine($"Building {solution} for {gitVersion.FullSemVer}");

var buildSettings = new DotNetCoreMSBuildSettings();
buildSettings.SetVersion(gitVersion.FullSemVer);

Task("Build")
    .Does(() => {
        var settings = new DotNetCoreBuildSettings
        {
            Configuration = buildConfiguration,                    
            MSBuildSettings = buildSettings
        };

        DotNetCoreBuild(solution, settings);
    });

Task("Test")
    .IsDependentOn("Build")
    .Does(() => {
        Information("Running tests");
    });

Task("Pack")
    .IsDependentOn("Test")
    .Does(() => {
        if (DirectoryExists(packageOutputDir)) { 
            DeleteDirectory(packageOutputDir, new DeleteDirectorySettings {
                Recursive = true,
                Force = true,
            });
        }
        DotNetCorePack(solution, new DotNetCorePackSettings 
        { 
            Configuration = buildConfiguration,
            OutputDirectory = packageOutputDir,   
            MSBuildSettings = buildSettings,                     
        });   
    });

Task("Push")
    .IsDependentOn("Test")
    .IsDependentOn("Pack")
    .Does(() => {
        var packageFiles = GetFiles(packageOutputDir + "*.nupkg");
        foreach (var packageFile in packageFiles) {
            NuGetPush(packageFile, new NuGetPushSettings {
                ApiKey = apiKey,
                Source = "https://api.nuget.org/v3/index.json",
                SkipDuplicate = true
            });
        }
    });

RunTarget(target);