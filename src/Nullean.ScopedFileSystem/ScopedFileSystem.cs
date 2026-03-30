// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information

using System.IO.Abstractions;

namespace Nullean.ScopedFileSystem;

/// <summary>
/// An <see cref="IFileSystem"/> decorator that restricts file read and write operations to one or more
/// configured scope directories and rejects symbolic links. All directory operations pass through
/// to the inner filesystem without restriction.
/// </summary>
/// <remarks>
/// <para>
/// Pass one or more absolute or relative directory paths as <c>scopeRoots</c>. Each path is resolved
/// to an absolute, canonical form via <see cref="IPath.GetFullPath(string)"/>.
/// </para>
/// <para>
/// <b>Root disjointness:</b> no root may be an ancestor of another root. If roots overlap
/// (e.g. <c>/data</c> and <c>/data/sub</c>) an <see cref="ArgumentException"/> is thrown at
/// construction time. Nested roots create ambiguous policy boundaries and are therefore forbidden.
/// </para>
/// </remarks>
public class ScopedFileSystem : IFileSystem
{
	private readonly IFileSystem _inner;
	private readonly IReadOnlyList<string> _scopeRoots;

	/// <summary>Initialises a scoped filesystem rooted at <paramref name="scopeRoot"/>.</summary>
	public ScopedFileSystem(IFileSystem inner, IDirectoryInfo scopeRoot)
		: this(inner, scopeRoot.FullName) { }

	/// <summary>
	/// Initializes a new <see cref="ScopedFileSystem"/>.
	/// </summary>
	/// <param name="inner">The underlying <see cref="IFileSystem"/> to wrap.</param>
	/// <param name="scopeRoots">
	/// One or more directory paths that define the allowed scope for file read and write operations.
	/// Each path is normalised to an absolute, canonical path via <see cref="IPath.GetFullPath(string)"/>.
	/// No root may be an ancestor of another; doing so throws <see cref="ArgumentException"/>.
	/// </param>
	/// <exception cref="ArgumentException">
	/// Thrown when fewer than one root is supplied, or when any root is an ancestor of another root.
	/// </exception>
	public ScopedFileSystem(IFileSystem inner, params string[] scopeRoots)
	{
		if (scopeRoots is null || scopeRoots.Length == 0)
			throw new ArgumentException("At least one scope root must be provided.", nameof(scopeRoots));

		_inner = inner;

		var normalized = scopeRoots
			.Select(r => _inner.Path.GetFullPath(r)
				.TrimEnd(_inner.Path.DirectorySeparatorChar, _inner.Path.AltDirectorySeparatorChar))
			.ToArray();

		PathValidator.ValidateRootsAreDisjoint(normalized, _inner);

		_scopeRoots = normalized;
		File = new ScopedFile(_inner.File, _inner, _scopeRoots);
		FileInfo = new ScopedFileInfoFactory(_inner.FileInfo, _inner, _scopeRoots);
	}

	public IFile File { get; }
	public IFileInfoFactory FileInfo { get; }

	// ── Pass-through ──────────────────────────────────────────────────────────

	public IDirectory Directory => _inner.Directory;
	public IPath Path => _inner.Path;
	public IDirectoryInfoFactory DirectoryInfo => _inner.DirectoryInfo;
	public IDriveInfoFactory DriveInfo => _inner.DriveInfo;
	public IFileStreamFactory FileStream => _inner.FileStream;
	public IFileSystemWatcherFactory FileSystemWatcher => _inner.FileSystemWatcher;
	public IFileVersionInfoFactory FileVersionInfo => _inner.FileVersionInfo;
}
