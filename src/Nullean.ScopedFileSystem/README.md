# Nullean.ScopedFileSystem

![nullean](https://raw.githubusercontent.com/nullean/nullean.github.io/main/assets/nullean-lockup-light-544x192.png)

A `System.IO.Abstractions` `IFileSystem` decorator that restricts file reads and writes to one or more scope root directories and rejects symbolic links, hidden files and path traversal.

[![NuGet](https://img.shields.io/nuget/v/Nullean.ScopedFileSystem?style=flat-square)](https://www.nuget.org/packages/Nullean.ScopedFileSystem)

## Install

```sh
dotnet add package Nullean.ScopedFileSystem
```

## Quick start

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

Wrap any `IFileSystem`, such as a `MockFileSystem` in tests, and allow what else you need:

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

## What it enforces

| Rule | Detail |
|------|--------|
| **Scope** | The resolved path must be within a configured scope root or an explicitly allowed OS special folder. |
| **No symlinks** | The target and all ancestor directories up to the scope root must not be symbolic links. |
| **No hidden files** | Files whose name starts with `.` are blocked unless the name is in `AllowedHiddenFileNames`. |
| **No hidden directories** | Directories whose name starts with `.` are blocked unless the name is in `AllowedHiddenFolderNames`. This applies to both the target directory and all ancestors up to the scope root. |
| **Exists is safe** | `File.Exists()` and `Directory.Exists()` return `false` for out-of-scope paths instead of throwing. |

Any violation throws `ScopedFileSystemException` (extends `SecurityException`).

Any violation throws `ScopedFileSystemException` (extends `SecurityException`). Paths are resolved with
`IPath.GetFullPath` before any check, so `..` cannot escape, and scope membership is checked by walking up the
directory tree, so `/docs-extra` is not inside `/docs`. Multiple roots must be disjoint, or the constructor throws
`ArgumentException`.

## Learn more

- Multiple roots, hidden names, special folders and the options reference: https://github.com/nullean/scoped-filesystem#multiple-roots
- Source and issues: https://github.com/nullean/scoped-filesystem
