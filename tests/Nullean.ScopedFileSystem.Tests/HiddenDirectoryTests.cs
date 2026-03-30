// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information

using System.IO.Abstractions.TestingHelpers;
using Xunit;

namespace Nullean.ScopedFileSystem.Tests;

public class HiddenDirectoryTests
{
	[Fact]
	public void ReadAllText_FileUnderHiddenDir_Throws()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/docs/.hidden/secret.txt", new MockFileData("secret"));

		Assert.Throws<ScopedFileSystemException>(() => scoped.File.ReadAllText("/docs/.hidden/secret.txt"));
	}

	[Fact]
	public void WriteAllText_FileUnderHiddenDir_Throws()
	{
		var (_, scoped) = Setup.Create("/docs");

		Assert.Throws<ScopedFileSystemException>(() =>
			scoped.File.WriteAllText("/docs/.hidden/evil.txt", "pwned"));
	}

	[Fact]
	public void ReadAllText_DeepHiddenAncestor_Throws()
	{
		var (mockFs, scoped) = Setup.Create("/docs");
		mockFs.AddFile("/docs/sub/.hidden/deep.txt", new MockFileData("deep secret"));

		Assert.Throws<ScopedFileSystemException>(() =>
			scoped.File.ReadAllText("/docs/sub/.hidden/deep.txt"));
	}
}
