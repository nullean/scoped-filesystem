// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information

using System.IO;
using System.IO.Abstractions;

namespace Nullean.ScopedFileSystem;

/// <summary>
/// An <see cref="IDirectoryInfoFactory"/> decorator that returns <see cref="ScopedDirectoryInfo"/> instances
/// so that all directory operations are subject to scope and symlink validation.
/// </summary>
internal class ScopedDirectoryInfoFactory(IDirectoryInfoFactory inner, IFileSystem innerFs, ValidationContext ctx)
	: IDirectoryInfoFactory
{
	public IFileSystem FileSystem => innerFs;

	public IDirectoryInfo New(string path) =>
		new ScopedDirectoryInfo(inner.New(path), innerFs, ctx);

	public IDirectoryInfo? Wrap(DirectoryInfo? directoryInfo) =>
		directoryInfo is null ? null : new ScopedDirectoryInfo(inner.Wrap(directoryInfo)!, innerFs, ctx);
}
