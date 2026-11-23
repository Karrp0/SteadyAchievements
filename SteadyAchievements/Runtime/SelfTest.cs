using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json;
using ArchiSteamFarm.Core;
using ArchiSteamFarm.Helpers.Json;
using SteadyAchievements.Core;
using SteadyAchievements.IPC;
using SteadyAchievements.Steam;
using SteadyAchievements.Storage;
using SteamKit2;

namespace SteadyAchievements.Runtime;

// Opt-in diagnostic (environment variable STEADY_SELFTEST=1): runs every part of the plugin that does not
// need a Steam connection on sample data and logs each result. Used to check ASF's trimmed builds, where a
// missing .NET member only shows up when the code using it runs. Each step is its own method so one
// failure (even a compile failure of that step) does not hide the others.
internal static class SelfTest {
	internal static bool Enabled => Environment.GetEnvironmentVariable("STEADY_SELFTEST") == "1";

	internal static int Run() {
		(string Name, Action Step)[] steps = [
			("settings", Settings),
			("planner", PlannerAndChains),
			("engine", EngineAndPicker),
			("eligibility", EligibilityRules),
			("schema", Schema),
			("appinfo", AppInfoParse),
			("percents", Percents),
			("clock.server", ClockServer),
			("clock.zone", ClockZone),
			("clock.day", ClockDay),
			("clock.describe", ClockDescribe),
			("clock.valid", ClockValid),
			("texts", TextsFormat),
			("state-json", StateJson),
			("ipc-json", IpcJson),
			("discord-json", DiscordJson),
			("json-file", JsonFile),
			("catch.ObjectDisposedException", CatchObjectDisposedException),
			("catch.HttpRequestException", CatchHttpRequestException),
			("catch.TaskCanceledException", CatchTaskCanceledException),
			("catch.UriFormatException", CatchUriFormatException),
			("catch.TimeoutException", CatchTimeoutException),
			("catch.InvalidOperationException", CatchInvalidOperationException),
			("catch.IOException", CatchIOException),
			("catch.JsonException", CatchJsonException),
			("catch.UnauthorizedAccessException", CatchUnauthorizedAccessException)
		];

		int failed = 0;

		foreach ((string name, Action step) in steps) {
			try {
				step();
				ASF.ArchiLogger.LogGenericInfo($"[Steady self-test] {name}: OK");
			} catch (Exception e) {
				failed++;
				ASF.ArchiLogger.LogGenericWarning($"[Steady self-test] {name}: FAILED {e.GetType().Name}: {e.Message}");
			}
		}

		ASF.ArchiLogger.LogGenericInfo($"[Steady self-test] done: {steps.Length - failed}/{steps.Length} passed");

		return failed;
	}

	private static void Check(bool condition, string what) {
		if (!condition) {
			throw new InvalidOperationException(what);
		}
	}

	private static List<AchievementDef> SampleSchema() => [
		new("A1", "Play 30 matches", "Play 30 matches", 1, 0, false, false, false, null, 0, "", ""),
		new("A2", "Play 100 matches", "Play 100 matches", 1, 1, false, false, false, null, 0, "", ""),
		new("A3", "Secret", "Find it", 1, 2, false, false, true, null, 0, "", ""),
		new("A4", "Server", "", 1, 3, false, true, false, null, 0, "", ""),
		new("A5", "Kills", "", 1, 4, true, false, false, 7, 50, "", "")
	];

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void Settings() {
		BotSettings settings = new() { Blacklist = [5, 0, 5, 3], BlockedDevelopers = [" Valve ", "valve"], ActiveHours = "22:00-02:00" };
		settings.Normalize();
		Check(settings.Blacklist.Count == 2, "normalize");
		Check(settings.Validate(Clock.IsValidTimeZone).Count == 0, "validate");
		Check(settings.Clone().Blacklist.Count == 2, "clone");
		Check(BotSettings.IsDiscordWebhook("https://discord.com/api/webhooks/1/x"), "webhook");
		Check(ActiveWindow.TryParse(settings.ActiveHours, out ActiveWindow window) && window.Contains(23 * 60) && (window.MinutesPerDay == 240), "window");
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void PlannerAndChains() {
		Dictionary<string, double> percents = new() { ["A1"] = 5, ["A2"] = 40, ["A3"] = 0.4 };
		BotSettings settings = new() { SkipRarerThan = 0.5 };
		GamePlan plan = Planner.Create(10, "Game", SampleSchema(), percents, settings, 900, Random.Shared, DateTime.UtcNow);
		Check(plan.Steps.Count == 2, "steps");
		Check(plan.Steps.Find(static s => s.ApiName == "A1")!.DueMinute < plan.Steps.Find(static s => s.ApiName == "A2")!.DueMinute, "tier order");
		Check((plan.ProtectedCount == 1) && (plan.SkippedRare == 1), "counts");
		Check(Planner.Extend(plan, [.. SampleSchema(), new("A6", "New", "", 1, 5, false, false, false, null, 0, "", "")], percents, settings, Random.Shared) == 1, "extend");
		Check(plan.LastDue() > 0, "last due");
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void EngineAndPicker() {
		BotState state = new();
		GamePlan plan = Planner.Create(10, "Game", SampleSchema(), new Dictionary<string, double>(), new BotSettings(), 900, Random.Shared, DateTime.UtcNow);
		plan.ActiveMinutes = 1_000_000;
		state.Plans[10] = plan;
		TickOutput output = Engine.Tick(state, new BotSettings(), new TickInput([10, 11], new HashSet<uint>(), true, DateTime.UtcNow, 0));
		Check((output.UnlockStep != null) && (output.NeedPlan.Count == 1), "tick");
		List<uint> picked = AutoPlayPicker.Pick([10, 11, 12], state, new Dictionary<uint, long> { [11] = 5, [12] = 50 }, 2);
		Check((picked.Count == 2) && (picked[0] == 10) && (picked[1] == 12), "picker");
		Check(AutoPlayPicker.Combine(picked, [12, 13]).Count == 3, "combine");
		state.AddHistory(new HistoryEntry { Utc = DateTime.UtcNow, Kind = HistoryKind.Simulated, AppId = 10, Game = "Game", Achievement = "A1", Percent = 5 });
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void EligibilityRules() {
		AppFacts facts = new(1, "G", "game", [Categories.Achievements], [2, 3], ["Dev"], ["Pub"], DateTime.UtcNow);
		Dictionary<uint, AppFacts> dlcs = new() { [2] = new AppFacts(2, "D", "dlc", [Categories.Achievements], [], [], [], DateTime.UtcNow) };
		List<uint> missing = Eligibility.MissingAchievementDlcs(facts, dlcs.GetValueOrDefault, new HashSet<uint> { 3 });
		Check(missing.Count == 1, "dlc");
		Check(Eligibility.Evaluate(1, facts, new BotSettings(), new HashSet<uint>(), missing) == Verdict.MissingDlc, "verdict");
		Check(Texts.Verdict("en", Verdict.Online).Length > 0, "texts");
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void Schema() {
		KeyValue bits = new("bits");
		KeyValue bit = new("0");
		bit.Children.Add(new KeyValue("name", "ACH"));
		KeyValue display = new("display");
		KeyValue name = new("name");
		name.Children.Add(new KeyValue("english", "Hello"));
		display.Children.Add(name);
		bit.Children.Add(display);
		bits.Children.Add(bit);
		KeyValue stat = new("1");
		stat.Children.Add(new KeyValue("type", "4"));
		stat.Children.Add(bits);
		KeyValue stats = new("stats");
		stats.Children.Add(stat);
		KeyValue root = new("1");
		root.Children.Add(stats);

		List<AchievementDef> list = SchemaParser.Parse(root, new Dictionary<uint, uint> { [1] = 1 }, "tr");
		Check((list.Count == 1) && list[0].Unlocked && (list[0].Name == "Hello"), "schema");
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void AppInfoParse() {
		KeyValue root = new("appinfo");
		KeyValue common = new("common");
		common.Children.Add(new KeyValue("type", "Game"));
		KeyValue category = new("category");
		category.Children.Add(new KeyValue("category_22", "1"));
		common.Children.Add(category);
		root.Children.Add(common);
		AppFacts facts = AppInfo.Parse(5, root, DateTime.UtcNow);
		Check((facts.Type == "game") && facts.HasCategory(Categories.Achievements), "appinfo");
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void Percents() {
		using JsonDocument doc = JsonDocument.Parse("""{"achievementpercentages":{"achievements":[{"name":"A","percent":"12.5"},{"name":"B","percent":3.25}]}}""");
		Dictionary<string, double> result = GlobalPercents.Parse(doc.RootElement);
		Check((result.Count == 2) && (Math.Abs(result["A"] - 12.5) < 1e-9), "percents");
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void ClockServer() => Check(Clock.ToLocal(DateTime.UtcNow, null).Year > 2000, "server time");

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void ClockZone() => Check(Clock.ToLocal(DateTime.UtcNow, "Europe/Istanbul").Year > 2000, "zone time");

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void ClockDay() => Check(Clock.DayKey(DateTime.UtcNow).Length == 10, "day key");

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void ClockDescribe() => Check(Clock.Describe(null).Length > 0, "describe");

	// Must not throw; on trimmed builds it simply reports "not valid"
	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void ClockValid() => _ = Clock.IsValidTimeZone("Europe/Istanbul") | Clock.IsValidTimeZone("No/Such_Zone");

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void TextsFormat() {
		Check(Texts.Format("tr", "log.planCreated", "G", 1, 2, 3, 4, 5).Length > 10, "format");
		Check(Texts.Duration("en", 125).Length > 0, "duration");
		Check(Texts.Percent(0.5) == "0.5", "percent");
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void StateJson() {
		BotState state = new();
		GamePlan plan = Planner.Create(10, "Game", SampleSchema(), new Dictionary<string, double>(), new BotSettings(), 900, Random.Shared, DateTime.UtcNow);
		plan.Steps[0].State = StepState.Simulated;
		state.Plans[10] = plan;
		state.Library[10] = new LibraryEntry { Unlockable = 3, CheckedUtc = DateTime.UtcNow };
		state.AddHistory(new HistoryEntry { Utc = DateTime.UtcNow, Kind = HistoryKind.Completed, AppId = 10, Game = "Game" });

		string json = JsonSerializer.Serialize(state, JsonStore.Options);
		Check(json.Contains("\"Simulated\"", StringComparison.Ordinal) && json.Contains("\"Completed\"", StringComparison.Ordinal), "enums as text");

		BotState? back = JsonSerializer.Deserialize<BotState>(json, JsonStore.Options);
		Check((back != null) && (back.Plans[10].Steps[0].State == StepState.Simulated) && (back.History[0].Kind == HistoryKind.Completed) && (back.Library[10].Unlockable == 3), "state round trip");

		BotSettings? settings = JsonSerializer.Deserialize<BotSettings>(JsonSerializer.Serialize(new BotSettings { Allowlist = [1, 2] }, JsonStore.Options), JsonStore.Options);
		Check(settings?.Allowlist.Count == 2, "settings round trip");

		// Older files may hold enums as numbers
		BotState? numeric = JsonSerializer.Deserialize<BotState>("""{"History":[{"Kind":4,"AppId":1}]}""", JsonStore.Options);
		Check(numeric?.History[0].Kind == HistoryKind.Completed, "numeric enum");
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void IpcJson() {
		StatusDto status = new() {
			Bot = "x",
			Games = [new GameDto { AppId = 1, Name = "G", NextEtaUtc = DateTime.UtcNow }],
			Playing = [new PlayingDto { AppId = 1, Reason = "Vac" }],
			History = [new HistoryDto { Kind = "Unlocked" }],
			AutoPlaying = [1, 2]
		};

		string json = JsonSerializer.Serialize(status, JsonUtilities.DefaultJsonSerializerOptions);
		Check(json.Contains("\"Reason\":\"Vac\"", StringComparison.Ordinal), "status dto");

		string detail = JsonSerializer.Serialize(new GameDetailDto { Achievements = [new AchievementDto { Percent = 1.5 }] }, JsonUtilities.DefaultJsonSerializerOptions);
		Check(detail.Contains("1.5", StringComparison.Ordinal), "detail dto");

		string library = JsonSerializer.Serialize(new List<LibraryGameDto> { new() { AppId = 1, Name = "G" } }, JsonUtilities.DefaultJsonSerializerOptions);
		Check(library.Contains("\"G\"", StringComparison.Ordinal), "library dto");

		BotSettings? incoming = JsonSerializer.Deserialize<BotSettings>("""{"Enabled":true,"Allowlist":[1,2],"TimeZone":null}""", JsonUtilities.DefaultJsonSerializerOptions);
		Check(incoming?.Allowlist.Count == 2, "settings from panel");
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void DiscordJson() {
		string json = Discord.BuildPayload("bot", NoticeKind.Completed, "text", "tr", true);
		Check(json.Contains("\"embeds\"", StringComparison.Ordinal) && json.Contains("text", StringComparison.Ordinal), "payload");
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void JsonFile() {
		const string file = "selftest.json";
		JsonStore.Save(file, new BotSettings { Allowlist = [7] }).GetAwaiter().GetResult();
		BotSettings? back = JsonStore.Load<BotSettings>(file).GetAwaiter().GetResult();
		System.IO.File.Delete(System.IO.Path.Combine(JsonStore.Directory, file));
		Check(back?.Allowlist.Count == 1, "file round trip");
	}

	// Each exception type the plugin filters on must load on this build, or the method with the filter cannot compile
	private static bool Matches(Exception e, Func<Exception, bool> test) => test(e);

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void CatchObjectDisposedException() => _ = Matches(new InvalidCastException(), static e => e is ObjectDisposedException);

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void CatchHttpRequestException() => _ = Matches(new InvalidCastException(), static e => e is System.Net.Http.HttpRequestException);

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void CatchTaskCanceledException() => _ = Matches(new InvalidCastException(), static e => e is System.Threading.Tasks.TaskCanceledException);

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void CatchUriFormatException() => _ = Matches(new InvalidCastException(), static e => e is UriFormatException);

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void CatchTimeoutException() => _ = Matches(new InvalidCastException(), static e => e is TimeoutException);

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void CatchInvalidOperationException() => _ = Matches(new InvalidCastException(), static e => e is InvalidOperationException);

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void CatchIOException() => _ = Matches(new InvalidCastException(), static e => e is System.IO.IOException);

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void CatchJsonException() => _ = Matches(new InvalidCastException(), static e => e is JsonException);

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void CatchUnauthorizedAccessException() => _ = Matches(new InvalidCastException(), static e => e is UnauthorizedAccessException);
}
