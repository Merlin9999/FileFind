///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument<string>("target", "Default");
var configuration = Argument<string>("configuration", "Release");

///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////

var solutions = GetFiles("./../src/**/*.sln");
var solutionPaths = solutions.Select(solution => solution.GetDirectory());
var packagePath = "../packages";
var workPath = "Work";
var binPath = "../src/FileFind/FileFind/bin/Release";
var assemblyInfoFilePath = "../src/FileFind/FileFind/Properties/AssemblyInfo.cs";

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(() =>
{
    // Executed BEFORE the first task.
    Information("Running tasks...");
});

Teardown(() =>
{
    // Executed AFTER the last task.
    Information("Finished running tasks.");
});

///////////////////////////////////////////////////////////////////////////////
// TASK DEFINITIONS
///////////////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    // Clean solution directories.
    foreach(var path in solutionPaths)
    {
        Information("Cleaning {0}", path);
        CleanDirectories(path + "/**/bin/" + configuration);
        CleanDirectories(path + "/**/obj/" + configuration);
    }
    
    CleanDirectories(packagePath);
    CleanDirectories(workPath);
});

Task("Restore")
    .Does(() =>
{
    // Restore all NuGet packages.
    foreach(var solution in solutions)
    {
        Information("Restoring {0}...", solution);
        NuGetRestore(solution);
    }
});

Task("Build")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .Does(() =>
{
    // Build all solutions.
    foreach(var solution in solutions)
    {
        Information("Building {0}", solution);
        MSBuild(solution, settings => 
            settings.SetPlatformTarget(PlatformTarget.MSIL)
                .WithProperty("TreatWarningsAsErrors","true")
                .WithTarget("Build")
                .SetConfiguration(configuration));
    }
});

Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
    NUnit("./../src/**/bin/" + configuration + "/*Tests.dll",
        new NUnitSettings {
            ToolPath = @"tools\NUnit.Console\tools\nunit3-console.exe"
        });
});

Task("ZipPackage")
    .IsDependentOn("Build")
    .Does(() =>
{
    if (!DirectoryExists(packagePath))
        CreateDirectory(packagePath);
    
    if (!DirectoryExists(workPath))
        CreateDirectory(workPath);
        
    Information(binPath);

    var exes = GetFiles(System.IO.Path.Combine(binPath, "*.exe")).Where(f => !f.FullPath.ToLowerInvariant().Contains(".vshost.exe"));
    var dlls = GetFiles(System.IO.Path.Combine(binPath, "*.dll"));
    var cfgs = GetFiles(System.IO.Path.Combine(binPath, "*.config")).Where(f => !f.FullPath.ToLowerInvariant().Contains(".vshost."));
    
    var assemblyInfo = ParseAssemblyInfo(assemblyInfoFilePath);
    var version = assemblyInfo.AssemblyVersion;
    
    var packageTempFolder = System.IO.Path.Combine(workPath, "FileFind-" + version);
    if (!DirectoryExists(packageTempFolder))
        CreateDirectory(packageTempFolder);
    
    CopyFiles(exes, packageTempFolder);
    CopyFiles(dlls, packageTempFolder);
    CopyFiles(cfgs, packageTempFolder);
    
    Zip(workPath, System.IO.Path.Combine(packagePath, "FileFind-" + version + ".zip"));
});

///////////////////////////////////////////////////////////////////////////////
// TARGETS
///////////////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Run-Unit-Tests")
    .IsDependentOn("ZipPackage");

///////////////////////////////////////////////////////////////////////////////
// EXECUTION
///////////////////////////////////////////////////////////////////////////////

RunTarget(target);
