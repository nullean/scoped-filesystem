// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information

using AwesomeAssertions;
using System.IO.Abstractions.TestingHelpers;
using Xunit;

namespace Nullean.ScopedFileSystem.Tests;

public class SymlinkTests
{
	[Fact]
	public void ReadAllText_FileIsSymlink_Throws()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/docs/link.md", new MockFileData("content") { LinkTarget = "/etc/passwd" });

		Action act = () => scoped.File.ReadAllText("/docs/link.md");
		act.Should().Throw<ScopedFileSystemException>();
	}

	[Fact]
	public void ReadAllText_FileIsSymlinkWithinScope_Throws()
	{
		// Even when the target is also within scope, symlinks are always rejected
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/docs/real.md", new MockFileData("real content"));
		mockFs.AddFile("/docs/link.md", new MockFileData("real content") { LinkTarget = "/docs/real.md" });

		Action act = () => scoped.File.ReadAllText("/docs/link.md");
		act.Should().Throw<ScopedFileSystemException>();
	}

	[Fact]
	public void WriteAllText_FileIsSymlink_Throws()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/docs/link.md", new MockFileData("") { LinkTarget = "/etc/passwd" });

		var act = () => scoped.File.WriteAllText("/docs/link.md", "evil");
		act.Should().Throw<ScopedFileSystemException>();
	}

	[Fact]
	public void Exists_FileIsSymlink_ReturnsTrue()
	{
		// Exists uses IsInScope only (no symlink check) — symlink detection fires on actual access.
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/docs/link.md", new MockFileData("") { LinkTarget = "/etc/passwd" });

		scoped.File.Exists("/docs/link.md").Should().BeTrue();
	}

	[Fact]
	public void FileInfoFactory_OpenRead_SymlinkedFile_Throws()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/docs/link.md", new MockFileData("") { LinkTarget = "/etc/passwd" });

			var act = () => scoped.FileInfo.New("/docs/link.md").OpenRead();
		act.Should().Throw<ScopedFileSystemException>();
	}
}
