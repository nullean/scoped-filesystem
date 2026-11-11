<p align="center">
  <a href="https://github.com/nullean">
    <picture>
      <source media="(prefers-color-scheme: light)" srcset="https://raw.githubusercontent.com/nullean/nullean.github.io/main/assets/nullean-lockup-light-544x192.png">
      <img src="https://raw.githubusercontent.com/nullean/nullean.github.io/main/assets/nullean-lockup-544x192.png" alt="nullean" width="272" height="96">
    </picture>
  </a>
</p>

<p align="center">A <code>System.IO.Abstractions</code> <code>IFileSystem</code> decorator that restricts file reads and writes to one or more scope root directories and rejects symbolic links, hidden files and path traversal.</p>

<p align="center">
  <a href="https://www.nuget.org/packages/Nullean.ScopedFileSystem"><img src="https://img.shields.io/nuget/v/Nullean.ScopedFileSystem?style=flat-square" alt="NuGet"></a>
  <a href="https://github.com/nullean/scoped-filesystem/actions/workflows/ci.yml"><img src="https://img.shields.io/github/actions/workflow/status/nullean/scoped-filesystem/ci.yml?branch=main&style=flat-square&label=ci" alt="CI"></a>
</p>

```csharp
using System.IO.Abstractions;
using Nullean.ScopedFileSystem;

IFileSystem scoped = new ScopedFileSystem("/var/www/docs");

var content = scoped.File.ReadAllText("/var/www/docs/page.md");      // OK: within scope
scoped.File.WriteAllText("/var/www/docs/output.md", content);         // OK: write within scope

bool exists = scoped.File.Exists("/etc/passwd");                      // false, no exception

scoped.File.ReadAllText("/etc/passwd");                               // ScopedFileSystemException: outside scope
scoped.File.ReadAllText("/var/www/docs/../../../etc/passwd");         // ScopedFileSystemException: .. is resolved first
scoped.File.ReadAllText("/var/www/docs/.env");                        // ScopedFileSystemException: hidden file
scoped.File.ReadAllText("/var/www/docs/link-to-secret");              // ScopedFileSystemException: symbolic link
```

## Install

```sh
dotnet add package Nullean.ScopedFileSystem
```

Targets .NET Standard 2.0 and 2.1, .NET 9 and .NET 10; trimming- and AOT-safe on .NET 9 and later.

## Quick start

Wrap any `IFileSystem` (the real one, or a `MockFileSystem` in tests) and configure what else is allowed:

```csharp
using System.IO.Abstractions;
using Nullean.ScopedFileSystem;

IFileSystem inner = new FileSystem();   // or a MockFileSystem in tests

IFileSystem scoped = new ScopedFileSystem(inner, new ScopedFileSystemOptions("/var/www/docs", "/var/www/data")
{
	AllowedHiddenFolderNames = [".git"],
	AllowedHiddenFileNames = [".gitkeep"],
	AllowedSpecialFolders = AllowedSpecialFolder.Temp | AllowedSpecialFolder.ApplicationData,
});

scoped.File.WriteAllText(Path.Combine(Path.GetTempPath(), "cache.txt"), "ok");   // OK: temp is allowed
```

`ScopedFileSystem` enforces these rules on every file and directory operation:

| Rule | Detail |
|------|--------|
| **Scope** | The resolved path must be within a configured scope root or an explicitly allowed OS special folder. |
| **No symlinks** | The target and all ancestor directories up to the scope root must not be symbolic links. |
| **No hidden files** | Files whose name starts with `.` are blocked unless the name is in `AllowedHiddenFileNames`. |
| **No hidden directories** | Directories whose name starts with `.` are blocked unless the name is in `AllowedHiddenFolderNames`. This applies to both the target directory and all ancestors up to the scope root. |
| **Exists is safe** | `File.Exists()` and `Directory.Exists()` return `false` for out-of-scope paths instead of throwing. |

Any violation throws `ScopedFileSystemException` (extends `SecurityException`).

Any violation throws `ScopedFileSystemException` (extends `SecurityException`).

## Multiple roots

Pass multiple paths to the constructor. Roots must be fully **disjoint** — no root may be an ancestor of another. Violating this throws `ArgumentException` at construction time.

```csharp
// OK — /docs and /data are siblings
new ScopedFileSystem(inner, "/docs", "/data");

// ArgumentException — /data is an ancestor of /data/sub
new ScopedFileSystem(inner, "/data", "/data/sub");
```

## Hidden files and directories

By default, any file or directory whose name begins with `.` is blocked. Use `ScopedFileSystemOptions` to allow specific names:

```csharp
var scoped = new ScopedFileSystem(new ScopedFileSystemOptions("/project")
{
    // Allow traversing .git directories (e.g. to read .git/config)
    AllowedHiddenFolderNames = [".git", ".nuget"],

    // Allow reading/writing specific hidden files
    AllowedHiddenFileNames = [".gitkeep", ".gitignore"],
});
```

Allowlist matching is case-sensitive on Linux and case-insensitive on Windows and macOS, matching the platform filesystem behaviour. Any `IReadOnlyCollection<string>` is accepted — no need to configure a comparer.

## Special folders

Opt in to read and write access outside the scope roots for well-known OS directories using the `AllowedSpecialFolders` flags enum:

| Flag | macOS | Windows | Linux |
|------|-------|---------|-------|
| `Temp` | `/var/folders/…` / `/tmp` | `%TEMP%` | `/tmp` |
| `ApplicationData` | `~/Library/Application Support` | `%APPDATA%` | `~/.config` |
| `LocalApplicationData` | `~/Library/Application Support` | `%LOCALAPPDATA%` | `~/.local/share` |
| `CommonApplicationData` | `/Library/Application Support` | `C:\ProgramData` | `/usr/share` |
| `All` | all four combined | | |

```csharp
var scoped = new ScopedFileSystem(new ScopedFileSystemOptions("/project")
{
    AllowedSpecialFolders = AllowedSpecialFolder.Temp | AllowedSpecialFolder.ApplicationData,
});

// OK — temp is allowed
scoped.File.WriteAllText(Path.Combine(Path.GetTempPath(), "cache.bin"), data);
```

Paths are resolved at construction time. Access to special folders bypasses all hidden-file and hidden-directory checks.

## `ScopedFileSystemOptions`

`ScopedFileSystemOptions` is the primary configuration surface. It accepts one or more scope roots either as strings or as `IDirectoryInfo` instances:

```csharp
// String roots
var options = new ScopedFileSystemOptions("/root1", "/root2");

// IDirectoryInfo roots
var options = new ScopedFileSystemOptions(fs.DirectoryInfo.New("/root1"));
```

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ScopeRoots` | `IReadOnlyList<string>` | _(required on constructor)_ | Scope root paths |
| `AllowedHiddenFileNames` | `IReadOnlyCollection<string>` | empty | Hidden file names that are allowed |
| `AllowedHiddenFolderNames` | `IReadOnlyCollection<string>` | empty | Hidden directory names that are allowed |
| `AllowedSpecialFolders` | `AllowedSpecialFolder` | `None` | OS special folders to allow |

The inner `IFileSystem` is passed directly to `ScopedFileSystem` rather than through options; overloads without an explicit `inner` default to `new FileSystem()`.

## Path validation

All paths are resolved via `IPath.GetFullPath` before any check. This means traversal sequences like `..` are collapsed first, so `"/docs/../etc/passwd"` is correctly identified as `"/etc/passwd"` and rejected. Scope membership is verified by walking up the directory tree rather than string prefix matching, which prevents sibling-prefix attacks (e.g. `/docs-extra` is not inside `/docs`).

## Platform behaviour

- **Case sensitivity**: Path comparison is case-sensitive on Linux, case-insensitive on Windows and macOS.
- **Separator normalisation**: Both `/` and `\` separators are handled correctly on all platforms.

## Learn more

- [Releases](https://github.com/nullean/scoped-filesystem/releases)
- [Contributing](https://github.com/nullean/.github/blob/main/CONTRIBUTING.md); `mise run test` runs the xUnit suite
  (on `MockFileSystem`), `mise run pack` packs and validates the package.

MIT licensed. Copyright Nullean and contributors.
