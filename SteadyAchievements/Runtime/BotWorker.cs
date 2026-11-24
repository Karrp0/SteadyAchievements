using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArchiSteamFarm.Core;
using ArchiSteamFarm.Steam;
using ArchiSteamFarm.Steam.Cards;
using SteadyAchievements.Core;
using SteadyAchievements.IPC;
using SteadyAchievements.Steam;
using SteadyAchievements.Storage;
using SteamKit2;
using SteamKit2.Internal;

namespace SteadyAchievements.Runtime;

// Runs one bot: every minute it looks at what the bot is playing, advances plans,
// creates new ones, unlocks at most one achievement and drives auto play.
internal sealed class BotWorker : IDisposable {
	private const int MaxStepFailures = 3;
	private const int MaxPercentFailures = 3;
	private const int LibraryScansPerTick = 3;
	private const string LogPrefix = "[Steady] ";

	private static readonly TimeSpan TickInterval = TimeSpan.FromMinutes(1);
	private static readonly TimeSpan FirstTickDelay = TimeSpan.FromSeconds(45);
	private static readonly TimeSpan CoolDown = TimeSpan.FromMinutes(30);
	private static readonly TimeSpan OwnedMaxAge = TimeSpan.FromHours(3);
	private static readonly TimeSpan OwnedAppsMaxAge = TimeSpan.FromMinutes(10);
	private static readonly TimeSpan LibraryMaxAge = TimeSpan.FromDays(7);
	private static readonly TimeSpan AutoPlayGrace = TimeSpan.FromMinutes(10);
	private static readonly TimeSpan ProblemNoticeEvery = TimeSpan.FromHours(6);

	internal Bot Bot { get; }

	private readonly StatsClient Stats;
	private readonly SemaphoreSlim Gate = new(1, 1);
	private readonly Dictionary<uint, DateTime> CoolingUntil = [];
	private readonly Dictionary<uint, int> PercentFailures = [];
	private readonly HashSet<string> LoggedOnce = new(StringComparer.Ordinal);

	private BotSettings Settings;
	private BotState State;
	private Timer? Timer;
	private Dictionary<uint, OwnedGame> Owned = [];
	private DateTime OwnedFetchedUtc;
	private HashSet<uint> OwnedApps = [];
	private DateTime OwnedAppsUtc;
	private List<PlayingDto> PlayingSnapshot = [];
	private bool InWindowNow;
	private bool StateDirty;
	private DateTime AutoPlayBlockedUntil;
	private DateTime LastProblemNoticeUtc;

	internal string Language => Settings.Language;

	private string Lang => Settings.Language;
	private string SettingsFile => $"{Bot.BotName}.settings.json";
	private string StateFile => $"{Bot.BotName}.state.json";

	private BotWorker(Bot bot, StatsClient stats, BotSettings settings, BotState state) {
		Bot = bot;
		Stats = stats;
		Settings = settings;
		State = state;
	}

	internal static async Task<BotWorker> Create(Bot bot, StatsClient stats) {
		ArgumentNullException.ThrowIfNull(bot);
		ArgumentNullException.ThrowIfNull(stats);

		BotSettings settings = await JsonStore.Load<BotSettings>($"{bot.BotName}.settings.json").ConfigureAwait(false) ?? new BotSettings();
		settings.Normalize();

		if (settings.Validate(Clock.IsValidTimeZone).Count > 0) {
			bot.ArchiLogger.LogGenericWarning(LogPrefix + string.Join(" ", settings.Validate(Clock.IsValidTimeZone)));
			settings = new BotSettings();
		}

		BotState state = await JsonStore.Load<BotState>($"{bot.BotName}.state.json").ConfigureAwait(false) ?? new BotState();

		// ASF starts with a fresh farmer after a restart, so a previous auto play session is gone
		state.AutoPlaying.Clear();
		state.AutoPlayStartedUtc = null;

		// Unfinished plans from an older planner model are rebuilt from the account's current state.
		// Achievements already unlocked on Steam stay unlocked and simply count as "already unlocked".
		foreach (GamePlan old in state.Plans.Values.Where(static p => (p.Model < Planner.Model) && !p.IsCompleted).ToList()) {
			state.Plans.Remove(old.AppId);
			bot.ArchiLogger.LogGenericInfo(LogPrefix + Texts.Format(settings.Language, "log.replanModel", old.Name));
		}

		BotWorker worker = new(bot, stats, settings, state);
		bot.ArchiLogger.LogGenericInfo(LogPrefix + Texts.Format(settings.Language, "log.started", state.Plans.Count, Texts.Get(settings.Language, settings.Simulation ? "word.simulation" : "word.real")));

		if (!File.Exists(Path.Combine(JsonStore.Directory, worker.SettingsFile))) {
			await JsonStore.Save(worker.SettingsFile, settings).ConfigureAwait(false);
		}

		return worker;
	}

	internal void Start() => Timer ??= new Timer(OnTimer, null, FirstTickDelay, TickInterval);

	public void Dispose() {
		Timer?.Dispose();
		Timer = null;
		Gate.Dispose();
	}

	internal void OnLoggedOn() {
		// A new session: ASF's farmer starts fresh and the library may have changed
		State.AutoPlaying.Clear();
		State.AutoPlayStartedUtc = null;
		OwnedFetchedUtc = DateTime.MinValue;
		OwnedAppsUtc = DateTime.MinValue;
	}

	private async void OnTimer(object? _) {
		try {
			await Tick().ConfigureAwait(false);
		} catch (ObjectDisposedException) {
			// Bot was destroyed while a tick was queued
		} catch (Exception e) {
			Bot.ArchiLogger.LogGenericException(e);
		}
	}

	// ---------------------------------------------------------------- tick

	private async Task Tick() {
		if (!await Gate.WaitAsync(0).ConfigureAwait(false)) {
			// The previous tick is still talking to Steam
			return;
		}

		try {
			if (!Bot.IsConnectedAndLoggedOn) {
				return;
			}

			DateTime now = DateTime.UtcNow;
			DateTime local = Clock.ToLocal(now, Settings.TimeZone);
			_ = ActiveWindow.TryParse(Settings.ActiveHours, out ActiveWindow window);
			bool inWindow = window.Contains(local);
			InWindowNow = inWindow;

			foreach (uint expired in CoolingUntil.Where(pair => pair.Value <= now).Select(static pair => pair.Key).ToList()) {
				CoolingUntil.Remove(expired);
			}

			await RefreshOwned(now).ConfigureAwait(false);
			await HandleAutoPlay(now, inWindow).ConfigureAwait(false);

			List<uint> playing = CurrentlyPlaying();
			Dictionary<uint, AppFacts> facts = await AppInfo.Get(Bot, playing).ConfigureAwait(false);
			List<uint> eligible = [];
			List<PlayingDto> snapshot = [];

			foreach (uint appId in playing) {
				AppFacts? appFacts = facts.GetValueOrDefault(appId);
				(bool ok, string reason, string code, double value) = await Check(appId, appFacts, false).ConfigureAwait(false);

				if (ok) {
					eligible.Add(appId);
				} else if (LoggedOnce.Add($"{appId}:{reason}")) {
					Bot.ArchiLogger.LogGenericInfo(LogPrefix + Texts.Format(Lang, "log.excluded", NameOf(appId), appId, reason));
				}

				snapshot.Add(new PlayingDto { AppId = appId, Name = NameOf(appId), Eligible = ok, Status = reason, Reason = code, ReasonValue = value });
			}

			TickInput input = new(eligible, CoolingUntil.Keys.ToHashSet(), inWindow, now, UnlocksToday(local));
			TickOutput output = Engine.Tick(State, Settings, input);
			StateDirty |= output.Slots.Count > 0;

			PlayingSnapshot = snapshot.Select(
				p => new PlayingDto {
					AppId = p.AppId,
					Name = p.Name,
					Eligible = p.Eligible,
					Status = p.Status,
					Reason = p.Reason,
					ReasonValue = p.ReasonValue,
					InSlot = output.Slots.Contains(p.AppId),
					Finished = p.Eligible && State.Plans.TryGetValue(p.AppId, out GamePlan? plan) && plan.IsCompleted,
					Capsule = CapsuleUrl(p.AppId)
				}
			).ToList();

			foreach (uint appId in output.NeedPlan) {
				await CreatePlan(appId, now).ConfigureAwait(false);
			}

			if (output.UnlockStep != null) {
				await Unlock(output.UnlockAppId, output.UnlockStep, now).ConfigureAwait(false);
			}

			await MaybeDailySummary(local).ConfigureAwait(false);
			await SaveState().ConfigureAwait(false);
		} catch (Exception e) when (e is not ObjectDisposedException) {
			Bot.ArchiLogger.LogGenericWarning(LogPrefix + Texts.Format(Lang, "log.tickError", e.Message));
			Bot.ArchiLogger.LogGenericException(e);
			await NotifyProblem(Texts.Format(Lang, "dc.problem.error", e.Message)).ConfigureAwait(false);
		} finally {
			Gate.Release();
		}
	}

	// What ASF is playing right now, when it can be known for sure
	private List<uint> CurrentlyPlaying() {
		CardsFarmer farmer = Bot.CardsFarmer;

		if (farmer.NowFarming) {
			return farmer.CurrentGamesFarmingReadOnly.Select(static game => game.AppID).ToList();
		}

		if (State.AutoPlaying.Count > 0) {
			return [.. State.AutoPlaying];
		}

		// A paused farmer means someone used !play or !pause: what is played is unknown, so do nothing
		if (Bot.IsPlayingPossible && !farmer.Paused) {
			return [.. Bot.BotConfig.GamesPlayedWhileIdle];
		}

		return [];
	}

	// ---------------------------------------------------------------- checks

	private async Task<(bool Ok, string Reason, string Code, double Value)> Check(uint appId, AppFacts? facts, bool ignorePlaytime) {
		IReadOnlySet<uint> asfBlacklist = (IReadOnlySet<uint>?) ASF.GlobalConfig?.Blacklist ?? new HashSet<uint>();
		Verdict verdict = Eligibility.Evaluate(appId, facts, Settings, asfBlacklist, []);

		if ((verdict == Verdict.Ok) && Settings.ProtectDlc && (facts != null) && (facts.Dlcs.Length > 0) && !Settings.Allowlist.Contains(appId)) {
			await AppInfo.Get(Bot, facts.Dlcs).ConfigureAwait(false);
			List<uint> missing = Eligibility.MissingAchievementDlcs(facts, AppInfo.Peek, GetOwnedApps());
			verdict = Eligibility.Evaluate(appId, facts, Settings, asfBlacklist, missing);
		}

		if (verdict != Verdict.Ok) {
			return (false, Texts.Verdict(Lang, verdict), verdict.ToString(), 0);
		}

		if (!ignorePlaytime && (Settings.MinPlaytimeHours > 0) && !State.Plans.ContainsKey(appId)) {
			long minutes = Owned.TryGetValue(appId, out OwnedGame? game) ? game.PlaytimeMinutes : 0;

			if (minutes < Settings.MinPlaytimeHours * 60) {
				return (false, Texts.Format(Lang, "verdict.Playtime", Settings.MinPlaytimeHours), "Playtime", Settings.MinPlaytimeHours);
			}
		}

		return (true, Texts.Verdict(Lang, Verdict.Ok), nameof(Verdict.Ok), 0);
	}

	private HashSet<uint> GetOwnedApps() {
		if (DateTime.UtcNow - OwnedAppsUtc < OwnedAppsMaxAge) {
			return OwnedApps;
		}

		HashSet<uint> apps = [];
		IReadOnlyDictionary<uint, ArchiSteamFarm.Storage.PackageData>? packages = ASF.GlobalDatabase?.PackagesDataReadOnly;

		if (packages != null) {
			foreach (uint packageId in Bot.OwnedPackages.Keys) {
				if (packages.TryGetValue(packageId, out ArchiSteamFarm.Storage.PackageData? data) && (data.AppIDs != null)) {
					apps.UnionWith(data.AppIDs);
				}
			}
		}

		OwnedApps = apps;
		OwnedAppsUtc = DateTime.UtcNow;

		return apps;
	}

	private async Task RefreshOwned(DateTime now) {
		if (now - OwnedFetchedUtc < OwnedMaxAge) {
			return;
		}

		List<OwnedGame>? games = await Stats.GetOwnedGames(Bot.SteamID).ConfigureAwait(false);

		// On failure try again in 10 minutes rather than every tick
		OwnedFetchedUtc = games != null ? now : now - OwnedMaxAge + TimeSpan.FromMinutes(10);

		if (games == null) {
			return;
		}

		Owned = games.GroupBy(static g => g.AppId).ToDictionary(static g => g.Key, static g => g.First());

		// Steam's playtime also counts play outside ASF (or before the plugin): let the plans catch up with it
		foreach (GamePlan plan in State.Plans.Values) {
			if (!plan.IsCompleted && Owned.TryGetValue(plan.AppId, out OwnedGame? game) && (game.PlaytimeMinutes > plan.ActiveMinutes)) {
				plan.ActiveMinutes = game.PlaytimeMinutes;
				StateDirty = true;
			}
		}
	}

	// ---------------------------------------------------------------- plans

	private async Task CreatePlan(uint appId, DateTime now) {
		string name = NameOf(appId);
		(List<AchievementDef>? schema, _, _) = await ReadStats(appId).ConfigureAwait(false);

		if (schema == null) {
			CoolingUntil[appId] = now + CoolDown;
			Bot.ArchiLogger.LogGenericWarning(LogPrefix + Texts.Format(Lang, "log.planFailed", name, appId));

			return;
		}

		Dictionary<string, double>? percents = await GlobalPercents.Get(appId).ConfigureAwait(false);

		if (percents == null) {
			int failures = PercentFailures[appId] = PercentFailures.GetValueOrDefault(appId) + 1;

			if (failures < MaxPercentFailures) {
				CoolingUntil[appId] = now + TimeSpan.FromMinutes(5);

				return;
			}

			// Steam's percentage API is down: plan with neutral percentages rather than never
			percents = [];
		}

		PercentFailures.Remove(appId);

		long playedMinutes = Owned.TryGetValue(appId, out OwnedGame? owned) ? owned.PlaytimeMinutes : 0;
		GamePlan plan = Planner.Create(appId, name, schema, percents, Settings, playedMinutes, Random.Shared, now);
		State.Plans[appId] = plan;
		State.Library[appId] = new LibraryEntry { Unlockable = plan.Steps.Count, CheckedUtc = now };
		StateDirty = true;

		Bot.ArchiLogger.LogGenericInfo(LogPrefix + Texts.Format(Lang, "log.planCreated", name, plan.Steps.Count, Math.Round(plan.BudgetMinutes / 60.0), plan.ProtectedCount, plan.UnlockedBefore, plan.SkippedRare));
	}

	// Reads the account's stats for a game. Schema is null when Steam did not answer.
	private async Task<(List<AchievementDef>? Schema, Dictionary<uint, uint> Values, uint Crc)> ReadStats(uint appId) {
		CMsgClientGetUserStatsResponse? response = await Stats.GetUserStats(Bot.SteamID, appId).ConfigureAwait(false);

		if ((response == null) || ((EResult) response.eresult != EResult.OK)) {
			return (null, [], 0);
		}

		Dictionary<uint, uint> values = [];

		foreach (CMsgClientGetUserStatsResponse.Stats stat in response.stats) {
			values[stat.stat_id] = stat.stat_value;
		}

		if ((response.schema == null) || (response.schema.Length == 0)) {
			return ([], values, response.crc_stats);
		}

		KeyValue schema = new();

		using (MemoryStream stream = new(response.schema)) {
			if (!schema.TryReadAsBinary(stream)) {
				return (null, values, response.crc_stats);
			}
		}

		return (SchemaParser.Parse(schema, values, Lang), values, response.crc_stats);
	}

	// ---------------------------------------------------------------- unlocking

	private async Task Unlock(uint appId, PlanStep step, DateTime now) {
		GamePlan plan = State.Plans[appId];

		if (Settings.Simulation) {
			Finish(plan, step, StepState.Simulated, now);
			Bot.ArchiLogger.LogGenericInfo(LogPrefix + Texts.Format(Lang, "log.simulated", plan.Name, step.Name, Texts.Percent(step.Percent)));

			if (Settings.NotifyEachUnlock) {
				await Notify(NoticeKind.Simulated, Texts.Format(Lang, "dc.unlock", plan.Name, step.Name, Texts.Percent(step.Percent))).ConfigureAwait(false);
			}

			await CheckCompleted(plan, now).ConfigureAwait(false);

			return;
		}

		(List<AchievementDef>? schema, Dictionary<uint, uint> values, uint crc) = await ReadStats(appId).ConfigureAwait(false);

		if (schema == null) {
			await Failed(plan, step, "no response", now).ConfigureAwait(false);

			return;
		}

		AchievementDef? def = schema.Find(a => a.ApiName == step.ApiName);

		if ((def == null) || def.Protected) {
			// Removed from the game, or turned server-only since the plan was made
			step.State = StepState.Skipped;
			step.FinishedUtc = now;
			State.AddHistory(History(HistoryKind.Skipped, plan, step, now));
			StateDirty = true;
			await CheckCompleted(plan, now).ConfigureAwait(false);

			return;
		}

		if (def.Unlocked) {
			step.State = StepState.Done;
			step.FinishedUtc = now;
			StateDirty = true;
			Bot.ArchiLogger.LogGenericInfo(LogPrefix + Texts.Format(Lang, "log.alreadyUnlocked", plan.Name, step.Name));
			await CheckCompleted(plan, now).ConfigureAwait(false);

			return;
		}

		List<(uint Id, uint Value)> writes = [(def.StatId, values.GetValueOrDefault(def.StatId) | (1u << def.Bit))];

		if (def.ProgressStatId is uint progressStat && (def.ProgressMax > 0) && (values.GetValueOrDefault(progressStat) < def.ProgressMax)) {
			writes.Add((progressStat, def.ProgressMax));
		}

		CMsgClientStoreUserStatsResponse? response = await Stats.StoreStats(Bot.SteamID, appId, crc, writes).ConfigureAwait(false);

		if ((response == null) || ((EResult) response.eresult != EResult.OK) || (response.stats_failed_validation.Count > 0)) {
			string reason = response == null ? "timeout" : response.stats_failed_validation.Count > 0 ? "failed validation" : ((EResult) response.eresult).ToString();
			await Failed(plan, step, reason, now).ConfigureAwait(false);

			return;
		}

		Finish(plan, step, StepState.Done, now);
		Bot.ArchiLogger.LogGenericInfo(LogPrefix + Texts.Format(Lang, "log.unlocked", plan.Name, step.Name, Texts.Percent(step.Percent)));

		if (Settings.NotifyEachUnlock) {
			await Notify(NoticeKind.Unlock, Texts.Format(Lang, "dc.unlock", plan.Name, step.Name, Texts.Percent(step.Percent))).ConfigureAwait(false);
		}

		await CheckCompleted(plan, now).ConfigureAwait(false);
	}

	private void Finish(GamePlan plan, PlanStep step, StepState state, DateTime now) {
		step.State = state;
		step.FinishedUtc = now;
		plan.LastUnlockUtc = now;
		State.LastUnlockUtc = now;
		State.AddHistory(History(state == StepState.Simulated ? HistoryKind.Simulated : HistoryKind.Unlocked, plan, step, now));
		StateDirty = true;
	}

	private async Task Failed(GamePlan plan, PlanStep step, string reason, DateTime now) {
		step.Failures++;
		CoolingUntil[plan.AppId] = now + CoolDown;
		StateDirty = true;
		Bot.ArchiLogger.LogGenericWarning(LogPrefix + Texts.Format(Lang, "log.unlockFailed", plan.Name, step.Name, reason));

		if (step.Failures < MaxStepFailures) {
			State.AddHistory(History(HistoryKind.Failed, plan, step, now));

			return;
		}

		step.State = StepState.Skipped;
		step.FinishedUtc = now;
		State.AddHistory(History(HistoryKind.Skipped, plan, step, now));
		Bot.ArchiLogger.LogGenericWarning(LogPrefix + Texts.Format(Lang, "log.stepSkipped", plan.Name, step.Name));
		await NotifyProblem(Texts.Format(Lang, "dc.problem.failures", plan.Name, step.Name)).ConfigureAwait(false);
		await CheckCompleted(plan, now).ConfigureAwait(false);
	}

	private async Task CheckCompleted(GamePlan plan, DateTime now) {
		if (plan.IsCompleted || (plan.NextPending() != null)) {
			return;
		}

		plan.CompletedUtc = now;
		State.Library[plan.AppId] = new LibraryEntry { Unlockable = 0, CheckedUtc = now };
		StateDirty = true;

		if (!plan.Steps.Any(static s => s.State is StepState.Done or StepState.Simulated)) {
			return;
		}

		State.AddHistory(new HistoryEntry { Utc = now, Kind = HistoryKind.Completed, AppId = plan.AppId, Game = plan.Name });
		Bot.ArchiLogger.LogGenericInfo(LogPrefix + Texts.Format(Lang, "log.completed", plan.Name));

		if (Settings.NotifyCompleted) {
			await Notify(NoticeKind.Completed, Texts.Format(Lang, "dc.completed", plan.Name)).ConfigureAwait(false);
		}
	}

	private static HistoryEntry History(HistoryKind kind, GamePlan plan, PlanStep step, DateTime now) => new() {
		Utc = now,
		Kind = kind,
		AppId = plan.AppId,
		Game = plan.Name,
		Achievement = step.Name,
		Percent = step.Percent
	};

	private int UnlocksToday(DateTime local) {
		string today = Clock.DayKey(local);

		return State.History.Count(h => (h.Kind is HistoryKind.Unlocked or HistoryKind.Simulated) && (Clock.DayKey(Clock.ToLocal(h.Utc, Settings.TimeZone)) == today));
	}

	// ---------------------------------------------------------------- auto play

	private async Task HandleAutoPlay(DateTime now, bool inWindow) {
		CardsFarmer farmer = Bot.CardsFarmer;
		bool wanted = Settings.Enabled && Settings.AutoPlay && inWindow && Bot.IsPlayingPossible && !farmer.NowFarming && (farmer.GamesToFarmReadOnly.Count == 0);

		if (State.AutoPlaying.Count > 0) {
			bool rotate = State.AutoPlayStartedUtc is DateTime started && (now - started >= TimeSpan.FromHours(Settings.AutoPlayRotateHours));

			// Not paused any more means the user (or ASF) took over; otherwise give ASF a look for new cards on rotation
			if (!wanted || rotate || !farmer.Paused) {
				if (farmer.Paused) {
					_ = Bot.Actions.Resume();
				}

				State.AutoPlaying.Clear();
				State.AutoPlayStartedUtc = null;
				AutoPlayBlockedUntil = now + AutoPlayGrace;
				StateDirty = true;
				Bot.ArchiLogger.LogGenericInfo(LogPrefix + Texts.Get(Lang, "log.autoStop"));
			}

			return;
		}

		// Paused by the user: leave it alone
		if (!wanted || farmer.Paused || (now < AutoPlayBlockedUntil)) {
			return;
		}

		List<uint> pool = await AutoPlayPool(now).ConfigureAwait(false);
		Dictionary<uint, long> playtimes = Owned.ToDictionary(static pair => pair.Key, static pair => pair.Value.PlaytimeMinutes);
		List<uint> picked = AutoPlayPicker.Pick(pool, State, playtimes, Settings.MaxActiveGames);

		if (picked.Count == 0) {
			return;
		}

		List<uint> play = AutoPlayPicker.Combine(picked, Bot.BotConfig.GamesPlayedWhileIdle);
		(bool success, string message) = await Bot.Actions.Play(play).ConfigureAwait(false);

		if (!success) {
			Bot.ArchiLogger.LogGenericWarning(LogPrefix + message);
			AutoPlayBlockedUntil = now + AutoPlayGrace;

			return;
		}

		State.AutoPlaying = play;
		State.AutoPlayStartedUtc = now;
		StateDirty = true;
		Bot.ArchiLogger.LogGenericInfo(LogPrefix + Texts.Format(Lang, "log.autoPlay", string.Join(", ", picked.Select(NameOf))));
	}

	// Owned games that may be auto played: eligible and with achievements left. Scans a few new games per tick.
	private async Task<List<uint>> AutoPlayPool(DateTime now) {
		if (Owned.Count == 0) {
			return [];
		}

		Dictionary<uint, AppFacts> facts = await AppInfo.Get(Bot, Owned.Keys).ConfigureAwait(false);
		List<uint> pool = [];
		int scans = 0;
		int wanted = Settings.MaxActiveGames * 3;

		foreach (uint appId in Owned.Values.OrderByDescending(static g => g.PlaytimeMinutes).Select(static g => g.AppId)) {
			if (pool.Count >= wanted) {
				break;
			}

			GamePlan? plan = State.Plans.GetValueOrDefault(appId);

			if (CoolingUntil.ContainsKey(appId) || (plan?.IsCompleted == true)) {
				continue;
			}

			if (!(await Check(appId, facts.GetValueOrDefault(appId), true).ConfigureAwait(false)).Ok) {
				continue;
			}

			if (plan != null) {
				pool.Add(appId);

				continue;
			}

			if (!State.Library.TryGetValue(appId, out LibraryEntry? entry) || (now - entry.CheckedUtc > LibraryMaxAge)) {
				if (scans >= LibraryScansPerTick) {
					continue;
				}

				scans++;
				(List<AchievementDef>? schema, _, _) = await ReadStats(appId).ConfigureAwait(false);

				if (schema == null) {
					continue;
				}

				entry = new LibraryEntry { Unlockable = schema.Count(static a => !a.Unlocked && !a.Protected), CheckedUtc = now };
				State.Library[appId] = entry;
				StateDirty = true;
			}

			if (entry.Unlockable > 0) {
				pool.Add(appId);
			}
		}

		return pool;
	}

	// ---------------------------------------------------------------- notifications

	private async Task MaybeDailySummary(DateTime local) {
		if (!Settings.NotifyDailySummary || (Settings.DiscordWebhook.Length == 0) || (local.Hour != Settings.DailySummaryHour)) {
			return;
		}

		string today = Clock.DayKey(local);

		if (State.LastSummaryDay == today) {
			return;
		}

		State.LastSummaryDay = today;
		StateDirty = true;

		List<HistoryEntry> todays = State.History.Where(h => (h.Kind is HistoryKind.Unlocked or HistoryKind.Simulated) && (Clock.DayKey(Clock.ToLocal(h.Utc, Settings.TimeZone)) == today)).ToList();
		string games = string.Join(", ", State.Plans.Values.Where(static p => !p.IsCompleted).Select(p => $"{p.Name} {p.Steps.Count(static s => s.State is StepState.Done or StepState.Simulated)}/{p.Steps.Count}"));

		if (games.Length == 0) {
			games = "—";
		}

		string text = todays.Count > 0
			? Texts.Format(Lang, "dc.summary", todays.Count, string.Join(", ", todays.GroupBy(static h => h.Game).Select(static g => $"{g.Key} ×{g.Count()}")), games)
			: Texts.Format(Lang, "dc.summaryNone", games);

		await Notify(NoticeKind.Summary, text).ConfigureAwait(false);
	}

	private async Task NotifyProblem(string text) {
		if (!Settings.NotifyProblems || (DateTime.UtcNow - LastProblemNoticeUtc < ProblemNoticeEvery)) {
			return;
		}

		LastProblemNoticeUtc = DateTime.UtcNow;
		await Notify(NoticeKind.Problem, text).ConfigureAwait(false);
	}

	private Task<bool> Notify(NoticeKind kind, string text) => Discord.Send(Settings.DiscordWebhook, Bot.BotName, kind, text, Lang, Settings.Simulation);

	// ---------------------------------------------------------------- persistence

	private async Task SaveState() {
		if (!StateDirty) {
			return;
		}

		await JsonStore.Save(StateFile, State).ConfigureAwait(false);
		StateDirty = false;
	}

	// Newest first
	private List<HistoryDto> RecentHistory(int count) {
		List<HistoryDto> result = new(Math.Min(count, State.History.Count));

		for (int i = State.History.Count - 1; (i >= 0) && (result.Count < count); i--) {
			HistoryEntry h = State.History[i];
			result.Add(new HistoryDto { Utc = h.Utc, Kind = h.Kind.ToString(), AppId = h.AppId, Game = h.Game, Achievement = h.Achievement, Percent = h.Percent });
		}

		return result;
	}

	private static string CapsuleUrl(uint appId) => AppFacts.ImageUrl(appId, AppInfo.Peek(appId)?.CapsuleImage ?? "", "capsule_231x87.jpg");

	private static string HeaderUrl(uint appId) => AppFacts.ImageUrl(appId, AppInfo.Peek(appId)?.HeaderImage ?? "", "header.jpg");

	private string NameOf(uint appId) {
		if (State.Plans.TryGetValue(appId, out GamePlan? plan) && (plan.Name.Length > 0)) {
			return plan.Name;
		}

		if (Owned.TryGetValue(appId, out OwnedGame? game) && (game.Name.Length > 0)) {
			return game.Name;
		}

		return AppInfo.Peek(appId)?.Name is { Length: > 0 } name ? name : appId.ToString(System.Globalization.CultureInfo.InvariantCulture);
	}

	// ---------------------------------------------------------------- API used by IPC and commands

	internal async Task<T> Locked<T>(Func<T> action) {
		ArgumentNullException.ThrowIfNull(action);

		await Gate.WaitAsync().ConfigureAwait(false);

		try {
			return action();
		} finally {
			Gate.Release();
		}
	}

	internal Task<BotSettings> GetSettings() => Locked(() => Settings.Clone());

	// The account's games (name, playtime) for the panel's game picker
	internal async Task<List<LibraryGameDto>?> GetLibrary() {
		await Gate.WaitAsync().ConfigureAwait(false);

		try {
			if ((Owned.Count == 0) && Bot.IsConnectedAndLoggedOn) {
				OwnedFetchedUtc = DateTime.MinValue;
				await RefreshOwned(DateTime.UtcNow).ConfigureAwait(false);
			}

			if (Owned.Count == 0) {
				return null;
			}

			return Owned.Values.OrderBy(static g => g.Name, StringComparer.OrdinalIgnoreCase).Select(
				static g => new LibraryGameDto {
					AppId = g.AppId,
					Name = g.Name,
					PlaytimeMinutes = g.PlaytimeMinutes,
					Capsule = AppInfo.Peek(g.AppId) is { CapsuleImage.Length: > 0 } facts ? AppFacts.ImageUrl(g.AppId, facts.CapsuleImage, "") : ""
				}
			).ToList();
		} finally {
			Gate.Release();
		}
	}

	internal async Task<List<string>> UpdateSettings(BotSettings incoming) {
		ArgumentNullException.ThrowIfNull(incoming);

		incoming.Normalize();
		List<string> errors = incoming.Validate(Clock.IsValidTimeZone);

		if (errors.Count > 0) {
			return errors;
		}

		await Gate.WaitAsync().ConfigureAwait(false);

		try {
			bool toReal = Settings.Simulation && !incoming.Simulation;
			Settings = incoming.Clone();

			if (toReal) {
				// Simulated progress is not real: rebuild those plans from the account's real state
				foreach (uint appId in State.Plans.Where(static pair => pair.Value.Steps.Any(static s => s.State == StepState.Simulated)).Select(static pair => pair.Key).ToList()) {
					State.Plans.Remove(appId);
				}

				State.LastUnlockUtc = null;
				StateDirty = true;
				Bot.ArchiLogger.LogGenericWarning(LogPrefix + Texts.Get(Lang, "log.modeReal"));
			}

			LoggedOnce.Clear();
			await JsonStore.Save(SettingsFile, Settings).ConfigureAwait(false);
			await SaveState().ConfigureAwait(false);
		} finally {
			Gate.Release();
		}

		return [];
	}

	internal async Task<bool> Replan(uint appId) {
		await Gate.WaitAsync().ConfigureAwait(false);

		try {
			bool removed = State.Plans.Remove(appId);
			State.Library.Remove(appId);
			CoolingUntil.Remove(appId);
			StateDirty = true;
			await SaveState().ConfigureAwait(false);

			return removed;
		} finally {
			Gate.Release();
		}
	}

	internal Task<bool> SendTestNotice() => Discord.Send(Settings.DiscordWebhook, Bot.BotName, NoticeKind.Test, Texts.Get(Lang, "dc.test"), Lang, Settings.Simulation);

	internal Task<StatusDto> BuildStatus() => Locked(StatusNow);

	// Minutes until the bot-wide minimum gap after the last unlock has passed
	private long FirstAllowedIn(DateTime now) => State.LastUnlockUtc is DateTime last ? (long) Math.Ceiling(Settings.MinGapMinutes - (now - last).TotalMinutes) : 0;

	private StatusDto StatusNow() {
		DateTime now = DateTime.UtcNow;
		long firstAllowedIn = FirstAllowedIn(now);
		HashSet<uint> playing = PlayingSnapshot.Select(static p => p.AppId).ToHashSet();
		HashSet<uint> slots = PlayingSnapshot.Where(static p => p.InSlot).Select(static p => p.AppId).ToHashSet();

		GameDto ToDto(GamePlan plan) {
			// Plan minutes are minutes of play; the ETAs assume the game keeps being played
			List<(PlanStep Step, long Wait)> waits = plan.PendingWaits(Settings.MinGapMinutes, firstAllowedIn);
			PlanStep? next = waits.Count > 0 ? waits[0].Step : null;
			long? nextIn = waits.Count > 0 ? waits[0].Wait : null;

			return new GameDto {
				AppId = plan.AppId,
				Name = plan.Name,
				Done = plan.Steps.Count(static s => s.State == StepState.Done),
				Simulated = plan.Steps.Count(static s => s.State == StepState.Simulated),
				Skipped = plan.Steps.Count(static s => s.State == StepState.Skipped),
				Planned = plan.Steps.Count,
				Total = plan.TotalAchievements,
				Protected = plan.ProtectedCount,
				UnlockedBefore = plan.UnlockedBefore,
				SkippedRare = plan.SkippedRare,
				ActiveMinutes = plan.ActiveMinutes,
				BudgetMinutes = plan.BudgetMinutes,
				NextName = next?.Name,
				NextPercent = next?.Percent,
				NextInMinutes = nextIn,
				NextEtaUtc = nextIn is long n ? now.AddMinutes(n) : null,
				EndEtaUtc = (plan.IsCompleted || (waits.Count == 0)) ? null : now.AddMinutes(waits[^1].Wait),
				CreatedUtc = plan.CreatedUtc,
				CompletedUtc = plan.CompletedUtc,
				Playing = playing.Contains(plan.AppId),
				InSlot = slots.Contains(plan.AppId),
				Header = HeaderUrl(plan.AppId),
				Capsule = CapsuleUrl(plan.AppId)
			};
		}

		return new StatusDto {
			Bot = Bot.BotName,
			Connected = Bot.IsConnectedAndLoggedOn,
			Enabled = Settings.Enabled,
			Simulation = Settings.Simulation,
			AutoPlay = Settings.AutoPlay,
			AutoPlaying = [.. State.AutoPlaying],
			InWindow = InWindowNow,
			ActiveHours = Settings.ActiveHours,
			TimeZone = Clock.Describe(Settings.TimeZone),
			LocalTime = Clock.ToLocal(now, Settings.TimeZone).ToString("yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture),
			UnlocksToday = UnlocksToday(Clock.ToLocal(now, Settings.TimeZone)),
			MaxPerDay = Settings.MaxPerDay,
			TrimmedRuntime = SteadyAchievementsPlugin.TrimmedRuntime,
			PluginVersion = typeof(SteadyAchievementsPlugin).Assembly.GetName().Version?.ToString(3) ?? "",
			Games = State.Plans.Values.Where(static p => !p.IsCompleted).OrderByDescending(p => slots.Contains(p.AppId)).ThenBy(static p => p.CreatedUtc).Select(ToDto).ToList(),
			Completed = State.Plans.Values.Where(static p => p.IsCompleted).OrderByDescending(static p => p.CompletedUtc).Select(ToDto).ToList(),
			Playing = [.. PlayingSnapshot],
			History = RecentHistory(100)
		};
	}

	internal async Task<GameDetailDto?> GetGameDetail(uint appId) {
		// Read Steam first, without holding the lock, then merge with the plan
		(List<AchievementDef>? schema, _, _) = await ReadStats(appId).ConfigureAwait(false);
		Dictionary<string, double> percents = await GlobalPercents.Get(appId).ConfigureAwait(false) ?? [];

		if (schema == null) {
			return null;
		}

		return await Locked(
			() => {
				DateTime now = DateTime.UtcNow;
				GamePlan? plan = State.Plans.GetValueOrDefault(appId);
				Dictionary<string, PlanStep> steps = plan?.Steps.ToDictionary(static s => s.ApiName, StringComparer.Ordinal) ?? [];
				Dictionary<string, long> waits = new(StringComparer.Ordinal);

				if (plan != null) {
					foreach ((PlanStep step, long wait) in plan.PendingWaits(Settings.MinGapMinutes, FirstAllowedIn(now))) {
						waits[step.ApiName] = wait;
					}
				}

				List<AchievementDto> list = schema.Select(
					def => {
						PlanStep? step = steps.GetValueOrDefault(def.ApiName);

						string state = def.Unlocked && (step?.State != StepState.Done) ? "unlocked"
							: def.Protected ? "protected"
							: step == null ? (plan != null ? "rare" : "none")
							: step.State switch {
								StepState.Done => "done",
								StepState.Simulated => "simulated",
								StepState.Skipped => "skipped",
								_ => "planned"
							};

						DateTime? eta = waits.TryGetValue(def.ApiName, out long wait) ? now.AddMinutes(wait) : null;

						return new AchievementDto {
							ApiName = def.ApiName,
							Name = def.Name,
							Description = def.Description,
							Percent = percents.TryGetValue(def.ApiName, out double p) ? Math.Round(p, 2) : null,
							State = state,
							FinishedUtc = step?.FinishedUtc,
							EtaUtc = eta,
							Icon = (def.Unlocked || (step?.State is StepState.Done or StepState.Simulated) ? def.Icon : def.IconGray) is { Length: > 0 } icon ? $"https://cdn.cloudflare.steamstatic.com/steamcommunity/public/images/apps/{appId}/{icon}" : "",
							Hidden = def.Hidden
						};
					}
				).OrderBy(static a => a.State switch {
					"done" or "unlocked" or "simulated" => 0,
					"planned" => 1,
					_ => 2
				}).ThenBy(static a => a.EtaUtc ?? DateTime.MaxValue).ToList();

				return new GameDetailDto { AppId = appId, Name = NameOf(appId), Achievements = list };
			}
		).ConfigureAwait(false);
	}

	internal async Task<bool> SetFlags(bool? enabled = null, bool? simulation = null, bool? autoPlay = null) {
		BotSettings copy = await GetSettings().ConfigureAwait(false);
		copy.Enabled = enabled ?? copy.Enabled;
		copy.Simulation = simulation ?? copy.Simulation;
		copy.AutoPlay = autoPlay ?? copy.AutoPlay;

		return (await UpdateSettings(copy).ConfigureAwait(false)).Count == 0;
	}

	internal Task<string> TextStatus() => Locked(
		() => {
			StatusDto status = StatusNow();
			string lang = Lang;
			List<string> lines = [
				Texts.Format(lang, "cmd.status", Texts.Get(lang, status.Simulation ? "word.simulation" : "word.real"), Texts.Get(lang, status.Enabled ? "word.on" : "word.off"), status.ActiveHours, Texts.Get(lang, status.InWindow ? "word.inWindow" : "word.outWindow"), status.UnlocksToday, status.MaxPerDay)
			];

			if (status.Games.Count == 0) {
				lines.Add(Texts.Get(lang, "cmd.noGames"));
			}

			foreach (GameDto game in status.Games) {
				lines.Add(Texts.Format(lang, "cmd.game", game.Name, game.Done + game.Simulated, game.Planned, game.NextName ?? "—", game.NextInMinutes is long m ? Texts.Duration(lang, m) : "—"));
			}

			return string.Join(Environment.NewLine, lines);
		}
	);

	internal Task<string?> TextPlan(uint appId) => Locked(
		() => {
			if (!State.Plans.TryGetValue(appId, out GamePlan? plan)) {
				return null;
			}

			IEnumerable<string> lines = plan.Steps.OrderBy(static s => s.DueMinute).Select(s => $"{(s.State == StepState.Pending ? "○" : "●")} {s.Name} ({Texts.Percent(s.Percent)}%) @{Texts.Duration(Lang, s.DueMinute)} [{s.State}]");

			return $"{plan.Name}: {Texts.Duration(Lang, plan.ActiveMinutes)} / {Texts.Duration(Lang, plan.BudgetMinutes)}{Environment.NewLine}{string.Join(Environment.NewLine, lines)}";
		}
	);
}
