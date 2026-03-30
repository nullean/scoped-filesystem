// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information

// Platform-specific APIs are delegated directly to the inner IFileInfo — suppress false positives.
#pragma warning disable CA1416

using System.IO.Abstractions;

namespace Nullean.ScopedFileSystem;

/// <summary>
/// An <see cref="IFileInfo"/> decorator that validates <see cref="OpenRead"/> stays within the scope root
/// and is not a symlink. All other members pass through to the inner instance.
/// </summary>
public class ScopedFileInfo(IFileInfo inner, IFileSystem innerFs, string scopeRoot) : IFileInfo
{
	public IFileSystem FileSystem => innerFs;

	// ── IFileSystemInfo pass-through ─────────────────────────────────────────

	public FileAttributes Attributes
	{
		get => inner.Attributes;
		set => inner.Attributes = value;
	}

	public DateTime CreationTime
	{
		get => inner.CreationTime;
		set => inner.CreationTime = value;
	}

	public DateTime CreationTimeUtc
	{
		get => inner.CreationTimeUtc;
		set => inner.CreationTimeUtc = value;
	}

	public bool Exists => inner.Exists;

	public string Extension => inner.Extension;

	public string FullName => inner.FullName;

	public DateTime LastAccessTime
	{
		get => inner.LastAccessTime;
		set => inner.LastAccessTime = value;
	}

	public DateTime LastAccessTimeUtc
	{
		get => inner.LastAccessTimeUtc;
		set => inner.LastAccessTimeUtc = value;
	}

	public DateTime LastWriteTime
	{
		get => inner.LastWriteTime;
		set => inner.LastWriteTime = value;
	}

	public DateTime LastWriteTimeUtc
	{
		get => inner.LastWriteTimeUtc;
		set => inner.LastWriteTimeUtc = value;
	}

	public string? LinkTarget => inner.LinkTarget;

	public string Name => inner.Name;

	public UnixFileMode UnixFileMode
	{
		get => inner.UnixFileMode;
		set => inner.UnixFileMode = value;
	}

	public void CreateAsSymbolicLink(string pathToTarget) =>
		inner.CreateAsSymbolicLink(pathToTarget);

	public void Delete() => inner.Delete();

	public void Refresh() => inner.Refresh();

	public IFileSystemInfo? ResolveLinkTarget(bool returnFinalTarget) =>
		inner.ResolveLinkTarget(returnFinalTarget);

	// ── IFileInfo pass-through ────────────────────────────────────────────────

	public IDirectoryInfo? Directory => inner.Directory;

	public string? DirectoryName => inner.DirectoryName;

	public bool IsReadOnly
	{
		get => inner.IsReadOnly;
		set => inner.IsReadOnly = value;
	}

	public long Length => inner.Length;

	public StreamWriter AppendText() => inner.AppendText();

	public IFileInfo CopyTo(string destFileName) => inner.CopyTo(destFileName);

	public IFileInfo CopyTo(string destFileName, bool overwrite) => inner.CopyTo(destFileName, overwrite);

	public FileSystemStream Create() => inner.Create();

	public StreamWriter CreateText() => inner.CreateText();

	public void Decrypt() => inner.Decrypt();

	public void Encrypt() => inner.Encrypt();

	public void MoveTo(string destFileName) => inner.MoveTo(destFileName);

	public void MoveTo(string destFileName, bool overwrite) => inner.MoveTo(destFileName, overwrite);

	public FileSystemStream Open(FileMode mode) => inner.Open(mode);

	public FileSystemStream Open(FileMode mode, FileAccess access) => inner.Open(mode, access);

	public FileSystemStream Open(FileMode mode, FileAccess access, FileShare share) =>
		inner.Open(mode, access, share);

	public FileSystemStream Open(FileStreamOptions options) => inner.Open(options);

	/// <summary>Validates that this file is within the scope root and is not a symlink before opening.</summary>
	public FileSystemStream OpenRead()
	{
		PathValidator.ValidateReadPath(inner.FullName, scopeRoot, innerFs);
		return inner.OpenRead();
	}

	public StreamReader OpenText() => inner.OpenText();

	public FileSystemStream OpenWrite() => inner.OpenWrite();

	public IFileInfo Replace(string destinationFileName, string? destinationBackupFileName) =>
		inner.Replace(destinationFileName, destinationBackupFileName);

	public IFileInfo Replace(string destinationFileName, string? destinationBackupFileName, bool ignoreMetadataErrors) =>
		inner.Replace(destinationFileName, destinationBackupFileName, ignoreMetadataErrors);
}
