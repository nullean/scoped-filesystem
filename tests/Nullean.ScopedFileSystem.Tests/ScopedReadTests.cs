// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information

using System.IO.Abstractions.TestingHelpers;
using Xunit;

namespace Nullean.ScopedFileSystem.Tests;

public class ScopedReadTests
{
	[Fact]
	public void ReadAllText_FileWithinScope_ReturnsContent()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/docs/readme.md", new MockFileData("hello"));

		Assert.Equal("hello", scoped.File.ReadAllText("/docs/readme.md"));
	}

	[Fact]
	public void ReadAllText_FileOutsideScope_Throws()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/etc/passwd", new MockFileData("secret"));

		Assert.Throws<ScopedFileSystemException>(() => scoped.File.ReadAllText("/etc/passwd"));
	}

	[Fact]
	public void ReadAllBytes_FileOutsideScope_Throws()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/etc/passwd", new MockFileData("secret"));

		Assert.Throws<ScopedFileSystemException>(() => scoped.File.ReadAllBytes("/etc/passwd"));
	}

	[Fact]
	public void ReadAllLines_FileWithinScope_ReturnsLines()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/docs/lines.txt", new MockFileData("line1\nline2\nline3"));

		var lines = scoped.File.ReadAllLines("/docs/lines.txt");

		Assert.Equal(3, lines.Length);
		Assert.Equal("line1", lines[0]);
	}

	[Fact]
	public void ReadAllLines_FileOutsideScope_Throws()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/etc/hosts", new MockFileData("127.0.0.1 localhost"));

		Assert.Throws<ScopedFileSystemException>(() => scoped.File.ReadAllLines("/etc/hosts"));
	}

	[Fact]
	public void OpenRead_FileOutsideScope_Throws()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/etc/passwd", new MockFileData("secret"));

		Assert.Throws<ScopedFileSystemException>(() => scoped.File.OpenRead("/etc/passwd"));
	}

	[Fact]
	public void OpenText_FileOutsideScope_Throws()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/etc/passwd", new MockFileData("secret"));

		Assert.Throws<ScopedFileSystemException>(() => scoped.File.OpenText("/etc/passwd"));
	}

	[Fact]
	public async Task ReadAllTextAsync_FileOutsideScope_Throws()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/etc/passwd", new MockFileData("secret"));

		await Assert.ThrowsAsync<ScopedFileSystemException>(
			() => scoped.File.ReadAllTextAsync("/etc/passwd"));
	}

	[Fact]
	public async Task ReadAllBytesAsync_FileOutsideScope_Throws()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/etc/passwd", new MockFileData("secret"));

		await Assert.ThrowsAsync<ScopedFileSystemException>(
			() => scoped.File.ReadAllBytesAsync("/etc/passwd"));
	}

	[Fact]
	public void Open_FileOutsideScope_Throws()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/etc/passwd", new MockFileData("secret"));

		Assert.Throws<ScopedFileSystemException>(() => scoped.File.Open("/etc/passwd", FileMode.Open));
		Assert.Throws<ScopedFileSystemException>(() => scoped.File.Open("/etc/passwd", FileMode.Open, FileAccess.Read));
		Assert.Throws<ScopedFileSystemException>(() => scoped.File.Open("/etc/passwd", FileMode.Open, FileAccess.Write));
	}

	[Fact]
	public void ScopeRoot_Itself_IsWithinScope()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/docs/top.md", new MockFileData("top"));

		Assert.Equal("top", scoped.File.ReadAllText("/docs/top.md"));
	}
}
