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
		// Note: /docs/sub is not hidden, but .env is a hidden file
		var scoped = new ScopedFileSystem(mockFs, "/docs");

		// Hidden ancestor directory (.env's parent is "sub" which is fine, but the file itself is hidden)
		// Wait: /docs/sub/.env — "sub" is not hidden, ".env" is the hidden file
		Assert.Throws<ScopedFileSystemException>(() => scoped.File.ReadAllText("/docs/sub/.env"));
	}

	// ── AllowedHiddenFileNames ───────────────────────────────────────────────

	[Fact]
	public void ReadAllText_HiddenFile_InAllowedHiddenFileNames_Succeeds()
	{
		var mockFs = new MockFileSystem();
		mockFs.AddFile("/docs/.gitkeep", new MockFileData(""));
		var scoped = new ScopedFileSystem(new ScopedFileSystemOptions("/docs")
		{
			Inner = mockFs,
			AllowedHiddenFileNames = new HashSet<string> { ".gitkeep" },
		});

		var content = scoped.File.ReadAllText("/docs/.gitkeep");

		Assert.Equal("", content);
	}

	[Fact]
	public void WriteAllText_HiddenFile_InAllowedHiddenFileNames_Succeeds()
	{
		var mockFs = new MockFileSystem();
		mockFs.AddDirectory("/docs");
		var scoped = new ScopedFileSystem(new ScopedFileSystemOptions("/docs")
		{
			Inner = mockFs,
			AllowedHiddenFileNames = new HashSet<string> { ".gitkeep" },
		});

		scoped.File.WriteAllText("/docs/.gitkeep", "");

		Assert.True(mockFs.File.Exists("/docs/.gitkeep"));
	}

	[Fact]
	public void ReadAllText_HiddenFile_NotInAllowedHiddenFileNames_StillThrows()
	{
		var mockFs = new MockFileSystem();
		mockFs.AddFile("/docs/.env", new MockFileData("SECRET=x"));
		mockFs.AddFile("/docs/.gitkeep", new MockFileData(""));
		var scoped = new ScopedFileSystem(new ScopedFileSystemOptions("/docs")
		{
			Inner = mockFs,
			AllowedHiddenFileNames = new HashSet<string> { ".gitkeep" },
		});

		// .env is not in the allowlist, so it should still throw
		Assert.Throws<ScopedFileSystemException>(() => scoped.File.ReadAllText("/docs/.env"));
	}

	// ── AllowedHiddenFolderNames ─────────────────────────────────────────────

	[Fact]
	public void ReadAllText_FileInAllowedHiddenDir_Succeeds()
	{
		var mockFs = new MockFileSystem();
		mockFs.AddFile("/docs/.git/config", new MockFileData("[core]"));
		var scoped = new ScopedFileSystem(new ScopedFileSystemOptions("/docs")
		{
			Inner = mockFs,
			AllowedHiddenFolderNames = new HashSet<string> { ".git" },
		});

		var content = scoped.File.ReadAllText("/docs/.git/config");

		Assert.Equal("[core]", content);
	}

	[Fact]
	public void WriteAllText_FileInAllowedHiddenDir_Succeeds()
	{
		var mockFs = new MockFileSystem();
		mockFs.AddDirectory("/docs/.git");
		var scoped = new ScopedFileSystem(new ScopedFileSystemOptions("/docs")
		{
			Inner = mockFs,
			AllowedHiddenFolderNames = new HashSet<string> { ".git" },
		});

		scoped.File.WriteAllText("/docs/.git/COMMIT_EDITMSG", "initial commit");

		Assert.Equal("initial commit", mockFs.File.ReadAllText("/docs/.git/COMMIT_EDITMSG"));
	}

	[Fact]
	public void ReadAllText_FileInNotAllowedHiddenDir_StillThrows()
	{
		var mockFs = new MockFileSystem();
		mockFs.AddFile("/docs/.git/config", new MockFileData("[core]"));
		mockFs.AddFile("/docs/.vscode/settings.json", new MockFileData("{}"));
		var scoped = new ScopedFileSystem(new ScopedFileSystemOptions("/docs")
		{
			Inner = mockFs,
			AllowedHiddenFolderNames = new HashSet<string> { ".git" },
		});

		// .git is allowed, but .vscode is not
		Assert.Throws<ScopedFileSystemException>(() => scoped.File.ReadAllText("/docs/.vscode/settings.json"));
	}

	[Fact]
	public void ReadAllText_HiddenFileInAllowedHiddenDir_StillBlockedByFileCheck()
	{
		// .git folder is allowed, but .env inside it is still a hidden file
		var mockFs = new MockFileSystem();
		mockFs.AddFile("/docs/.git/.env", new MockFileData("SECRET=x"));
		var scoped = new ScopedFileSystem(new ScopedFileSystemOptions("/docs")
		{
			Inner = mockFs,
			AllowedHiddenFolderNames = new HashSet<string> { ".git" },
		});

		Assert.Throws<ScopedFileSystemException>(() => scoped.File.ReadAllText("/docs/.git/.env"));
	}

	[Fact]
	public void ReadAllText_HiddenFileInAllowedHiddenDir_WithBothAllowlists_Succeeds()
	{
		// Both the folder (.git) and the file (.env) are explicitly allowed
		var mockFs = new MockFileSystem();
		mockFs.AddFile("/docs/.git/.gitignore_global", new MockFileData("*.log"));
		var scoped = new ScopedFileSystem(new ScopedFileSystemOptions("/docs")
		{
			Inner = mockFs,
			AllowedHiddenFolderNames = new HashSet<string> { ".git" },
			AllowedHiddenFileNames = new HashSet<string> { ".gitignore_global" },
		});

		var content = scoped.File.ReadAllText("/docs/.git/.gitignore_global");

		Assert.Equal("*.log", content);
	}
}
