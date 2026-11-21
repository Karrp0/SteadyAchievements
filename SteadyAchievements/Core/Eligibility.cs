using System;
using System.Collections.Generic;
using System.Linq;

namespace SteadyAchievements.Core;

internal enum Verdict : byte {
	Ok,
	Unknown,
	Blacklisted,
	AsfBlacklisted,
	Vac,
	NotAGame,
	BlockedDeveloper,
	BlockedPublisher,
	Online,
	NoAchievements,
	MissingDlc
}

// Steam store category ids (public data shown on every store page).
internal static class Categories {
	internal const int MultiPlayer = 1;
	internal const int Vac = 8;
	internal const int Mmo = 20;
	internal const int Achievements = 22;
	internal const int CrossPlatformMultiplayer = 27;
	internal const int OnlinePvP = 36;
	internal const int OnlineCoop = 38;
	internal const int PvP = 49;

	internal static readonly int[] Online = [MultiPlayer, Mmo, CrossPlatformMultiplayer, OnlinePvP, OnlineCoop, PvP];
}

internal static class Eligibility {
	// Decides whether achievements of an app may be touched at all.
	// Order matters: explicit blocks first, VAC always wins (even over the allowlist),
	// then the allowlist lets the user override the softer checks.
	internal static Verdict Evaluate(uint appId, AppFacts? facts, BotSettings settings, IReadOnlySet<uint> asfBlacklist, IReadOnlyCollection<uint> missingAchievementDlcs) {
		ArgumentNullException.ThrowIfNull(settings);
		ArgumentNullException.ThrowIfNull(asfBlacklist);
		ArgumentNullException.ThrowIfNull(missingAchievementDlcs);

		if (settings.Blacklist.Contains(appId)) {
			return Verdict.Blacklisted;
		}

		if (settings.RespectAsfBlacklist && asfBlacklist.Contains(appId)) {
			return Verdict.AsfBlacklisted;
		}

		if (settings.ExcludeVac && (facts != null) && facts.HasCategory(Categories.Vac)) {
			return Verdict.Vac;
		}

		if (settings.Allowlist.Contains(appId)) {
			return Verdict.Ok;
		}

		if (facts == null) {
			return Verdict.Unknown;
		}

		if (!string.Equals(facts.Type, "game", StringComparison.OrdinalIgnoreCase)) {
			return Verdict.NotAGame;
		}

		if (MatchesAny(facts.Developers, settings.BlockedDevelopers)) {
			return Verdict.BlockedDeveloper;
		}

		if (MatchesAny(facts.Publishers, settings.BlockedPublishers)) {
			return Verdict.BlockedPublisher;
		}

		if (settings.ExcludeOnline && Categories.Online.Any(facts.HasCategory)) {
			return Verdict.Online;
		}

		if (!facts.HasCategory(Categories.Achievements)) {
			return Verdict.NoAchievements;
		}

		if (settings.ProtectDlc && (missingAchievementDlcs.Count > 0)) {
			return Verdict.MissingDlc;
		}

		return Verdict.Ok;
	}

	// DLCs of the app that have their own achievements and that the account does not own.
	internal static List<uint> MissingAchievementDlcs(AppFacts facts, Func<uint, AppFacts?> dlcFacts, IReadOnlySet<uint> ownedApps) {
		ArgumentNullException.ThrowIfNull(facts);
		ArgumentNullException.ThrowIfNull(dlcFacts);
		ArgumentNullException.ThrowIfNull(ownedApps);

		List<uint> missing = [];

		foreach (uint dlc in facts.Dlcs) {
			if (ownedApps.Contains(dlc)) {
				continue;
			}

			AppFacts? info = dlcFacts(dlc);

			// Unknown DLC info: assume it may add achievements (the safe side)
			if ((info == null) || info.HasCategory(Categories.Achievements)) {
				missing.Add(dlc);
			}
		}

		return missing;
	}

	private static bool MatchesAny(IEnumerable<string> names, List<string> blocked) => (blocked.Count > 0) && names.Any(name => blocked.Any(block => string.Equals(name, block, StringComparison.OrdinalIgnoreCase)));
}
