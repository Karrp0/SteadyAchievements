using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArchiSteamFarm.Core;
using ArchiSteamFarm.Steam;
using SteadyAchievements.Core;
using SteadyAchievements.Storage;
using SteamKit2;

namespace SteadyAchievements.Steam;

// Store facts (type, categories, DLC list, developers, publishers) from Steam's app info (PICS),
// shared by all bots and cached on disk for a week. PICS goes over the bot's Steam connection,
// so there is no store web API rate limit to worry about.
internal static class AppInfo {
	private const string FileName = "appinfo.json";
	private const int BatchSize = 100;
	private static readonly TimeSpan MaxAge = TimeSpan.FromDays(7);
	private static readonly TimeSpan RetryFailedAfter = TimeSpan.FromMinutes(30);

	private static readonly ConcurrentDictionary<uint, AppFacts> Cache = new();
	private static readonly ConcurrentDictionary<uint, DateTime> FailedUntil = new();
	private static readonly SemaphoreSlim FetchLock = new(1, 1);
	private static bool Loaded;
	private static bool Dirty;

	internal static AppFacts? Peek(uint appId) => Cache.GetValueOrDefault(appId);

	internal static async Task<Dictionary<uint, AppFacts>> Get(Bot bot, IEnumerable<uint> appIds) {
		ArgumentNullException.ThrowIfNull(bot);
		ArgumentNullException.ThrowIfNull(appIds);

		await FetchLock.WaitAsync().ConfigureAwait(false);

		try {
			if (!Loaded) {
				await LoadFromDisk().ConfigureAwait(false);
			}

			DateTime now = DateTime.UtcNow;
			List<uint> wanted = appIds.Distinct().ToList();
			List<uint> missing = wanted.Where(id => (!Cache.TryGetValue(id, out AppFacts? facts) || (now - facts.FetchedUtc > MaxAge)) && (!FailedUntil.TryGetValue(id, out DateTime until) || (until < now))).ToList();

			for (int i = 0; i < missing.Count; i += BatchSize) {
				List<uint> batch = missing.GetRange(i, Math.Min(BatchSize, missing.Count - i));

				if (!await FetchBatch(bot, batch).ConfigureAwait(false)) {
					foreach (uint id in batch) {
						FailedUntil[id] = now + RetryFailedAfter;
					}
				}
			}

			if (Dirty) {
				await SaveToDisk().ConfigureAwait(false);
			}

			return wanted.Where(Cache.ContainsKey).ToDictionary(static id => id, static id => Cache[id]);
		} finally {
			FetchLock.Release();
		}
	}

	private static async Task<bool> FetchBatch(Bot bot, List<uint> appIds) {
		if (!bot.IsConnectedAndLoggedOn) {
			return false;
		}

		try {
			SteamApps.PICSTokensCallback tokens = await bot.SteamApps.PICSGetAccessTokens(appIds, []).ToLongRunningTask().ConfigureAwait(false);
			List<SteamApps.PICSRequest> requests = appIds.Select(id => new SteamApps.PICSRequest(id, tokens.AppTokens.GetValueOrDefault(id))).ToList();
			AsyncJobMultiple<SteamApps.PICSProductInfoCallback>.ResultSet resultSet = await bot.SteamApps.PICSGetProductInfo(requests, []).ToLongRunningTask().ConfigureAwait(false);

			if (resultSet.Results == null) {
				return false;
			}

			DateTime now = DateTime.UtcNow;
			HashSet<uint> seen = [];

			foreach (SteamApps.PICSProductInfoCallback result in resultSet.Results) {
				foreach ((uint appId, SteamApps.PICSProductInfoCallback.PICSProductInfo info) in result.Apps) {
					if (info.KeyValues != KeyValue.Invalid) {
						Cache[appId] = Parse(appId, info.KeyValues, now);
						seen.Add(appId);
					}
				}

				// Removed or hidden apps: remember that they exist without store data
				foreach (uint appId in result.UnknownApps) {
					Cache[appId] = new AppFacts(appId, "", "unknown", [], [], [], [], now);
					seen.Add(appId);
				}
			}

			Dirty |= seen.Count > 0;

			return seen.Count > 0;
		} catch (Exception e) when (e is TaskCanceledException or TimeoutException or InvalidOperationException) {
			bot.ArchiLogger.LogGenericWarning($"[{nameof(SteadyAchievements)}] PICS: {e.Message}");

			return false;
		}
	}

	internal static AppFacts Parse(uint appId, KeyValue root, DateTime fetchedUtc) {
		ArgumentNullException.ThrowIfNull(root);

		KeyValue common = root["common"];
		KeyValue extended = root["extended"];

		HashSet<int> categories = [];

		foreach (KeyValue category in common["category"].Children) {
			// "category_22": "1"
			if ((category.Name?.StartsWith("category_", StringComparison.Ordinal) == true) && int.TryParse(category.Name.AsSpan(9), NumberStyles.None, CultureInfo.InvariantCulture, out int id) && (category.Value != "0")) {
				categories.Add(id);
			}
		}

		List<uint> dlcs = [];

		foreach (string part in (extended["listofdlc"].Value ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)) {
			if (uint.TryParse(part, NumberStyles.None, CultureInfo.InvariantCulture, out uint dlc) && (dlc != 0)) {
				dlcs.Add(dlc);
			}
		}

		List<string> developers = [];
		List<string> publishers = [];

		foreach (KeyValue association in common["associations"].Children) {
			string? name = association["name"].Value;

			if (string.IsNullOrWhiteSpace(name)) {
				continue;
			}

			switch (association["type"].Value) {
				case "developer":
					developers.Add(name.Trim());

					break;
				case "publisher":
					publishers.Add(name.Trim());

					break;
			}
		}

		AddIfMissing(developers, extended["developer"].Value);
		AddIfMissing(publishers, extended["publisher"].Value);

		return new AppFacts(appId, common["name"].Value ?? "", (common["type"].Value ?? "").ToUpperInvariant() switch {
			"GAME" => "game",
			"DLC" => "dlc",
			"" => "unknown",
			string other => other.ToLowerInvariant()
		}, [.. categories], [.. dlcs.Distinct()], [.. developers], [.. publishers], fetchedUtc) {
			HeaderImage = ImageFile(common["header_image"]),
			CapsuleImage = ImageFile(common["small_capsule"])
		};
	}

	// "header_image" { "english" "<file>" ... }: prefer English, else the first language given
	private static string ImageFile(KeyValue node) {
		string? file = node["english"].Value ?? node.Value ?? node.Children.Select(static child => child.Value).FirstOrDefault(static value => !string.IsNullOrEmpty(value));

		// Only a relative file path is expected; anything else is ignored
		return string.IsNullOrWhiteSpace(file) || file.Contains("..", StringComparison.Ordinal) || file.Contains(':', StringComparison.Ordinal) ? "" : file.Trim().TrimStart('/');
	}

	private static void AddIfMissing(List<string> list, string? name) {
		if (!string.IsNullOrWhiteSpace(name) && !list.Contains(name.Trim(), StringComparer.OrdinalIgnoreCase)) {
			list.Add(name.Trim());
		}
	}

	private static async Task LoadFromDisk() {
		Loaded = true;
		List<FactsDto>? stored = await JsonStore.Load<List<FactsDto>>(FileName).ConfigureAwait(false);

		foreach (FactsDto dto in stored ?? []) {
			// Entries saved before image names were stored: refresh them on next use
			DateTime fetched = (dto.Type == "game") && (dto.HeaderImage.Length == 0) ? DateTime.MinValue : dto.FetchedUtc;

			Cache[dto.AppId] = new AppFacts(dto.AppId, dto.Name, dto.Type, [.. dto.Categories], [.. dto.Dlcs], [.. dto.Developers], [.. dto.Publishers], fetched) {
				HeaderImage = dto.HeaderImage,
				CapsuleImage = dto.CapsuleImage
			};
		}
	}

	private static async Task SaveToDisk() {
		List<FactsDto> dtos = Cache.Values.OrderBy(static f => f.AppId).Select(static f => new FactsDto {
			AppId = f.AppId,
			Name = f.Name,
			Type = f.Type,
			Categories = [.. f.Categories.Order()],
			Dlcs = [.. f.Dlcs],
			Developers = [.. f.Developers],
			Publishers = [.. f.Publishers],
			FetchedUtc = f.FetchedUtc,
			HeaderImage = f.HeaderImage,
			CapsuleImage = f.CapsuleImage
		}).ToList();

		await JsonStore.Save(FileName, dtos).ConfigureAwait(false);
		Dirty = false;
	}

	private sealed class FactsDto {
		public uint AppId { get; set; }
		public string Name { get; set; } = "";
		public string Type { get; set; } = "";
		public List<int> Categories { get; set; } = [];
		public List<uint> Dlcs { get; set; } = [];
		public List<string> Developers { get; set; } = [];
		public List<string> Publishers { get; set; } = [];
		public DateTime FetchedUtc { get; set; }
		public string HeaderImage { get; set; } = "";
		public string CapsuleImage { get; set; } = "";
	}
}
