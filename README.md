# Nullean.ScopedFileSystem

A `System.IO.Abstractions` `IFileSystem` decorator that restricts file **read and write** operations to one or more configured scope root directories and rejects symbolic links.

## What it does

`ScopedFileSystem` wraps any `IFileSystem` and enforces the following rules on all file operations:

1. The resolved (canonicalized via `GetFullPath`) path must fall within at least one configured scope root — unless the path is within an explicitly allowed OS special folder.
2. The target file must not be a symbolic link (`IFileInfo.LinkTarget == null`).
3. The target file's own name must not start with `.` (hidden file), unless the name is in `AllowedHiddenFileNames`.
4. No ancestor directory between the file and the matched scope root may be a symlink or a hidden directory (name starting with `.`), unless the directory name is in `AllowedHiddenFolderNames`.
5. `File.Exists()` returns `false` for out-of-scope paths — it never throws.
6. `Directory.Exists()` returns `false` for out-of-scope paths — it never throws.
7. All `IDirectory` and `IDirectoryInfo` operations are validated with the same rules as file operations.

A `ScopedFileSystemException` (extending `SecurityException`) is thrown on any violation.

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
