// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information

using System.IO.Abstractions.TestingHelpers;
using Xunit;

namespace Nullean.ScopedFileSystem.Tests;

public class SpecialFolderTests
{
	private static string TrimSep(string path) =>
		path.TrimEnd(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);

	private static string TempRoot => TrimSep(System.IO.Path.GetTempPath());

	private static string AppDataRoot =>
		TrimSep(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));

	// ── Temp folder ──────────────────────────────────────────────────────────

	[Fact]
	public void ReadAllText_InAllowedTempFolder_Succeeds()
	{
		var tempFile = $"{TempRoot}/scoped-test.txt";
		var mockFs = new MockFileSystem();
		mockFs.AddFile(tempFile, new MockFileData("temp content"));
		var scoped = new ScopedFileSystem(new ScopedFileSystemOptions("/docs")
		{
			Inner = mockFs,
			AllowedSpecialFolders = AllowedSpecialFolder.Temp,
		});

		var content = scoped.File.ReadAllText(tempFile);

		Assert.Equal("temp content", content);
	}

	[Fact]
	public void WriteAllText_InAllowedTempFolder_Succeeds()
	{
		var tempFile = $"{TempRoot}/scoped-write-test.txt";
		var mockFs = new MockFileSystem();
		mockFs.AddDirectory(TempRoot);
		var scoped = new ScopedFileSystem(new ScopedFileSystemOptions("/docs")
		{
			Inner = mockFs,
			AllowedSpecialFolders = AllowedSpecialFolder.Temp,
		});

		scoped.File.WriteAllText(tempFile, "written");

		Assert.Equal("written", mockFs.File.ReadAllText(tempFile));
	}

	[Fact]
	public void ReadAllText_InTempFolder_NotInAllowedList_Throws()
	{
		var tempFile = $"{TempRoot}/scoped-test.txt";
		var mockFs = new MockFileSystem();
		mockFs.AddFile(tempFile, new MockFileData("temp content"));
		// AllowedSpecialFolders is None (default) — temp path should be blocked
		var scoped = new ScopedFileSystem(mockFs, "/docs");

		Assert.Throws<ScopedFileSystemException>(() => scoped.File.ReadAllText(tempFile));
	}

	[Fact]
	public void Exists_InAllowedTempFolder_ReturnsTrue()
	{
		var tempFile = $"{TempRoot}/scoped-exists-test.txt";
		var mockFs = new MockFileSystem();
		mockFs.AddFile(tempFile, new MockFileData("x"));
		var scoped = new ScopedFileSystem(new ScopedFileSystemOptions("/docs")
		{
			Inner = mockFs,
			AllowedSpecialFolders = AllowedSpecialFolder.Temp,
		});

		Assert.True(scoped.File.Exists(tempFile));
	}

	[Fact]
	public void Exists_InTempFolder_NotAllowed_ReturnsFalse()
	{
		var tempFile = $"{TempRoot}/scoped-exists-test.txt";
		var mockFs = new MockFileSystem();
		mockFs.AddFile(tempFile, new MockFileData("x"));
		var scoped = new ScopedFileSystem(mockFs, "/docs");

		Assert.False(scoped.File.Exists(tempFile));
	}

	// ── Multiple special folders ─────────────────────────────────────────────

	[Fact]
	public void ReadAllText_MultipleAllowedSpecialFolders_BothAccessible()
	{
		// Skip if ApplicationData is not resolvable on this OS
		if (string.IsNullOrEmpty(AppDataRoot))
			return;

		var tempFile = $"{TempRoot}/multi-test.txt";
		var appDataFile = $"{AppDataRoot}/multi-test.txt";

		var mockFs = new MockFileSystem();
		mockFs.AddFile(tempFile, new MockFileData("from temp"));
		mockFs.AddFile(appDataFile, new MockFileData("from appdata"));

		var scoped = new ScopedFileSystem(new ScopedFileSystemOptions("/docs")
		{
			Inner = mockFs,
			AllowedSpecialFolders = AllowedSpecialFolder.Temp | AllowedSpecialFolder.ApplicationData,
		});

		Assert.Equal("from temp", scoped.File.ReadAllText(tempFile));
		Assert.Equal("from appdata", scoped.File.ReadAllText(appDataFile));
	}

	[Fact]
	public void AllowedSpecialFolder_All_GrantsAccessToAllFourFolders()
	{
		var tempFile = $"{TempRoot}/all-test.txt";
		var mockFs = new MockFileSystem();
		mockFs.AddFile(tempFile, new MockFileData("temp"));

		var scoped = new ScopedFileSystem(new ScopedFileSystemOptions("/docs")
		{
			Inner = mockFs,
			AllowedSpecialFolders = AllowedSpecialFolder.All,
		});

		Assert.Equal("temp", scoped.File.ReadAllText(tempFile));
	}

	// ── Hidden files inside special folders are not blocked ──────────────────

	[Fact]
	public void ReadAllText_HiddenFileInAllowedSpecialFolder_Succeeds()
	{
		// Special folder access bypasses all hidden checks
		var hiddenFile = $"{TempRoot}/.hidden-config";
		var mockFs = new MockFileSystem();
		mockFs.AddFile(hiddenFile, new MockFileData("hidden in temp"));
		var scoped = new ScopedFileSystem(new ScopedFileSystemOptions("/docs")
		{
			Inner = mockFs,
			AllowedSpecialFolders = AllowedSpecialFolder.Temp,
		});

		var content = scoped.File.ReadAllText(hiddenFile);

		Assert.Equal("hidden in temp", content);
	}
}
