// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information

using System.IO.Abstractions.TestingHelpers;

namespace Nullean.ScopedFileSystem.Tests;

internal static class Setup
{
	public static (MockFileSystem MockFs, ScopedFileSystem Scoped) Create(params string[] roots)
	{
		var mockFs = new MockFileSystem();
		foreach (var root in roots)
			mockFs.AddDirectory(root);
		return (mockFs, new ScopedFileSystem(mockFs, roots));
	}
}
