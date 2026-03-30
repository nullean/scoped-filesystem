// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information

using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;

namespace Nullean.ScopedFileSystem;

/// <summary>Extension methods for filesystem path validation.</summary>
public static class FileSystemExtensions
{
    /// <summary>Returns true when <paramref name="file"/> resides inside <paramref name="parentDirectory"/>.</summary>
    public static bool IsSubPathOf(this IFileInfo file, IDirectoryInfo parentDirectory)
    {
        var parent = file.Directory;
        return parent is not null && parent.IsSubPathOf(parentDirectory);
    }

    /// <summary>Returns true when <paramref name="directory"/> is <paramref name="parentDirectory"/> or a subdirectory of it.</summary>
    public static bool IsSubPathOf(this IDirectoryInfo directory, IDirectoryInfo parentDirectory)
    {
        var cmp = FileSystemPlatform.IsCaseSensitiveFileSystem
            ? StringComparison.Ordinal
            : StringComparison.OrdinalIgnoreCase;
        var current = directory;
        do
        {
            if (string.Equals(current.FullName, parentDirectory.FullName, cmp))
                return true;
            current = current.Parent;
        } while (current != null);
        return false;
    }

    /// <summary>
    /// Validates that <paramref name="file"/> is not a symlink and that no ancestor directory
    /// between <paramref name="file"/> and <paramref name="docRoot"/> is a symlink or hidden.
    /// Returns true if access is valid; false with a non-null <paramref name="error"/> describing the violation.
    /// </summary>
    public static bool TryValidateSymlinkAccess(
        this IFileInfo file,
        IDirectoryInfo docRoot,
        [NotNullWhen(false)] out string? error)
    {
#if NET6_0_OR_GREATER
        if (file.Exists && file.LinkTarget != null)
#else
        if (file.Exists && new System.IO.FileInfo(file.FullName).Attributes.HasFlag(FileAttributes.ReparsePoint))
#endif
        {
            error = "path must not point to a symlink";
            return false;
        }

        var cmp = FileSystemPlatform.IsCaseSensitiveFileSystem
            ? StringComparison.Ordinal
            : StringComparison.OrdinalIgnoreCase;

        var dir = file.Directory;
        while (dir != null && !string.Equals(dir.FullName, docRoot.FullName, cmp))
        {
            if (dir.Name.StartsWith("."))
            {
                error = "path must not traverse hidden directories";
                return false;
            }
#if NET6_0_OR_GREATER
            if (dir.Exists && dir.LinkTarget != null)
#else
            if (dir.Exists && new System.IO.DirectoryInfo(dir.FullName).Attributes.HasFlag(FileAttributes.ReparsePoint))
#endif
            {
                error = "path must not traverse symlinked directories";
                return false;
            }
            dir = dir.Parent;
        }
        error = null;
        return true;
    }
}
