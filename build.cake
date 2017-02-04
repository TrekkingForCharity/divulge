#tool "nuget:?package=GitReleaseNotes"
#tool "nuget:?package=GitVersion.CommandLine"
#tool "nuget:?package=gitlink"
#tool "nuget:?package=NUnit.ConsoleRunner"

var target = Argument("target", "Default");
var outputDir = "./artifacts/";
var solutionPath = "./Divulge.sln";

Task("Clean")
    .Does(() => {
        if (DirectoryExists(outputDir))
        {
            DeleteDirectory(outputDir, recursive:true);
        }
        CreateDirectory(outputDir);
    });

Task("Restore")
    .Does(() => {
        NuGetRestore(solutionPath);
    });

GitVersion versionInfo = null;
Task("Version")
    .Does(() => {
        GitVersion(new GitVersionSettings{
            UpdateAssemblyInfo = true,
            OutputType = GitVersionOutput.BuildServer
        });
        versionInfo = GitVersion(new GitVersionSettings{ OutputType = GitVersionOutput.Json });
    });

Task("Build")
    .IsDependentOn("Clean")
    .IsDependentOn("Version")
    .IsDependentOn("Restore")
    .Does(() => {
        MSBuild(solutionPath, new MSBuildSettings {
    Verbosity = Verbosity.Minimal,
    ToolVersion = MSBuildToolVersion.VS2015,
    Configuration = "Release",
    PlatformTarget = PlatformTarget.MSIL
    });
    });


Task("Test")
    .IsDependentOn("Build")
    .Does(() => {
        var testAssemblies = GetFiles("./**/bin/Release/Tests.dll");
        NUnit3(testAssemblies, new NUnit3Settings {
            NoResults = true
        });
    });

Task("Package")
    .IsDependentOn("Test")
    .Does(() => {

        NuGetPack("./Divulge/Divulge.Fody.nuspec", new NuGetPackSettings{
            Version = versionInfo.SemVer,
            OutputDirectory = "./artifacts/"
        });

         if (AppVeyor.IsRunningOnAppVeyor)
         {
             foreach (var file in GetFiles(outputDir + "**/*"))
                 AppVeyor.UploadArtifact(file.FullPath);
         }
    });

Task("Default")
    .IsDependentOn("Package");

RunTarget(target);

    