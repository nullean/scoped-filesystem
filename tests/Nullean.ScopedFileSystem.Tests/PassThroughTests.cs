// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information

using AwesomeAssertions;
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
}
