// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information

using AwesomeAssertions;
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

		stream.Should().NotBeNull();
	}

	[Fact]
	public void FileInfo_OpenRead_OutsideScope_Throws()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/etc/passwd", new MockFileData("secret"));

		var act = () => scoped.FileInfo.New("/etc/passwd").OpenRead();
		act.Should().Throw<ScopedFileSystemException>();
	}

	[Fact]
	public void FileInfo_OpenText_OutsideScope_Throws()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/etc/passwd", new MockFileData("secret"));

		var act = () => scoped.FileInfo.New("/etc/passwd").OpenText();
		act.Should().Throw<ScopedFileSystemException>();
	}

	[Fact]
	public void FileInfo_OpenWrite_OutsideScope_Throws()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/etc/file.txt", new MockFileData("data"));

		var act = () => scoped.FileInfo.New("/etc/file.txt").OpenWrite();
		act.Should().Throw<ScopedFileSystemException>();
	}

	[Fact]
	public void FileInfo_Create_OutsideScope_Throws()
	{
		var (_, scoped) = Setup.Create("/docs");

		var act = () => scoped.FileInfo.New("/etc/new.txt").Create();
		act.Should().Throw<ScopedFileSystemException>();
	}

	[Fact]
	public void FileInfo_Delete_OutsideScope_Throws()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/etc/file.txt", new MockFileData("data"));

		var act = () => scoped.FileInfo.New("/etc/file.txt").Delete();
		act.Should().Throw<ScopedFileSystemException>();
	}

	[Fact]
	public void FileInfo_CopyTo_DestOutsideScope_Throws()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/docs/source.txt", new MockFileData("data"));
		mockFs.AddDirectory("/etc");

		var act = () => scoped.FileInfo.New("/docs/source.txt").CopyTo("/etc/dest.txt");
		act.Should().Throw<ScopedFileSystemException>();
	}

	[Fact]
	public void FileInfo_Open_OutsideScope_Throws()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/etc/file.txt", new MockFileData("data"));

		var act = () => scoped.FileInfo.New("/etc/file.txt").Open(FileMode.Open);
		act.Should().Throw<ScopedFileSystemException>();
	}

	[Fact]
	public void FileInfo_TraversalPath_Throws()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/etc/passwd", new MockFileData("secret"));

		var act = () => scoped.FileInfo.New("/docs/../etc/passwd").OpenRead();
		act.Should().Throw<ScopedFileSystemException>();
	}
}
