using System;
using System.Collections.Generic;
using System.Linq;

namespace SteadyAchievements.Core;

// Builds the unlock schedule of a game from how players actually earn achievements.
//
// The plan clock is minutes of play: it starts at the game's Steam playtime when the plan is made and
// advances whenever the game is played.
// 1) Target: an achievement that p% of players have is due after about
//        PlayHours(p) = HoursFactor * PaceScale * ln(100 / p)^2   hours of play
//    At normal pace: 80% → minutes, 30% → ~1 h, 10% → ~4 h, 1% → ~17 h, 0.01% → ~68 h.
//    The floor (MinHours) scales with the pace too, so every time is exactly proportional to PaceScale.
//    Log-normal noise makes every plan different.
// 2) Order: by target, so common achievements come first; tiered chains ("30 matches" before
//    "100 matches") always keep their natural order.
// 3) Spacing: never closer than MinGapMinutes to the previous one.
internal static class Planner {
	// Plans made with an older model are rebuilt once
	internal const int Model = 2;

	internal const double UnknownPercent = 50;
	internal const double MinPercent = 0.01;
	internal const double HoursFactor = 0.8;
	internal const double MinHours = 0.1;
	internal const double Noise = 0.3;

	internal static double PlayHours(double percent, double paceScale) {
		double surprise = Math.Log(100 / Math.Min(Math.Max(percent, MinPercent), 100));

		return paceScale * Math.Max(MinHours, HoursFactor * surprise * surprise);
	}

	internal static GamePlan Create(uint appId, string name, IReadOnlyList<AchievementDef> schema, IReadOnlyDictionary<string, double> percents, BotSettings settings, long playedMinutes, Random random, DateTime nowUtc) {
		ArgumentNullException.ThrowIfNull(schema);
		ArgumentNullException.ThrowIfNull(percents);
		ArgumentNullException.ThrowIfNull(settings);
		ArgumentNullException.ThrowIfNull(random);

		List<AchievementDef> unlockable = schema.Where(static a => !a.Protected).ToList();
		List<(AchievementDef Def, double Percent)> candidates = Candidates(unlockable.Where(static a => !a.Unlocked), percents, settings, out int skippedRare);

		GamePlan plan = new() {
			Model = Model,
			AppId = appId,
			Name = name,
			CreatedUtc = nowUtc,
			ActiveMinutes = Math.Max(0, playedMinutes),
			TotalAchievements = schema.Count,
			ProtectedCount = schema.Count - unlockable.Count,
			UnlockedBefore = unlockable.Count(static a => a.Unlocked),
			SkippedRare = skippedRare
		};

		if (candidates.Count == 0) {
			plan.CompletedUtc = nowUtc;

			return plan;
		}

		// Targets are absolute play time: a game already played for hours starts with its early achievements due
		plan.Steps = Schedule(candidates, settings, random, 0);
		plan.BudgetMinutes = plan.LastDue();

		return plan;
	}

	// Adds achievements that appeared after the plan was made (game updates), counted from the current play time.
	internal static int Extend(GamePlan plan, IReadOnlyList<AchievementDef> schema, IReadOnlyDictionary<string, double> percents, BotSettings settings, Random random) {
		ArgumentNullException.ThrowIfNull(plan);
		ArgumentNullException.ThrowIfNull(schema);
		ArgumentNullException.ThrowIfNull(percents);
		ArgumentNullException.ThrowIfNull(settings);
		ArgumentNullException.ThrowIfNull(random);

		HashSet<string> known = plan.Steps.Select(static s => s.ApiName).ToHashSet(StringComparer.Ordinal);
		List<(AchievementDef Def, double Percent)> added = Candidates(schema.Where(a => !a.Protected && !a.Unlocked && !known.Contains(a.ApiName)), percents, settings, out _);

		if (added.Count == 0) {
			return 0;
		}

		plan.Steps.AddRange(Schedule(added, settings, random, plan.ActiveMinutes));
		plan.TotalAchievements += added.Count;
		plan.BudgetMinutes = plan.LastDue();
		plan.CompletedUtc = null;

		return added.Count;
	}

	internal static double PercentOf(string apiName, IReadOnlyDictionary<string, double> percents) => Math.Min(Math.Max(percents.TryGetValue(apiName, out double p) && double.IsFinite(p) ? p : UnknownPercent, MinPercent), 100);

	private static List<(AchievementDef Def, double Percent)> Candidates(IEnumerable<AchievementDef> defs, IReadOnlyDictionary<string, double> percents, BotSettings settings, out int skippedRare) {
		List<(AchievementDef Def, double Percent)> result = [];
		skippedRare = 0;

		foreach (AchievementDef def in defs) {
			double percent = PercentOf(def.ApiName, percents);

			// Only skip on a known percentage; unknown ones are treated as average
			if ((settings.SkipRarerThan > 0) && percents.ContainsKey(def.ApiName) && (percent < settings.SkipRarerThan)) {
				skippedRare++;

				continue;
			}

			result.Add((def, percent));
		}

		return result;
	}

	private static List<PlanStep> Schedule(List<(AchievementDef Def, double Percent)> items, BotSettings settings, Random random, long offsetMinutes) {
		// Each achievement's own target, with noise
		List<((AchievementDef Def, double Percent) Item, double Minutes)> targets = items.Select(item => (item, Minutes: offsetMinutes + (PlayHours(item.Percent, settings.PaceScale) * 60 * Math.Exp(Noise * Normal(random))))).OrderBy(static x => x.Minutes).ToList();

		// Tier chains swap places among themselves; the times stay in rising order
		List<(AchievementDef Def, double Percent)> ordered = Chains.Repair(targets.Select(static x => x.Item).ToList(), static item => item.Def);
		List<PlanStep> steps = new(ordered.Count);
		double previous = double.MinValue;

		for (int i = 0; i < ordered.Count; i++) {
			double due = Math.Max(targets[i].Minutes, previous + settings.MinGapMinutes);
			previous = due;
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
