// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information

// Platform-specific APIs are delegated directly to the inner IFile — suppress false positives.
#pragma warning disable CA1416

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif
#if NET6_0_OR_GREATER
using Microsoft.Win32.SafeHandles;
#endif

namespace Nullean.ScopedFileSystem;

/// <summary>
/// An <see cref="IFile"/> decorator that validates all read and write operations stay within the
/// configured scope roots and rejects symbolic links.
/// </summary>
internal class ScopedFile(IFile inner, IFileSystem innerFs, ValidationContext ctx) : IFile
{
	public IFileSystem FileSystem => innerFs;

	// ── Validation helpers ───────────────────────────────────────────────────

	private void Validate(string path) =>
		PathValidator.ValidatePath(path, ctx, innerFs);

	private bool InScope(string path) =>
		PathValidator.IsInScope(path, ctx, innerFs);

	// ── Scoped: Exists (graceful, no throw) ──────────────────────────────────

	public bool Exists(string? path) =>
		path != null && InScope(path) && inner.Exists(path);

	// ── Scoped: Read methods ─────────────────────────────────────────────────

	public string ReadAllText(string path)
	{
		Validate(path);
		return inner.ReadAllText(path);
	}

	public string ReadAllText(string path, Encoding encoding)
	{
		Validate(path);
		return inner.ReadAllText(path, encoding);
	}

#if !NETSTANDARD2_0
	public async Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default)
	{
		Validate(path);
		return await inner.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);
	}

	public async Task<string> ReadAllTextAsync(string path, Encoding encoding, CancellationToken cancellationToken = default)
	{
		Validate(path);
		return await inner.ReadAllTextAsync(path, encoding, cancellationToken).ConfigureAwait(false);
	}
#endif

	public byte[] ReadAllBytes(string path)
	{
		Validate(path);
		return inner.ReadAllBytes(path);
	}

#if !NETSTANDARD2_0
	public async Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken = default)
	{
		Validate(path);
		return await inner.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
	}
#endif

	public string[] ReadAllLines(string path)
	{
		Validate(path);
		return inner.ReadAllLines(path);
	}

	public string[] ReadAllLines(string path, Encoding encoding)
	{
		Validate(path);
		return inner.ReadAllLines(path, encoding);
	}

#if !NETSTANDARD2_0
	public async Task<string[]> ReadAllLinesAsync(string path, CancellationToken cancellationToken = default)
	{
		Validate(path);
		return await inner.ReadAllLinesAsync(path, cancellationToken).ConfigureAwait(false);
	}

	public async Task<string[]> ReadAllLinesAsync(string path, Encoding encoding, CancellationToken cancellationToken = default)
	{
		Validate(path);
		return await inner.ReadAllLinesAsync(path, encoding, cancellationToken).ConfigureAwait(false);
	}
#endif

	public IEnumerable<string> ReadLines(string path)
	{
		Validate(path);
		return inner.ReadLines(path);
	}

	public IEnumerable<string> ReadLines(string path, Encoding encoding)
	{
		Validate(path);
		return inner.ReadLines(path, encoding);
	}

#if NET8_0_OR_GREATER
	public IAsyncEnumerable<string> ReadLinesAsync(string path, CancellationToken cancellationToken = default)
	{
		Validate(path);
		return inner.ReadLinesAsync(path, cancellationToken);
	}

	public IAsyncEnumerable<string> ReadLinesAsync(string path, Encoding encoding, CancellationToken cancellationToken = default)
	{
		Validate(path);
		return inner.ReadLinesAsync(path, encoding, cancellationToken);
	}
#endif

	public FileSystemStream OpenRead(string path)
	{
		Validate(path);
		return inner.OpenRead(path);
	}

	public StreamReader OpenText(string path)
	{
		Validate(path);
		return inner.OpenText(path);
	}

	public FileSystemStream Open(string path, FileMode mode)
	{
		Validate(path);
		return inner.Open(path, mode);
	}

	public FileSystemStream Open(string path, FileMode mode, FileAccess access)
	{
		Validate(path);
		return inner.Open(path, mode, access);
	}

	public FileSystemStream Open(string path, FileMode mode, FileAccess access, FileShare share)
	{
		Validate(path);
		return inner.Open(path, mode, access, share);
	}

#if NET6_0_OR_GREATER
	public FileSystemStream Open(string path, FileStreamOptions options)
	{
		Validate(path);
		return inner.Open(path, options);
	}
#endif

	// ── Scoped: Write / mutate methods ───────────────────────────────────────

#if NET8_0_OR_GREATER
	public void AppendAllBytes(string path, byte[] bytes)
	{
		Validate(path);
		inner.AppendAllBytes(path, bytes);
	}
#endif

#if NET7_0_OR_GREATER
	public void AppendAllBytes(string path, ReadOnlySpan<byte> bytes)
	{
		Validate(path);
		inner.AppendAllBytes(path, bytes);
	}
#endif

#if NET8_0_OR_GREATER
	public Task AppendAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default)
	{
		Validate(path);
		return inner.AppendAllBytesAsync(path, bytes, cancellationToken);
	}
#endif

#if NET7_0_OR_GREATER
	public Task AppendAllBytesAsync(string path, ReadOnlyMemory<byte> bytes, CancellationToken cancellationToken = default)
	{
		Validate(path);
		return inner.AppendAllBytesAsync(path, bytes, cancellationToken);
	}
#endif

	public void AppendAllLines(string path, IEnumerable<string> contents)
	{
		Validate(path);
		inner.AppendAllLines(path, contents);
	}

	public void AppendAllLines(string path, IEnumerable<string> contents, Encoding encoding)
	{
		Validate(path);
		inner.AppendAllLines(path, contents, encoding);
	}

#if !NETSTANDARD2_0
	public Task AppendAllLinesAsync(string path, IEnumerable<string> contents, CancellationToken cancellationToken = default)
	{
		Validate(path);
		return inner.AppendAllLinesAsync(path, contents, cancellationToken);
	}

	public Task AppendAllLinesAsync(string path, IEnumerable<string> contents, Encoding encoding, CancellationToken cancellationToken = default)
	{
		Validate(path);
		return inner.AppendAllLinesAsync(path, contents, encoding, cancellationToken);
	}
#endif

	public void AppendAllText(string path, string? contents)
	{
		Validate(path);
		inner.AppendAllText(path, contents);
	}

#if NET7_0_OR_GREATER
	public void AppendAllText(string path, ReadOnlySpan<char> contents)
	{
		Validate(path);
		inner.AppendAllText(path, contents);
	}
#endif

	public void AppendAllText(string path, string? contents, Encoding encoding)
	{
		Validate(path);
		inner.AppendAllText(path, contents, encoding);
	}

#if NET7_0_OR_GREATER
	public void AppendAllText(string path, ReadOnlySpan<char> contents, Encoding encoding)
	{
		Validate(path);
		inner.AppendAllText(path, contents, encoding);
	}
#endif

#if !NETSTANDARD2_0
	public Task AppendAllTextAsync(string path, string? contents, CancellationToken cancellationToken = default)
	{
		Validate(path);
		return inner.AppendAllTextAsync(path, contents, cancellationToken);
	}
#endif

#if NET7_0_OR_GREATER
	public Task AppendAllTextAsync(string path, ReadOnlyMemory<char> contents, CancellationToken cancellationToken = default)
	{
		Validate(path);
		return inner.AppendAllTextAsync(path, contents, cancellationToken);
	}
#endif

#if !NETSTANDARD2_0
	public Task AppendAllTextAsync(string path, string? contents, Encoding encoding, CancellationToken cancellationToken = default)
	{
		Validate(path);
		return inner.AppendAllTextAsync(path, contents, encoding, cancellationToken);
	}
#endif

#if NET7_0_OR_GREATER
	public Task AppendAllTextAsync(string path, ReadOnlyMemory<char> contents, Encoding encoding, CancellationToken cancellationToken = default)
	{
		Validate(path);
		return inner.AppendAllTextAsync(path, contents, encoding, cancellationToken);
	}
#endif

	public StreamWriter AppendText(string path)
	{
		Validate(path);
		return inner.AppendText(path);
	}

	public void Copy(string sourceFileName, string destFileName)
	{
		Validate(sourceFileName);
		Validate(destFileName);
		inner.Copy(sourceFileName, destFileName);
	}

	public void Copy(string sourceFileName, string destFileName, bool overwrite)
	{
		Validate(sourceFileName);
		Validate(destFileName);
		inner.Copy(sourceFileName, destFileName, overwrite);
	}

	public FileSystemStream Create(string path)
	{
		Validate(path);
		return inner.Create(path);
	}

	public FileSystemStream Create(string path, int bufferSize)
	{
		Validate(path);
		return inner.Create(path, bufferSize);
	}

	public FileSystemStream Create(string path, int bufferSize, FileOptions options)
	{
		Validate(path);
		return inner.Create(path, bufferSize, options);
	}

#if NET6_0_OR_GREATER
	public IFileSystemInfo CreateSymbolicLink(string path, string pathToTarget)
	{
		Validate(path);
		return inner.CreateSymbolicLink(path, pathToTarget);
	}
#endif

	public StreamWriter CreateText(string path)
	{
		Validate(path);
		return inner.CreateText(path);
	}

#if NET5_0_OR_GREATER
	[SupportedOSPlatform("windows")]
#endif
	public void Decrypt(string path)
	{
		Validate(path);
		inner.Decrypt(path);
	}

	public void Delete(string path)
	{
		Validate(path);
		inner.Delete(path);
	}

#if NET5_0_OR_GREATER
	[SupportedOSPlatform("windows")]
#endif
	public void Encrypt(string path)
	{
		Validate(path);
		inner.Encrypt(path);
	}

	public FileAttributes GetAttributes(string path) => inner.GetAttributes(path);

#if NET6_0_OR_GREATER
	public FileAttributes GetAttributes(SafeFileHandle fileHandle) => inner.GetAttributes(fileHandle);
#endif

	public DateTime GetCreationTime(string path) => inner.GetCreationTime(path);

#if NET6_0_OR_GREATER
	public DateTime GetCreationTime(SafeFileHandle fileHandle) => inner.GetCreationTime(fileHandle);
#endif

	public DateTime GetCreationTimeUtc(string path) => inner.GetCreationTimeUtc(path);

#if NET6_0_OR_GREATER
	public DateTime GetCreationTimeUtc(SafeFileHandle fileHandle) => inner.GetCreationTimeUtc(fileHandle);
#endif

	public DateTime GetLastAccessTime(string path) => inner.GetLastAccessTime(path);

#if NET6_0_OR_GREATER
	public DateTime GetLastAccessTime(SafeFileHandle fileHandle) => inner.GetLastAccessTime(fileHandle);
#endif

	public DateTime GetLastAccessTimeUtc(string path) => inner.GetLastAccessTimeUtc(path);

#if NET6_0_OR_GREATER
	public DateTime GetLastAccessTimeUtc(SafeFileHandle fileHandle) => inner.GetLastAccessTimeUtc(fileHandle);
#endif

	public DateTime GetLastWriteTime(string path) => inner.GetLastWriteTime(path);

#if NET6_0_OR_GREATER
	public DateTime GetLastWriteTime(SafeFileHandle fileHandle) => inner.GetLastWriteTime(fileHandle);
#endif

	public DateTime GetLastWriteTimeUtc(string path) => inner.GetLastWriteTimeUtc(path);

#if NET6_0_OR_GREATER
	public DateTime GetLastWriteTimeUtc(SafeFileHandle fileHandle) => inner.GetLastWriteTimeUtc(fileHandle);
#endif

#if NET7_0_OR_GREATER
	public UnixFileMode GetUnixFileMode(string path) => inner.GetUnixFileMode(path);

	public UnixFileMode GetUnixFileMode(SafeFileHandle fileHandle) => inner.GetUnixFileMode(fileHandle);
#endif

	public void Move(string sourceFileName, string destFileName)
	{
		Validate(sourceFileName);
		Validate(destFileName);
		inner.Move(sourceFileName, destFileName);
	}

#if NET6_0_OR_GREATER
	public void Move(string sourceFileName, string destFileName, bool overwrite)
	{
		Validate(sourceFileName);
		Validate(destFileName);
		inner.Move(sourceFileName, destFileName, overwrite);
	}
#endif

	public FileSystemStream OpenWrite(string path)
	{
		Validate(path);
		return inner.OpenWrite(path);
	}

	public void Replace(string sourceFileName, string destinationFileName, string? destinationBackupFileName)
	{
		Validate(sourceFileName);
		Validate(destinationFileName);
		if (destinationBackupFileName != null)
			Validate(destinationBackupFileName);
		inner.Replace(sourceFileName, destinationFileName, destinationBackupFileName);
	}

	public void Replace(string sourceFileName, string destinationFileName, string? destinationBackupFileName, bool ignoreMetadataErrors)
	{
		Validate(sourceFileName);
		Validate(destinationFileName);
		if (destinationBackupFileName != null)
			Validate(destinationBackupFileName);
		inner.Replace(sourceFileName, destinationFileName, destinationBackupFileName, ignoreMetadataErrors);
	}

#if NET6_0_OR_GREATER
	public IFileSystemInfo? ResolveLinkTarget(string linkPath, bool returnFinalTarget) =>
		inner.ResolveLinkTarget(linkPath, returnFinalTarget);
#endif

	public void SetAttributes(string path, FileAttributes fileAttributes)
	{
		Validate(path);
		inner.SetAttributes(path, fileAttributes);
	}

#if NET6_0_OR_GREATER
	public void SetAttributes(SafeFileHandle fileHandle, FileAttributes fileAttributes) =>
		inner.SetAttributes(fileHandle, fileAttributes);
#endif

	public void SetCreationTime(string path, DateTime creationTime)
	{
		Validate(path);
		inner.SetCreationTime(path, creationTime);
	}

#if NET6_0_OR_GREATER
	public void SetCreationTime(SafeFileHandle fileHandle, DateTime creationTime) =>
		inner.SetCreationTime(fileHandle, creationTime);
#endif

	public void SetCreationTimeUtc(string path, DateTime creationTimeUtc)
	{
		Validate(path);
		inner.SetCreationTimeUtc(path, creationTimeUtc);
	}

#if NET6_0_OR_GREATER
	public void SetCreationTimeUtc(SafeFileHandle fileHandle, DateTime creationTimeUtc) =>
		inner.SetCreationTimeUtc(fileHandle, creationTimeUtc);
#endif

	public void SetLastAccessTime(string path, DateTime lastAccessTime)
	{
		Validate(path);
		inner.SetLastAccessTime(path, lastAccessTime);
	}

#if NET6_0_OR_GREATER
	public void SetLastAccessTime(SafeFileHandle fileHandle, DateTime lastAccessTime) =>
		inner.SetLastAccessTime(fileHandle, lastAccessTime);
#endif

	public void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc)
	{
		Validate(path);
		inner.SetLastAccessTimeUtc(path, lastAccessTimeUtc);
	}

#if NET6_0_OR_GREATER
	public void SetLastAccessTimeUtc(SafeFileHandle fileHandle, DateTime lastAccessTimeUtc) =>
		inner.SetLastAccessTimeUtc(fileHandle, lastAccessTimeUtc);
#endif

	public void SetLastWriteTime(string path, DateTime lastWriteTime)
	{
		Validate(path);
		inner.SetLastWriteTime(path, lastWriteTime);
	}

#if NET6_0_OR_GREATER
	public void SetLastWriteTime(SafeFileHandle fileHandle, DateTime lastWriteTime) =>
		inner.SetLastWriteTime(fileHandle, lastWriteTime);
#endif

	public void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
	{
		Validate(path);
		inner.SetLastWriteTimeUtc(path, lastWriteTimeUtc);
	}

#if NET6_0_OR_GREATER
	public void SetLastWriteTimeUtc(SafeFileHandle fileHandle, DateTime lastWriteTimeUtc) =>
		inner.SetLastWriteTimeUtc(fileHandle, lastWriteTimeUtc);
#endif

#if NET7_0_OR_GREATER
	public void SetUnixFileMode(string path, UnixFileMode mode)
	{
		Validate(path);
		inner.SetUnixFileMode(path, mode);
	}

	public void SetUnixFileMode(SafeFileHandle fileHandle, UnixFileMode mode) =>
		inner.SetUnixFileMode(fileHandle, mode);
#endif

	public void WriteAllBytes(string path, byte[] bytes)
	{
		Validate(path);
		inner.WriteAllBytes(path, bytes);
	}

#if NET7_0_OR_GREATER
	public void WriteAllBytes(string path, ReadOnlySpan<byte> bytes)
	{
		Validate(path);
		inner.WriteAllBytes(path, bytes);
	}
#endif

#if !NETSTANDARD2_0
	public Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default)
	{
		Validate(path);
		return inner.WriteAllBytesAsync(path, bytes, cancellationToken);
	}
#endif

#if NET7_0_OR_GREATER
	public Task WriteAllBytesAsync(string path, ReadOnlyMemory<byte> bytes, CancellationToken cancellationToken = default)
	{
		Validate(path);
		return inner.WriteAllBytesAsync(path, bytes, cancellationToken);
	}
#endif

	public void WriteAllLines(string path, string[] contents)
	{
		Validate(path);
		inner.WriteAllLines(path, contents);
	}

	public void WriteAllLines(string path, IEnumerable<string> contents)
	{
		Validate(path);
		inner.WriteAllLines(path, contents);
	}

	public void WriteAllLines(string path, string[] contents, Encoding encoding)
	{
		Validate(path);
		inner.WriteAllLines(path, contents, encoding);
	}

	public void WriteAllLines(string path, IEnumerable<string> contents, Encoding encoding)
	{
		Validate(path);
		inner.WriteAllLines(path, contents, encoding);
	}

#if !NETSTANDARD2_0
	public Task WriteAllLinesAsync(string path, IEnumerable<string> contents, CancellationToken cancellationToken = default)
	{
		Validate(path);
		return inner.WriteAllLinesAsync(path, contents, cancellationToken);
	}

	public Task WriteAllLinesAsync(string path, IEnumerable<string> contents, Encoding encoding, CancellationToken cancellationToken = default)
	{
		Validate(path);
		return inner.WriteAllLinesAsync(path, contents, encoding, cancellationToken);
	}
#endif

	public void WriteAllText(string path, string? contents)
	{
		Validate(path);
		inner.WriteAllText(path, contents);
	}

#if NET7_0_OR_GREATER
	public void WriteAllText(string path, ReadOnlySpan<char> contents)
	{
		Validate(path);
		inner.WriteAllText(path, contents);
	}
#endif

	public void WriteAllText(string path, string? contents, Encoding encoding)
	{
		Validate(path);
		inner.WriteAllText(path, contents, encoding);
	}

#if NET7_0_OR_GREATER
	public void WriteAllText(string path, ReadOnlySpan<char> contents, Encoding encoding)
	{
		Validate(path);
		inner.WriteAllText(path, contents, encoding);
	}
#endif

#if !NETSTANDARD2_0
	public Task WriteAllTextAsync(string path, string? contents, CancellationToken cancellationToken = default)
	{
		Validate(path);
		return inner.WriteAllTextAsync(path, contents, cancellationToken);
	}
#endif

#if NET7_0_OR_GREATER
	public Task WriteAllTextAsync(string path, ReadOnlyMemory<char> contents, CancellationToken cancellationToken = default)
	{
		Validate(path);
		return inner.WriteAllTextAsync(path, contents, cancellationToken);
	}
#endif

#if !NETSTANDARD2_0
	public Task WriteAllTextAsync(string path, string? contents, Encoding encoding, CancellationToken cancellationToken = default)
	{
		Validate(path);
		return inner.WriteAllTextAsync(path, contents, encoding, cancellationToken);
	}
#endif

#if NET7_0_OR_GREATER
	public Task WriteAllTextAsync(string path, ReadOnlyMemory<char> contents, Encoding encoding, CancellationToken cancellationToken = default)
	{
		Validate(path);
		return inner.WriteAllTextAsync(path, contents, encoding, cancellationToken);
	}
#endif
}
