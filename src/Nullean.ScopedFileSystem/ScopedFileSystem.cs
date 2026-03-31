// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;

namespace Nullean.ScopedFileSystem;

/// <summary>
/// An <see cref="IFileSystem"/> decorator that restricts file and directory read and write operations
/// to one or more configured scope directories and rejects symbolic links and hidden paths.
/// </summary>
/// <remarks>
/// <para>
/// For full configuration (hidden-path allowlists, special-folder permissions) use
/// <see cref="ScopedFileSystem(IFileSystem,ScopedFileSystemOptions)"/> or
/// <see cref="ScopedFileSystem(ScopedFileSystemOptions)"/>.
/// Convenience overloads are provided for the common single-root case.
/// </para>
/// <para>
/// <b>Root disjointness:</b> no root may be an ancestor of another. Nested roots throw
/// <see cref="ArgumentException"/> at construction time.
/// </para>
/// </remarks>
public class ScopedFileSystem : IFileSystem
{
	private readonly IFileSystem _inner;

	// ── Full-options constructors ─────────────────────────────────────────────

	/// <summary>
	/// Initialises a <see cref="ScopedFileSystem"/> with an explicit inner filesystem and full options.
	/// </summary>
	public ScopedFileSystem(IFileSystem inner, ScopedFileSystemOptions options)
		: this(inner, options.ScopeRoots, options.AllowedHiddenFileNames, options.AllowedHiddenFolderNames, options.AllowedSpecialFolders, options.VerboseExceptions) { }

	/// <summary>
	/// Initialises a <see cref="ScopedFileSystem"/> using <see cref="FileSystem"/> as the inner filesystem.
	/// </summary>
	public ScopedFileSystem(ScopedFileSystemOptions options)
		: this(new FileSystem(), options) { }

	// ── Convenience overloads ─────────────────────────────────────────────────

	/// <summary>Initialises a scoped filesystem rooted at <paramref name="scopeRoot"/>.</summary>
	public ScopedFileSystem(IFileSystem inner, IDirectoryInfo scopeRoot)
		: this(inner, new ScopedFileSystemOptions(scopeRoot.FullName)) { }

	/// <summary>Initialises a scoped filesystem rooted at <paramref name="scopeRoot"/> using the real filesystem.</summary>
	public ScopedFileSystem(IDirectoryInfo scopeRoot)
		: this(new FileSystem(), scopeRoot) { }

	/// <summary>Initialises a scoped filesystem with one or more explicit scope roots.</summary>
	/// <exception cref="ArgumentException">Thrown when fewer than one root is supplied or any root is an ancestor of another.</exception>
	public ScopedFileSystem(IFileSystem inner, params string[] scopeRoots)
		: this(inner, new ScopedFileSystemOptions(scopeRoots)) { }

	/// <summary>Initialises a scoped filesystem with one or more scope roots using the real filesystem.</summary>
	/// <exception cref="ArgumentException">Thrown when fewer than one root is supplied or any root is an ancestor of another.</exception>
	public ScopedFileSystem(params string[] scopeRoots)
		: this(new FileSystem(), scopeRoots) { }

	// ── Core constructor ──────────────────────────────────────────────────────

	private ScopedFileSystem(
		IFileSystem inner,
		IReadOnlyList<string> scopeRoots,
		IReadOnlyCollection<string> allowedHiddenFileNames,
		IReadOnlyCollection<string> allowedHiddenFolderNames,
		AllowedSpecialFolder allowedSpecialFolders,
		bool verboseExceptions = false)
	{
		_inner = inner;

		var normalized = scopeRoots
			.Select(r => _inner.Path.GetFullPath(r)
				.TrimEnd(_inner.Path.DirectorySeparatorChar, _inner.Path.AltDirectorySeparatorChar))
			.ToArray();

		PathValidator.ValidateRootsAreDisjoint(normalized, _inner);

		var ctx = new ValidationContext(
			NormalizedRoots: normalized,
			ResolvedSpecialFolderPaths: ValidationContext.ResolveSpecialFolderPaths(allowedSpecialFolders),
			AllowedHiddenFileNames: ValidationContext.ToAllowSet(allowedHiddenFileNames),
			AllowedHiddenFolderNames: ValidationContext.ToAllowSet(allowedHiddenFolderNames),
			VerboseExceptions: verboseExceptions
		);

		File = new ScopedFile(_inner.File, _inner, ctx);
		FileInfo = new ScopedFileInfoFactory(_inner.FileInfo, _inner, ctx);
		Directory = new ScopedDirectory(_inner.Directory, _inner, ctx);
		DirectoryInfo = new ScopedDirectoryInfoFactory(_inner.DirectoryInfo, _inner, ctx);
	}

	public IFile File { get; }
	public IFileInfoFactory FileInfo { get; }
	public IDirectory Directory { get; }
	public IDirectoryInfoFactory DirectoryInfo { get; }

	// ── Pass-through ──────────────────────────────────────────────────────────

	public IPath Path => _inner.Path;
	public IDriveInfoFactory DriveInfo => _inner.DriveInfo;
	public IFileStreamFactory FileStream => _inner.FileStream;
	public IFileSystemWatcherFactory FileSystemWatcher => _inner.FileSystemWatcher;
	public IFileVersionInfoFactory FileVersionInfo => _inner.FileVersionInfo;
}
