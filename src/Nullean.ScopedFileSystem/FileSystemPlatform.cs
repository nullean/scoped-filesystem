// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information

namespace Nullean.ScopedFileSystem;

/// <summary>Provides platform-level filesystem characteristics.</summary>
public static class FileSystemPlatform
{
    /// <summary>Returns true when running on a case-sensitive filesystem (Linux).</summary>
    public static bool IsCaseSensitiveFileSystem { get; } =
#if NET5_0_OR_GREATER
        OperatingSystem.IsLinux();
#else
        System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux);
#endif
}
