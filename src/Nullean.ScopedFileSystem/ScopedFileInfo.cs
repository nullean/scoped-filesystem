// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information

// Platform-specific APIs are delegated directly to the inner IFileInfo — suppress false positives.
#pragma warning disable CA1416

using System.IO.Abstractions;

namespace Nullean.ScopedFileSystem;

/// <summary>
/// An <see cref="IFileInfo"/> decorator that validates all read and write operations stay within
/// the scope roots and rejects symbolic links. All other members pass through to the inner instance.
/// </summary>
internal class ScopedFileInfo(IFileInfo inner, IFileSystem innerFs, ValidationContext ctx) : IFileInfo
{
	public IFileSystem FileSystem => innerFs;

	private void ValidateSelf() =>
		PathValidator.ValidatePath(inner.FullName, ctx, innerFs);

	private void ValidateDest(string path) =>
		PathValidator.ValidatePath(path, ctx, innerFs);

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

#if NET6_0_OR_GREATER
	public string? LinkTarget => inner.LinkTarget;
#endif

	public string Name => inner.Name;

#if NET7_0_OR_GREATER
	public UnixFileMode UnixFileMode
	{
		get => inner.UnixFileMode;
		set => inner.UnixFileMode = value;
	}
#endif

#if NET6_0_OR_GREATER
	public void CreateAsSymbolicLink(string pathToTarget)
	{
		ValidateSelf();
		inner.CreateAsSymbolicLink(pathToTarget);
	}
#endif

	public void Delete()
	{
		ValidateSelf();
		inner.Delete();
	}

	public void Refresh() => inner.Refresh();

#if NET6_0_OR_GREATER
	public IFileSystemInfo? ResolveLinkTarget(bool returnFinalTarget) =>
		inner.ResolveLinkTarget(returnFinalTarget);
#endif

	// ── IFileInfo ─────────────────────────────────────────────────────────────

	public IDirectoryInfo? Directory => inner.Directory;

	public string? DirectoryName => inner.DirectoryName;

	public bool IsReadOnly
	{
		get => inner.IsReadOnly;
		set => inner.IsReadOnly = value;
	}

	public long Length => inner.Length;

	public StreamWriter AppendText()
	{
		ValidateSelf();
		return inner.AppendText();
	}

	public IFileInfo CopyTo(string destFileName)
	{
		ValidateSelf();
		ValidateDest(destFileName);
		return inner.CopyTo(destFileName);
	}

	public IFileInfo CopyTo(string destFileName, bool overwrite)
	{
		ValidateSelf();
		ValidateDest(destFileName);
		return inner.CopyTo(destFileName, overwrite);
	}

	public FileSystemStream Create()
	{
		ValidateSelf();
		return inner.Create();
	}

	public StreamWriter CreateText()
	{
		ValidateSelf();
		return inner.CreateText();
	}

	public void Decrypt()
	{
		ValidateSelf();
		inner.Decrypt();
	}

	public void Encrypt()
	{
		ValidateSelf();
		inner.Encrypt();
	}

	public void MoveTo(string destFileName)
	{
		ValidateSelf();
		ValidateDest(destFileName);
		inner.MoveTo(destFileName);
	}

#if NET6_0_OR_GREATER
	public void MoveTo(string destFileName, bool overwrite)
	{
		ValidateSelf();
		ValidateDest(destFileName);
		inner.MoveTo(destFileName, overwrite);
	}
#endif

	public FileSystemStream Open(FileMode mode)
	{
		ValidateSelf();
		return inner.Open(mode);
	}

	public FileSystemStream Open(FileMode mode, FileAccess access)
	{
		ValidateSelf();
		return inner.Open(mode, access);
	}

	public FileSystemStream Open(FileMode mode, FileAccess access, FileShare share)
	{
		ValidateSelf();
		return inner.Open(mode, access, share);
	}

#if NET6_0_OR_GREATER
	public FileSystemStream Open(FileStreamOptions options)
	{
		ValidateSelf();
		return inner.Open(options);
	}
#endif

	/// <summary>Validates that this file is within the scope roots and is not a symlink before opening.</summary>
	public FileSystemStream OpenRead()
	{
		ValidateSelf();
		return inner.OpenRead();
	}

	public StreamReader OpenText()
	{
		ValidateSelf();
		return inner.OpenText();
	}

	public FileSystemStream OpenWrite()
	{
		ValidateSelf();
		return inner.OpenWrite();
	}

	public IFileInfo Replace(string destinationFileName, string? destinationBackupFileName)
	{
		ValidateSelf();
		ValidateDest(destinationFileName);
		if (destinationBackupFileName != null)
			ValidateDest(destinationBackupFileName);
		return inner.Replace(destinationFileName, destinationBackupFileName);
	}

	public IFileInfo Replace(string destinationFileName, string? destinationBackupFileName, bool ignoreMetadataErrors)
	{
		ValidateSelf();
		ValidateDest(destinationFileName);
		if (destinationBackupFileName != null)
			ValidateDest(destinationBackupFileName);
		return inner.Replace(destinationFileName, destinationBackupFileName, ignoreMetadataErrors);
	}
}
