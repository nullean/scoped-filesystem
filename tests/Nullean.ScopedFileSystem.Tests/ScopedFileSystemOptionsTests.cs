// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information

using AwesomeAssertions;
using System.IO.Abstractions.TestingHelpers;
using Xunit;

namespace Nullean.ScopedFileSystem.Tests;

public class ScopedFileSystemOptionsTests
{
	// ── Constructor: string roots ────────────────────────────────────────────

	[Fact]
	public void Constructor_SingleStringRoot_SetsScopeRoots()
	{
		var options = new ScopedFileSystemOptions("/docs");

		options.ScopeRoots.Should().ContainSingle();
		options.ScopeRoots.First().Should().Be("/docs");
	}

	[Fact]
	public void Constructor_MultipleStringRoots_SetsScopeRoots()
	{
		var options = new ScopedFileSystemOptions("/docs", "/data");

		options.ScopeRoots.Should().HaveCount(2);
		options.ScopeRoots.Should().Contain("/docs");
		options.ScopeRoots.Should().Contain("/data");
	}

	[Fact]
	public void Constructor_NoStringRoots_ThrowsArgumentException()
	{
		var act = () => new ScopedFileSystemOptions(Array.Empty<string>());
		act.Should().Throw<ArgumentException>();
	}

	// ── Constructor: IDirectoryInfo roots ────────────────────────────────────

	[Fact]
	public void Constructor_SingleDirectoryInfoRoot_SetsScopeRoots()
	{
		var mockFs = new MockFileSystem();
		mockFs.AddDirectory("/docs");
		var dirInfo = mockFs.DirectoryInfo.New("/docs");

		var options = new ScopedFileSystemOptions(dirInfo);

		options.ScopeRoots.Should().ContainSingle();
		options.ScopeRoots.First().Should().Be("/docs");
	}

	[Fact]
	public void Constructor_MultipleDirectoryInfoRoots_SetsScopeRoots()
	{
		var mockFs = new MockFileSystem();
		mockFs.AddDirectory("/docs");
		mockFs.AddDirectory("/data");

		var options = new ScopedFileSystemOptions(
			mockFs.DirectoryInfo.New("/docs"),
			mockFs.DirectoryInfo.New("/data"));

		options.ScopeRoots.Should().HaveCount(2);
	}

	[Fact]
	public void Constructor_NoDirectoryInfoRoots_ThrowsArgumentException()
	{
		var act = () => new ScopedFileSystemOptions(Array.Empty<System.IO.Abstractions.IDirectoryInfo>());
		act.Should().Throw<ArgumentException>();
	}

	// ── ScopedFileSystem integration ─────────────────────────────────────────

	[Fact]
	public void ScopedFileSystem_InnerPlusOptions_ReadsFromScopeRoot()
	{
		var mockFs = new MockFileSystem();
		mockFs.AddFile("/docs/readme.txt", new MockFileData("hello"));

		var scoped = new ScopedFileSystem(mockFs, new ScopedFileSystemOptions("/docs"));

		scoped.File.ReadAllText("/docs/readme.txt").Should().Be("hello");
	}

	[Fact]
	public void ScopedFileSystem_OptionsOnly_UsesRealFilesystem()
	{
		// Just verify it constructs without error; can't read real files in unit tests
		var options = new ScopedFileSystemOptions("/tmp");
		var scoped = new ScopedFileSystem(options);

		scoped.Should().NotBeNull();
	}

	[Fact]
	public void ScopedFileSystem_Options_DefaultAllowedHiddenFileNamesIsEmpty() =>
		new ScopedFileSystemOptions("/tmp").AllowedHiddenFileNames.Should().BeEmpty();

	[Fact]
	public void ScopedFileSystem_Options_DefaultAllowedHiddenFolderNamesIsEmpty() =>
		new ScopedFileSystemOptions("/tmp").AllowedHiddenFolderNames.Should().BeEmpty();

	[Fact]
	public void ScopedFileSystem_Options_DefaultAllowedSpecialFoldersIsNone() =>
		new ScopedFileSystemOptions("/tmp").AllowedSpecialFolders.Should().Be(AllowedSpecialFolder.None);

	[Fact]
	public void ScopedFileSystem_Options_DefaultVerboseExceptionsIsFalse()
	{
		Assert.False(new ScopedFileSystemOptions("/tmp").VerboseExceptions);
	}

	// ── VerboseExceptions ────────────────────────────────────────────────────

	[Fact]
	public void VerboseExceptions_False_MessageDoesNotIncludeAccessiblePaths()
	{
		var mockFs = new MockFileSystem();
		mockFs.AddFile("/etc/passwd", new MockFileData("secret"));
		var scoped = new ScopedFileSystem(mockFs, new ScopedFileSystemOptions("/docs") { VerboseExceptions = false });

		var ex = Assert.Throws<ScopedFileSystemException>(() => scoped.File.ReadAllText("/etc/passwd"));

		Assert.DoesNotContain("Accessible paths", ex.Message);
	}

	[Fact]
	public void VerboseExceptions_True_MessageIncludesScopeRoot()
	{
		var mockFs = new MockFileSystem();
		mockFs.AddFile("/etc/passwd", new MockFileData("secret"));
		var scoped = new ScopedFileSystem(mockFs, new ScopedFileSystemOptions("/docs") { VerboseExceptions = true });

		var ex = Assert.Throws<ScopedFileSystemException>(() => scoped.File.ReadAllText("/etc/passwd"));

		Assert.Contains("Accessible paths", ex.Message);
		Assert.Contains("/docs", ex.Message);
	}

	[Fact]
	public void VerboseExceptions_True_MessageIncludesAllScopeRoots()
	{
		var mockFs = new MockFileSystem();
		mockFs.AddFile("/etc/passwd", new MockFileData("secret"));
		var scoped = new ScopedFileSystem(mockFs, new ScopedFileSystemOptions("/docs", "/data") { VerboseExceptions = true });

		var ex = Assert.Throws<ScopedFileSystemException>(() => scoped.File.ReadAllText("/etc/passwd"));

		Assert.Contains("/docs", ex.Message);
		Assert.Contains("/data", ex.Message);
	}

	[Fact]
	public void VerboseExceptions_True_DirectoryAccessOutsideScope_MessageIncludesScopeRoot()
	{
		var mockFs = new MockFileSystem();
		mockFs.AddDirectory("/etc/secret");
		var scoped = new ScopedFileSystem(mockFs, new ScopedFileSystemOptions("/docs") { VerboseExceptions = true });

		var ex = Assert.Throws<ScopedFileSystemException>(() => scoped.Directory.CreateDirectory("/etc/secret"));

		Assert.Contains("Accessible paths", ex.Message);
		Assert.Contains("/docs", ex.Message);
	}

	[Fact]
	public void ScopedFileSystem_DirectoryInfoRoots_WorksWithInner()
	{
		var mockFs = new MockFileSystem();
		mockFs.AddFile("/docs/readme.txt", new MockFileData("hello"));
		var dirInfo = mockFs.DirectoryInfo.New("/docs");

		var scoped = new ScopedFileSystem(mockFs, new ScopedFileSystemOptions(dirInfo));

		scoped.File.ReadAllText("/docs/readme.txt").Should().Be("hello");
	}
}
