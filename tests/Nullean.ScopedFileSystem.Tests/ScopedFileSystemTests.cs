// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information

using System.IO.Abstractions.TestingHelpers;
using Nullean.ScopedFileSystem;
using Xunit;

namespace Nullean.ScopedFileSystem.Tests;

public class ScopedFileSystemTests
{
	private static (MockFileSystem MockFs, ScopedFileSystem Scoped) CreateScoped(string scopeRoot = "/docs")
	{
		var mockFs = new MockFileSystem();
		mockFs.AddDirectory(scopeRoot);
		var scoped = new ScopedFileSystem(mockFs, scopeRoot);
		return (mockFs, scoped);
	}

	[Fact]
	public void ReadAllText_FileWithinScope_ReturnsContent()
	{
		var (mockFs, scoped) = CreateScoped();
		mockFs.AddFile("/docs/readme.md", new MockFileData("hello"));

		var content = scoped.File.ReadAllText("/docs/readme.md");

		Assert.Equal("hello", content);
	}

	[Fact]
	public void ReadAllText_FileOutsideScope_ThrowsScopedFileSystemException()
	{
		var (mockFs, scoped) = CreateScoped();
		mockFs.AddFile("/etc/passwd", new MockFileData("secret"));

		Assert.Throws<ScopedFileSystemException>(() => scoped.File.ReadAllText("/etc/passwd"));
	}

	[Fact]
	public void ReadAllText_PathTraversal_ThrowsScopedFileSystemException()
	{
		var (mockFs, scoped) = CreateScoped();
		mockFs.AddFile("/etc/passwd", new MockFileData("secret"));

		// /docs/../etc/passwd normalizes to /etc/passwd — outside scope
		Assert.Throws<ScopedFileSystemException>(() => scoped.File.ReadAllText("/docs/../etc/passwd"));
	}

	[Fact]
	public void Exists_FileWithinScope_ReturnsTrue()
	{
		var (mockFs, scoped) = CreateScoped();
		mockFs.AddFile("/docs/readme.md", new MockFileData("hello"));

		Assert.True(scoped.File.Exists("/docs/readme.md"));
	}

	[Fact]
	public void Exists_FileOutsideScope_ReturnsFalse()
	{
		var (mockFs, scoped) = CreateScoped();
		mockFs.AddFile("/etc/passwd", new MockFileData("secret"));

		// Must return false without throwing
		Assert.False(scoped.File.Exists("/etc/passwd"));
	}

	[Fact]
	public void ReadAllText_SymlinkedFile_ThrowsScopedFileSystemException()
	{
		var (mockFs, scoped) = CreateScoped();
		var symlinkData = new MockFileData("symlink content") { LinkTarget = "/etc/passwd" };
		mockFs.AddFile("/docs/link.md", symlinkData);

		Assert.Throws<ScopedFileSystemException>(() => scoped.File.ReadAllText("/docs/link.md"));
	}

	[Fact]
	public async Task ReadAllTextAsync_FileOutsideScope_ThrowsScopedFileSystemException()
	{
		var (mockFs, scoped) = CreateScoped();
		mockFs.AddFile("/etc/passwd", new MockFileData("secret"));

		await Assert.ThrowsAsync<ScopedFileSystemException>(
			() => scoped.File.ReadAllTextAsync("/etc/passwd")
		);
	}

	[Fact]
	public void ReadAllBytes_FileOutsideScope_ThrowsScopedFileSystemException()
	{
		var (mockFs, scoped) = CreateScoped();
		mockFs.AddFile("/etc/passwd", new MockFileData("secret"));

		Assert.Throws<ScopedFileSystemException>(() => scoped.File.ReadAllBytes("/etc/passwd"));
	}

	[Fact]
	public void OpenRead_FileOutsideScope_ThrowsScopedFileSystemException()
	{
		var (mockFs, scoped) = CreateScoped();
		mockFs.AddFile("/etc/passwd", new MockFileData("secret"));

		Assert.Throws<ScopedFileSystemException>(() => scoped.File.OpenRead("/etc/passwd"));
	}

	[Fact]
	public void WriteAllText_FileOutsideScope_DoesNotThrow()
	{
		var (mockFs, scoped) = CreateScoped();
		// Create the target directory so MockFileSystem doesn't throw DirectoryNotFoundException
		mockFs.AddDirectory("/etc");

		// Write operations pass through unrestricted — no ScopedFileSystemException expected
		var exception = Record.Exception(() => scoped.File.WriteAllText("/etc/new-file.txt", "content"));

		Assert.Null(exception);
		Assert.Equal("content", mockFs.File.ReadAllText("/etc/new-file.txt"));
	}

	[Fact]
	public void ScopeRoot_Itself_IsWithinScope()
	{
		var (mockFs, scoped) = CreateScoped("/docs");
		mockFs.AddFile("/docs/top-level.md", new MockFileData("top level content"));

		var content = scoped.File.ReadAllText("/docs/top-level.md");

		Assert.Equal("top level content", content);
	}

	[Fact]
	public void ReadAllLines_FileWithinScope_ReturnsLines()
	{
		var (mockFs, scoped) = CreateScoped();
		mockFs.AddFile("/docs/lines.txt", new MockFileData("line1\nline2\nline3"));

		var lines = scoped.File.ReadAllLines("/docs/lines.txt");

		Assert.Equal(3, lines.Length);
		Assert.Equal("line1", lines[0]);
	}

	[Fact]
	public void ReadAllLines_FileOutsideScope_ThrowsScopedFileSystemException()
	{
		var (mockFs, scoped) = CreateScoped();
		mockFs.AddFile("/etc/hosts", new MockFileData("127.0.0.1 localhost"));

		Assert.Throws<ScopedFileSystemException>(() => scoped.File.ReadAllLines("/etc/hosts"));
	}

	[Fact]
	public void FileInfoFactory_OpenRead_FileOutsideScope_ThrowsScopedFileSystemException()
	{
		var (mockFs, scoped) = CreateScoped();
		mockFs.AddFile("/etc/passwd", new MockFileData("secret"));

		var fileInfo = scoped.FileInfo.New("/etc/passwd");

		Assert.Throws<ScopedFileSystemException>(() => fileInfo.OpenRead());
	}

	[Fact]
	public void FileInfoFactory_OpenRead_FileWithinScope_Succeeds()
	{
		var (mockFs, scoped) = CreateScoped();
		mockFs.AddFile("/docs/readme.md", new MockFileData("hello"));

		var fileInfo = scoped.FileInfo.New("/docs/readme.md");

		using var stream = fileInfo.OpenRead();
		Assert.NotNull(stream);
	}

	[Fact]
	public void PassThrough_DirectoryOperations_AreUnrestricted()
	{
		var (mockFs, scoped) = CreateScoped();

		// Directory operations are passed through without scope enforcement
		var exception = Record.Exception(() => scoped.Directory.CreateDirectory("/outside/new-dir"));

		Assert.Null(exception);
		Assert.True(mockFs.Directory.Exists("/outside/new-dir"));
	}
}
