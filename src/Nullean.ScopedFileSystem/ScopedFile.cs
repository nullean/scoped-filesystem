// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information

// Platform-specific APIs are delegated directly to the inner IFile — suppress false positives.
#pragma warning disable CA1416

using System.IO.Abstractions;
using System.Runtime.Versioning;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace Nullean.ScopedFileSystem;

/// <summary>
/// An <see cref="IFile"/> decorator that validates all read operations stay within the configured scope root
/// and rejects symlinks. Write and mutating operations pass through to the inner implementation.
/// </summary>
public class ScopedFile(IFile inner, IFileSystem innerFs, string scopeRoot) : IFile
{
	public IFileSystem FileSystem => innerFs;

	// ── Validation helpers ───────────────────────────────────────────────────

	private void Validate(string path) =>
		PathValidator.ValidateReadPath(path, scopeRoot, innerFs);

	private bool InScope(string path) =>
		PathValidator.IsInScope(path, scopeRoot, innerFs);

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

	public byte[] ReadAllBytes(string path)
	{
		Validate(path);
		return inner.ReadAllBytes(path);
	}

	public async Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken = default)
	{
		Validate(path);
		return await inner.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
	}

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
		// Validate for read-access modes
		if (IsReadMode(mode))
			Validate(path);
		return inner.Open(path, mode);
	}

	public FileSystemStream Open(string path, FileMode mode, FileAccess access)
	{
		if (access.HasFlag(FileAccess.Read))
			Validate(path);
		return inner.Open(path, mode, access);
	}

	public FileSystemStream Open(string path, FileMode mode, FileAccess access, FileShare share)
	{
		if (access.HasFlag(FileAccess.Read))
			Validate(path);
		return inner.Open(path, mode, access, share);
	}

	public FileSystemStream Open(string path, FileStreamOptions options)
	{
		if (options.Access.HasFlag(FileAccess.Read))
			Validate(path);
		return inner.Open(path, options);
	}

	// ── Pass-through: all write/mutate operations ─────────────────────────────

	public void AppendAllBytes(string path, byte[] bytes) =>
		inner.AppendAllBytes(path, bytes);

	public void AppendAllBytes(string path, ReadOnlySpan<byte> bytes) =>
		inner.AppendAllBytes(path, bytes);

	public Task AppendAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default) =>
		inner.AppendAllBytesAsync(path, bytes, cancellationToken);

	public Task AppendAllBytesAsync(string path, ReadOnlyMemory<byte> bytes, CancellationToken cancellationToken = default) =>
		inner.AppendAllBytesAsync(path, bytes, cancellationToken);

	public void AppendAllLines(string path, IEnumerable<string> contents) =>
		inner.AppendAllLines(path, contents);

	public void AppendAllLines(string path, IEnumerable<string> contents, Encoding encoding) =>
		inner.AppendAllLines(path, contents, encoding);

	public Task AppendAllLinesAsync(string path, IEnumerable<string> contents, CancellationToken cancellationToken = default) =>
		inner.AppendAllLinesAsync(path, contents, cancellationToken);

	public Task AppendAllLinesAsync(string path, IEnumerable<string> contents, Encoding encoding, CancellationToken cancellationToken = default) =>
		inner.AppendAllLinesAsync(path, contents, encoding, cancellationToken);

	public void AppendAllText(string path, string? contents) =>
		inner.AppendAllText(path, contents);

	public void AppendAllText(string path, ReadOnlySpan<char> contents) =>
		inner.AppendAllText(path, contents);

	public void AppendAllText(string path, string? contents, Encoding encoding) =>
		inner.AppendAllText(path, contents, encoding);

	public void AppendAllText(string path, ReadOnlySpan<char> contents, Encoding encoding) =>
		inner.AppendAllText(path, contents, encoding);

	public Task AppendAllTextAsync(string path, string? contents, CancellationToken cancellationToken = default) =>
		inner.AppendAllTextAsync(path, contents, cancellationToken);

	public Task AppendAllTextAsync(string path, ReadOnlyMemory<char> contents, CancellationToken cancellationToken = default) =>
		inner.AppendAllTextAsync(path, contents, cancellationToken);

	public Task AppendAllTextAsync(string path, string? contents, Encoding encoding, CancellationToken cancellationToken = default) =>
		inner.AppendAllTextAsync(path, contents, encoding, cancellationToken);

	public Task AppendAllTextAsync(string path, ReadOnlyMemory<char> contents, Encoding encoding, CancellationToken cancellationToken = default) =>
		inner.AppendAllTextAsync(path, contents, encoding, cancellationToken);

	public StreamWriter AppendText(string path) => inner.AppendText(path);

	public void Copy(string sourceFileName, string destFileName) =>
		inner.Copy(sourceFileName, destFileName);

	public void Copy(string sourceFileName, string destFileName, bool overwrite) =>
		inner.Copy(sourceFileName, destFileName, overwrite);

	public FileSystemStream Create(string path) => inner.Create(path);

	public FileSystemStream Create(string path, int bufferSize) => inner.Create(path, bufferSize);

	public FileSystemStream Create(string path, int bufferSize, FileOptions options) =>
		inner.Create(path, bufferSize, options);

	public IFileSystemInfo CreateSymbolicLink(string path, string pathToTarget) =>
		inner.CreateSymbolicLink(path, pathToTarget);

	public StreamWriter CreateText(string path) => inner.CreateText(path);

	[SupportedOSPlatform("windows")]
	public void Decrypt(string path) => inner.Decrypt(path);

	public void Delete(string path) => inner.Delete(path);

	[SupportedOSPlatform("windows")]
	public void Encrypt(string path) => inner.Encrypt(path);

	public FileAttributes GetAttributes(string path) => inner.GetAttributes(path);

	public FileAttributes GetAttributes(SafeFileHandle fileHandle) => inner.GetAttributes(fileHandle);

	public DateTime GetCreationTime(string path) => inner.GetCreationTime(path);

	public DateTime GetCreationTime(SafeFileHandle fileHandle) => inner.GetCreationTime(fileHandle);

	public DateTime GetCreationTimeUtc(string path) => inner.GetCreationTimeUtc(path);

	public DateTime GetCreationTimeUtc(SafeFileHandle fileHandle) => inner.GetCreationTimeUtc(fileHandle);

	public DateTime GetLastAccessTime(string path) => inner.GetLastAccessTime(path);

	public DateTime GetLastAccessTime(SafeFileHandle fileHandle) => inner.GetLastAccessTime(fileHandle);

	public DateTime GetLastAccessTimeUtc(string path) => inner.GetLastAccessTimeUtc(path);

	public DateTime GetLastAccessTimeUtc(SafeFileHandle fileHandle) => inner.GetLastAccessTimeUtc(fileHandle);

	public DateTime GetLastWriteTime(string path) => inner.GetLastWriteTime(path);

	public DateTime GetLastWriteTime(SafeFileHandle fileHandle) => inner.GetLastWriteTime(fileHandle);

	public DateTime GetLastWriteTimeUtc(string path) => inner.GetLastWriteTimeUtc(path);

	public DateTime GetLastWriteTimeUtc(SafeFileHandle fileHandle) => inner.GetLastWriteTimeUtc(fileHandle);

	public UnixFileMode GetUnixFileMode(string path) => inner.GetUnixFileMode(path);

	public UnixFileMode GetUnixFileMode(SafeFileHandle fileHandle) => inner.GetUnixFileMode(fileHandle);

	public void Move(string sourceFileName, string destFileName) =>
		inner.Move(sourceFileName, destFileName);

	public void Move(string sourceFileName, string destFileName, bool overwrite) =>
		inner.Move(sourceFileName, destFileName, overwrite);

	public FileSystemStream OpenWrite(string path) => inner.OpenWrite(path);

	public void Replace(string sourceFileName, string destinationFileName, string? destinationBackupFileName) =>
		inner.Replace(sourceFileName, destinationFileName, destinationBackupFileName);

	public void Replace(string sourceFileName, string destinationFileName, string? destinationBackupFileName, bool ignoreMetadataErrors) =>
		inner.Replace(sourceFileName, destinationFileName, destinationBackupFileName, ignoreMetadataErrors);

	public IFileSystemInfo? ResolveLinkTarget(string linkPath, bool returnFinalTarget) =>
		inner.ResolveLinkTarget(linkPath, returnFinalTarget);

	public void SetAttributes(string path, FileAttributes fileAttributes) =>
		inner.SetAttributes(path, fileAttributes);

	public void SetAttributes(SafeFileHandle fileHandle, FileAttributes fileAttributes) =>
		inner.SetAttributes(fileHandle, fileAttributes);

	public void SetCreationTime(string path, DateTime creationTime) =>
		inner.SetCreationTime(path, creationTime);

	public void SetCreationTime(SafeFileHandle fileHandle, DateTime creationTime) =>
		inner.SetCreationTime(fileHandle, creationTime);

	public void SetCreationTimeUtc(string path, DateTime creationTimeUtc) =>
		inner.SetCreationTimeUtc(path, creationTimeUtc);

	public void SetCreationTimeUtc(SafeFileHandle fileHandle, DateTime creationTimeUtc) =>
		inner.SetCreationTimeUtc(fileHandle, creationTimeUtc);

	public void SetLastAccessTime(string path, DateTime lastAccessTime) =>
		inner.SetLastAccessTime(path, lastAccessTime);

	public void SetLastAccessTime(SafeFileHandle fileHandle, DateTime lastAccessTime) =>
		inner.SetLastAccessTime(fileHandle, lastAccessTime);

	public void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc) =>
		inner.SetLastAccessTimeUtc(path, lastAccessTimeUtc);

	public void SetLastAccessTimeUtc(SafeFileHandle fileHandle, DateTime lastAccessTimeUtc) =>
		inner.SetLastAccessTimeUtc(fileHandle, lastAccessTimeUtc);

	public void SetLastWriteTime(string path, DateTime lastWriteTime) =>
		inner.SetLastWriteTime(path, lastWriteTime);

	public void SetLastWriteTime(SafeFileHandle fileHandle, DateTime lastWriteTime) =>
		inner.SetLastWriteTime(fileHandle, lastWriteTime);

	public void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc) =>
		inner.SetLastWriteTimeUtc(path, lastWriteTimeUtc);

	public void SetLastWriteTimeUtc(SafeFileHandle fileHandle, DateTime lastWriteTimeUtc) =>
		inner.SetLastWriteTimeUtc(fileHandle, lastWriteTimeUtc);

	public void SetUnixFileMode(string path, UnixFileMode mode) =>
		inner.SetUnixFileMode(path, mode);

	public void SetUnixFileMode(SafeFileHandle fileHandle, UnixFileMode mode) =>
		inner.SetUnixFileMode(fileHandle, mode);

	public void WriteAllBytes(string path, byte[] bytes) =>
		inner.WriteAllBytes(path, bytes);

	public void WriteAllBytes(string path, ReadOnlySpan<byte> bytes) =>
		inner.WriteAllBytes(path, bytes);

	public Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default) =>
		inner.WriteAllBytesAsync(path, bytes, cancellationToken);

	public Task WriteAllBytesAsync(string path, ReadOnlyMemory<byte> bytes, CancellationToken cancellationToken = default) =>
		inner.WriteAllBytesAsync(path, bytes, cancellationToken);

	public void WriteAllLines(string path, string[] contents) =>
		inner.WriteAllLines(path, contents);

	public void WriteAllLines(string path, IEnumerable<string> contents) =>
		inner.WriteAllLines(path, contents);

	public void WriteAllLines(string path, string[] contents, Encoding encoding) =>
		inner.WriteAllLines(path, contents, encoding);

	public void WriteAllLines(string path, IEnumerable<string> contents, Encoding encoding) =>
		inner.WriteAllLines(path, contents, encoding);

	public Task WriteAllLinesAsync(string path, IEnumerable<string> contents, CancellationToken cancellationToken = default) =>
		inner.WriteAllLinesAsync(path, contents, cancellationToken);

	public Task WriteAllLinesAsync(string path, IEnumerable<string> contents, Encoding encoding, CancellationToken cancellationToken = default) =>
		inner.WriteAllLinesAsync(path, contents, encoding, cancellationToken);

	public void WriteAllText(string path, string? contents) =>
		inner.WriteAllText(path, contents);

	public void WriteAllText(string path, ReadOnlySpan<char> contents) =>
		inner.WriteAllText(path, contents);

	public void WriteAllText(string path, string? contents, Encoding encoding) =>
		inner.WriteAllText(path, contents, encoding);

	public void WriteAllText(string path, ReadOnlySpan<char> contents, Encoding encoding) =>
		inner.WriteAllText(path, contents, encoding);

	public Task WriteAllTextAsync(string path, string? contents, CancellationToken cancellationToken = default) =>
		inner.WriteAllTextAsync(path, contents, cancellationToken);

	public Task WriteAllTextAsync(string path, ReadOnlyMemory<char> contents, CancellationToken cancellationToken = default) =>
		inner.WriteAllTextAsync(path, contents, cancellationToken);

	public Task WriteAllTextAsync(string path, string? contents, Encoding encoding, CancellationToken cancellationToken = default) =>
		inner.WriteAllTextAsync(path, contents, encoding, cancellationToken);

	public Task WriteAllTextAsync(string path, ReadOnlyMemory<char> contents, Encoding encoding, CancellationToken cancellationToken = default) =>
		inner.WriteAllTextAsync(path, contents, encoding, cancellationToken);

	// ── Private helpers ───────────────────────────────────────────────────────

	/// <summary>True when the FileMode implies read access (Open or OpenOrCreate without explicit write-only mode).</summary>
	private static bool IsReadMode(FileMode mode) =>
		mode is FileMode.Open or FileMode.OpenOrCreate;
}
