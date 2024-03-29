#r "paket:
source https://api.nuget.org/v3/index.json
framework: netstandard2.0
nuget Fake.Core
nuget Fake.Core.Target
nuget Fake.Core.ReleaseNotes
nuget Fake.DotNet.Cli 
nuget Fake.DotNet.Paket //"

#load ".fake/build.fsx/intellisense.fsx"
open Fake.Core
open Fake.Core.TargetOperators
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.DotNet

module BuildPath =
    let root = __SOURCE_DIRECTORY__
    let bin = root </> "bin"
    let nuget = bin </> "nuget"
    let test = bin </> "test"


    let sln = Directory.findFirstMatchingFile "*.sln" root
    let releaseNotes = root </> "RELEASE_NOTES.md"


let releaseNotes = ReleaseNotes.load BuildPath.releaseNotes

let version =
    releaseNotes.SemVer
    


Target.create "Clean" <| fun _ ->
    Directory.delete BuildPath.bin


Target.create "Build" <| fun _ ->
    DotNet.build (fun p ->
        { p with 
            Configuration = DotNet.BuildConfiguration.Release } )
        BuildPath.sln


Target.create "Test" <| fun _ ->
    DotNet.test (fun p ->
        { p with
            Configuration = DotNet.BuildConfiguration.Release
            NoBuild = true
            ResultsDirectory  = Some BuildPath.test } )
        BuildPath.sln


Target.create "Nuget" <| fun _ ->
    Paket.pack (fun p ->
        { p with 
            BuildConfig = "Release"
            OutputPath = BuildPath.nuget
            Version = version.AsString }
    )
Target.create "All" ignore

"Clean" 
    ==> "Build"
    ==> "Test"
    ==> "Nuget"
    ==> "All"

Target.runOrDefault "All"





