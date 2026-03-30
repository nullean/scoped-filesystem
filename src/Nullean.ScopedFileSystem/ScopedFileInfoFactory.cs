// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information

using System.IO.Abstractions;

namespace Nullean.ScopedFileSystem;

/// <summary>
/// An <see cref="IFileInfoFactory"/> decorator that returns <see cref="ScopedFileInfo"/> instances
/// so that all file operations are subject to scope and symlink validation.
/// </summary>
public class ScopedFileInfoFactory(IFileInfoFactory inner, IFileSystem innerFs, IReadOnlyList<string> scopeRoots) : IFileInfoFactory
{
	public IFileSystem FileSystem => innerFs;

	public IFileInfo New(string fileName) =>
		new ScopedFileInfo(inner.New(fileName), innerFs, scopeRoots);

	public IFileInfo? Wrap(FileInfo? fileInfo) =>
		fileInfo is null ? null : new ScopedFileInfo(inner.Wrap(fileInfo)!, innerFs, scopeRoots);
}
