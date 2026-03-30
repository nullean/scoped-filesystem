// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information

using System.IO.Abstractions;

namespace Nullean.ScopedFileSystem;

/// <summary>Shared path validation logic for scope and symlink enforcement.</summary>
internal static class PathValidator
{
	/// <summary>
	/// Returns true when <paramref name="path"/> is within <paramref name="scopeRoot"/>,
	/// without throwing. Used by <c>Exists</c> to silently return false for out-of-scope paths.
	/// </summary>
	internal static bool IsInScope(string path, string scopeRoot, IFileSystem inner)
	{
		var fullPath = inner.Path.GetFullPath(path);
		return IsWithinRoot(fullPath, scopeRoot);
	}

	/// <summary>
	/// Validates that <paramref name="path"/> is within <paramref name="scopeRoot"/>,
	/// is not a symlink, and has no hidden or symlinked ancestors up to the scope root.
	/// Throws <see cref="ScopedFileSystemException"/> on any violation.
	/// </summary>
	internal static void ValidateReadPath(string path, string scopeRoot, IFileSystem inner)
	{
		var fullPath = inner.Path.GetFullPath(path);

		if (!IsWithinRoot(fullPath, scopeRoot))
			throw new ScopedFileSystemException(
				$"Access denied: '{path}' resolves to '{fullPath}' which is outside the scope root '{scopeRoot}'."
			);

		var fileInfo = inner.FileInfo.New(fullPath);
		if (fileInfo.LinkTarget != null)
			throw new ScopedFileSystemException(
				$"Access denied: '{fullPath}' is a symbolic link. Symlinks are not permitted within the scope root."
			);

		ValidateAncestors(fullPath, scopeRoot, inner);
	}

	private static bool IsWithinRoot(string fullPath, string scopeRoot)
	{
		var comparison = OperatingSystem.IsLinux()
			? StringComparison.Ordinal
			: StringComparison.OrdinalIgnoreCase;

		return fullPath.Equals(scopeRoot, comparison)
			|| fullPath.StartsWith(scopeRoot + System.IO.Path.DirectorySeparatorChar, comparison)
			|| fullPath.StartsWith(scopeRoot + System.IO.Path.AltDirectorySeparatorChar, comparison);
	}

	private static void ValidateAncestors(string fullPath, string scopeRoot, IFileSystem inner)
	{
		var comparison = OperatingSystem.IsLinux()
			? StringComparison.Ordinal
			: StringComparison.OrdinalIgnoreCase;

		var current = inner.Path.GetDirectoryName(fullPath);
		while (current != null && !current.Equals(scopeRoot, comparison))
		{
			var dirInfo = inner.DirectoryInfo.New(current);

			if (dirInfo.LinkTarget != null)
				throw new ScopedFileSystemException(
					$"Access denied: ancestor directory '{current}' is a symbolic link."
				);

			if (dirInfo.Name.StartsWith('.'))
				throw new ScopedFileSystemException(
					$"Access denied: ancestor directory '{current}' is hidden (starts with '.')."
				);

			current = inner.Path.GetDirectoryName(current);
		}
	}
}
