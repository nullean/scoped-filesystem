// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information

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

		Assert.Single(options.ScopeRoots);
		Assert.Equal("/docs", options.ScopeRoots.First());
	}

	[Fact]
	public void Constructor_MultipleStringRoots_SetsScopeRoots()
	{
		var options = new ScopedFileSystemOptions("/docs", "/data");

		Assert.Equal(2, options.ScopeRoots.Count);
		Assert.Contains("/docs", options.ScopeRoots);
		Assert.Contains("/data", options.ScopeRoots);
	}

	[Fact]
	public void Constructor_NoStringRoots_ThrowsArgumentException()
	{
		Assert.Throws<ArgumentException>(() => new ScopedFileSystemOptions(Array.Empty<string>()));
	}

	// ── Constructor: IDirectoryInfo roots ────────────────────────────────────

	[Fact]
	public void Constructor_SingleDirectoryInfoRoot_SetsScopeRoots()
	{
		var mockFs = new MockFileSystem();
		mockFs.AddDirectory("/docs");
		var dirInfo = mockFs.DirectoryInfo.New("/docs");

		var options = new ScopedFileSystemOptions(dirInfo);

		Assert.Single(options.ScopeRoots);
		Assert.Equal("/docs", options.ScopeRoots.First());
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

		Assert.Equal(2, options.ScopeRoots.Count);
	}

	[Fact]
	public void Constructor_NoDirectoryInfoRoots_ThrowsArgumentException()
	{
		Assert.Throws<ArgumentException>(() =>
			new ScopedFileSystemOptions(Array.Empty<System.IO.Abstractions.IDirectoryInfo>()));
	}

	// ── ScopedFileSystem integration ─────────────────────────────────────────

	[Fact]
	public void ScopedFileSystem_InnerPlusOptions_ReadsFromScopeRoot()
	{
		var mockFs = new MockFileSystem();
		mockFs.AddFile("/docs/readme.txt", new MockFileData("hello"));

		var scoped = new ScopedFileSystem(mockFs, new ScopedFileSystemOptions("/docs"));

		Assert.Equal("hello", scoped.File.ReadAllText("/docs/readme.txt"));
	}

	[Fact]
	public void ScopedFileSystem_OptionsOnly_UsesRealFilesystem()
	{
		// Just verify it constructs without error; can't read real files in unit tests
		var options = new ScopedFileSystemOptions("/tmp");
		var scoped = new ScopedFileSystem(options);

		Assert.NotNull(scoped);
	}

	[Fact]
	public void ScopedFileSystem_Options_DefaultAllowedHiddenFileNamesIsEmpty()
	{
		Assert.Empty(new ScopedFileSystemOptions("/tmp").AllowedHiddenFileNames);
	}

	[Fact]
	public void ScopedFileSystem_Options_DefaultAllowedHiddenFolderNamesIsEmpty()
	{
		Assert.Empty(new ScopedFileSystemOptions("/tmp").AllowedHiddenFolderNames);
	}

	[Fact]
	public void ScopedFileSystem_Options_DefaultAllowedSpecialFoldersIsNone()
	{
		Assert.Equal(AllowedSpecialFolder.None, new ScopedFileSystemOptions("/tmp").AllowedSpecialFolders);
	}

	[Fact]
	public void ScopedFileSystem_DirectoryInfoRoots_WorksWithInner()
	{
		var mockFs = new MockFileSystem();
		mockFs.AddFile("/docs/readme.txt", new MockFileData("hello"));
		var dirInfo = mockFs.DirectoryInfo.New("/docs");

		var scoped = new ScopedFileSystem(mockFs, new ScopedFileSystemOptions(dirInfo));

		Assert.Equal("hello", scoped.File.ReadAllText("/docs/readme.txt"));
	}
}
