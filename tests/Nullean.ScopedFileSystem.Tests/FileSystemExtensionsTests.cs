// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information

using AwesomeAssertions;
using System.IO.Abstractions.TestingHelpers;
using Xunit;

namespace Nullean.ScopedFileSystem.Tests;

public class FileSystemExtensionsTests
{
	// ── IsSubPathOf ───────────────────────────────────────────────────────────

	[Fact]
	public void IsSubPathOf_FileInsideScope_ReturnsTrue()
	{
		var mockFs = new MockFileSystem();
		mockFs.AddFile("/docs/sub/readme.md", new MockFileData("hello"));
		var file = mockFs.FileInfo.New("/docs/sub/readme.md");
		var root = mockFs.DirectoryInfo.New("/docs");

		file.IsSubPathOf(root).Should().BeTrue();
	}

	[Fact]
	public void IsSubPathOf_FileOutsideScope_ReturnsFalse()
	{
		var mockFs = new MockFileSystem();
		mockFs.AddFile("/etc/passwd", new MockFileData("secret"));
		var file = mockFs.FileInfo.New("/etc/passwd");
		var root = mockFs.DirectoryInfo.New("/docs");

		file.IsSubPathOf(root).Should().BeFalse();
	}

	[Fact]
	public void IsSubPathOf_DirectoryIsExactMatch_ReturnsTrue()
	{
		var mockFs = new MockFileSystem();
		mockFs.AddDirectory("/docs");
		var dir = mockFs.DirectoryInfo.New("/docs");
		var root = mockFs.DirectoryInfo.New("/docs");

		dir.IsSubPathOf(root).Should().BeTrue();
	}

	[Fact]
	public void IsSubPathOf_DirectoryOutsideScope_ReturnsFalse()
	{
		var mockFs = new MockFileSystem();
		mockFs.AddDirectory("/etc");
		var dir = mockFs.DirectoryInfo.New("/etc");
		var root = mockFs.DirectoryInfo.New("/docs");

		dir.IsSubPathOf(root).Should().BeFalse();
	}

	// ── TryValidateSymlinkAccess ──────────────────────────────────────────────

	[Fact]
	public void TryValidateSymlinkAccess_RegularFile_ReturnsTrue()
	{
		var mockFs = new MockFileSystem();
		mockFs.AddFile("/docs/readme.md", new MockFileData("hello"));
		var file = mockFs.FileInfo.New("/docs/readme.md");
		var root = mockFs.DirectoryInfo.New("/docs");

		file.TryValidateSymlinkAccess(root, out var error).Should().BeTrue();
		error.Should().BeNull();
	}

	[Fact]
	public void TryValidateSymlinkAccess_SymlinkedFile_ReturnsFalseWithError()
	{
		var mockFs = new MockFileSystem();
		mockFs.AddFile("/docs/link.md", new MockFileData("content") { LinkTarget = "/etc/passwd" });
		var file = mockFs.FileInfo.New("/docs/link.md");
		var root = mockFs.DirectoryInfo.New("/docs");

		file.TryValidateSymlinkAccess(root, out var error).Should().BeFalse();
		error.Should().NotBeNull();
	}

	[Fact]
	public void TryValidateSymlinkAccess_HiddenAncestorDirectory_ReturnsFalseWithError()
	{
		var mockFs = new MockFileSystem();
		mockFs.AddFile("/docs/.hidden/readme.md", new MockFileData("hello"));
		var file = mockFs.FileInfo.New("/docs/.hidden/readme.md");
		var root = mockFs.DirectoryInfo.New("/docs");

		file.TryValidateSymlinkAccess(root, out var error).Should().BeFalse();
		error.Should().NotBeNull();
	}

	// ── ScopedFileSystem IDirectoryInfo overload ──────────────────────────────

	[Fact]
	public void ScopedFileSystem_AcceptsIDirectoryInfoOverload_Works()
	{
		var mockFs = new MockFileSystem();
		mockFs.AddFile("/docs/readme.md", new MockFileData("hello"));
		var scopeDir = mockFs.DirectoryInfo.New("/docs");

		var scoped = new ScopedFileSystem(mockFs, scopeDir);

		scoped.File.ReadAllText("/docs/readme.md").Should().Be("hello");
	}
}
