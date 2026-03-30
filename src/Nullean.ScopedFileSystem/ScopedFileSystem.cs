// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information

using System.IO.Abstractions;

namespace Nullean.ScopedFileSystem;

/// <summary>
/// An <see cref="IFileSystem"/> decorator that restricts file reads to a configured scope directory
/// and rejects symlinks. All other operations (writes, directory ops) pass through to the inner filesystem.
/// </summary>
public class ScopedFileSystem : IFileSystem
{
	private readonly IFileSystem _inner;
	private readonly string _scopeRoot;

	public ScopedFileSystem(IFileSystem inner, string scopeRoot)
	{
		_inner = inner;
		// Normalize to absolute, platform-canonical path with no trailing separator
		_scopeRoot = System.IO.Path.GetFullPath(scopeRoot)
			.TrimEnd(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);

		File = new ScopedFile(_inner.File, _inner, _scopeRoot);
		FileInfo = new ScopedFileInfoFactory(_inner.FileInfo, _inner, _scopeRoot);
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
