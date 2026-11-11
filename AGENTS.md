# Nullean.ScopedFileSystem

A `System.IO.Abstractions` `IFileSystem` decorator that confines file and directory reads and writes to one or more
scope roots (plus opted-in OS special folders), and rejects symbolic links and hidden files and directories.
Violations throw `ScopedFileSystemException`.

## Layout

| Path | What |
|---|---|
| `src/Nullean.ScopedFileSystem` | the library (netstandard2.0, netstandard2.1, net9.0, net10.0), strong-named |
| `src/Nullean.ScopedFileSystem/skills` | the agent skill packed into the package (`skills/scoped-filesystem/SKILL.md`) |
| `tests/Nullean.ScopedFileSystem.Tests` | xUnit tests, on net10.0 |
| `build/scripts` | the F# build ([Nullean.Make.Fs](https://www.nuget.org/packages/Nullean.Make.Fs), net11.0) the `mise` tasks call |
| `build/keys` | the strong-name key pair |

## Commands

mise installs the SDK pinned in `global.json` (plus the net10.0 runtime the tests run on). Run everything through it:

```bash
mise run build    # build the solution
mise run test     # build, then run the tests
mise run pack     # build, pack, validate the packages, report public API changes
mise run ci       # exactly what CI runs: the curb check, test, pack
mise run format   # format the repository with curb
mise run check    # formatting drift (curb) and workflow security checks (zizmor)
mise run clean    # delete build output
mise run release  # show the release draft; `mise run release publish` releases
```

`mise tasks ls` lists them all.

## Releasing

release-drafter keeps a draft release up to date from the labels of merged pull requests. **Publishing the draft**
releases: it creates the tag, and the `release` job in `ci.yml` packs at that version and pushes to nuget.org. `mise run release` shows
the draft. The [releasing](.claude/skills/releasing/SKILL.md) and [breaking-changes](.claude/skills/breaking-changes/SKILL.md)
skills have the details. Every pull request carries exactly one release label. Package output and API reports go to `build/output`.

## Rules

- Public API is a contract: `mise run pack` diffs it against the previous release (`breaking-changes-*.md`). A change
  that removes or alters public API is a breaking change.
- The library depends only on `System.IO.Abstractions`. Do not add package references to `src/Nullean.ScopedFileSystem`.
- It is AOT- and trim-compatible on net8.0+ (`IsAotCompatible`); keep it free of reflection the analyzers flag.
- Package versions live in `Directory.Packages.props` (Central Package Management), never on a `PackageReference`.
- Package metadata lives in `Directory.Build.props`; the project file keeps only id, title, description and tags.
- The packed skill is written from the code, and every snippet in it must compile: update it with the public API.
- Package validation runs on pack. `CompatibilitySuppressions.xml` records the one known difference between target
  frameworks; do not add to it to silence a new break.
- Build targets are Make.Fs `target { }` blocks in `build/scripts/Program.fs` whose commands are Proc.Fs `sh $"…"`
  lines; values come from `text`/`lines` inside `shell { }`. Do not call the C# `ProcNet` API from the build.
- Formatting is curb's, from `.editorconfig` (the nullean template, identical in every repo): run `mise run format`
  before committing C#. CI fails on drift. See the [format](.claude/skills/format/SKILL.md) skill.
