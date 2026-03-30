// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information

using System.IO.Abstractions;

namespace Nullean.ScopedFileSystem;

/// <summary>
/// Configuration for <see cref="ScopedFileSystem"/>. All settings except
/// <see cref="ScopeRoots"/> are optional and fall back to secure defaults
/// (everything denied unless explicitly allowed).
/// </summary>
/// <remarks>
/// <para>
/// Construct with one or more scope root paths, then use <c>with</c>-style
/// object-initializer syntax to set optional properties:
/// </para>
/// <code>
/// var options = new ScopedFileSystemOptions("/my/project")
/// {
///     AllowedHiddenFolderNames = new HashSet&lt;string&gt; { ".git", ".nuget" },
///     AllowedSpecialFolders    = AllowedSpecialFolder.Temp | AllowedSpecialFolder.ApplicationData,
/// };
/// var fs = new ScopedFileSystem(options);
/// </code>
/// </remarks>
public sealed class ScopedFileSystemOptions
{
	/// <summary>
	/// Initialises options with one or more scope root paths supplied as strings.
	/// </summary>
	/// <param name="scopeRoots">
	/// One or more directory paths that define the allowed scope. At least one is required.
	/// Paths are normalised to absolute, canonical form by <see cref="ScopedFileSystem"/>.
	/// </param>
	/// <exception cref="ArgumentException">Thrown when no roots are provided.</exception>
	public ScopedFileSystemOptions(params string[] scopeRoots)
	{
		if (scopeRoots is null || scopeRoots.Length == 0)
			throw new ArgumentException("At least one scope root must be provided.", nameof(scopeRoots));
		ScopeRoots = scopeRoots;
	}

	/// <summary>
	/// Initialises options with one or more scope root paths supplied as <see cref="IDirectoryInfo"/> instances.
	/// </summary>
	/// <param name="scopeRoots">
	/// One or more directory infos whose <see cref="IFileSystemInfo.FullName"/> values are used as scope roots.
	/// At least one is required.
	/// </param>
	/// <exception cref="ArgumentException">Thrown when no roots are provided.</exception>
	public ScopedFileSystemOptions(params IDirectoryInfo[] scopeRoots)
		: this(scopeRoots is null || scopeRoots.Length == 0
			? []
			: scopeRoots.Select(d => d.FullName).ToArray())
	{
	}

	/// <summary>
	/// The root directory paths that scope all file read and write access.
	/// No root may be an ancestor of another; <see cref="ScopedFileSystem"/> enforces disjointness at construction.
	/// </summary>
	public IReadOnlyList<string> ScopeRoots { get; }

	/// <summary>
	/// The underlying filesystem implementation. Defaults to the real <see cref="FileSystem"/>.
	/// Override with a <c>MockFileSystem</c> in tests.
	/// </summary>
	public IFileSystem Inner { get; init; } = new FileSystem();

	/// <summary>
	/// Hidden file names (names starting with <c>.</c>) that are exempt from the hidden-file protection.
	/// The check is against the file's own name segment only (not its ancestor directories).
	/// Default comparer is <see cref="StringComparer.OrdinalIgnoreCase"/>; supply your own <see cref="HashSet{T}"/>
	/// with <see cref="StringComparer.Ordinal"/> when running on a case-sensitive filesystem.
	/// </summary>
	/// <example><c>new HashSet&lt;string&gt; { ".gitkeep" }</c></example>
	public IReadOnlySet<string> AllowedHiddenFileNames { get; init; }
		= new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	/// <summary>
	/// Hidden directory names (names starting with <c>.</c>) that are exempt from the hidden-directory protection.
	/// The check is against each ancestor directory's name as the validator walks up from the target file
	/// to the matched scope root.
	/// Default comparer is <see cref="StringComparer.OrdinalIgnoreCase"/>; supply your own <see cref="HashSet{T}"/>
	/// with <see cref="StringComparer.Ordinal"/> when running on a case-sensitive filesystem.
	/// </summary>
	/// <example><c>new HashSet&lt;string&gt; { ".git", ".nuget" }</c></example>
	public IReadOnlySet<string> AllowedHiddenFolderNames { get; init; }
		= new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	/// <summary>
	/// OS special folders that are additionally permitted for read and write access,
	/// even when they fall outside all configured <see cref="ScopeRoots"/>.
	/// Combine multiple values with the bitwise OR operator (<c>|</c>).
	/// Paths are resolved at <see cref="ScopedFileSystem"/> construction time; access to these folders
	/// bypasses all hidden-path checks.
	/// </summary>
	/// <example><c>AllowedSpecialFolder.Temp | AllowedSpecialFolder.ApplicationData</c></example>
	public AllowedSpecialFolder AllowedSpecialFolders { get; init; } = AllowedSpecialFolder.None;
}
