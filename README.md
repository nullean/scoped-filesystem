# Nullean.ScopedFileSystem

A `System.IO.Abstractions` `IFileSystem` decorator that restricts file **read** operations to a configured scope root directory and rejects symbolic links.

## What it does

`ScopedFileSystem` wraps any `IFileSystem` and enforces the following rules on **all read operations**:

1. The resolved (canonicalized) path must fall within the configured `scopeRoot`.
2. The target file must not be a symbolic link (`IFileInfo.LinkTarget == null`).
3. No ancestor directory between the file and the scope root may be a symlink or a hidden directory (name starting with `.`).
4. `File.Exists()` returns `false` for out-of-scope paths — it never throws.
5. All **write, delete, move, directory, and path** operations pass through to the inner `IFileSystem` unchanged.

A `ScopedFileSystemException` (extending `SecurityException`) is thrown on any read violation.

## Usage

```csharp
using System.IO.Abstractions;
using Nullean.ScopedFileSystem;

IFileSystem inner = new FileSystem();
IFileSystem scoped = new ScopedFileSystem(inner, "/var/www/docs");

// OK - within scope
var content = scoped.File.ReadAllText("/var/www/docs/page.md");

// Throws ScopedFileSystemException - outside scope
scoped.File.ReadAllText("/etc/passwd");

// Returns false - no exception
bool exists = scoped.File.Exists("/etc/passwd");

// Throws ScopedFileSystemException - symlink
scoped.File.ReadAllText("/var/www/docs/link-to-secret");
```

## Platform behaviour

- **Case sensitivity**: Path comparison is case-sensitive on Linux, case-insensitive on Windows and macOS (matching OS conventions).
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
