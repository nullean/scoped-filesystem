// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information

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

		Assert.Throws<ScopedFileSystemException>(() => scoped.File.ReadAllText("/docs/link.md"));
	}

	[Fact]
	public void ReadAllText_FileIsSymlinkWithinScope_Throws()
	{
		// Even when the target is also within scope, symlinks are always rejected
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/docs/real.md", new MockFileData("real content"));
		mockFs.AddFile("/docs/link.md", new MockFileData("real content") { LinkTarget = "/docs/real.md" });

		Assert.Throws<ScopedFileSystemException>(() => scoped.File.ReadAllText("/docs/link.md"));
	}

	[Fact]
	public void WriteAllText_FileIsSymlink_Throws()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/docs/link.md", new MockFileData("") { LinkTarget = "/etc/passwd" });

		Assert.Throws<ScopedFileSystemException>(() => scoped.File.WriteAllText("/docs/link.md", "evil"));
	}

	[Fact]
	public void Exists_FileIsSymlink_ReturnsTrue()
	{
		// Exists uses IsInScope only (no symlink check) — symlink detection fires on actual access.
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/docs/link.md", new MockFileData("") { LinkTarget = "/etc/passwd" });

		Assert.True(scoped.File.Exists("/docs/link.md"));
	}

	[Fact]
	public void FileInfoFactory_OpenRead_SymlinkedFile_Throws()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/docs/link.md", new MockFileData("") { LinkTarget = "/etc/passwd" });

		Assert.Throws<ScopedFileSystemException>(() => scoped.FileInfo.New("/docs/link.md").OpenRead());
	}
}
