// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information

#pragma warning disable CA1416

using System.IO.Abstractions;
using System.Runtime.Versioning;

namespace Nullean.ScopedFileSystem;

/// <summary>
/// An <see cref="IDirectoryInfo"/> decorator that validates all operations stay within the
/// configured scope roots and rejects symbolic links and hidden directories.
/// </summary>
internal class ScopedDirectoryInfo(IDirectoryInfo inner, IFileSystem innerFs, ValidationContext ctx) : IDirectoryInfo
{
	public IFileSystem FileSystem => innerFs;

	private void ValidateSelf() =>
		PathValidator.ValidateDirectoryPath(inner.FullName, ctx, innerFs);

	private void ValidateDest(string path) =>
		PathValidator.ValidateDirectoryPath(path, ctx, innerFs);

	// ── IFileSystemInfo ──────────────────────────────────────────────────────

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

	public void CreateAsSymbolicLink(string pathToTarget)
	{
		ValidateSelf();
		inner.CreateAsSymbolicLink(pathToTarget);
	}

	public void Delete()
	{
		ValidateSelf();
		inner.Delete();
	}

	public void Refresh() => inner.Refresh();

	public IFileSystemInfo? ResolveLinkTarget(bool returnFinalTarget) =>
		inner.ResolveLinkTarget(returnFinalTarget);

	// ── IDirectoryInfo ───────────────────────────────────────────────────────

	public IDirectoryInfo? Parent => inner.Parent is null ? null : new ScopedDirectoryInfo(inner.Parent, innerFs, ctx);

	public IDirectoryInfo Root => new ScopedDirectoryInfo(inner.Root, innerFs, ctx);

	public void Create()
	{
		ValidateSelf();
		inner.Create();
	}

	public IDirectoryInfo CreateSubdirectory(string path)
	{
		ValidateSelf();
		return new ScopedDirectoryInfo(inner.CreateSubdirectory(path), innerFs, ctx);
	}

	public void Delete(bool recursive)
	{
		ValidateSelf();
		inner.Delete(recursive);
	}

	public void MoveTo(string destDirName)
	{
		ValidateSelf();
		ValidateDest(destDirName);
		inner.MoveTo(destDirName);
	}

	// ── Enumerate: wrap returned infos in Scoped decorators ─────────────────

	public IEnumerable<IDirectoryInfo> EnumerateDirectories()
	{
		ValidateSelf();
		return inner.EnumerateDirectories().Select(d => new ScopedDirectoryInfo(d, innerFs, ctx));
	}

	public IEnumerable<IDirectoryInfo> EnumerateDirectories(string searchPattern)
	{
		ValidateSelf();
		return inner.EnumerateDirectories(searchPattern).Select(d => new ScopedDirectoryInfo(d, innerFs, ctx));
	}

	public IEnumerable<IDirectoryInfo> EnumerateDirectories(string searchPattern, SearchOption searchOption)
	{
		ValidateSelf();
		return inner.EnumerateDirectories(searchPattern, searchOption).Select(d => new ScopedDirectoryInfo(d, innerFs, ctx));
	}

	public IEnumerable<IDirectoryInfo> EnumerateDirectories(string searchPattern, EnumerationOptions enumerationOptions)
	{
		ValidateSelf();
		return inner.EnumerateDirectories(searchPattern, enumerationOptions).Select(d => new ScopedDirectoryInfo(d, innerFs, ctx));
	}

	public IEnumerable<IFileInfo> EnumerateFiles()
	{
		ValidateSelf();
		return inner.EnumerateFiles().Select(f => new ScopedFileInfo(f, innerFs, ctx));
	}

	public IEnumerable<IFileInfo> EnumerateFiles(string searchPattern)
	{
		ValidateSelf();
		return inner.EnumerateFiles(searchPattern).Select(f => new ScopedFileInfo(f, innerFs, ctx));
	}

	public IEnumerable<IFileInfo> EnumerateFiles(string searchPattern, SearchOption searchOption)
	{
		ValidateSelf();
		return inner.EnumerateFiles(searchPattern, searchOption).Select(f => new ScopedFileInfo(f, innerFs, ctx));
	}

	public IEnumerable<IFileInfo> EnumerateFiles(string searchPattern, EnumerationOptions enumerationOptions)
	{
		ValidateSelf();
		return inner.EnumerateFiles(searchPattern, enumerationOptions).Select(f => new ScopedFileInfo(f, innerFs, ctx));
	}

	public IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos()
	{
		ValidateSelf();
		return inner.EnumerateFileSystemInfos().Select(WrapFileSystemInfo);
	}

	public IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos(string searchPattern)
	{
		ValidateSelf();
		return inner.EnumerateFileSystemInfos(searchPattern).Select(WrapFileSystemInfo);
	}

	public IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos(string searchPattern, SearchOption searchOption)
	{
		ValidateSelf();
		return inner.EnumerateFileSystemInfos(searchPattern, searchOption).Select(WrapFileSystemInfo);
	}

	public IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos(string searchPattern, EnumerationOptions enumerationOptions)
	{
		ValidateSelf();
		return inner.EnumerateFileSystemInfos(searchPattern, enumerationOptions).Select(WrapFileSystemInfo);
	}

	public IDirectoryInfo[] GetDirectories()
	{
		ValidateSelf();
		return inner.GetDirectories().Select(d => (IDirectoryInfo)new ScopedDirectoryInfo(d, innerFs, ctx)).ToArray();
	}

	public IDirectoryInfo[] GetDirectories(string searchPattern)
	{
		ValidateSelf();
		return inner.GetDirectories(searchPattern).Select(d => (IDirectoryInfo)new ScopedDirectoryInfo(d, innerFs, ctx)).ToArray();
	}

	public IDirectoryInfo[] GetDirectories(string searchPattern, SearchOption searchOption)
	{
		ValidateSelf();
		return inner.GetDirectories(searchPattern, searchOption).Select(d => (IDirectoryInfo)new ScopedDirectoryInfo(d, innerFs, ctx)).ToArray();
	}

	public IDirectoryInfo[] GetDirectories(string searchPattern, EnumerationOptions enumerationOptions)
	{
		ValidateSelf();
		return inner.GetDirectories(searchPattern, enumerationOptions).Select(d => (IDirectoryInfo)new ScopedDirectoryInfo(d, innerFs, ctx)).ToArray();
	}

	public IFileInfo[] GetFiles()
	{
		ValidateSelf();
		return inner.GetFiles().Select(f => (IFileInfo)new ScopedFileInfo(f, innerFs, ctx)).ToArray();
	}

	public IFileInfo[] GetFiles(string searchPattern)
	{
		ValidateSelf();
		return inner.GetFiles(searchPattern).Select(f => (IFileInfo)new ScopedFileInfo(f, innerFs, ctx)).ToArray();
	}

	public IFileInfo[] GetFiles(string searchPattern, SearchOption searchOption)
	{
		ValidateSelf();
		return inner.GetFiles(searchPattern, searchOption).Select(f => (IFileInfo)new ScopedFileInfo(f, innerFs, ctx)).ToArray();
	}

	public IFileInfo[] GetFiles(string searchPattern, EnumerationOptions enumerationOptions)
	{
		ValidateSelf();
		return inner.GetFiles(searchPattern, enumerationOptions).Select(f => (IFileInfo)new ScopedFileInfo(f, innerFs, ctx)).ToArray();
	}

	public IFileSystemInfo[] GetFileSystemInfos()
	{
		ValidateSelf();
		return inner.GetFileSystemInfos().Select(WrapFileSystemInfo).ToArray();
	}

	public IFileSystemInfo[] GetFileSystemInfos(string searchPattern)
	{
		ValidateSelf();
		return inner.GetFileSystemInfos(searchPattern).Select(WrapFileSystemInfo).ToArray();
	}

	public IFileSystemInfo[] GetFileSystemInfos(string searchPattern, SearchOption searchOption)
	{
		ValidateSelf();
		return inner.GetFileSystemInfos(searchPattern, searchOption).Select(WrapFileSystemInfo).ToArray();
	}

	public IFileSystemInfo[] GetFileSystemInfos(string searchPattern, EnumerationOptions enumerationOptions)
	{
		ValidateSelf();
		return inner.GetFileSystemInfos(searchPattern, enumerationOptions).Select(WrapFileSystemInfo).ToArray();
	}

	private IFileSystemInfo WrapFileSystemInfo(IFileSystemInfo info) => info switch
	{
		IFileInfo fi => new ScopedFileInfo(fi, innerFs, ctx),
		IDirectoryInfo di => new ScopedDirectoryInfo(di, innerFs, ctx),
		_ => info
	};
}
