// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information

using System.IO.Abstractions.TestingHelpers;
using Xunit;

namespace Nullean.ScopedFileSystem.Tests;

public class MultipleRootsTests
{
	[Fact]
	public void ReadAllText_FileInFirstRoot_Succeeds()
	{
		var (mockFs, scoped) = Setup.Create("/docs", "/data");
		mockFs.AddFile("/docs/readme.md", new MockFileData("docs content"));

		Assert.Equal("docs content", scoped.File.ReadAllText("/docs/readme.md"));
	}

	[Fact]
	public void ReadAllText_FileInSecondRoot_Succeeds()
	{
		var (mockFs, scoped) = Setup.Create("/docs", "/data");
		mockFs.AddFile("/data/dataset.csv", new MockFileData("1,2,3"));

		Assert.Equal("1,2,3", scoped.File.ReadAllText("/data/dataset.csv"));
	}

	[Fact]
	public void ReadAllText_FileOutsideBothRoots_Throws()
	{
		var (mockFs, scoped) = Setup.Create("/docs", "/data");
		mockFs.AddFile("/etc/passwd", new MockFileData("secret"));

		Assert.Throws<ScopedFileSystemException>(() => scoped.File.ReadAllText("/etc/passwd"));
	}

	[Fact]
	public void WriteAllText_FileInSecondRoot_Succeeds()
	{
		var (mockFs, scoped) = Setup.Create("/docs", "/data");

		scoped.File.WriteAllText("/data/output.txt", "result");

		Assert.Equal("result", mockFs.File.ReadAllText("/data/output.txt"));
	}

	[Fact]
	public void Exists_FileInEitherRoot_ReturnsTrue()
	{
		var (mockFs, scoped) = Setup.Create("/docs", "/data");
		mockFs.AddFile("/docs/a.txt", new MockFileData("a"));
		mockFs.AddFile("/data/b.txt", new MockFileData("b"));

		Assert.True(scoped.File.Exists("/docs/a.txt"));
		Assert.True(scoped.File.Exists("/data/b.txt"));
	}

	[Fact]
	public void Copy_AcrossRoots_Succeeds()
	{
		var (mockFs, scoped) = Setup.Create("/docs", "/data");
		mockFs.AddFile("/docs/source.txt", new MockFileData("cross-root"));

		scoped.File.Copy("/docs/source.txt", "/data/dest.txt");

		Assert.Equal("cross-root", mockFs.File.ReadAllText("/data/dest.txt"));
	}

	[Fact]
	public void Constructor_ParentChildRoots_ThrowsArgumentException()
	{
		var mockFs = new MockFileSystem();
		mockFs.AddDirectory("/data");
		mockFs.AddDirectory("/data/sub");

		Assert.Throws<ArgumentException>(() => new ScopedFileSystem(mockFs, "/data", "/data/sub"));
	}

	[Fact]
	public void Constructor_ChildParentRoots_ThrowsArgumentException()
	{
		// Order must not matter — child before parent is also rejected
		var mockFs = new MockFileSystem();
		mockFs.AddDirectory("/data");
		mockFs.AddDirectory("/data/sub");

		Assert.Throws<ArgumentException>(() => new ScopedFileSystem(mockFs, "/data/sub", "/data"));
	}

	[Fact]
	public void Constructor_GrandparentDescendant_ThrowsArgumentException()
	{
		var mockFs = new MockFileSystem();
		mockFs.AddDirectory("/a/b/c");

		Assert.Throws<ArgumentException>(() => new ScopedFileSystem(mockFs, "/a", "/a/b/c"));
	}

	[Fact]
	public void Constructor_DisjointRoots_DoesNotThrow()
	{
		var mockFs = new MockFileSystem();
		mockFs.AddDirectory("/docs");
		mockFs.AddDirectory("/data");

		var ex = Record.Exception(() => new ScopedFileSystem(mockFs, "/docs", "/data"));
		Assert.Null(ex);
	}

	[Fact]
	public void Constructor_NoRoots_ThrowsArgumentException()
	{
		var mockFs = new MockFileSystem();

		Assert.Throws<ArgumentException>(() => new ScopedFileSystem(mockFs));
	}
}
