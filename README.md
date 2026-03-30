# Nullean.ScopedFileSystem

A `System.IO.Abstractions` `IFileSystem` decorator that restricts file **read and write** operations to one or more configured scope root directories and rejects symbolic links.

## What it does

`ScopedFileSystem` wraps any `IFileSystem` and enforces the following rules on all file operations:

1. The resolved (canonicalized via `GetFullPath`) path must fall within at least one configured scope root.
2. The target file must not be a symbolic link (`IFileInfo.LinkTarget == null`).
3. No ancestor directory between the file and the matched scope root may be a symlink or a hidden directory (name starting with `.`).
4. `File.Exists()` returns `false` for out-of-scope paths — it never throws.
5. Directory operations (`IDirectory`, `IDirectoryInfo`) pass through to the inner `IFileSystem` unchanged.

A `ScopedFileSystemException` (extending `SecurityException`) is thrown on any violation.

## Usage

```csharp
using System.IO.Abstractions;
using Nullean.ScopedFileSystem;

IFileSystem inner = new FileSystem();

// Single root
IFileSystem scoped = new ScopedFileSystem(inner, "/var/www/docs");

// Multiple disjoint roots
IFileSystem multi = new ScopedFileSystem(inner, "/var/www/docs", "/var/data");
```

```csharp
// OK — within scope
var content = scoped.File.ReadAllText("/var/www/docs/page.md");

// OK — write within scope
scoped.File.WriteAllText("/var/www/docs/output.md", content);

// Throws ScopedFileSystemException — outside scope
scoped.File.ReadAllText("/etc/passwd");

// Throws ScopedFileSystemException — outside scope
scoped.File.WriteAllText("/etc/evil.txt", "pwned");

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
