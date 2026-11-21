using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Threading.Tasks;
using ArchiSteamFarm.Core;
using ArchiSteamFarm.Web.Responses;

namespace SteadyAchievements.Steam;

// Global unlock percentages (public Steam Web API, no key needed), cached in memory for a day.
internal static class GlobalPercents {
	private static readonly TimeSpan MaxAge = TimeSpan.FromHours(24);
	private static readonly ConcurrentDictionary<uint, (DateTime FetchedUtc, Dictionary<string, double> Percents)> Cache = new();

	// null when Steam could not be reached; an empty dictionary when the game has no data
	internal static async Task<Dictionary<string, double>?> Get(uint appId) {
		if (Cache.TryGetValue(appId, out (DateTime FetchedUtc, Dictionary<string, double> Percents) cached) && (DateTime.UtcNow - cached.FetchedUtc < MaxAge)) {
			return cached.Percents;
		}

		if (ASF.WebBrowser == null) {
			return null;
		}

		Uri uri = new($"https://api.steampowered.com/ISteamUserStats/GetGlobalAchievementPercentagesForApp/v2/?gameid={appId.ToString(CultureInfo.InvariantCulture)}");
		ObjectResponse<JsonElement>? response = await ASF.WebBrowser.UrlGetToJsonObject<JsonElement>(uri, maxTries: 2).ConfigureAwait(false);

		if (response == null) {
			return null;
		}

		Dictionary<string, double> percents = Parse(response.Content);
		Cache[appId] = (DateTime.UtcNow, percents);

		return percents;
	}

	internal static Dictionary<string, double> Parse(JsonElement root) {
		Dictionary<string, double> result = new(StringComparer.Ordinal);

		if ((root.ValueKind != JsonValueKind.Object) || !root.TryGetProperty("achievementpercentages", out JsonElement wrapper) || !wrapper.TryGetProperty("achievements", out JsonElement list) || (list.ValueKind != JsonValueKind.Array)) {
			return result;
		}

		foreach (JsonElement item in list.EnumerateArray()) {
			if (!item.TryGetProperty("name", out JsonElement name) || (name.GetString() is not { Length: > 0 } apiName) || !item.TryGetProperty("percent", out JsonElement percent)) {
				continue;
			}

			// The v2 endpoint sends the percentage as a string, older ones as a number
			double? value = percent.ValueKind switch {
				JsonValueKind.Number => percent.GetDouble(),
				JsonValueKind.String when double.TryParse(percent.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out double parsed) => parsed,
				_ => null
			};

			if (value is { } v && double.IsFinite(v)) {
				result[apiName] = v;
			}
		}

		return result;
	}
}
