// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information

using System.IO.Abstractions.TestingHelpers;
using Xunit;

namespace Nullean.ScopedFileSystem.Tests;

public class ScopedFileInfoTests
{
	[Fact]
	public void FileInfo_OpenRead_WithinScope_Succeeds()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/docs/readme.md", new MockFileData("hello"));

		using var stream = scoped.FileInfo.New("/docs/readme.md").OpenRead();

		Assert.NotNull(stream);
	}

	[Fact]
	public void FileInfo_OpenRead_OutsideScope_Throws()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/etc/passwd", new MockFileData("secret"));

		Assert.Throws<ScopedFileSystemException>(() => scoped.FileInfo.New("/etc/passwd").OpenRead());
	}

	[Fact]
	public void FileInfo_OpenText_OutsideScope_Throws()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/etc/passwd", new MockFileData("secret"));

		Assert.Throws<ScopedFileSystemException>(() => scoped.FileInfo.New("/etc/passwd").OpenText());
	}

	[Fact]
	public void FileInfo_OpenWrite_OutsideScope_Throws()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/etc/file.txt", new MockFileData("data"));

		Assert.Throws<ScopedFileSystemException>(() => scoped.FileInfo.New("/etc/file.txt").OpenWrite());
	}

	[Fact]
	public void FileInfo_Create_OutsideScope_Throws()
	{
		var (_, scoped) = Setup.Create("/docs");

		Assert.Throws<ScopedFileSystemException>(() => scoped.FileInfo.New("/etc/new.txt").Create());
	}

	[Fact]
	public void FileInfo_Delete_OutsideScope_Throws()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/etc/file.txt", new MockFileData("data"));

		Assert.Throws<ScopedFileSystemException>(() => scoped.FileInfo.New("/etc/file.txt").Delete());
	}

	[Fact]
	public void FileInfo_CopyTo_DestOutsideScope_Throws()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/docs/source.txt", new MockFileData("data"));
		mockFs.AddDirectory("/etc");

		Assert.Throws<ScopedFileSystemException>(() =>
			scoped.FileInfo.New("/docs/source.txt").CopyTo("/etc/dest.txt"));
	}

	[Fact]
	public void FileInfo_Open_OutsideScope_Throws()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/etc/file.txt", new MockFileData("data"));

		Assert.Throws<ScopedFileSystemException>(() =>
			scoped.FileInfo.New("/etc/file.txt").Open(FileMode.Open));
	}

	[Fact]
	public void FileInfo_TraversalPath_Throws()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/etc/passwd", new MockFileData("secret"));

		Assert.Throws<ScopedFileSystemException>(() =>
			scoped.FileInfo.New("/docs/../etc/passwd").OpenRead());
	}
}
