// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information

using System.IO.Abstractions.TestingHelpers;
using Xunit;

namespace Nullean.ScopedFileSystem.Tests;

/// <summary>
/// Tests for hidden file and hidden directory blocking with allowlists.
/// Hidden ancestor-directory tests without allowlists remain in <see cref="HiddenDirectoryTests"/>.
/// </summary>
public class HiddenPathTests
{
	// ── Hidden file blocking ────────────────────────────────────────────────

	[Fact]
	public void ReadAllText_HiddenFile_DirectlyInScopeRoot_Throws()
	{
		var mockFs = new MockFileSystem();
		mockFs.AddFile("/docs/.env", new MockFileData("SECRET=password"));
		var scoped = new ScopedFileSystem(mockFs, "/docs");

		Assert.Throws<ScopedFileSystemException>(() => scoped.File.ReadAllText("/docs/.env"));
	}

	[Fact]
	public void WriteAllText_HiddenFile_DirectlyInScopeRoot_Throws()
	{
		var mockFs = new MockFileSystem();
		mockFs.AddDirectory("/docs");
		var scoped = new ScopedFileSystem(mockFs, "/docs");

		Assert.Throws<ScopedFileSystemException>(() => scoped.File.WriteAllText("/docs/.env", "SECRET=password"));
	}

	[Fact]
	public void ReadAllText_HiddenFile_InSubDirectory_Throws()
	{
		var mockFs = new MockFileSystem();
		mockFs.AddFile("/docs/sub/.env", new MockFileData("SECRET=password"));
		var scoped = new ScopedFileSystem(mockFs, "/docs");

		Assert.Throws<ScopedFileSystemException>(() => scoped.File.ReadAllText("/docs/sub/.env"));
	}

	// ── AllowedHiddenFileNames ───────────────────────────────────────────────

	[Fact]
	public void ReadAllText_HiddenFile_InAllowedHiddenFileNames_Succeeds()
	{
		var mockFs = new MockFileSystem();
		mockFs.AddFile("/docs/.gitkeep", new MockFileData(""));
		var scoped = new ScopedFileSystem(mockFs, new ScopedFileSystemOptions("/docs")
		{
			AllowedHiddenFileNames = [".gitkeep"],
		});

		Assert.Equal("", scoped.File.ReadAllText("/docs/.gitkeep"));
	}

	[Fact]
	public void WriteAllText_HiddenFile_InAllowedHiddenFileNames_Succeeds()
	{
		var mockFs = new MockFileSystem();
		mockFs.AddDirectory("/docs");
		var scoped = new ScopedFileSystem(mockFs, new ScopedFileSystemOptions("/docs")
		{
			AllowedHiddenFileNames = [".gitkeep"],
		});

		scoped.File.WriteAllText("/docs/.gitkeep", "");

		Assert.True(mockFs.File.Exists("/docs/.gitkeep"));
	}

	[Fact]
	public void ReadAllText_HiddenFile_NotInAllowedHiddenFileNames_StillThrows()
	{
		var mockFs = new MockFileSystem();
		mockFs.AddFile("/docs/.env", new MockFileData("SECRET=x"));
		var scoped = new ScopedFileSystem(mockFs, new ScopedFileSystemOptions("/docs")
		{
			AllowedHiddenFileNames = [".gitkeep"],
		});

		Assert.Throws<ScopedFileSystemException>(() => scoped.File.ReadAllText("/docs/.env"));
	}

	// ── AllowedHiddenFolderNames ─────────────────────────────────────────────

	[Fact]
	public void ReadAllText_FileInAllowedHiddenDir_Succeeds()
	{
		var mockFs = new MockFileSystem();
		mockFs.AddFile("/docs/.git/config", new MockFileData("[core]"));
		var scoped = new ScopedFileSystem(mockFs, new ScopedFileSystemOptions("/docs")
		{
			AllowedHiddenFolderNames = [".git"],
		});

		Assert.Equal("[core]", scoped.File.ReadAllText("/docs/.git/config"));
	}

	[Fact]
	public void WriteAllText_FileInAllowedHiddenDir_Succeeds()
	{
		var mockFs = new MockFileSystem();
		mockFs.AddDirectory("/docs/.git");
		var scoped = new ScopedFileSystem(mockFs, new ScopedFileSystemOptions("/docs")
		{
			AllowedHiddenFolderNames = [".git"],
		});

		scoped.File.WriteAllText("/docs/.git/COMMIT_EDITMSG", "initial commit");

		Assert.Equal("initial commit", mockFs.File.ReadAllText("/docs/.git/COMMIT_EDITMSG"));
	}

	[Fact]
	public void ReadAllText_FileInNotAllowedHiddenDir_StillThrows()
	{
		var mockFs = new MockFileSystem();
		mockFs.AddFile("/docs/.vscode/settings.json", new MockFileData("{}"));
		var scoped = new ScopedFileSystem(mockFs, new ScopedFileSystemOptions("/docs")
		{
			AllowedHiddenFolderNames = [".git"],
		});

		Assert.Throws<ScopedFileSystemException>(() => scoped.File.ReadAllText("/docs/.vscode/settings.json"));
	}

	[Fact]
	public void ReadAllText_HiddenFileInAllowedHiddenDir_StillBlockedByFileCheck()
	{
		var mockFs = new MockFileSystem();
		mockFs.AddFile("/docs/.git/.env", new MockFileData("SECRET=x"));
		var scoped = new ScopedFileSystem(mockFs, new ScopedFileSystemOptions("/docs")
		{
			AllowedHiddenFolderNames = [".git"],
		});

		Assert.Throws<ScopedFileSystemException>(() => scoped.File.ReadAllText("/docs/.git/.env"));
	}

	[Fact]
	public void ReadAllText_HiddenFileInAllowedHiddenDir_WithBothAllowlists_Succeeds()
	{
		var mockFs = new MockFileSystem();
		mockFs.AddFile("/docs/.git/.gitignore_global", new MockFileData("*.log"));
		var scoped = new ScopedFileSystem(mockFs, new ScopedFileSystemOptions("/docs")
		{
			AllowedHiddenFolderNames = [".git"],
			AllowedHiddenFileNames = [".gitignore_global"],
		});

		Assert.Equal("*.log", scoped.File.ReadAllText("/docs/.git/.gitignore_global"));
	}
}
