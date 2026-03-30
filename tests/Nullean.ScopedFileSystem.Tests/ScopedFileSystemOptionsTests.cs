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
		var dirInfo1 = mockFs.DirectoryInfo.New("/docs");
		var dirInfo2 = mockFs.DirectoryInfo.New("/data");

		var options = new ScopedFileSystemOptions(dirInfo1, dirInfo2);

		Assert.Equal(2, options.ScopeRoots.Count);
	}

	[Fact]
	public void Constructor_NoDirectoryInfoRoots_ThrowsArgumentException()
	{
		Assert.Throws<ArgumentException>(() => new ScopedFileSystemOptions(Array.Empty<System.IO.Abstractions.IDirectoryInfo>()));
	}

	// ── ScopedFileSystem integration ─────────────────────────────────────────

	[Fact]
	public void ScopedFileSystem_AcceptsOptionsConstructor_ReadsFromScopeRoot()
	{
		var mockFs = new MockFileSystem();
		mockFs.AddFile("/docs/readme.txt", new MockFileData("hello"));

		var scoped = new ScopedFileSystem(new ScopedFileSystemOptions("/docs") { Inner = mockFs });

		Assert.Equal("hello", scoped.File.ReadAllText("/docs/readme.txt"));
	}

	[Fact]
	public void ScopedFileSystem_Options_DefaultInnerIsRealFileSystem()
	{
		var options = new ScopedFileSystemOptions("/tmp");

		Assert.IsType<System.IO.Abstractions.FileSystem>(options.Inner);
	}

	[Fact]
	public void ScopedFileSystem_Options_DefaultAllowedHiddenFileNamesIsEmpty()
	{
		var options = new ScopedFileSystemOptions("/tmp");

		Assert.Empty(options.AllowedHiddenFileNames);
	}

	[Fact]
	public void ScopedFileSystem_Options_DefaultAllowedHiddenFolderNamesIsEmpty()
	{
		var options = new ScopedFileSystemOptions("/tmp");

		Assert.Empty(options.AllowedHiddenFolderNames);
	}

	[Fact]
	public void ScopedFileSystem_Options_DefaultAllowedSpecialFoldersIsNone()
	{
		var options = new ScopedFileSystemOptions("/tmp");

		Assert.Equal(AllowedSpecialFolder.None, options.AllowedSpecialFolders);
	}

	[Fact]
	public void ScopedFileSystem_OptionsWithDirectoryInfo_UsesFullName()
	{
		var mockFs = new MockFileSystem();
		mockFs.AddFile("/docs/readme.txt", new MockFileData("hello"));
		var dirInfo = mockFs.DirectoryInfo.New("/docs");

		var scoped = new ScopedFileSystem(new ScopedFileSystemOptions(dirInfo) { Inner = mockFs });

		Assert.Equal("hello", scoped.File.ReadAllText("/docs/readme.txt"));
	}
}
