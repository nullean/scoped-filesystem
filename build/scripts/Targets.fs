module Targets

open System
open System.IO
open ProcNet


let private exec binary args =
    Proc.Exec (binary, args |> List.toArray)

let private restoreTools = lazy(exec "dotnet" ["tool"; "restore"])
let private currentSha =
    lazy(
        let r = Proc.Start("git", "rev-parse", "HEAD")
        (r.ConsoleOut |> Seq.find (fun l -> not l.Error)).Line.Trim()
    )
let private currentVersion =
    lazy(
        restoreTools.Value |> ignore
        let r = Proc.Start("dotnet", "minver", "-p", "canary.0", "-m", "0.1")
        let o = r.ConsoleOut |> Seq.find (fun l -> not(l.Line.StartsWith("MinVer:")))
        o.Line
    )
let private currentVersionInformational =
    lazy(
        match Paths.IncludeGitHashInInformational with
        | false -> currentVersion.Value
        | true -> sprintf "%s+%s" currentVersion.Value currentSha.Value
    )

let clean () =
    if (Paths.Output.Exists) then Paths.Output.Delete (true)
    // clean every project but this build itself: `dotnet clean` on the solution would delete the assemblies the
    // running build still loads lazily and fail the next target
    ["src"; "tests"]
    |> Seq.collect (fun d -> Directory.EnumerateFiles(d, "*.?sproj", SearchOption.AllDirectories))
    |> Seq.iter (fun p -> exec "dotnet" ["clean"; p; "-c"; "Release"; "-v"; "q"] |> ignore)

let build () = exec "dotnet" ["build"; "-c"; "Release"] |> ignore

let test () =
    exec "dotnet" ["test"; "-c"; "RELEASE"; "--logger:GithubActions"; "--logger:pretty"] |> ignore

let generatePackages () =
    let output = Paths.RootRelative Paths.Output.FullName
    exec "dotnet" ["pack"; "-c"; "Release"; "-o"; output] |> ignore

let validatePackages () =
    let output = Paths.RootRelative <| Paths.Output.FullName
    let nugetPackages =
        Paths.Output.GetFiles("*.nupkg") |> Seq.sortByDescending(fun f -> f.CreationTimeUtc)
        |> Seq.map (fun p -> Paths.RootRelative p.FullName)

    let jenkinsOnWindowsArgs =
        if not (String.IsNullOrEmpty(Environment.GetEnvironmentVariable "JENKINS_URL")) && OperatingSystem.IsWindows() then ["-r"; "true"] else []

    let args = ["-v"; currentVersionInformational.Value; "-k"; Paths.SignKey; "-t"; output] @ jenkinsOnWindowsArgs
    nugetPackages |> Seq.iter (fun p -> exec "dotnet" (["nupkg-validator"; p] @ args) |> ignore)


let generateApiChanges () =
    let output = Paths.RootRelative <| Paths.Output.FullName
    let currentVersion = currentVersion.Value
    let nugetPackages =
        Paths.Output.GetFiles("*.nupkg") |> Seq.sortByDescending(fun f -> f.CreationTimeUtc)
        |> Seq.map (fun p -> Path.GetFileNameWithoutExtension(Paths.RootRelative p.FullName).Replace("." + currentVersion, ""))
    nugetPackages
    |> Seq.iter(fun p ->
        let outputFile =
            let f = sprintf "breaking-changes-%s.md" p
            Path.Combine(output, f)
        let args =
            [
                "assembly-differ"
                (sprintf "previous-nuget|%s|%s|%s" p currentVersion Paths.MainTFM);
                (sprintf "directory|src/%s/bin/Release/%s" p Paths.MainTFM);
                "-a"; "true"; "--target"; p; "-f"; "github-comment"; "--output"; outputFile
            ]

        exec "dotnet" args |> ignore
    )
