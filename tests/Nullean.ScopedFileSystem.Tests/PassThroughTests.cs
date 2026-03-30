// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information

using Xunit;

namespace Nullean.ScopedFileSystem.Tests;

public class PassThroughTests
{
	[Fact]
	public void Directory_CreateOutsideScope_Succeeds()
	{
		var (mockFs, scoped) = Setup.Create("/docs");

		var ex = Record.Exception(() => scoped.Directory.CreateDirectory("/outside/new-dir"));

		Assert.Null(ex);
		Assert.True(mockFs.Directory.Exists("/outside/new-dir"));
	}
}
