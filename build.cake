var target = Argument("Target", "Default");
var configuration = Argument("Configuration", "Release");
var solution = "./ignis.sln";
var testProject = "./Tests/Tests.csproj";

Task("Restore").Does(() => {
    DotNetCoreRestore();
});

Task("Clean").Does(() => {
    DotNetCoreClean(solution);
    CleanDirectory("./docs/coverage/");
    CleanDirectory("./docs/api/");
});

Task("Build").Does(() => {
    DotNetCoreBuild(solution, new DotNetCoreBuildSettings {
        Configuration = configuration
    });
}).OnError(ex => {
    Error("Build Failed");
    throw ex;
});

Task("Test").Does(() => {
    DotNetCoreTest(testProject, new DotNetCoreTestSettings {
        NoBuild = true,
        Configuration = configuration,
        ArgumentCustomization = (b) => b
            .Append("/p:CollectCoverage=true")
            .Append("/p:CoverletOutputFormat=opencover")
            .Append("/p:CoverletOutput=./coverage.xml")
    });
});

Task("ApiDoc").Does(() => {
    DotNetCoreTool(solution, "doc", "-f Html -s ./Ignis/ -o ./docs/api/");
    CopyFile("docs/index.css", "docs/api/index.css");
});

Task("ReportCoverage").Does(() => {
    var param = "\"-reports:./Tests/coverage.xml\" " +
        "\"-targetdir:./docs/coverage/\" " +
        "\"-sourcedirs:./Ignis/\" " +
        "\"-reporttypes:HTML;Badges\"";
    Information("Running 'reportgenerator " + param + "'");
    StartProcess("reportgenerator", new ProcessSettings {
        Arguments = param
    });
    // DotNetCoreTool(testProject, "reportgenerator", param);
});

Task("InstallTools").Does(() => {
    StartProcess("dotnet", new ProcessSettings {
        Arguments = "tool install --global dotnet-reportgenerator-globaltool"
    });
    StartProcess("dotnet", new ProcessSettings {
        Arguments = "tool install --global dotbook"
    });
});

Task("Default")
    .IsDependentOn("Build")
    .IsDependentOn("Test")
    .IsDependentOn("ReportCoverage")
    .IsDependentOn("ApiDoc");


Task("CleanBuild")
    .IsDependentOn("Clean")
    .IsDependentOn("Default");

Task("CI")
    .IsDependentOn("InstallTools")
    .IsDependentOn("CleanBuild");

RunTarget(target);
