// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information

#pragma warning disable CA1416

using System.IO.Abstractions;
using System.Runtime.Versioning;

namespace Nullean.ScopedFileSystem;

/// <summary>
/// An <see cref="IDirectory"/> decorator that validates all operations stay within the
/// configured scope roots and rejects symbolic links and hidden directories.
/// </summary>
internal class ScopedDirectory(IDirectory inner, IFileSystem innerFs, ValidationContext ctx) : IDirectory
{
	public IFileSystem FileSystem => innerFs;

	private void Validate(string path) =>
		PathValidator.ValidateDirectoryPath(path, ctx, innerFs);

	private bool InScope(string path) =>
		PathValidator.IsInScope(path, ctx, innerFs);

	// ── Exists (graceful) ────────────────────────────────────────────────────

	public bool Exists([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] string? path) =>
		path != null && InScope(path) && inner.Exists(path);

	// ── Create / Delete / Move ───────────────────────────────────────────────

	public IDirectoryInfo CreateDirectory(string path)
	{
		Validate(path);
		return inner.CreateDirectory(path);
	}

#if NET7_0_OR_GREATER
	public IDirectoryInfo CreateDirectory(string path, UnixFileMode unixCreateMode)
	{
		Validate(path);
		return inner.CreateDirectory(path, unixCreateMode);
	}
#endif

#if NET6_0_OR_GREATER
	public IFileSystemInfo CreateSymbolicLink(string path, string pathToTarget)
	{
		Validate(path);
		return inner.CreateSymbolicLink(path, pathToTarget);
	}
#endif

#if NET7_0_OR_GREATER
	public IDirectoryInfo? CreateTempSubdirectory(string? prefix = null) =>
		inner.CreateTempSubdirectory(prefix);
#endif

	public void Delete(string path)
	{
		Validate(path);
		inner.Delete(path);
	}

	public void Delete(string path, bool recursive)
	{
		Validate(path);
		inner.Delete(path, recursive);
	}

	public void Move(string sourceDirName, string destDirName)
	{
		Validate(sourceDirName);
		Validate(destDirName);
		inner.Move(sourceDirName, destDirName);
	}

	// ── Enumerate / Get ──────────────────────────────────────────────────────

	public IEnumerable<string> EnumerateDirectories(string path)
	{
		Validate(path);
		return inner.EnumerateDirectories(path);
	}

	public IEnumerable<string> EnumerateDirectories(string path, string searchPattern)
	{
		Validate(path);
		return inner.EnumerateDirectories(path, searchPattern);
	}

	public IEnumerable<string> EnumerateDirectories(string path, string searchPattern, SearchOption searchOption)
	{
		Validate(path);
		return inner.EnumerateDirectories(path, searchPattern, searchOption);
	}

#if !NETSTANDARD2_0
	public IEnumerable<string> EnumerateDirectories(string path, string searchPattern, EnumerationOptions enumerationOptions)
	{
		Validate(path);
		return inner.EnumerateDirectories(path, searchPattern, enumerationOptions);
	}
#endif

	public IEnumerable<string> EnumerateFiles(string path)
	{
		Validate(path);
		return inner.EnumerateFiles(path);
	}

	public IEnumerable<string> EnumerateFiles(string path, string searchPattern)
	{
		Validate(path);
		return inner.EnumerateFiles(path, searchPattern);
	}

	public IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption)
	{
		Validate(path);
		return inner.EnumerateFiles(path, searchPattern, searchOption);
	}

#if !NETSTANDARD2_0
	public IEnumerable<string> EnumerateFiles(string path, string searchPattern, EnumerationOptions enumerationOptions)
	{
		Validate(path);
		return inner.EnumerateFiles(path, searchPattern, enumerationOptions);
	}
#endif

	public IEnumerable<string> EnumerateFileSystemEntries(string path)
	{
		Validate(path);
		return inner.EnumerateFileSystemEntries(path);
	}

	public IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern)
	{
		Validate(path);
		return inner.EnumerateFileSystemEntries(path, searchPattern);
	}

	public IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern, SearchOption searchOption)
	{
		Validate(path);
		return inner.EnumerateFileSystemEntries(path, searchPattern, searchOption);
	}

#if !NETSTANDARD2_0
	public IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern, EnumerationOptions enumerationOptions)
	{
		Validate(path);
		return inner.EnumerateFileSystemEntries(path, searchPattern, enumerationOptions);
	}
#endif

	public string[] GetDirectories(string path)
	{
		Validate(path);
		return inner.GetDirectories(path);
	}

	public string[] GetDirectories(string path, string searchPattern)
	{
		Validate(path);
		return inner.GetDirectories(path, searchPattern);
	}

	public string[] GetDirectories(string path, string searchPattern, SearchOption searchOption)
	{
		Validate(path);
		return inner.GetDirectories(path, searchPattern, searchOption);
	}

#if !NETSTANDARD2_0
	public string[] GetDirectories(string path, string searchPattern, EnumerationOptions enumerationOptions)
	{
		Validate(path);
		return inner.GetDirectories(path, searchPattern, enumerationOptions);
	}
#endif

	public string[] GetFiles(string path)
	{
		Validate(path);
		return inner.GetFiles(path);
	}

	public string[] GetFiles(string path, string searchPattern)
	{
		Validate(path);
		return inner.GetFiles(path, searchPattern);
	}

	public string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
	{
		Validate(path);
		return inner.GetFiles(path, searchPattern, searchOption);
	}

#if !NETSTANDARD2_0
	public string[] GetFiles(string path, string searchPattern, EnumerationOptions enumerationOptions)
	{
		Validate(path);
		return inner.GetFiles(path, searchPattern, enumerationOptions);
	}
#endif

	public string[] GetFileSystemEntries(string path)
	{
		Validate(path);
		return inner.GetFileSystemEntries(path);
	}

	public string[] GetFileSystemEntries(string path, string searchPattern)
	{
		Validate(path);
		return inner.GetFileSystemEntries(path, searchPattern);
	}

	public string[] GetFileSystemEntries(string path, string searchPattern, SearchOption searchOption)
	{
		Validate(path);
		return inner.GetFileSystemEntries(path, searchPattern, searchOption);
	}

#if !NETSTANDARD2_0
	public string[] GetFileSystemEntries(string path, string searchPattern, EnumerationOptions enumerationOptions)
	{
		Validate(path);
		return inner.GetFileSystemEntries(path, searchPattern, enumerationOptions);
	}
#endif

	// ── Metadata (read) ──────────────────────────────────────────────────────

	public DateTime GetCreationTime(string path)
	{
		Validate(path);
		return inner.GetCreationTime(path);
	}

	public DateTime GetCreationTimeUtc(string path)
	{
		Validate(path);
		return inner.GetCreationTimeUtc(path);
	}

	public DateTime GetLastAccessTime(string path)
	{
		Validate(path);
		return inner.GetLastAccessTime(path);
	}

	public DateTime GetLastAccessTimeUtc(string path)
	{
		Validate(path);
		return inner.GetLastAccessTimeUtc(path);
	}

	public DateTime GetLastWriteTime(string path)
	{
		Validate(path);
		return inner.GetLastWriteTime(path);
	}

	public DateTime GetLastWriteTimeUtc(string path)
	{
		Validate(path);
		return inner.GetLastWriteTimeUtc(path);
	}

	// ── Metadata (write) ─────────────────────────────────────────────────────

	public void SetCreationTime(string path, DateTime creationTime)
	{
		Validate(path);
		inner.SetCreationTime(path, creationTime);
	}

	public void SetCreationTimeUtc(string path, DateTime creationTimeUtc)
	{
		Validate(path);
		inner.SetCreationTimeUtc(path, creationTimeUtc);
	}

	public void SetLastAccessTime(string path, DateTime lastAccessTime)
	{
		Validate(path);
		inner.SetLastAccessTime(path, lastAccessTime);
	}

	public void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc)
	{
		Validate(path);
		inner.SetLastAccessTimeUtc(path, lastAccessTimeUtc);
	}

	public void SetLastWriteTime(string path, DateTime lastWriteTime)
	{
		Validate(path);
		inner.SetLastWriteTime(path, lastWriteTime);
	}

	public void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
	{
		Validate(path);
		inner.SetLastWriteTimeUtc(path, lastWriteTimeUtc);
	}

	// ── Pass-through: system-level / no path to validate ────────────────────

	public string GetCurrentDirectory() => inner.GetCurrentDirectory();

	public void SetCurrentDirectory(string path) => inner.SetCurrentDirectory(path);

	public string[] GetLogicalDrives() => inner.GetLogicalDrives();

	public string? GetDirectoryRoot(string path) => inner.GetDirectoryRoot(path);

	public IDirectoryInfo? GetParent(string path) => inner.GetParent(path);

#if NET6_0_OR_GREATER
	public IFileSystemInfo? ResolveLinkTarget(string linkPath, bool returnFinalTarget) =>
		inner.ResolveLinkTarget(linkPath, returnFinalTarget);
#endif
}
