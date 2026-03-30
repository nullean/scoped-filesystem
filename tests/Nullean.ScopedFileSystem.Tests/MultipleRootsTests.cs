// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information

using AwesomeAssertions;
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

		scoped.File.ReadAllText("/docs/readme.md").Should().Be("docs content");
	}

	[Fact]
	public void ReadAllText_FileInSecondRoot_Succeeds()
	{
		var (mockFs, scoped) = Setup.Create("/docs", "/data");
		mockFs.AddFile("/data/dataset.csv", new MockFileData("1,2,3"));

		scoped.File.ReadAllText("/data/dataset.csv").Should().Be("1,2,3");
	}

	[Fact]
	public void ReadAllText_FileOutsideBothRoots_Throws()
	{
		var (mockFs, scoped) = Setup.Create("/docs", "/data");
		mockFs.AddFile("/etc/passwd", new MockFileData("secret"));

		var act = () => scoped.File.ReadAllText("/etc/passwd");
		act.Should().Throw<ScopedFileSystemException>();
	}

	[Fact]
	public void WriteAllText_FileInSecondRoot_Succeeds()
	{
		var (mockFs, scoped) = Setup.Create("/docs", "/data");

		scoped.File.WriteAllText("/data/output.txt", "result");

		mockFs.File.ReadAllText("/data/output.txt").Should().Be("result");
	}

	[Fact]
	public void Exists_FileInEitherRoot_ReturnsTrue()
	{
		var (mockFs, scoped) = Setup.Create("/docs", "/data");
		mockFs.AddFile("/docs/a.txt", new MockFileData("a"));
		mockFs.AddFile("/data/b.txt", new MockFileData("b"));

		scoped.File.Exists("/docs/a.txt").Should().BeTrue();
		scoped.File.Exists("/data/b.txt").Should().BeTrue();
	}

	[Fact]
	public void Copy_AcrossRoots_Succeeds()
	{
		var (mockFs, scoped) = Setup.Create("/docs", "/data");
		mockFs.AddFile("/docs/source.txt", new MockFileData("cross-root"));

		scoped.File.Copy("/docs/source.txt", "/data/dest.txt");

		mockFs.File.ReadAllText("/data/dest.txt").Should().Be("cross-root");
	}

	[Fact]
	public void Constructor_ParentChildRoots_ThrowsArgumentException()
	{
		var mockFs = new MockFileSystem();
		mockFs.AddDirectory("/data");
		mockFs.AddDirectory("/data/sub");

		var act = () => new ScopedFileSystem(mockFs, "/data", "/data/sub");
		act.Should().Throw<ArgumentException>();
	}

	[Fact]
	public void Constructor_ChildParentRoots_ThrowsArgumentException()
	{
		// Order must not matter — child before parent is also rejected
		var mockFs = new MockFileSystem();
		mockFs.AddDirectory("/data");
		mockFs.AddDirectory("/data/sub");

		var act = () => new ScopedFileSystem(mockFs, "/data/sub", "/data");
		act.Should().Throw<ArgumentException>();
	}

	[Fact]
	public void Constructor_GrandparentDescendant_ThrowsArgumentException()
	{
		var mockFs = new MockFileSystem();
		mockFs.AddDirectory("/a/b/c");

		var act = () => new ScopedFileSystem(mockFs, "/a", "/a/b/c");
		act.Should().Throw<ArgumentException>();
	}

	[Fact]
	public void Constructor_DisjointRoots_DoesNotThrow()
	{
		var mockFs = new MockFileSystem();
		mockFs.AddDirectory("/docs");
		mockFs.AddDirectory("/data");

		var act = () => new ScopedFileSystem(mockFs, "/docs", "/data");
		act.Should().NotThrow();
	}

	[Fact]
	public void Constructor_NoRoots_ThrowsArgumentException()
	{
		var mockFs = new MockFileSystem();

		var act = () => new ScopedFileSystem(mockFs);
		act.Should().Throw<ArgumentException>();
	}
}
