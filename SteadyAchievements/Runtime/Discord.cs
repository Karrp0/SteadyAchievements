using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ArchiSteamFarm.Core;
using ArchiSteamFarm.Web.Responses;
using SteadyAchievements.Core;
using SteadyAchievements.Storage;

namespace SteadyAchievements.Runtime;

internal enum NoticeKind : byte {
	Unlock,
	Simulated,
	Completed,
	Summary,
	Problem,
	Test
}

// Sends embeds to a Discord webhook. Failures are logged and never break the plugin.
internal static class Discord {
	internal static async Task<bool> Send(string webhook, string botName, NoticeKind kind, string description, string language, bool simulation) {
		if (string.IsNullOrEmpty(webhook) || !BotSettings.IsDiscordWebhook(webhook) || (ASF.WebBrowser == null)) {
			return false;
		}

		string payload = BuildPayload(botName, kind, description, language, simulation);

		try {
			using StringContent content = new(payload, Encoding.UTF8, "application/json");
			BasicResponse? response = await ASF.WebBrowser.UrlPost(new Uri(webhook), data: content, maxTries: 2).ConfigureAwait(false);

			return response != null;
		} catch (Exception e) when (e is HttpRequestException or TaskCanceledException or UriFormatException) {
			ASF.ArchiLogger.LogGenericWarning($"[{nameof(SteadyAchievements)}] Discord: {e.Message}");

			return false;
		}
	}

	internal static string BuildPayload(string botName, NoticeKind kind, string description, string language, bool simulation) {
		ArgumentNullException.ThrowIfNull(description);

		(string titleKey, int color) = kind switch {
			NoticeKind.Unlock => ("dc.title.unlock", 0x2ECC71),
			NoticeKind.Simulated => ("dc.title.sim", 0x95A5A6),
			NoticeKind.Completed => ("dc.title.completed", 0xF1C40F),
			NoticeKind.Summary => ("dc.title.summary", 0x3498DB),
			NoticeKind.Problem => ("dc.title.problem", 0xE74C3C),
			_ => ("dc.title.test", 0x9B59B6)
		};

		Dictionary<string, object> payload = new() {
			["username"] = "SteadyAchievements",
			["embeds"] = new object[] {
				new Dictionary<string, object> {
					["title"] = Texts.Get(language, titleKey),
					["description"] = description.Length > 3900 ? description[..3900] : description,
					["color"] = color,
					["footer"] = new Dictionary<string, string> { ["text"] = $"{botName} · {Texts.Get(language, simulation ? "dc.footer.sim" : "dc.footer.real")}" },
					["timestamp"] = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture)
				}
			},
			["allowed_mentions"] = new Dictionary<string, object> { ["parse"] = Array.Empty<string>() }
		};

		return JsonSerializer.Serialize(payload, JsonStore.Compact);
	}
}
