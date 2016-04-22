#r "packages/FAKE/tools/FakeLib.dll" // include Fake lib
open Fake 
open Fake.Testing.NUnit3
open System.IO
open ArchiveHelper.Zip

let configuration = "Release"
let srcPath = "./../src"
let solutions = !! (srcPath @@ "**/*.sln")
let solutionPaths = solutions |> Seq.map (fun solutionFile -> Path.GetDirectoryName solutionFile)
let packagesOutPath = "./../packages"
let packagesWorkPath = packagesOutPath @@ "Work"
let binPath = srcPath @@ "FileFind/FileFind/bin" @@ configuration;
let assemblyInfoCsFile = srcPath @@ "FileFind/FileFind/Properties/AssemblyInfo.cs"
let nugetPackagesFolder = srcPath @@ "FileFind/packages"

exception UnknownLibraryVersion of string

Target "Clean" (fun _ ->
    solutionPaths |> Seq.iter (fun solutionPath -> CleanDirs !! (solutionPath + "/**/bin/" + configuration))
    solutionPaths |> Seq.iter (fun solutionPath -> CleanDirs !! (solutionPath + "/**/obj/" + configuration))
   
    CleanDirs [packagesOutPath]
)

Target "RestorePackages" (fun _ -> 
    CreateDir nugetPackagesFolder

    for solutionFile in solutions do 
        (RestoreMSSolutionPackages (fun p ->
            { p with
                Sources = "https://nuget.org/api/v2" :: p.Sources
                OutputPath = nugetPackagesFolder
                Retries = 4 })  
            solutionFile) |> ignore
)

Target "Build" (fun _ ->
    let msBuildParams defaults =
        { defaults with
            Verbosity = Some(Quiet)
            Targets = ["Build"]
            Properties =
                [
                    "Optimize", "True"
                    //"TreatWarningsAsErrors", "True"
                    "Platform", "Any CPU" // "Any CPU" or "x86", or "x64"
                    "DebugSymbols", "True"
                    "Configuration", configuration
                ]
         }

    for solutionFile in solutions do 
        build msBuildParams solutionFile
            |> ignore
)

Target "NUnitTest" (fun _ -> 
    // !! ("./../src/**/bin/" + configuration + "/*Tests.dll")
    //     |> NUnit3 (fun p -> 
    //         {p with
    //             ShadowCopy = false;
    //             //OutputFile = testDir + "TestResults.xml";
    //             })
    DoNothing |> ignore
)

Target "ZipPackage" (fun _ ->
    CreateDir packagesOutPath
    CreateDir packagesWorkPath

    let someDllVersion = AssemblyInfoFile.GetAttributeValue "AssemblyVersion" assemblyInfoCsFile
    let dllVersion = 
        match someDllVersion with
        | Some x -> x.Trim('"')
        | None -> raise (UnknownLibraryVersion "AssemblyVersion not found in Assembly Info file!")
    let rootZipFileName = "FileFind-" + dllVersion

    let packageFiles = 
        !! (binPath @@ "*.exe")
            ++ (binPath @@ "*.dll")
            ++ (binPath @@ "*.config")
            -- (binPath @@ "*.vshost.*")

    CopyFiles (packagesWorkPath @@ rootZipFileName) packageFiles            

    CompressDirWithDefaults (DirectoryInfo packagesWorkPath) (FileInfo (packagesOutPath @@ rootZipFileName + ".zip"))
                
    CleanDir packagesWorkPath
    DeleteDir packagesWorkPath
)

Target "Default" (fun _ ->
    DoNothing |> ignore
)

// Target Dependencies...
"Clean"
    ==> "RestorePackages"
    ==> "Build"
    ==> "NUnitTest"
    ==> "ZipPackage"
    ==> "Default"

RunTargetOrDefault "Default"
