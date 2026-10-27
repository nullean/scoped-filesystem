// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information

using AwesomeAssertions;
using System.IO.Abstractions.TestingHelpers;
using Xunit;

namespace Nullean.ScopedFileSystem.Tests;

public class PassThroughTests
{
	[Fact]
	public void Directory_CreateOutsideScope_Throws()
	{
		var (_, scoped) = Setup.Create("/docs");

		var act = () => scoped.Directory.CreateDirectory("/outside/new-dir");
		act.Should().Throw<ScopedFileSystemException>();
	}

	[Fact]
	public void OpenHandle_FileOutsideScope_Throws()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/etc/passwd", new MockFileData("secret"));

		var act = () => scoped.File.OpenHandle("/etc/passwd");
		act.Should().Throw<ScopedFileSystemException>();
	}

	[Fact]
	public void RandomAccess_ReturnsInnerRandomAccess()
	{
		var (mockFs, scoped) = Setup.Create("/docs");

		scoped.RandomAccess.Should().BeSameAs(mockFs.RandomAccess);
	}
}
