// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information

#if NET11_0_OR_GREATER
using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using Xunit;

namespace Nullean.ScopedFileSystem.Tests;

// On net11.0 IFile and IFileInfo gained hard link members. A build for an older target does not implement them, and
// the decorator then fails to load when a net11.0 application uses it.
public class HardLinkTests
{
	[Fact]
	public void File_CreateHardLink_LinkOutsideScope_Throws()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/docs/readme.md", new MockFileData("hello"));

		var act = () => scoped.File.CreateHardLink("/etc/readme.md", "/docs/readme.md");

		act.Should().Throw<ScopedFileSystemException>();
	}

	[Fact]
	public void File_CreateHardLink_TargetOutsideScope_Throws()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/etc/passwd", new MockFileData("secret"));

		var act = () => scoped.File.CreateHardLink("/docs/passwd", "/etc/passwd");

		act.Should().Throw<ScopedFileSystemException>();
	}

	[Fact]
	public void FileInfo_CreateAsHardLink_TargetOutsideScope_Throws()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/etc/passwd", new MockFileData("secret"));

		var act = () => scoped.FileInfo.New("/docs/passwd").CreateAsHardLink("/etc/passwd");

		act.Should().Throw<ScopedFileSystemException>();
	}

	[Fact]
	public void FileInfo_CreateAsHardLink_LinkOutsideScope_Throws()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/docs/readme.md", new MockFileData("hello"));

		var act = () => scoped.FileInfo.New("/etc/readme.md").CreateAsHardLink("/docs/readme.md");

		act.Should().Throw<ScopedFileSystemException>();
	}

	[Fact]
	public void File_CreateHardLink_WithinScope_IsNotRejectedByTheScope()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/docs/readme.md", new MockFileData("hello"));

		var act = () => scoped.File.CreateHardLink("/docs/copy.md", "/docs/readme.md");

		act.Should().NotThrow<ScopedFileSystemException>();
	}
}
#endif
