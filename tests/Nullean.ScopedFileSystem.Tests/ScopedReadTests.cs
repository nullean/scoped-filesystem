// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information

using AwesomeAssertions;
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

		scoped.File.ReadAllText("/docs/readme.md").Should().Be("hello");
	}

	[Fact]
	public void ReadAllText_FileOutsideScope_Throws()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/etc/passwd", new MockFileData("secret"));

		var act = () => scoped.File.ReadAllText("/etc/passwd");
		act.Should().Throw<ScopedFileSystemException>();
	}

	[Fact]
	public void ReadAllBytes_FileOutsideScope_Throws()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/etc/passwd", new MockFileData("secret"));

		var act = () => scoped.File.ReadAllBytes("/etc/passwd");
		act.Should().Throw<ScopedFileSystemException>();
	}

	[Fact]
	public void ReadAllLines_FileWithinScope_ReturnsLines()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/docs/lines.txt", new MockFileData("line1\nline2\nline3"));

		var lines = scoped.File.ReadAllLines("/docs/lines.txt");

		lines.Should().HaveCount(3);
		lines[0].Should().Be("line1");
	}

	[Fact]
	public void ReadAllLines_FileOutsideScope_Throws()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/etc/hosts", new MockFileData("127.0.0.1 localhost"));

		var act = () => scoped.File.ReadAllLines("/etc/hosts");
		act.Should().Throw<ScopedFileSystemException>();
	}

	[Fact]
	public void OpenRead_FileOutsideScope_Throws()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/etc/passwd", new MockFileData("secret"));

		var act = () => scoped.File.OpenRead("/etc/passwd");
		act.Should().Throw<ScopedFileSystemException>();
	}

	[Fact]
	public void OpenText_FileOutsideScope_Throws()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/etc/passwd", new MockFileData("secret"));

		var act = () => scoped.File.OpenText("/etc/passwd");
		act.Should().Throw<ScopedFileSystemException>();
	}

	[Fact]
	public async Task ReadAllTextAsync_FileOutsideScope_Throws()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/etc/passwd", new MockFileData("secret"));

		var act = async () => await scoped.File.ReadAllTextAsync("/etc/passwd");
		await act.Should().ThrowAsync<ScopedFileSystemException>();
	}

	[Fact]
	public async Task ReadAllBytesAsync_FileOutsideScope_Throws()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/etc/passwd", new MockFileData("secret"));

		var act = async () => await scoped.File.ReadAllBytesAsync("/etc/passwd");
		await act.Should().ThrowAsync<ScopedFileSystemException>();
	}

	[Fact]
	public void Open_FileOutsideScope_Throws()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/etc/passwd", new MockFileData("secret"));

		var act1 = () => scoped.File.Open("/etc/passwd", FileMode.Open);
		var act2 = () => scoped.File.Open("/etc/passwd", FileMode.Open, FileAccess.Read);
		var act3 = () => scoped.File.Open("/etc/passwd", FileMode.Open, FileAccess.Write);
		act1.Should().Throw<ScopedFileSystemException>();
		act2.Should().Throw<ScopedFileSystemException>();
		act3.Should().Throw<ScopedFileSystemException>();
	}

	[Fact]
	public void ScopeRoot_Itself_IsWithinScope()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/docs/top.md", new MockFileData("top"));

		scoped.File.ReadAllText("/docs/top.md").Should().Be("top");
	}
}
