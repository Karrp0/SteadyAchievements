using System;
using System.Collections.Generic;
using System.Linq;

namespace SteadyAchievements.Core;

// Chooses which library games to play while ASF has nothing to farm.
internal static class AutoPlayPicker {
	// ASF can show at most 32 games as played at once
	internal const int MaxPlayedGames = 32;

	// eligible: owned games that passed every check and still have achievements to unlock.
	// Games with a running plan come first (oldest first) so started games get finished,
	// then the most played games, which makes new progress look the least out of place.
	internal static List<uint> Pick(IEnumerable<uint> eligible, BotState state, IReadOnlyDictionary<uint, long> playtimeMinutes, int count) {
		ArgumentNullException.ThrowIfNull(eligible);
		ArgumentNullException.ThrowIfNull(state);
		ArgumentNullException.ThrowIfNull(playtimeMinutes);

		List<uint> pool = eligible.Distinct().ToList();

		IEnumerable<uint> started = pool.Where(appId => state.Plans.TryGetValue(appId, out GamePlan? plan) && !plan.IsCompleted).OrderBy(appId => state.Plans[appId].CreatedUtc);
		IEnumerable<uint> others = pool.Where(appId => !state.Plans.ContainsKey(appId)).OrderByDescending(appId => playtimeMinutes.GetValueOrDefault(appId)).ThenBy(static appId => appId);

		return started.Concat(others).Take(Math.Max(count, 0)).ToList();
	}

	// What ASF should show as played: the picked games plus the user's own idle list (for hours), capped.
	internal static List<uint> Combine(IReadOnlyList<uint> picked, IEnumerable<uint> idleList) {
		ArgumentNullException.ThrowIfNull(picked);
		ArgumentNullException.ThrowIfNull(idleList);

		return picked.Concat(idleList).Distinct().Take(MaxPlayedGames).ToList();
	}
}
