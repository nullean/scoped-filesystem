// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information

using System.IO.Abstractions.TestingHelpers;
using Xunit;

namespace Nullean.ScopedFileSystem.Tests;

public class ExistsTests
{
	[Fact]
	public void Exists_FileWithinScope_ReturnsTrue()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/docs/readme.md", new MockFileData("hello"));

		Assert.True(scoped.File.Exists("/docs/readme.md"));
	}

	[Fact]
	public void Exists_FileOutsideScope_ReturnsFalseWithoutThrowing()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/etc/passwd", new MockFileData("secret"));

		// Must not throw — callers use Exists to probe paths
		Assert.False(scoped.File.Exists("/etc/passwd"));
	}

	[Fact]
	public void Exists_NullPath_ReturnsFalse()
	{
		var (_, scoped) = Setup.Create("/docs");

		Assert.False(scoped.File.Exists(null));
	}
}
