using System;
using System.Collections.Generic;
using System.Globalization;
using SteadyAchievements.Core;
using SteamKit2;

namespace SteadyAchievements.Steam;

// Reads the binary KeyValues stats schema that Steam returns with a user's stats.
//
// Shape (public Steamworks format):
//   stats/<statId>/type = "4" | "ACHIEVEMENTS"  -> bits/<bit>/{ name, permission, display/{ name, desc, hidden, icon, icon_gray }, progress/{ value/operand1, max_val } }
//   stats/<statId>/type = "1" | "INT"           -> { name, permission }
// An achievement is unlocked when bit <bit> of stat <statId> is set. A "permission" means only the game's
// servers may write it; such achievements are never touched.
internal static class SchemaParser {
	private static readonly string[] TurkishOrder = ["turkish", "english"];
	private static readonly string[] EnglishOrder = ["english"];

	internal static List<AchievementDef> Parse(KeyValue schema, IReadOnlyDictionary<uint, uint> statValues, string language) {
		ArgumentNullException.ThrowIfNull(schema);
		ArgumentNullException.ThrowIfNull(statValues);

		string[] languages = Texts.NormalizeLanguage(language) == Texts.English ? EnglishOrder : TurkishOrder;
		KeyValue stats = schema["stats"];

		// Integer stats by name, for achievements whose progress is driven by a stat
		Dictionary<string, (uint Id, int Permission)> intStats = new(StringComparer.Ordinal);

		foreach (KeyValue stat in stats.Children) {
			if (IsType(stat, "1", "INT") && TryUInt(stat.Name, out uint id) && (stat["name"].Value is { Length: > 0 } name)) {
				intStats[name] = (id, ToInt(stat["permission"].Value));
			}
		}

		List<AchievementDef> result = [];

		foreach (KeyValue stat in stats.Children) {
			if (!IsType(stat, "4", "ACHIEVEMENTS") || !TryUInt(stat.Name, out uint statId)) {
				continue;
			}

			uint value = statValues.GetValueOrDefault(statId);

			foreach (KeyValue bitNode in stat["bits"].Children) {
				if (!int.TryParse(bitNode.Name, NumberStyles.None, CultureInfo.InvariantCulture, out int bit) || bit is < 0 or > 31) {
					continue;
				}

				string? apiName = bitNode["name"].Value;

				if (string.IsNullOrEmpty(apiName)) {
					continue;
				}

				bool isProtected = HasPermission(bitNode["permission"].Value);
				uint? progressStat = null;
				uint progressMax = 0;

				if ((bitNode["progress"]["value"]["operand1"].Value is { Length: > 0 } operand) && intStats.TryGetValue(operand, out (uint Id, int Permission) dependency)) {
					progressStat = dependency.Id;
					_ = TryUInt(bitNode["progress"]["max_val"].Value, out progressMax);

					// A progress stat that only servers may write makes the achievement server-only too
					isProtected |= dependency.Permission > 1;
				}

				KeyValue display = bitNode["display"];

				result.Add(
					new AchievementDef(
						apiName,
						Localized(display["name"], languages) ?? apiName,
						Localized(display["desc"], languages) ?? "",
						statId,
						bit,
						(value & (1u << bit)) != 0,
						isProtected,
						display["hidden"].Value == "1",
						progressStat,
						progressMax,
						display["icon"].Value ?? "",
						display["icon_gray"].Value ?? ""
					)
				);
			}
		}

		return result;
	}

	private static bool IsType(KeyValue stat, string number, string word) {
		string? type = stat["type"].Value;

		return string.Equals(type, number, StringComparison.Ordinal) || string.Equals(type, word, StringComparison.OrdinalIgnoreCase);
	}

	// Any non-zero permission on an achievement bit means it is set by the game's servers only
	private static bool HasPermission(string? value) => !string.IsNullOrWhiteSpace(value) && (value.Trim() != "0");

	private static string? Localized(KeyValue node, string[] languages) {
		// Some schemas store a single string instead of per-language children
		if (!string.IsNullOrWhiteSpace(node.Value)) {
			return node.Value;
		}

		foreach (string language in languages) {
			if (node[language].Value is { } text && !string.IsNullOrWhiteSpace(text)) {
				return text;
			}
		}

		return null;
	}

	private static int ToInt(string? text) => int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value) ? value : 0;

	private static bool TryUInt(string? text, out uint value) => uint.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out value);
}
