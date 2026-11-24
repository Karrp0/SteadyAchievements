using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace SteadyAchievements.Core;

// One achievement as read from the game's stats schema.
internal sealed record AchievementDef(
	string ApiName,
	string Name,
	string Description,
	uint StatId,
	int Bit,
	bool Unlocked,
	bool Protected,
	bool Hidden,
	uint? ProgressStatId,
	uint ProgressMax,
	string Icon,
	string IconGray
);

// Store facts about an app, read from Steam's app info (PICS).
internal sealed record AppFacts(
	uint AppId,
	string Name,
	string Type,
	ImmutableHashSet<int> Categories,
	ImmutableArray<uint> Dlcs,
	ImmutableArray<string> Developers,
	ImmutableArray<string> Publishers,
	DateTime FetchedUtc
) {
	// Store image file names from app info, relative to the app's store asset folder
	// (old apps: "header.jpg"; newer apps: "<hash>/header_....jpg"). Empty when unknown.
	internal string HeaderImage { get; init; } = "";
	internal string CapsuleImage { get; init; } = "";

	internal bool HasCategory(int category) => Categories.Contains(category);

	internal const string StoreAssets = "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/";

	internal static string ImageUrl(uint appId, string file, string fallback) => $"{StoreAssets}{appId}/{(file.Length > 0 ? file : fallback)}";
}

internal enum StepState : byte {
	Pending,
	Done,
	Simulated,
	Skipped
}

internal sealed class PlanStep {
	public string ApiName { get; set; } = "";
	public string Name { get; set; } = "";
	public uint StatId { get; set; }
	public int Bit { get; set; }
	public double Percent { get; set; }

	// Unlock once the game has accumulated this many active minutes in its plan.
	public long DueMinute { get; set; }

	public StepState State { get; set; }
	public DateTime? FinishedUtc { get; set; }
	public int Failures { get; set; }
}

internal sealed class GamePlan {
	// Planner model that built this plan (0 = the first, day-based model)
	public int Model { get; set; }

	public uint AppId { get; set; }
	public string Name { get; set; } = "";
	public DateTime CreatedUtc { get; set; }
	public DateTime? CompletedUtc { get; set; }
	public DateTime? LastUnlockUtc { get; set; }

	// Minutes of play: the Steam playtime when the plan was made, plus every minute played since
	public long ActiveMinutes { get; set; }

	public long BudgetMinutes { get; set; }
	public int TotalAchievements { get; set; }
	public int ProtectedCount { get; set; }
	public int UnlockedBefore { get; set; }
	public int SkippedRare { get; set; }
	public List<PlanStep> Steps { get; set; } = [];

	internal bool IsCompleted => CompletedUtc != null;

	internal long LastDue() {
		long last = 0;

		foreach (PlanStep step in Steps) {
			last = Math.Max(last, step.DueMinute);
		}

		return last;
	}

	// Minutes from now until each pending step unlocks, in order. A step waits for its due minute and for
	// the minimum gap after the previous one, so a plan that is already behind (a game with lots of Steam
	// playtime) shows when the catch-up really ends instead of "now".
	internal List<(PlanStep Step, long Wait)> PendingWaits(int minGapMinutes, long firstAllowedIn) {
		List<PlanStep> pending = Steps.FindAll(static s => s.State == StepState.Pending);
		pending.Sort(static (a, b) => a.DueMinute.CompareTo(b.DueMinute));

		List<(PlanStep Step, long Wait)> result = new(pending.Count);
		long earliest = Math.Max(0, firstAllowedIn);

		foreach (PlanStep step in pending) {
			long wait = Math.Max(step.DueMinute - ActiveMinutes, earliest);
			result.Add((step, wait));
			earliest = wait + minGapMinutes;
		}

		return result;
	}

	internal PlanStep? NextPending() {
		PlanStep? next = null;

		foreach (PlanStep step in Steps) {
			if ((step.State == StepState.Pending) && ((next == null) || (step.DueMinute < next.DueMinute))) {
				next = step;
			}
		}

		return next;
	}
}

internal enum HistoryKind : byte {
	Unlocked,
	Simulated,
	Failed,
	Skipped,
	Completed
}

internal sealed class HistoryEntry {
	public DateTime Utc { get; set; }
	public HistoryKind Kind { get; set; }
	public uint AppId { get; set; }
	public string Game { get; set; } = "";
	public string Achievement { get; set; } = "";
	public double Percent { get; set; }
}

internal sealed class LibraryEntry {
	public int Unlockable { get; set; }
	public DateTime CheckedUtc { get; set; }
}

// Everything that changes over time for one bot; stored in <bot>.state.json.
internal sealed class BotState {
	internal const int MaxHistory = 300;

	public Dictionary<uint, GamePlan> Plans { get; set; } = [];
	public List<HistoryEntry> History { get; set; } = [];
	public DateTime? LastUnlockUtc { get; set; }
	public string? LastSummaryDay { get; set; }

	// Auto play bookkeeping
	public List<uint> AutoPlaying { get; set; } = [];
	public DateTime? AutoPlayStartedUtc { get; set; }

	// Library scan for auto play: how many achievements each owned game still has to unlock
	public Dictionary<uint, LibraryEntry> Library { get; set; } = [];

	internal void AddHistory(HistoryEntry entry) {
		History.Add(entry);

		if (History.Count > MaxHistory) {
			History.RemoveRange(0, History.Count - MaxHistory);
		}
	}
}
