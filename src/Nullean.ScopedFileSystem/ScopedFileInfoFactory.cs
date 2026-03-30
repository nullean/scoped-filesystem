// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information

using System.IO;
using System.IO.Abstractions;

namespace Nullean.ScopedFileSystem;

/// <summary>
/// An <see cref="IFileInfoFactory"/> decorator that returns <see cref="ScopedFileInfo"/> instances
/// so that all file operations are subject to scope and symlink validation.
/// </summary>
internal class ScopedFileInfoFactory(IFileInfoFactory inner, IFileSystem innerFs, ValidationContext ctx) : IFileInfoFactory
{
	public IFileSystem FileSystem => innerFs;

	public IFileInfo New(string fileName) =>
		new ScopedFileInfo(inner.New(fileName), innerFs, ctx);

	public IFileInfo? Wrap(FileInfo? fileInfo) =>
		fileInfo is null ? null : new ScopedFileInfo(inner.Wrap(fileInfo)!, innerFs, ctx);
}
