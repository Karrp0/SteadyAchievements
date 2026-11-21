using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace SteadyAchievements.Core;

// Keeps "tiered" achievements in a sensible order: "Play 30 matches" must never unlock after "Play 100 matches".
//
// Two achievements are in the same chain when
//  - they are driven by the same progress stat (ordered by the stat's target value), or
//  - their descriptions (or, without a description, their names) are the same text apart from the numbers
//    ("Win 10 games" / "Win 1,000 games"), ordered by the first number.
// The random order is kept for everything else; within a chain only the members swap places among the
// positions the chain already occupies, so the overall pacing does not change.
internal static partial class Chains {
	internal static List<T> Repair<T>(List<T> ordered, Func<T, AchievementDef> def) {
		ArgumentNullException.ThrowIfNull(ordered);
		ArgumentNullException.ThrowIfNull(def);

		Dictionary<string, List<int>> chains = new(StringComparer.Ordinal);

		for (int i = 0; i < ordered.Count; i++) {
			if (KeyOf(def(ordered[i])) is { } key) {
				if (!chains.TryGetValue(key, out List<int>? positions)) {
					chains[key] = positions = [];
				}

				positions.Add(i);
			}
		}

		List<T> result = [.. ordered];

		foreach (List<int> positions in chains.Values.Where(static p => p.Count > 1)) {
			List<T> members = positions.Select(i => ordered[i]).OrderBy(item => RankOf(def(item))).ToList();

			for (int i = 0; i < positions.Count; i++) {
				result[positions[i]] = members[i];
			}
		}

		return result;
	}

	internal static string? KeyOf(AchievementDef def) {
		ArgumentNullException.ThrowIfNull(def);

		if (def.ProgressStatId is uint stat) {
			return $"stat:{stat.ToString(CultureInfo.InvariantCulture)}";
		}

		if (Template(def.Description) is { } description) {
			return $"desc:{description}";
		}

		return Template(def.Name) is { } name ? $"name:{name}" : null;
	}

	internal static double RankOf(AchievementDef def) {
		ArgumentNullException.ThrowIfNull(def);

		if (def.ProgressStatId != null) {
			return def.ProgressMax;
		}

		return FirstNumber(def.Description) ?? FirstNumber(def.Name) ?? 0;
	}

	// The text with every number replaced by '#', or null when it has no number
	internal static string? Template(string text) {
		if (string.IsNullOrWhiteSpace(text)) {
			return null;
		}

		string joined = ThousandsSeparator().Replace(text, "");

		if (!Digits().IsMatch(joined)) {
			return null;
		}

		return Digits().Replace(joined, "#").Trim().ToLowerInvariant();
	}

	internal static double? FirstNumber(string text) {
		if (string.IsNullOrWhiteSpace(text)) {
			return null;
		}

		Match match = Digits().Match(ThousandsSeparator().Replace(text, ""));

		return match.Success && double.TryParse(match.Value, NumberStyles.None, CultureInfo.InvariantCulture, out double value) ? value : null;
	}

	[GeneratedRegex(@"\d+")]
	private static partial Regex Digits();

	// "1,000" / "1.000" / "1 000" -> "1000"
	[GeneratedRegex(@"(?<=\d)[,.  ](?=\d{3}(?!\d))")]
	private static partial Regex ThousandsSeparator();
}
