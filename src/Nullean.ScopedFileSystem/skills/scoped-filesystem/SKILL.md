---
name: scoped-filesystem
description: >-
  Use when C# code reads or writes files on paths that come from users, config or content (docs builders, static
  site generators, servers, plugins) and must not escape a root directory. Nullean.ScopedFileSystem wraps any
  System.IO.Abstractions IFileSystem and throws on paths outside the scope roots, symlinks and hidden files.
---

# Nullean.ScopedFileSystem

`using Nullean.ScopedFileSystem;` `ScopedFileSystem` is an `IFileSystem`: hand it to code that already takes one.
It guards `File`, `FileInfo`, `Directory` and `DirectoryInfo`. Do not use it as a general sandbox: it does not
stop a process, native code or `System.IO` calls that bypass the abstraction.

## The calls you need

```csharp
// one root on the real filesystem
IFileSystem fs = new ScopedFileSystem("/srv/docs");

// several disjoint roots over any inner filesystem (MockFileSystem in tests)
IFileSystem multi = new ScopedFileSystem(new FileSystem(), "/srv/docs", "/srv/assets");

// everything optional is denied until allowed
IFileSystem custom = new ScopedFileSystem(new FileSystem(), new ScopedFileSystemOptions("/srv/docs")
{
    AllowedHiddenFolderNames = [".github"],          // checked on the directory and every ancestor up to the root
    AllowedHiddenFileNames = [".gitkeep"],           // checked on the file's own name only
    AllowedSpecialFolders = AllowedSpecialFolder.Temp | AllowedSpecialFolder.ApplicationData,
    VerboseExceptions = true,                        // list the accessible paths in the exception message
});
```

```csharp
try
{
    var text = fs.File.ReadAllText(userSuppliedPath);
}
catch (ScopedFileSystemException e) // a SecurityException: out of scope, symlink or hidden path
{
    Console.Error.WriteLine(e.Message);
}

// Exists never throws: an out-of-scope path is simply false
if (fs.File.Exists("../../etc/passwd")) { }
```

## Gotchas

- `FileStream`, `FileSystemWatcher`, `DriveInfo`, `FileVersionInfo` and `Path` pass straight through to the inner
  filesystem with **no checks**. Open files through `fs.File` or `fs.FileInfo`, not `fs.FileStream.New(...)`.
- Relative paths resolve against the process's current directory (`Path.GetFullPath`), not against a scope root.
  Combine with the root yourself: `fs.Path.Combine(root, relative)`.
- Scope roots must be disjoint: a root inside another root throws `ArgumentException` in the constructor.
- A `ScopedFileSystem` cannot wrap another one (`ArgumentException`).
- Allowed special folders skip the hidden-path checks entirely, and on Linux `ApplicationData` is `~/.config`.
- Name matching is case-insensitive on Windows and macOS, case-sensitive on Linux.
