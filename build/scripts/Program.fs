module Program

open System
open System.IO
open Nullean.Make.Fs
open Proc.Fs

/// The build's targets; `dotnet run --project build/scripts -- --help` lists them with these descriptions
type Target =
    /// clean known output locations
    | Clean
    /// Run build
    | Build
    /// Runs build then tests
    | Test
    /// Runs build, then creates and validates the packages and reports public API changes
    | Pack
    /// Runs tests, then creates and validates the packages: exactly what CI runs
    | Ci
    /// Pack the NuGet packages into build/output
    | GeneratePackages
    /// Validate the packed NuGet packages
    | ValidatePackages
    /// Report public API changes against the previous release
    | GenerateApiChanges

/// build/output, relative to the repository root
let private output = Paths.RootRelative Paths.Output.FullName

/// The packed .nupkg files, newest first
let private packages () =
    Paths.Output.GetFiles("*.nupkg") |> Seq.sortByDescending (fun f -> f.CreationTimeUtc) |> Seq.toList

/// Every project but this build itself: `dotnet clean` on the solution would delete the assemblies the running
/// build still loads lazily and fail the next target
let private projectsToClean () =
    ["src"; "tests"]
    |> List.collect (fun d -> Directory.EnumerateFiles(d, "*.?sproj", SearchOption.AllDirectories) |> Seq.toList)

[<EntryPoint>]
let main argv =
    let app = MakeApp<Target>("dotnet run --project build/scripts --", None)

    // `-s` skips what a target `requires`, never what it `composes`: build always cleans first
    app.Bind <| function
        | Clean -> target [] {
              if Paths.Output.Exists then Paths.Output.Delete true
              for project in projectsToClean () do
                  sh $"dotnet clean {project} -c Release -v q" }
        | Build -> command {
              composes [Clean]
              executes (shell { sh $"dotnet build -c Release" }) }
        | Test -> target [Build] { sh $"dotnet test -c RELEASE --logger:GithubActions --logger:pretty" }
        | Pack -> command { requires [Build]; composes [GeneratePackages; ValidatePackages; GenerateApiChanges] }
        | Ci -> command { requires [Test]; composes [GeneratePackages; ValidatePackages; GenerateApiChanges] }
        | GeneratePackages -> target [] { sh $"dotnet pack -c Release -o {output}" }
        | ValidatePackages -> target [] {
              let version = Versions.informational.Value
              let jenkinsOnWindows =
                  not (String.IsNullOrEmpty(Environment.GetEnvironmentVariable "JENKINS_URL")) && OperatingSystem.IsWindows()
              for package in packages () do
                  let package = Paths.RootRelative package.FullName
                  if jenkinsOnWindows then
                      sh $"dotnet nupkg-validator {package} -v {version} -k {Paths.SignKey} -t {output} -r true"
                  else
                      sh $"dotnet nupkg-validator {package} -v {version} -k {Paths.SignKey} -t {output}" }
        | GenerateApiChanges -> target [] {
              let version = Versions.current.Value
              let tfm = Paths.MainTFM
              for package in packages () do
                  let id = Path.GetFileNameWithoutExtension(package.Name).Replace("." + version, "")
                  let report = Path.Combine(output, $"breaking-changes-{id}.md")
                  // quoted: an unquoted | would pipe
                  sh $"dotnet assembly-differ 'previous-nuget|{id}|{version}|{tfm}' 'directory|src/{id}/bin/Release/{tfm}' -a true --target {id} -f github-comment --output {report}" }

    app.RunAsync(argv).GetAwaiter().GetResult()
