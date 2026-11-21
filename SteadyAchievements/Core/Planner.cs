using System;
using System.Collections.Generic;
using System.Linq;

namespace SteadyAchievements.Core;

// Builds the unlock schedule of a game.
//
// Time is measured in "active minutes": minutes in which the game is being played inside the active window.
// 1) Budget: a random number of days between MinDays and MaxDays, times the active minutes per day,
//    scaled by the share of achievements still left (a half-finished game needs about half the time).
// 2) Order: Plackett-Luce sampling with weight percent^Sharpness (Gumbel-max trick), so common
//    achievements tend to come first and rare ones last, but never in a fixed order.
// 3) Spacing: the wait before an achievement grows with its "surprise" 1 + ln(100 / percent), with
//    log-normal noise, scaled to fill the budget and never shorter than MinGapMinutes.
internal static class Planner {
	internal const double UnknownPercent = 50;
	internal const double MinPercent = 0.05;
	internal const double Sharpness = 1.35;
	internal const double GapNoise = 0.35;

	internal static GamePlan Create(uint appId, string name, IReadOnlyList<AchievementDef> schema, IReadOnlyDictionary<string, double> percents, BotSettings settings, int windowMinutesPerDay, Random random, DateTime nowUtc) {
		ArgumentNullException.ThrowIfNull(schema);
		ArgumentNullException.ThrowIfNull(percents);
		ArgumentNullException.ThrowIfNull(settings);
		ArgumentNullException.ThrowIfNull(random);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(windowMinutesPerDay);

		List<AchievementDef> unlockable = schema.Where(static a => !a.Protected).ToList();
		List<(AchievementDef Def, double Percent)> candidates = [];
		int skippedRare = 0;

		foreach (AchievementDef def in unlockable.Where(static a => !a.Unlocked)) {
			double percent = PercentOf(def.ApiName, percents);

			if ((settings.SkipRarerThan > 0) && percents.ContainsKey(def.ApiName) && (percent < settings.SkipRarerThan)) {
				skippedRare++;

				continue;
			}

			candidates.Add((def, percent));
		}

		GamePlan plan = new() {
			AppId = appId,
			Name = name,
			CreatedUtc = nowUtc,
			TotalAchievements = schema.Count,
			ProtectedCount = schema.Count - unlockable.Count,
			UnlockedBefore = unlockable.Count(static a => a.Unlocked),
			SkippedRare = skippedRare
		};

		if (candidates.Count == 0) {
			plan.CompletedUtc = nowUtc;

			return plan;
		}

		double days = settings.MinDays + (random.NextDouble() * (settings.MaxDays - settings.MinDays));
		double share = (double) candidates.Count / unlockable.Count;
		double budget = Math.Max(days * windowMinutesPerDay * share, (double) candidates.Count * settings.MinGapMinutes);

		plan.BudgetMinutes = (long) Math.Round(budget);
		plan.Steps = Schedule(candidates, budget, settings.MinGapMinutes, random, 0);

		return plan;
	}

	// Adds achievements that appeared after the plan was made (game updates), after the last scheduled one.
	internal static int Extend(GamePlan plan, IReadOnlyList<AchievementDef> schema, IReadOnlyDictionary<string, double> percents, BotSettings settings, int windowMinutesPerDay, Random random) {
		ArgumentNullException.ThrowIfNull(plan);
		ArgumentNullException.ThrowIfNull(schema);
		ArgumentNullException.ThrowIfNull(percents);
		ArgumentNullException.ThrowIfNull(settings);
		ArgumentNullException.ThrowIfNull(random);

		HashSet<string> known = plan.Steps.Select(static s => s.ApiName).ToHashSet(StringComparer.Ordinal);
		List<(AchievementDef Def, double Percent)> added = [];

		foreach (AchievementDef def in schema) {
			if (def.Protected || def.Unlocked || known.Contains(def.ApiName)) {
				continue;
			}

			double percent = PercentOf(def.ApiName, percents);

			if ((settings.SkipRarerThan > 0) && percents.ContainsKey(def.ApiName) && (percent < settings.SkipRarerThan)) {
				continue;
			}

			added.Add((def, percent));
		}

		if (added.Count == 0) {
			return 0;
		}

		// Roughly MinDays' worth of time per "full game", proportional to how many were added
		double perAchievement = Math.Max((double) settings.MinDays * windowMinutesPerDay / Math.Max(plan.Steps.Count + added.Count, 1), settings.MinGapMinutes);
		long start = Math.Max(plan.ActiveMinutes, plan.Steps.Count > 0 ? plan.Steps.Max(static s => s.DueMinute) : 0);

		plan.Steps.AddRange(Schedule(added, perAchievement * added.Count, settings.MinGapMinutes, random, start));
		plan.TotalAchievements += added.Count;
		plan.CompletedUtc = null;

		return added.Count;
	}

	internal static double PercentOf(string apiName, IReadOnlyDictionary<string, double> percents) => Math.Clamp(percents.TryGetValue(apiName, out double p) && double.IsFinite(p) ? p : UnknownPercent, MinPercent, 100);

	private static List<PlanStep> Schedule(List<(AchievementDef Def, double Percent)> items, double budget, int minGap, Random random, long startMinute) {
		// Order: larger key first; key = Sharpness * ln(p) + Gumbel noise
		List<(AchievementDef Def, double Percent)> ordered = items.Select(item => (item, Key: (Sharpness * Math.Log(item.Percent)) + Gumbel(random))).OrderByDescending(static x => x.Key).Select(static x => x.item).ToList();

		// Tiered achievements ("30 matches" before "100 matches") always keep their natural order
		ordered = Chains.Repair(ordered, static item => item.Def);

		double[] gaps = ordered.Select(item => (1 + Math.Log(100 / item.Percent)) * Math.Exp(GapNoise * Normal(random))).ToArray();
		double scale = budget / gaps.Sum();
		List<PlanStep> steps = new(ordered.Count);
		double due = startMinute;

		for (int i = 0; i < ordered.Count; i++) {
			due += Math.Max(gaps[i] * scale, minGap);
			(AchievementDef def, double percent) = ordered[i];

			steps.Add(
				new PlanStep {
					ApiName = def.ApiName,
					Name = def.Name,
					StatId = def.StatId,
					Bit = def.Bit,
					Percent = Math.Round(percent, 2),
					DueMinute = (long) Math.Round(due)
				}
			);
		}

		return steps;
	}

	private static double Gumbel(Random random) => -Math.Log(-Math.Log(Open01(random)));

	// Box-Muller standard normal
	private static double Normal(Random random) => Math.Sqrt(-2 * Math.Log(Open01(random))) * Math.Cos(2 * Math.PI * random.NextDouble());

	private static double Open01(Random random) {
		double value;

		do {
			value = random.NextDouble();
		} while (value <= 0);

		return value;
	}
}
