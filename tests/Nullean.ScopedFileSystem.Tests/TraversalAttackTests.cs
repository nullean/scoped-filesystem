// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information

using System.IO.Abstractions.TestingHelpers;
using Xunit;

namespace Nullean.ScopedFileSystem.Tests;

public class TraversalAttackTests
{
	[Fact]
	public void ReadAllText_DotDotEscapesScope_Throws()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/etc/passwd", new MockFileData("secret"));

		// /docs/../etc/passwd → /etc/passwd
		Assert.Throws<ScopedFileSystemException>(() => scoped.File.ReadAllText("/docs/../etc/passwd"));
	}

	[Fact]
	public void ReadAllText_DeepDotDotEscapesScope_Throws()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/etc/passwd", new MockFileData("secret"));

		// /docs/sub/../../etc/passwd → /etc/passwd
		Assert.Throws<ScopedFileSystemException>(() => scoped.File.ReadAllText("/docs/sub/../../etc/passwd"));
	}

	[Fact]
	public void ReadAllText_DotDotResolvesWithinScope_Succeeds()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/docs/readme.md", new MockFileData("hello"));

		// /docs/sub/../readme.md → /docs/readme.md — still inside scope
		Assert.Equal("hello", scoped.File.ReadAllText("/docs/sub/../readme.md"));
	}

	[Fact]
	public void WriteAllText_DotDotEscapesScope_Throws()
	{
		var (_, scoped) = Setup.Create("/docs");

		Assert.Throws<ScopedFileSystemException>(() =>
			scoped.File.WriteAllText("/docs/../etc/evil.txt", "pwned"));
	}

	[Fact]
	public void ReadAllText_SiblingPrefixPath_Throws()
	{
		// /docs is the scope; /docs-extra shares the prefix string but is NOT inside scope
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/docs-extra/file.txt", new MockFileData("sibling"));

		Assert.Throws<ScopedFileSystemException>(() => scoped.File.ReadAllText("/docs-extra/file.txt"));
	}

	[Fact]
	public void ReadAllText_SiblingWithSamePrefix_Throws()
	{
		// Regression guard: a naive string-prefix check would pass /data2/file when scope is /data
		var (mockFs, scoped) = Setup.Create("/data");
		mockFs.AddFile("/data2/secret.txt", new MockFileData("leak"));

		Assert.Throws<ScopedFileSystemException>(() => scoped.File.ReadAllText("/data2/secret.txt"));
	}

	[Fact]
	public void Exists_SiblingPrefixPath_ReturnsFalse()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/docs-extra/file.txt", new MockFileData("sibling"));

		Assert.False(scoped.File.Exists("/docs-extra/file.txt"));
	}

	[Fact]
	public void ReadAllText_AbsolutePathOutsideScope_Throws()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/tmp/evil.txt", new MockFileData("evil"));

		Assert.Throws<ScopedFileSystemException>(() => scoped.File.ReadAllText("/tmp/evil.txt"));
	}

	[Fact]
	public void ReadAllText_RootPath_Throws()
	{
		var (_, scoped) = Setup.Create("/docs");

		// Accessing the filesystem root is outside scope
		Assert.Throws<ScopedFileSystemException>(() => scoped.File.ReadAllText("/"));
	}
}
