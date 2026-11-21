using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using ArchiSteamFarm;
using ArchiSteamFarm.Core;

namespace SteadyAchievements.Storage;

// Small JSON files in config/SteadyAchievements/, written atomically (temp file + rename).
internal static class JsonStore {
	internal static readonly string Directory = Path.Combine(SharedInfo.ConfigDirectory, nameof(SteadyAchievements));

	// The resolver is set explicitly: trimmed .NET apps (ASF's OS-specific builds) turn reflection-based
	// System.Text.Json off by default, and ASF itself opts back in the same way.
	internal static readonly JsonSerializerOptions Options = new() {
		WriteIndented = true,
		PropertyNameCaseInsensitive = true,
		Converters = { new StepStateConverter(), new HistoryKindConverter() },
		TypeInfoResolver = new DefaultJsonTypeInfoResolver()
	};

	internal static readonly JsonSerializerOptions Compact = new() {
		TypeInfoResolver = new DefaultJsonTypeInfoResolver()
	};

	internal static async Task<T?> Load<T>(string fileName) where T : class {
		string path = Path.Combine(Directory, fileName);

		if (!File.Exists(path)) {
			return null;
		}

		try {
			await using FileStream stream = File.OpenRead(path);

			return await JsonSerializer.DeserializeAsync<T>(stream, Options).ConfigureAwait(false);
		} catch (Exception e) when (e is IOException or JsonException or UnauthorizedAccessException) {
			ASF.ArchiLogger.LogGenericWarning($"[{nameof(SteadyAchievements)}] {fileName}: {e.Message}");

			// Keep the unreadable file for inspection instead of silently overwriting it
			try {
				File.Copy(path, $"{path}.bozuk", true);
			} catch (IOException) {
				// Best effort only
			}

			return null;
		}
	}

	internal static async Task Save<T>(string fileName, T value) {
		System.IO.Directory.CreateDirectory(Directory);

		string path = Path.Combine(Directory, fileName);
		string temp = $"{path}.tmp";

		await using (FileStream stream = File.Create(temp)) {
			await JsonSerializer.SerializeAsync(stream, value, Options).ConfigureAwait(false);
		}

		File.Move(temp, path, true);
	}
}
