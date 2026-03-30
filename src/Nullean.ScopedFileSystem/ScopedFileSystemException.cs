// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information

using System.Security;

namespace Nullean.ScopedFileSystem;

/// <summary>Thrown when a file access operation attempts to read outside the configured scope root.</summary>
public class ScopedFileSystemException : SecurityException
{
	public ScopedFileSystemException(string message) : base(message) { }
}
