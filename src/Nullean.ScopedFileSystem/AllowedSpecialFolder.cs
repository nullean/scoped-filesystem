// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information

namespace Nullean.ScopedFileSystem;

/// <summary>
/// OS special folders that may be granted read and write access outside the configured scope roots.
/// Values are bit flags and can be combined with the bitwise OR operator (<c>|</c>).
/// </summary>
/// <remarks>
/// Only the four folders that are meaningful on all three major platforms (macOS, Windows, Linux)
/// are exposed. Paths are resolved at <see cref="ScopedFileSystem"/> construction time via
/// <see cref="System.IO.Path.GetTempPath"/> (for <see cref="Temp"/>) and
/// <see cref="Environment.GetFolderPath(Environment.SpecialFolder)"/> for the rest.
/// If a folder path cannot be resolved on the current OS (empty string), it is silently ignored.
/// </remarks>
[Flags]
public enum AllowedSpecialFolder
{
	/// <summary>No special folders are allowed.</summary>
	None = 0,

	/// <summary>
	/// The system temporary directory.
	/// Resolved via <see cref="System.IO.Path.GetTempPath"/>.
	/// macOS: <c>/var/folders/…</c> or <c>/tmp</c> &nbsp;|&nbsp;
	/// Windows: <c>%TEMP%</c> &nbsp;|&nbsp;
	/// Linux: <c>/tmp</c>
	/// </summary>
	Temp = 1 << 0,

	/// <summary>
	/// Roaming user-specific application data.
	/// Resolved via <see cref="Environment.SpecialFolder.ApplicationData"/>.
	/// macOS: <c>~/Library/Application Support</c> &nbsp;|&nbsp;
	/// Windows: <c>%APPDATA%</c> &nbsp;|&nbsp;
	/// Linux: <c>~/.config</c>
	/// </summary>
	ApplicationData = 1 << 1,

	/// <summary>
	/// Local (non-roaming) user-specific application data.
	/// Resolved via <see cref="Environment.SpecialFolder.LocalApplicationData"/>.
	/// macOS: <c>~/Library/Application Support</c> &nbsp;|&nbsp;
	/// Windows: <c>%LOCALAPPDATA%</c> &nbsp;|&nbsp;
	/// Linux: <c>~/.local/share</c>
	/// </summary>
	LocalApplicationData = 1 << 2,

	/// <summary>
	/// Machine-wide application data shared across all users.
	/// Resolved via <see cref="Environment.SpecialFolder.CommonApplicationData"/>.
	/// macOS: <c>/Library/Application Support</c> &nbsp;|&nbsp;
	/// Windows: <c>C:\ProgramData</c> &nbsp;|&nbsp;
	/// Linux: <c>/usr/share</c>
	/// </summary>
	CommonApplicationData = 1 << 3,

	/// <summary>All four special folders combined.</summary>
	All = Temp | ApplicationData | LocalApplicationData | CommonApplicationData,
}
