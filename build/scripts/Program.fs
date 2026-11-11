module Program

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

[<EntryPoint>]
let main argv =
    let app = MakeApp<Target>("dotnet run --project build/scripts --", None)

    // `-s` skips what a target `requires`, never what it `composes`: build always cleans first
    app.Bind <| function
        | Clean -> target [] { Targets.clean () }
        | Build -> command { composes [Clean]; executes (Targets.build ()) }
        | Test -> target [Build] { Targets.test () }
        | Pack -> command { requires [Build]; composes [GeneratePackages; ValidatePackages; GenerateApiChanges] }
        | Ci -> command { requires [Test]; composes [GeneratePackages; ValidatePackages; GenerateApiChanges] }
        | GeneratePackages -> target [] { Targets.generatePackages () }
        | ValidatePackages -> target [] { Targets.validatePackages () }
        | GenerateApiChanges -> target [] { Targets.generateApiChanges () }

    app.RunAsync(argv).GetAwaiter().GetResult()
