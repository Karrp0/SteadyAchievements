using System;
using System.Collections.Generic;

namespace SteadyAchievements.IPC;

// Shapes returned by the IPC API and used by the /steady/ panel.

public sealed class StatusDto {
	public string Bot { get; init; } = "";
	public bool Connected { get; init; }
	public bool Enabled { get; init; }
	public bool Simulation { get; init; }
	public bool AutoPlay { get; init; }
	public IReadOnlyList<uint> AutoPlaying { get; init; } = [];
	public bool InWindow { get; init; }
	public string ActiveHours { get; init; } = "";
	public string TimeZone { get; init; } = "";
	public string LocalTime { get; init; } = "";
	public int UnlocksToday { get; init; }
	public int MaxPerDay { get; init; }
	public bool TrimmedRuntime { get; init; }
	public IReadOnlyList<GameDto> Games { get; init; } = [];
	public IReadOnlyList<GameDto> Completed { get; init; } = [];
	public IReadOnlyList<PlayingDto> Playing { get; init; } = [];
	public IReadOnlyList<HistoryDto> History { get; init; } = [];
}

public sealed class GameDto {
	public uint AppId { get; init; }
	public string Name { get; init; } = "";
	public int Done { get; init; }
	public int Simulated { get; init; }
	public int Skipped { get; init; }
	public int Planned { get; init; }
	public int Total { get; init; }
	public int Protected { get; init; }
	public int UnlockedBefore { get; init; }
	public int SkippedRare { get; init; }
	public long ActiveMinutes { get; init; }
	public long BudgetMinutes { get; init; }
	public string? NextName { get; init; }
	public double? NextPercent { get; init; }
	public long? NextInMinutes { get; init; }
	public DateTime? NextEtaUtc { get; init; }
	public DateTime? EndEtaUtc { get; init; }
	public DateTime CreatedUtc { get; init; }
	public DateTime? CompletedUtc { get; init; }
	public bool Playing { get; init; }
	public bool InSlot { get; init; }
	public string Header { get; init; } = "";
	public string Capsule { get; init; } = "";
}

public sealed class PlayingDto {
	public uint AppId { get; init; }
	public string Name { get; init; } = "";
	public bool Eligible { get; init; }
	public bool InSlot { get; init; }

	// Eligible, but its plan has nothing left to unlock
	public bool Finished { get; init; }

	public string Status { get; init; } = "";
	public string Capsule { get; init; } = "";
}

public sealed class HistoryDto {
	public DateTime Utc { get; init; }
	public string Kind { get; init; } = "";
	public uint AppId { get; init; }
	public string Game { get; init; } = "";
	public string Achievement { get; init; } = "";
	public double Percent { get; init; }
}

public sealed class GameDetailDto {
	public uint AppId { get; init; }
	public string Name { get; init; } = "";
	public IReadOnlyList<AchievementDto> Achievements { get; init; } = [];
}

public sealed class AchievementDto {
	public string ApiName { get; init; } = "";
	public string Name { get; init; } = "";
	public string Description { get; init; } = "";
	public double? Percent { get; init; }

	// unlocked | protected | done | simulated | skipped | planned | rare | none
	public string State { get; init; } = "";

	public DateTime? FinishedUtc { get; init; }
	public DateTime? EtaUtc { get; init; }
	public string Icon { get; init; } = "";
	public bool Hidden { get; init; }
}

