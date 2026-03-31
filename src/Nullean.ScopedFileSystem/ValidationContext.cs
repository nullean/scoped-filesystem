// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;

namespace Nullean.ScopedFileSystem;

/// <summary>
/// Immutable, pre-normalized validation parameters threaded through
/// <see cref="ScopedFile"/>, <see cref="ScopedFileInfo"/>, and <see cref="PathValidator"/>.
/// Built once by <see cref="ScopedFileSystem"/> and then treated as read-only.
/// </summary>
internal sealed record ValidationContext(
	IReadOnlyList<string> NormalizedRoots,
	IReadOnlyList<string> ResolvedSpecialFolderPaths,
	IReadOnlySet<string> AllowedHiddenFileNames,
	IReadOnlySet<string> AllowedHiddenFolderNames,
	bool VerboseExceptions = false
)
{
	/// <summary>
	/// Converts a caller-supplied allow-list into an internal set using the platform-appropriate
	/// string comparer (ordinal on case-sensitive filesystems, ordinal-ignore-case elsewhere).
	/// </summary>
	internal static IReadOnlySet<string> ToAllowSet(IReadOnlyCollection<string> names)
	{
		var comparer = FileSystemPlatform.IsCaseSensitiveFileSystem
			? StringComparer.Ordinal
			: StringComparer.OrdinalIgnoreCase;
#if NETSTANDARD2_0 || NETSTANDARD2_1
		return new ReadOnlySetWrapper<string>(names, comparer);
#else
		return new HashSet<string>(names, comparer);
#endif
	}

	/// <summary>Resolves the <see cref="AllowedSpecialFolder"/> flags to normalized absolute paths.</summary>
	internal static IReadOnlyList<string> ResolveSpecialFolderPaths(AllowedSpecialFolder flags)
	{
		if (flags == AllowedSpecialFolder.None)
			return [];

		var paths = new List<string>(4);

		if (flags.HasFlag(AllowedSpecialFolder.Temp))
			Add(paths, System.IO.Path.GetTempPath());

		if (flags.HasFlag(AllowedSpecialFolder.ApplicationData))
			Add(paths, Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));

		if (flags.HasFlag(AllowedSpecialFolder.LocalApplicationData))
			Add(paths, Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));

		if (flags.HasFlag(AllowedSpecialFolder.CommonApplicationData))
			Add(paths, Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));

		return paths;

		static void Add(List<string> list, string path)
		{
			if (!string.IsNullOrEmpty(path))
				list.Add(path.TrimEnd(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar));
		}
	}
}
