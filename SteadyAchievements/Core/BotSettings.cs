using System;
using System.Collections.Generic;
using System.Linq;

namespace SteadyAchievements.Core;

// Per-bot settings, stored in config/SteadyAchievements/<bot>.settings.json and edited from the panel.
// Plain mutable properties keep JSON round-trips simple; always Normalize() + Validate() before use.
public sealed class BotSettings {
	// Wide enough for the panel's custom pace: a 50% achievement after 1 minute up to ~11 hours of play
	internal const double MinPace = 0.04;
	internal const double MaxPace = 30;
	internal const int MaxActiveGamesLimit = 10;

	// How it runs
	public bool Enabled { get; set; } = true;
	public bool Simulation { get; set; } = true;
	public string Language { get; set; } = "tr";

	// Pace: multiplies how many hours of play each achievement needs (1 = normal, below 1 = faster)
	public double PaceScale { get; set; } = 1;
	public string ActiveHours { get; set; } = "10:00-01:00";
	public string? TimeZone { get; set; }
	public int MaxActiveGames { get; set; } = 2;
	public int MinGapMinutes { get; set; } = 20;
	public int MaxPerDay { get; set; } = 8;
	public double MinPlaytimeHours { get; set; } = 1;
	public double SkipRarerThan { get; set; }

	// Which games
	public bool ExcludeVac { get; set; } = true;
	public bool ExcludeOnline { get; set; } = true;
	public bool ProtectDlc { get; set; } = true;
	public bool RespectAsfBlacklist { get; set; } = true;
	public List<string> BlockedDevelopers { get; set; } = [];
	public List<string> BlockedPublishers { get; set; } = [];
	public List<uint> Blacklist { get; set; } = [];
	public List<uint> Allowlist { get; set; } = [];

	// Auto play: pick games from the library while ASF has nothing to farm
	public bool AutoPlay { get; set; }
	public int AutoPlayRotateHours { get; set; } = 4;

	// Discord
	public string DiscordWebhook { get; set; } = "";
	public bool NotifyCompleted { get; set; } = true;
	public bool NotifyEachUnlock { get; set; }
	public bool NotifyDailySummary { get; set; } = true;
	public int DailySummaryHour { get; set; } = 23;
	public bool NotifyProblems { get; set; } = true;

	internal BotSettings Clone() {
		BotSettings copy = (BotSettings) MemberwiseClone();
		copy.BlockedDevelopers = [.. BlockedDevelopers];
		copy.BlockedPublishers = [.. BlockedPublishers];
		copy.Blacklist = [.. Blacklist];
		copy.Allowlist = [.. Allowlist];

		return copy;
	}

	// Fixes harmless shape problems (nulls, duplicates, whitespace) that JSON or the panel may send.
	internal void Normalize() {
		Language = Texts.NormalizeLanguage(Language);
		ActiveHours = (ActiveHours ?? "").Trim();
		TimeZone = string.IsNullOrWhiteSpace(TimeZone) ? null : TimeZone.Trim();
		DiscordWebhook = (DiscordWebhook ?? "").Trim();
		BlockedDevelopers = CleanNames(BlockedDevelopers);
		BlockedPublishers = CleanNames(BlockedPublishers);
		Blacklist = (Blacklist ?? []).Where(static id => id != 0).Distinct().OrderBy(static id => id).ToList();
		Allowlist = (Allowlist ?? []).Where(static id => id != 0).Distinct().OrderBy(static id => id).ToList();
	}

	internal List<string> Validate(Func<string?, bool> isValidTimeZone) {
		ArgumentNullException.ThrowIfNull(isValidTimeZone);

		string lang = Language;
		List<string> errors = [];

		if (double.IsNaN(PaceScale) || PaceScale is < MinPace or > MaxPace) {
			errors.Add(Texts.Format(lang, "val.pace", MinPace, MaxPace));
		}

		if (!ActiveWindow.TryParse(ActiveHours, out _)) {
			errors.Add(Texts.Get(lang, "val.activeHours"));
		}

		if ((TimeZone != null) && !isValidTimeZone(TimeZone)) {
			errors.Add(Texts.Format(lang, "val.timeZone", TimeZone));
		}

		if (MaxActiveGames is < 1 or > MaxActiveGamesLimit) {
			errors.Add(Texts.Format(lang, "val.maxActiveGames", MaxActiveGamesLimit));
		}

		if (MinGapMinutes is < 5 or > 1440) {
			errors.Add(Texts.Get(lang, "val.minGap"));
		}

		if (MaxPerDay is < 1 or > 200) {
			errors.Add(Texts.Get(lang, "val.maxPerDay"));
		}

		if (double.IsNaN(MinPlaytimeHours) || MinPlaytimeHours is < 0 or > 1000) {
			errors.Add(Texts.Get(lang, "val.minPlaytime"));
		}

		if (double.IsNaN(SkipRarerThan) || SkipRarerThan is < 0 or > 50) {
			errors.Add(Texts.Get(lang, "val.skipRarer"));
		}

		if (AutoPlayRotateHours is < 1 or > 48) {
			errors.Add(Texts.Get(lang, "val.rotate"));
		}

		if (DailySummaryHour is < 0 or > 23) {
			errors.Add(Texts.Get(lang, "val.summaryHour"));
		}

		if ((DiscordWebhook.Length > 0) && !IsDiscordWebhook(DiscordWebhook)) {
			errors.Add(Texts.Get(lang, "val.webhook"));
		}

		return errors;
	}

	internal static bool IsDiscordWebhook(string url) {
		if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri) || (uri.Scheme != Uri.UriSchemeHttps)) {
			return false;
		}

		string host = uri.Host.ToUpperInvariant();

		return host is "DISCORD.COM" or "DISCORDAPP.COM" or "PTB.DISCORD.COM" or "CANARY.DISCORD.COM" && uri.AbsolutePath.StartsWith("/api/webhooks/", StringComparison.OrdinalIgnoreCase);
	}

	private static List<string> CleanNames(List<string>? names) => (names ?? []).Select(static name => (name ?? "").Trim()).Where(static name => name.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
}
