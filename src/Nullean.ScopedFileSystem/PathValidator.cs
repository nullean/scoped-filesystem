// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information

using System.IO.Abstractions;

namespace Nullean.ScopedFileSystem;

/// <summary>Shared path validation logic for scope, symlink, and hidden-path enforcement.</summary>
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
	/// Returns true when <paramref name="path"/> is within any of the scope roots or allowed special
	/// folder paths in <paramref name="ctx"/>, without throwing.
	/// Used by <c>Exists</c> to silently return false for out-of-scope paths.
	/// </summary>
	internal static bool IsInScope(string path, ValidationContext ctx, IFileSystem inner)
	{
		var fullPath = inner.Path.GetFullPath(path);
		return ctx.NormalizedRoots.Any(root => IsWithinRoot(fullPath, root, inner))
			|| ctx.ResolvedSpecialFolderPaths.Any(sf => IsWithinRoot(fullPath, sf, inner));
	}

	/// <summary>
	/// Validates that the directory at <paramref name="path"/> is permitted.
	/// Same rules as <see cref="ValidatePath"/> except the hidden-name check uses
	/// <see cref="ValidationContext.AllowedHiddenFolderNames"/> (not <c>AllowedHiddenFileNames</c>)
	/// for the directory's own name, and ancestor validation uses the directory-info overload
	/// of <see cref="FileSystemExtensions.TryValidateSymlinkAccess"/>.
	/// Throws <see cref="ScopedFileSystemException"/> on any violation.
	/// </summary>
	internal static void ValidateDirectoryPath(string path, ValidationContext ctx, IFileSystem inner)
	{
		var fullPath = inner.Path.GetFullPath(path);

		if (ctx.ResolvedSpecialFolderPaths.Any(sf => IsWithinRoot(fullPath, sf, inner)))
			return;

		var matchedRoot = ctx.NormalizedRoots.FirstOrDefault(root => IsWithinRoot(fullPath, root, inner));
		if (matchedRoot is null)
			throw new ScopedFileSystemException(
				$"Access denied: '{path}' resolves to '{fullPath}' which is outside all configured scope roots.");

		// Hidden directory name check on the target directory itself
		var dirName = inner.Path.GetFileName(fullPath);
		if (!string.IsNullOrEmpty(dirName)
			&& dirName.StartsWith('.')
			&& !ctx.AllowedHiddenFolderNames.Contains(dirName))
			throw new ScopedFileSystemException(
				$"Access denied: '{dirName}' is a hidden directory. " +
				$"Add '{dirName}' to {nameof(ScopedFileSystemOptions.AllowedHiddenFolderNames)} to permit access.");

		ValidateDirectoryAncestors(fullPath, matchedRoot, ctx.AllowedHiddenFolderNames, inner);
	}

	/// <summary>
	/// Validates that <paramref name="path"/> is permitted:
	/// <list type="bullet">
	///   <item>Paths within a resolved special folder bypass all other checks.</item>
	///   <item>Otherwise, the path must be within one of the scope roots, the target file must not
	///   be hidden (unless in <see cref="ValidationContext.AllowedHiddenFileNames"/>), and no ancestor
	///   directory up to the matched root may be hidden (unless in
	///   <see cref="ValidationContext.AllowedHiddenFolderNames"/>) or a symlink.</item>
	/// </list>
	/// Throws <see cref="ScopedFileSystemException"/> on any violation.
	/// </summary>
	internal static void ValidatePath(string path, ValidationContext ctx, IFileSystem inner)
	{
		var fullPath = inner.Path.GetFullPath(path);

		// Special folder bypass: full access with no further checks
		if (ctx.ResolvedSpecialFolderPaths.Any(sf => IsWithinRoot(fullPath, sf, inner)))
			return;

		var matchedRoot = ctx.NormalizedRoots.FirstOrDefault(root => IsWithinRoot(fullPath, root, inner));
		if (matchedRoot is null)
			throw new ScopedFileSystemException(
				$"Access denied: '{path}' resolves to '{fullPath}' which is outside all configured scope roots.");

		// Hidden file check: block files whose own name starts with '.' unless explicitly allowed
		var fileName = inner.Path.GetFileName(fullPath);
		if (!string.IsNullOrEmpty(fileName)
			&& fileName.StartsWith('.')
			&& !ctx.AllowedHiddenFileNames.Contains(fileName))
			throw new ScopedFileSystemException(
				$"Access denied: '{fileName}' is a hidden file. " +
				$"Add '{fileName}' to {nameof(ScopedFileSystemOptions.AllowedHiddenFileNames)} to permit access.");

		ValidateAncestors(fullPath, matchedRoot, ctx.AllowedHiddenFolderNames, inner);
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

	private static void ValidateDirectoryAncestors(
		string fullPath,
		string scopeRoot,
		IReadOnlySet<string> allowedHiddenFolderNames,
		IFileSystem inner)
	{
		var dirInfo = inner.DirectoryInfo.New(fullPath);
		var rootInfo = inner.DirectoryInfo.New(scopeRoot);
		if (!dirInfo.TryValidateSymlinkAccess(rootInfo, allowedHiddenFolderNames, out var error))
			throw new ScopedFileSystemException($"Access denied: {error}.");
	}

	/// <summary>
	/// Validates that <paramref name="fullPath"/> is not a symlink and that no ancestor directory
	/// up to <paramref name="scopeRoot"/> is a symlink or hidden (unless its name is in
	/// <paramref name="allowedHiddenFolderNames"/>), using
	/// <see cref="FileSystemExtensions.TryValidateSymlinkAccess"/>.
	/// </summary>
	private static void ValidateAncestors(
		string fullPath,
		string scopeRoot,
		IReadOnlySet<string> allowedHiddenFolderNames,
		IFileSystem inner)
	{
		var fileInfo = inner.FileInfo.New(fullPath);
		var rootInfo = inner.DirectoryInfo.New(scopeRoot);
		if (!fileInfo.TryValidateSymlinkAccess(rootInfo, allowedHiddenFolderNames, out var error))
			throw new ScopedFileSystemException($"Access denied: {error}.");
	}
}
