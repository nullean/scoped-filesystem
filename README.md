# Nullean.ScopedFileSystem

A `System.IO.Abstractions` `IFileSystem` decorator that restricts file **read and write** operations to one or more configured scope root directories and rejects symbolic links.

## What it does

`ScopedFileSystem` wraps any `IFileSystem` and enforces these rules on every file and directory operation:

| Rule | Detail |
|------|--------|
| **Scope** | The resolved path must be within a configured scope root or an explicitly allowed OS special folder. |
| **No symlinks** | The target and all ancestor directories up to the scope root must not be symbolic links. |
| **No hidden files** | Files whose name starts with `.` are blocked unless the name is in `AllowedHiddenFileNames`. |
| **No hidden directories** | Directories whose name starts with `.` are blocked unless the name is in `AllowedHiddenFolderNames`. This applies to both the target directory and all ancestors up to the scope root. |
| **Exists is safe** | `File.Exists()` and `Directory.Exists()` return `false` for out-of-scope paths instead of throwing. |

Any violation throws `ScopedFileSystemException` (extends `SecurityException`).

## Usage

```csharp
using System.IO.Abstractions;
using Nullean.ScopedFileSystem;

// Convenience: single root, real filesystem
IFileSystem scoped = new ScopedFileSystem("/var/www/docs");

// Convenience: custom inner filesystem (e.g. MockFileSystem in tests)
IFileSystem scoped = new ScopedFileSystem(inner, "/var/www/docs");

// Full control via ScopedFileSystemOptions
IFileSystem scoped = new ScopedFileSystem(inner, new ScopedFileSystemOptions("/var/www/docs")
{
    AllowedHiddenFolderNames = new HashSet<string> { ".git" },
    AllowedHiddenFileNames   = new HashSet<string> { ".gitkeep" },
    AllowedSpecialFolders    = AllowedSpecialFolder.Temp | AllowedSpecialFolder.ApplicationData,
});
```

```csharp
// OK — within scope
var content = scoped.File.ReadAllText("/var/www/docs/page.md");

// OK — write within scope
scoped.File.WriteAllText("/var/www/docs/output.md", content);

// Throws ScopedFileSystemException — outside scope
scoped.File.ReadAllText("/etc/passwd");

// Throws ScopedFileSystemException — hidden file (.env)
scoped.File.ReadAllText("/var/www/docs/.env");

// Throws ScopedFileSystemException — hidden ancestor directory (.hidden)
scoped.File.ReadAllText("/var/www/docs/.hidden/secret.txt");

// Returns false — no exception
bool exists = scoped.File.Exists("/etc/passwd");

// Throws ScopedFileSystemException — symlink
scoped.File.ReadAllText("/var/www/docs/link-to-secret");
```

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
    AllowedHiddenFolderNames = new HashSet<string> { ".git", ".nuget" },

    // Allow reading/writing specific hidden files
    AllowedHiddenFileNames = new HashSet<string> { ".gitkeep", ".gitignore" },
});
```

Allowlist matching uses `OrdinalIgnoreCase` by default (the `HashSet<string>` comparer). Supply a `HashSet<string>` with `StringComparer.Ordinal` when running on a case-sensitive filesystem and exact case matters.

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
| `AllowedHiddenFileNames` | `IReadOnlySet<string>` | empty | Hidden file names that are allowed |
| `AllowedHiddenFolderNames` | `IReadOnlySet<string>` | empty | Hidden directory names that are allowed |
| `AllowedSpecialFolders` | `AllowedSpecialFolder` | `None` | OS special folders to allow |

The inner `IFileSystem` is passed directly to `ScopedFileSystem` rather than through options; overloads without an explicit `inner` default to `new FileSystem()`.

## Path validation

All paths are resolved via `IPath.GetFullPath` before any check. This means traversal sequences like `..` are collapsed first, so `"/docs/../etc/passwd"` is correctly identified as `"/etc/passwd"` and rejected. Scope membership is verified by walking up the directory tree rather than string prefix matching, which prevents sibling-prefix attacks (e.g. `/docs-extra` is not inside `/docs`).

## Platform behaviour

- **Case sensitivity**: Path comparison is case-sensitive on Linux, case-insensitive on Windows and macOS.
- **Separator normalisation**: Both `/` and `\` separators are handled correctly on all platforms.

## Projects

| Project | Description |
|---------|-------------|
| `src/Nullean.ScopedFileSystem` | Library |
| `tests/Nullean.ScopedFileSystem.Tests` | xUnit test suite using `MockFileSystem` |

## Building

```bash
dotnet build
dotnet test
```
