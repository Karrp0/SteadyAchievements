using System;
using System.Collections.Generic;
using System.Linq;

namespace SteadyAchievements.Core;

internal sealed record TickInput(
	// Games being played right now that passed every eligibility and playtime check, in priority order
	IReadOnlyList<uint> PlayingEligible,
	// Games that failed recently and wait before being retried
	IReadOnlySet<uint> CoolingDown,
	bool InWindow,
	DateTime NowUtc,
	int UnlocksToday
);

internal sealed record TickOutput(IReadOnlyList<uint> Slots, IReadOnlyList<uint> NeedPlan, uint UnlockAppId, PlanStep? UnlockStep) {
	internal static readonly TickOutput Idle = new([], [], 0, null);
}

// One minute of progress for one bot. Pure: it only changes the in-memory state it is given
// (active minutes) and tells the caller what to create or unlock.
internal static class Engine {
	internal static TickOutput Tick(BotState state, BotSettings settings, TickInput input) {
		ArgumentNullException.ThrowIfNull(state);
		ArgumentNullException.ThrowIfNull(settings);
		ArgumentNullException.ThrowIfNull(input);

		if (!settings.Enabled || !input.InWindow) {
			return TickOutput.Idle;
		}

		List<uint> slots = PickSlots(state, settings, input);
		List<uint> needPlan = [];

		foreach (uint appId in slots) {
			if (state.Plans.TryGetValue(appId, out GamePlan? plan)) {
				plan.ActiveMinutes++;
			} else {
				needPlan.Add(appId);
			}
		}

		(uint unlockAppId, PlanStep? step) = PickDue(state, settings, input, slots);

		return new TickOutput(slots, needPlan, unlockAppId, step);
	}

	internal static List<uint> PickSlots(BotState state, BotSettings settings, TickInput input) {
		ArgumentNullException.ThrowIfNull(state);
		ArgumentNullException.ThrowIfNull(settings);
		ArgumentNullException.ThrowIfNull(input);

		List<uint> usable = input.PlayingEligible.Distinct().Where(appId => !input.CoolingDown.Contains(appId) && (!state.Plans.TryGetValue(appId, out GamePlan? plan) || !plan.IsCompleted)).ToList();

		// Games that already have a plan keep their place, oldest first; new games follow in the given order
		IEnumerable<uint> started = usable.Where(state.Plans.ContainsKey).OrderBy(appId => state.Plans[appId].CreatedUtc);
		IEnumerable<uint> fresh = usable.Where(appId => !state.Plans.ContainsKey(appId));

		return started.Concat(fresh).Take(settings.MaxActiveGames).ToList();
	}

	private static (uint AppId, PlanStep? Step) PickDue(BotState state, BotSettings settings, TickInput input, List<uint> slots) {
		if (input.UnlocksToday >= settings.MaxPerDay) {
			return (0, null);
		}

		if ((state.LastUnlockUtc is DateTime last) && ((input.NowUtc - last).TotalMinutes < settings.MinGapMinutes)) {
			return (0, null);
		}

		uint bestApp = 0;
		PlanStep? bestStep = null;
		long bestOverdue = -1;

		foreach (uint appId in slots) {
			if (!state.Plans.TryGetValue(appId, out GamePlan? plan)) {
				continue;
			}

			PlanStep? next = plan.NextPending();

			if ((next == null) || (next.DueMinute > plan.ActiveMinutes)) {
				continue;
			}

			long overdue = plan.ActiveMinutes - next.DueMinute;

			if (overdue > bestOverdue) {
				(bestApp, bestStep, bestOverdue) = (appId, next, overdue);
			}
		}

		return (bestApp, bestStep);
	}
}
