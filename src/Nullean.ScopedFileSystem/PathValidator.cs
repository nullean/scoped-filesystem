// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information

using System.IO.Abstractions;

namespace Nullean.ScopedFileSystem;

/// <summary>Shared path validation logic for scope and symlink enforcement.</summary>
internal static class PathValidator
{
	private static StringComparison Comparison =>
		FileSystemPlatform.IsCaseSensitiveFileSystem ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

	/// <summary>
	/// Verifies that no root in <paramref name="normalizedRoots"/> is an ancestor of another.
	/// </summary>
	/// <param name="normalizedRoots">Absolute, canonical (already <c>GetFullPath</c>-resolved) scope roots.</param>
	/// <param name="inner">The filesystem used for path traversal.</param>
	/// <exception cref="ArgumentException">
	/// Thrown when any root is a strict ancestor of another root. Roots must be fully disjoint;
	/// nesting is not permitted because it creates ambiguity about which policies apply and
	/// makes reasoning about the effective scope significantly harder.
	/// </exception>
	internal static void ValidateRootsAreDisjoint(IReadOnlyList<string> normalizedRoots, IFileSystem inner)
	{
		for (var i = 0; i < normalizedRoots.Count; i++)
		for (var j = i + 1; j < normalizedRoots.Count; j++)
		{
			var a = normalizedRoots[i];
			var b = normalizedRoots[j];
			if (IsStrictAncestorOf(a, b, inner) || IsStrictAncestorOf(b, a, inner))
				throw new ArgumentException(
					$"Scope roots must be disjoint: '{a}' is an ancestor of '{b}' (or vice versa). " +
					"No scope root may be a parent or child of another scope root.",
					nameof(normalizedRoots));
		}
	}

	/// <summary>
	/// Returns true when <paramref name="path"/> is within any of the <paramref name="scopeRoots"/>,
	/// without throwing. Used by <c>Exists</c> to silently return false for out-of-scope paths.
	/// </summary>
	internal static bool IsInScope(string path, IReadOnlyList<string> scopeRoots, IFileSystem inner)
	{
		var fullPath = inner.Path.GetFullPath(path);
		return scopeRoots.Any(root => IsWithinRoot(fullPath, root, inner));
	}

	/// <summary>
	/// Validates that <paramref name="path"/> is within one of the <paramref name="scopeRoots"/>,
	/// is not a symlink, and has no symlinked or hidden ancestors between the file and the matched scope root.
	/// Throws <see cref="ScopedFileSystemException"/> on any violation.
	/// </summary>
	internal static void ValidatePath(string path, IReadOnlyList<string> scopeRoots, IFileSystem inner)
	{
		var fullPath = inner.Path.GetFullPath(path);

		var matchedRoot = scopeRoots.FirstOrDefault(root => IsWithinRoot(fullPath, root, inner));
		if (matchedRoot is null)
			throw new ScopedFileSystemException(
				$"Access denied: '{path}' resolves to '{fullPath}' which is outside all configured scope roots.");

		ValidateAncestors(fullPath, matchedRoot, inner);
	}

	/// <summary>
	/// Walks up the directory tree from <paramref name="fullPath"/> using <c>inner.Path.GetDirectoryName</c>
	/// to determine whether <paramref name="scopeRoot"/> is a true ancestor (or the path itself).
	/// This traversal-based check avoids string-prefix tricks (e.g. /docs vs /docs-extra).
	/// </summary>
	private static bool IsWithinRoot(string fullPath, string scopeRoot, IFileSystem inner)
	{
		var comparison = Comparison;
		var current = fullPath;
		while (current != null)
		{
			if (string.Equals(current, scopeRoot, comparison))
				return true;
			var parent = inner.Path.GetDirectoryName(current);
			if (parent is null || string.Equals(parent, current, comparison))
				break; // reached the filesystem root
			current = parent;
		}
		return false;
	}

	/// <summary>
	/// Returns true when <paramref name="ancestor"/> is a strict (non-equal) parent of
	/// <paramref name="descendant"/>. Both paths must already be canonical (GetFullPath-resolved).
	/// Uses <c>inner.Path.GetDirectoryName</c> traversal so that paths sharing a prefix
	/// but not in an actual parent-child relationship are not falsely matched.
	/// </summary>
	private static bool IsStrictAncestorOf(string ancestor, string descendant, IFileSystem inner)
	{
		var comparison = Comparison;
		var current = inner.Path.GetDirectoryName(descendant);
		while (current is not null)
		{
			if (string.Equals(current, ancestor, comparison))
				return true;
			var parent = inner.Path.GetDirectoryName(current);
			if (parent is null || string.Equals(parent, current, comparison))
				break;
			current = parent;
		}
		return false;
	}

	/// <summary>
	/// Validates that <paramref name="fullPath"/> is not a symlink and that no ancestor directory
	/// up to <paramref name="scopeRoot"/> is a symlink or hidden, using <see cref="FileSystemExtensions.TryValidateSymlinkAccess"/>.
	/// </summary>
	private static void ValidateAncestors(string fullPath, string scopeRoot, IFileSystem inner)
	{
		var fileInfo = inner.FileInfo.New(fullPath);
		var rootInfo = inner.DirectoryInfo.New(scopeRoot);
		if (!fileInfo.TryValidateSymlinkAccess(rootInfo, out var error))
			throw new ScopedFileSystemException($"Access denied: {error}.");
	}
}
