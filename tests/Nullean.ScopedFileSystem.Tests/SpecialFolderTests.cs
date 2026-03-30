// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information

using AwesomeAssertions;
using System.IO.Abstractions.TestingHelpers;
using Xunit;

namespace Nullean.ScopedFileSystem.Tests;

public class SpecialFolderTests
{
	private static string TrimSep(string path) =>
		path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

	private static string TempRoot => TrimSep(Path.GetTempPath());

	private static string AppDataRoot =>
		TrimSep(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));

	// ── Temp folder ──────────────────────────────────────────────────────────

	[Fact]
	public void ReadAllText_InAllowedTempFolder_Succeeds()
	{
		var tempFile = $"{TempRoot}/scoped-test.txt";
		var mockFs = new MockFileSystem();
		mockFs.AddFile(tempFile, new MockFileData("temp content"));
		var scoped = new ScopedFileSystem(mockFs, new ScopedFileSystemOptions("/docs")
		{
			AllowedSpecialFolders = AllowedSpecialFolder.Temp,
		});

		scoped.File.ReadAllText(tempFile).Should().Be("temp content");
	}

	[Fact]
	public void WriteAllText_InAllowedTempFolder_Succeeds()
	{
		var tempFile = $"{TempRoot}/scoped-write-test.txt";
		var mockFs = new MockFileSystem();
		mockFs.AddDirectory(TempRoot);
		var scoped = new ScopedFileSystem(mockFs, new ScopedFileSystemOptions("/docs")
		{
			AllowedSpecialFolders = AllowedSpecialFolder.Temp,
		});

		scoped.File.WriteAllText(tempFile, "written");

		mockFs.File.ReadAllText(tempFile).Should().Be("written");
	}

	[Fact]
	public void ReadAllText_InTempFolder_NotInAllowedList_Throws()
	{
		var tempFile = $"{TempRoot}/scoped-test.txt";
		var mockFs = new MockFileSystem();
		mockFs.AddFile(tempFile, new MockFileData("temp content"));
		var scoped = new ScopedFileSystem(mockFs, "/docs");

		var act = () => scoped.File.ReadAllText(tempFile);
		act.Should().Throw<ScopedFileSystemException>();
	}

	[Fact]
	public void Exists_InAllowedTempFolder_ReturnsTrue()
	{
		var tempFile = $"{TempRoot}/scoped-exists-test.txt";
		var mockFs = new MockFileSystem();
		mockFs.AddFile(tempFile, new MockFileData("x"));
		var scoped = new ScopedFileSystem(mockFs, new ScopedFileSystemOptions("/docs")
		{
			AllowedSpecialFolders = AllowedSpecialFolder.Temp,
		});

		scoped.File.Exists(tempFile).Should().BeTrue();
	}

	[Fact]
	public void Exists_InTempFolder_NotAllowed_ReturnsFalse()
	{
		var tempFile = $"{TempRoot}/scoped-exists-test.txt";
		var mockFs = new MockFileSystem();
		mockFs.AddFile(tempFile, new MockFileData("x"));
		var scoped = new ScopedFileSystem(mockFs, "/docs");

		scoped.File.Exists(tempFile).Should().BeFalse();
	}

	// ── Multiple special folders ─────────────────────────────────────────────

	[Fact]
	public void ReadAllText_MultipleAllowedSpecialFolders_BothAccessible()
	{
		if (string.IsNullOrEmpty(AppDataRoot))
			return;

		var tempFile = $"{TempRoot}/multi-test.txt";
		var appDataFile = $"{AppDataRoot}/multi-test.txt";
		var mockFs = new MockFileSystem();
		mockFs.AddFile(tempFile, new MockFileData("from temp"));
		mockFs.AddFile(appDataFile, new MockFileData("from appdata"));
		var scoped = new ScopedFileSystem(mockFs, new ScopedFileSystemOptions("/docs")
		{
			AllowedSpecialFolders = AllowedSpecialFolder.Temp | AllowedSpecialFolder.ApplicationData,
		});

		scoped.File.ReadAllText(tempFile).Should().Be("from temp");
		scoped.File.ReadAllText(appDataFile).Should().Be("from appdata");
	}

	[Fact]
	public void AllowedSpecialFolder_All_GrantsAccessToAllFourFolders()
	{
		var tempFile = $"{TempRoot}/all-test.txt";
		var mockFs = new MockFileSystem();
		mockFs.AddFile(tempFile, new MockFileData("temp"));
		var scoped = new ScopedFileSystem(mockFs, new ScopedFileSystemOptions("/docs")
		{
			AllowedSpecialFolders = AllowedSpecialFolder.All,
		});

		scoped.File.ReadAllText(tempFile).Should().Be("temp");
	}

	// ── Hidden files inside special folders are not blocked ──────────────────

	[Fact]
	public void ReadAllText_HiddenFileInAllowedSpecialFolder_Succeeds()
	{
		var hiddenFile = $"{TempRoot}/.hidden-config";
		var mockFs = new MockFileSystem();
		mockFs.AddFile(hiddenFile, new MockFileData("hidden in temp"));
		var scoped = new ScopedFileSystem(mockFs, new ScopedFileSystemOptions("/docs")
		{
			AllowedSpecialFolders = AllowedSpecialFolder.Temp,
		});

		scoped.File.ReadAllText(hiddenFile).Should().Be("hidden in temp");
	}
}
