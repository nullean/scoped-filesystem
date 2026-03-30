// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information

using System.IO.Abstractions.TestingHelpers;
using Xunit;

namespace Nullean.ScopedFileSystem.Tests;

public class ScopedWriteTests
{
	[Fact]
	public void WriteAllText_FileWithinScope_Succeeds()
	{
		var (mockFs, scoped) = Setup.Create("/docs");

		scoped.File.WriteAllText("/docs/new.txt", "content");

		Assert.Equal("content", mockFs.File.ReadAllText("/docs/new.txt"));
	}

	[Fact]
	public void WriteAllText_FileOutsideScope_Throws()
	{
		var (_, scoped) = Setup.Create("/docs");

		Assert.Throws<ScopedFileSystemException>(() => scoped.File.WriteAllText("/etc/new.txt", "content"));
	}

	[Fact]
	public void AppendAllText_FileOutsideScope_Throws()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddDirectory("/etc");

		Assert.Throws<ScopedFileSystemException>(() => scoped.File.AppendAllText("/etc/log.txt", "entry"));
	}

	[Fact]
	public void AppendAllText_FileWithinScope_Succeeds()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/docs/log.txt", new MockFileData("first\n"));

		scoped.File.AppendAllText("/docs/log.txt", "second");

		Assert.Contains("second", mockFs.File.ReadAllText("/docs/log.txt"));
	}

	[Fact]
	public void Delete_FileOutsideScope_Throws()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/etc/target.txt", new MockFileData("data"));

		Assert.Throws<ScopedFileSystemException>(() => scoped.File.Delete("/etc/target.txt"));
	}

	[Fact]
	public void Delete_FileWithinScope_Succeeds()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/docs/deleteme.txt", new MockFileData("bye"));

		scoped.File.Delete("/docs/deleteme.txt");

		Assert.False(mockFs.File.Exists("/docs/deleteme.txt"));
	}

	[Fact]
	public void Copy_SourceOutsideScope_Throws()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/etc/source.txt", new MockFileData("secret"));

		Assert.Throws<ScopedFileSystemException>(() => scoped.File.Copy("/etc/source.txt", "/docs/dest.txt"));
	}

	[Fact]
	public void Copy_DestOutsideScope_Throws()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/docs/source.txt", new MockFileData("data"));
		mockFs.AddDirectory("/etc");

		Assert.Throws<ScopedFileSystemException>(() => scoped.File.Copy("/docs/source.txt", "/etc/dest.txt"));
	}

	[Fact]
	public void Move_SourceOutsideScope_Throws()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/etc/source.txt", new MockFileData("secret"));

		Assert.Throws<ScopedFileSystemException>(() => scoped.File.Move("/etc/source.txt", "/docs/dest.txt"));
	}

	[Fact]
	public void Move_DestOutsideScope_Throws()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/docs/source.txt", new MockFileData("data"));
		mockFs.AddDirectory("/etc");

		Assert.Throws<ScopedFileSystemException>(() => scoped.File.Move("/docs/source.txt", "/etc/dest.txt"));
	}

	[Fact]
	public void Create_FileOutsideScope_Throws()
	{
		var (_, scoped) = Setup.Create("/docs");

		Assert.Throws<ScopedFileSystemException>(() => scoped.File.Create("/etc/new.txt"));
	}

	[Fact]
	public void OpenWrite_FileOutsideScope_Throws()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/etc/existing.txt", new MockFileData("data"));

		Assert.Throws<ScopedFileSystemException>(() => scoped.File.OpenWrite("/etc/existing.txt"));
	}

	[Fact]
	public async Task WriteAllTextAsync_FileOutsideScope_Throws()
	{
		var (_, scoped) = Setup.Create("/docs");

		await Assert.ThrowsAsync<ScopedFileSystemException>(
			() => scoped.File.WriteAllTextAsync("/etc/new.txt", "content"));
	}

	[Fact]
	public async Task AppendAllBytesAsync_FileOutsideScope_Throws()
	{
		var (_, scoped) = Setup.Create("/docs");

		await Assert.ThrowsAsync<ScopedFileSystemException>(
			() => scoped.File.AppendAllBytesAsync("/etc/new.txt", new byte[] { 1, 2, 3 }));
	}

	[Fact]
	public void Replace_DestOutsideScope_Throws()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/docs/source.txt", new MockFileData("new"));
		mockFs.AddDirectory("/etc");

		Assert.Throws<ScopedFileSystemException>(() =>
			scoped.File.Replace("/docs/source.txt", "/etc/dest.txt", null));
	}

	[Fact]
	public void SetAttributes_FileOutsideScope_Throws()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/etc/file.txt", new MockFileData("data"));

		Assert.Throws<ScopedFileSystemException>(() =>
			scoped.File.SetAttributes("/etc/file.txt", FileAttributes.ReadOnly));
	}
}
