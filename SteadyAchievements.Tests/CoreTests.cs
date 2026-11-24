using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SteadyAchievements.Core;

namespace SteadyAchievements.Tests;

[TestClass]
public sealed class ActiveWindowTests {
	[TestMethod]
	[DataRow("10:00-01:00", 900)]
	[DataRow("09:30-17:00", 450)]
	[DataRow("00:00-00:00", 1440)]
	[DataRow("8:00-08:00", 1440)]
	public void MinutesPerDay(string text, int expected) {
		Assert.IsTrue(ActiveWindow.TryParse(text, out ActiveWindow window));
		Assert.AreEqual(expected, window.MinutesPerDay);
	}

	[TestMethod]
	public void CrossesMidnight() {
		Assert.IsTrue(ActiveWindow.TryParse("22:00-02:00", out ActiveWindow window));
		Assert.IsTrue(window.Contains((23 * 60) + 59));
		Assert.IsTrue(window.Contains(60));
		Assert.IsFalse(window.Contains(2 * 60));
		Assert.IsFalse(window.Contains(12 * 60));
	}

	[TestMethod]
	[DataRow("")]
	[DataRow("10:00")]
	[DataRow("24:00-01:00")]
	[DataRow("10:60-11:00")]
	[DataRow("10:0-11:00")]
	[DataRow("a:00-11:00")]
	[DataRow("10:00-11:00-12:00")]
	public void RejectsInvalid(string text) => Assert.IsFalse(ActiveWindow.TryParse(text, out _));
}

[TestClass]
public sealed class PlannerTests {
	internal static AchievementDef Def(string api, bool unlocked = false, bool isProtected = false) => new(api, api, "", 1, int.Parse(api[1..], System.Globalization.CultureInfo.InvariantCulture) % 32, unlocked, isProtected, false, null, 0, "", "");

	private static BotSettings Settings() => new() { MinGapMinutes = 15 };

	[TestMethod]
	public void ExcludesProtectedAndUnlocked() {
		List<AchievementDef> schema = [Def("a1"), Def("a2", unlocked: true), Def("a3", isProtected: true), Def("a4")];
		GamePlan plan = Planner.Create(1, "G", schema, new Dictionary<string, double>(), Settings(), 0, new Random(1), DateTime.UtcNow);

		CollectionAssert.AreEquivalent(new[] { "a1", "a4" }, plan.Steps.Select(static s => s.ApiName).ToArray());
		Assert.AreEqual(1, plan.ProtectedCount);
		Assert.AreEqual(1, plan.UnlockedBefore);
		Assert.AreEqual(Planner.Model, plan.Model);
		Assert.IsFalse(plan.IsCompleted);
	}

	[TestMethod]
	public void NothingToDoIsCompleted() {
		GamePlan plan = Planner.Create(1, "G", [Def("a1", isProtected: true), Def("a2", unlocked: true)], new Dictionary<string, double>(), Settings(), 0, new Random(1), DateTime.UtcNow);

		Assert.AreEqual(0, plan.Steps.Count);
		Assert.IsTrue(plan.IsCompleted);
	}

	[TestMethod]
	[DataRow(80, 0.0, 0.2)]
	[DataRow(30, 0.9, 1.5)]
	[DataRow(10, 3.5, 5)]
	[DataRow(1, 15, 19)]
	[DataRow(0.01, 60, 75)]
	public void HoursOfPlayFollowRarity(double percent, double min, double max) {
		double hours = Planner.PlayHours(percent, 1);

		Assert.IsTrue((hours >= min) && (hours <= max), $"{percent}% -> {hours} h");
	}

	[TestMethod]
	public void PaceScalesTheHours() {
		Assert.AreEqual(Planner.PlayHours(5, 1) * 2, Planner.PlayHours(5, 2), 1e-9);
		Assert.AreEqual(Planner.PlayHours(5, 1) / 2, Planner.PlayHours(5, 0.5), 1e-9);
	}

	[TestMethod]
	public void CustomPaceIsProportionalEvenForCommonAchievements() {
		// The panel's custom pace: a 50% achievement after 1 minute; every other time follows the same ratio
		double pace = 1 / 60.0 / Planner.PlayHours(50, 1);

		Assert.AreEqual(1 / 60.0, Planner.PlayHours(50, pace), 1e-9);
		Assert.AreEqual(Planner.PlayHours(80, 1) * pace, Planner.PlayHours(80, pace), 1e-12);
		Assert.AreEqual(Planner.PlayHours(0.01, 1) / Planner.PlayHours(50, 1) / 60, Planner.PlayHours(0.01, pace), 1e-9);
		Assert.IsTrue(pace >= BotSettings.MinPace);
	}

	[TestMethod]
	public void DueTimesRiseWithRarityAndKeepTheGap() {
		List<AchievementDef> schema = Enumerable.Range(0, 40).Select(static i => Def($"a{i}")).ToList();
		Dictionary<string, double> percents = schema.Select((a, i) => (a.ApiName, P: 80.0 / (i + 1))).ToDictionary(static x => x.ApiName, static x => x.P);

		for (int seed = 0; seed < 50; seed++) {
			GamePlan plan = Planner.Create(1, "G", schema, percents, Settings(), 0, new Random(seed), DateTime.UtcNow);
			List<PlanStep> ordered = plan.Steps.OrderBy(static s => s.DueMinute).ToList();

			Assert.IsTrue(ordered.Zip(ordered.Skip(1)).All(static p => p.Second.DueMinute - p.First.DueMinute >= 14), "min gap");
			Assert.AreEqual(plan.LastDue(), plan.BudgetMinutes);

			double common = ordered.Where(static s => s.Percent >= 20).Average(static s => s.DueMinute);
			double rare = ordered.Where(static s => s.Percent < 3).Average(static s => s.DueMinute);
			Assert.IsTrue(rare > common * 5, $"common {common} rare {rare}");
		}
	}

	[TestMethod]
	public void FirstHoursAlreadyHaveSeveralAchievements() {
		// Like a typical story game: a few achievements most players get in the first hours
		List<AchievementDef> schema = Enumerable.Range(0, 8).Select(static i => Def($"a{i}")).ToList();
		Dictionary<string, double> percents = new() { ["a0"] = 76.6, ["a1"] = 65.2, ["a2"] = 45.7, ["a3"] = 34.1, ["a4"] = 17.1, ["a5"] = 8.6, ["a6"] = 2.1, ["a7"] = 0.4 };
		double dueInThreeHours = 0;

		for (int seed = 0; seed < 200; seed++) {
			GamePlan plan = Planner.Create(1, "G", schema, percents, new BotSettings(), 0, new Random(seed), DateTime.UtcNow);
			dueInThreeHours += plan.Steps.Count(static s => s.DueMinute <= 180);
		}

		dueInThreeHours /= 200;
		Assert.IsTrue(dueInThreeHours is >= 3.5 and <= 6, $"average due in 3 h: {dueInThreeHours}");
	}

	[TestMethod]
	public void PlaytimeSoFarStartsTheClock() {
		GamePlan plan = Planner.Create(1, "G", [Def("a1"), Def("a2")], new Dictionary<string, double> { ["a1"] = 70, ["a2"] = 0.5 }, Settings(), 600, new Random(1), DateTime.UtcNow);

		Assert.AreEqual(600, plan.ActiveMinutes);
		Assert.IsTrue(plan.Steps.Single(static s => s.ApiName == "a1").DueMinute <= 600, "a common achievement is already due after 10 h of play");
		Assert.IsTrue(plan.Steps.Single(static s => s.ApiName == "a2").DueMinute > 600, "a 0.5% one is not");
	}

	[TestMethod]
	public void CommonAchievementsComeFirst() {
		List<AchievementDef> schema = [Def("a1"), Def("a2")];
		Dictionary<string, double> percents = new() { ["a1"] = 80, ["a2"] = 2 };

		for (int seed = 0; seed < 300; seed++) {
			GamePlan plan = Planner.Create(1, "G", schema, percents, Settings(), 0, new Random(seed), DateTime.UtcNow);

			Assert.AreEqual("a1", plan.Steps.OrderBy(static s => s.DueMinute).First().ApiName, $"seed {seed}");
		}
	}

	[TestMethod]
	public void SkipsTooRareOnlyWhenPercentKnown() {
		List<AchievementDef> schema = [Def("a1"), Def("a2"), Def("a3")];
		Dictionary<string, double> percents = new() { ["a1"] = 0.3, ["a2"] = 12 };
		BotSettings settings = Settings();
		settings.SkipRarerThan = 1;

		GamePlan plan = Planner.Create(1, "G", schema, percents, settings, 0, new Random(1), DateTime.UtcNow);

		CollectionAssert.AreEquivalent(new[] { "a2", "a3" }, plan.Steps.Select(static s => s.ApiName).ToArray());
		Assert.AreEqual(1, plan.SkippedRare);
	}

	[TestMethod]
	public void CatchUpWaitsRespectTheGap() {
		// 370 h of Steam playtime: every step is already due, so they unlock one gap apart, not all "now"
		GamePlan plan = Planner.Create(1, "G", [Def("a1"), Def("a2"), Def("a3")], new Dictionary<string, double> { ["a1"] = 60, ["a2"] = 20, ["a3"] = 5 }, Settings(), 370 * 60, new Random(1), DateTime.UtcNow);

		List<(PlanStep Step, long Wait)> waits = plan.PendingWaits(20, 5);

		CollectionAssert.AreEqual(new long[] { 5, 25, 45 }, waits.Select(static w => w.Wait).ToArray());
	}

	[TestMethod]
	public void FutureStepsWaitForTheirDueMinute() {
		GamePlan plan = new() { ActiveMinutes = 100, Steps = [new PlanStep { ApiName = "a", DueMinute = 160 }, new PlanStep { ApiName = "b", DueMinute = 170 }, new PlanStep { ApiName = "c", DueMinute = 90, State = StepState.Done }] };

		CollectionAssert.AreEqual(new long[] { 60, 80 }, plan.PendingWaits(20, 0).Select(static w => w.Wait).ToArray());
	}

	[TestMethod]
	public void ExtendCountsFromCurrentPlaytime() {
		BotSettings settings = Settings();
		GamePlan plan = Planner.Create(1, "G", [Def("a1"), Def("a2")], new Dictionary<string, double>(), settings, 0, new Random(1), DateTime.UtcNow);
		plan.ActiveMinutes = 5000;

		int added = Planner.Extend(plan, [Def("a1"), Def("a2"), Def("a3"), Def("a4", isProtected: true)], new Dictionary<string, double>(), settings, new Random(2));

		Assert.AreEqual(1, added);
		Assert.IsTrue(plan.Steps.Single(static s => s.ApiName == "a3").DueMinute > 5000);
	}
}

[TestClass]
public sealed class ChainTests {
	private static AchievementDef Ach(string api, string name, string description, uint? stat = null, uint max = 0) => new(api, name, description, 1, 0, false, false, false, stat, max, "", "");

	[TestMethod]
	public void TiersAlwaysInOrder() {
		List<AchievementDef> schema = [
			Ach("m100", "Veteran", "Play 100 matches"),
			Ach("m30", "Regular", "Play 30 matches"),
			Ach("m1000", "Legend", "Play 1,000 matches"),
			Ach("k50", "Killer", "", stat: 7, max: 50),
			Ach("k10", "Rookie", "", stat: 7, max: 10),
			Ach("x", "Other", "Find the secret"),
			Ach("lvl20", "Level 20", ""),
			Ach("lvl5", "Level 5", "")
		];

		// Rarer tiers get higher percentages on purpose, so the random order would often be wrong
		Dictionary<string, double> percents = new() { ["m1000"] = 60, ["m100"] = 40, ["m30"] = 5, ["k50"] = 50, ["k10"] = 2, ["x"] = 20, ["lvl20"] = 70, ["lvl5"] = 1 };

		for (int seed = 0; seed < 300; seed++) {
			GamePlan plan = Planner.Create(1, "G", schema, percents, new BotSettings { MinGapMinutes = 15 }, 0, new Random(seed), DateTime.UtcNow);
			Dictionary<string, long> due = plan.Steps.ToDictionary(static s => s.ApiName, static s => s.DueMinute);

			Assert.IsTrue(due["m30"] < due["m100"] && due["m100"] < due["m1000"], $"matches, seed {seed}");
			Assert.IsTrue(due["k10"] < due["k50"], $"stat, seed {seed}");
			Assert.IsTrue(due["lvl5"] < due["lvl20"], $"names, seed {seed}");
		}
	}

	[TestMethod]
	[DataRow("Play 1,000 matches", "play # matches")]
	[DataRow("Play 1.000 matches", "play # matches")]
	[DataRow("Reach level 30 in 2 days", "reach level # in # days")]
	[DataRow("Find the secret", null)]
	[DataRow("", null)]
	public void Templates(string text, string? expected) => Assert.AreEqual(expected, Chains.Template(text));

	[TestMethod]
	public void FirstNumberJoinsThousands() => Assert.AreEqual(1000d, Chains.FirstNumber("Play 1,000 matches"));
}

[TestClass]
public sealed class EngineTests {
	private static GamePlan PlanWith(uint appId, DateTime created, params long[] dues) => new() {
		AppId = appId,
		Name = $"G{appId}",
		CreatedUtc = created,
		Steps = dues.Select((d, i) => new PlanStep { ApiName = $"s{i}", Name = $"S{i}", DueMinute = d }).ToList()
	};

	private static TickInput Input(IReadOnlyList<uint> playing, DateTime now, int today = 0, bool inWindow = true, IReadOnlySet<uint>? cooling = null) => new(playing, cooling ?? new HashSet<uint>(), inWindow, now, today);

	[TestMethod]
	public void OutsideWindowCountsPlayButNeverUnlocks() {
		BotState state = new();
		state.Plans[1] = PlanWith(1, DateTime.UtcNow, 0);

		TickOutput output = Engine.Tick(state, new BotSettings(), Input([1], DateTime.UtcNow, inWindow: false));

		Assert.IsNull(output.UnlockStep);
		Assert.AreEqual(1, state.Plans[1].ActiveMinutes, "play time counts at any hour");
	}

	[TestMethod]
	public void AdvancesAndUnlocksWhenDue() {
		BotState state = new();
		state.Plans[1] = PlanWith(1, DateTime.UtcNow, 2, 50);
		BotSettings settings = new();

		Assert.IsNull(Engine.Tick(state, settings, Input([1], DateTime.UtcNow)).UnlockStep);
		TickOutput output = Engine.Tick(state, settings, Input([1], DateTime.UtcNow));

		Assert.AreEqual(2, state.Plans[1].ActiveMinutes);
		Assert.AreEqual("s0", output.UnlockStep?.ApiName);
		Assert.AreEqual(1u, output.UnlockAppId);
	}

	[TestMethod]
	public void RespectsMinGapAndDailyCap() {
		DateTime now = DateTime.UtcNow;
		BotState state = new() { LastUnlockUtc = now.AddMinutes(-5) };
		state.Plans[1] = PlanWith(1, now, 0);
		BotSettings settings = new() { MinGapMinutes = 20, MaxPerDay = 3 };

		Assert.IsNull(Engine.Tick(state, settings, Input([1], now)).UnlockStep, "min gap");

		state.LastUnlockUtc = now.AddMinutes(-30);
		Assert.IsNull(Engine.Tick(state, settings, Input([1], now, today: 3)).UnlockStep, "daily cap");
		Assert.IsNotNull(Engine.Tick(state, settings, Input([1], now, today: 2)).UnlockStep);
	}

	[TestMethod]
	public void SlotsPreferStartedPlansAndRespectLimit() {
		DateTime now = DateTime.UtcNow;
		BotState state = new();
		state.Plans[5] = PlanWith(5, now.AddDays(-2), 100);
		state.Plans[6] = PlanWith(6, now.AddDays(-1), 100);
		state.Plans[7] = PlanWith(7, now, 100);
		state.Plans[7].CompletedUtc = now;

		TickOutput output = Engine.Tick(state, new BotSettings { MaxActiveGames = 2 }, Input([9, 7, 6, 5], now));

		CollectionAssert.AreEqual(new uint[] { 5, 6 }, output.Slots.ToArray());
		Assert.AreEqual(0, output.NeedPlan.Count);

		output = Engine.Tick(state, new BotSettings { MaxActiveGames = 3 }, Input([9, 7, 6, 5], now));
		CollectionAssert.AreEqual(new uint[] { 5, 6, 9 }, output.Slots.ToArray());
		CollectionAssert.AreEqual(new uint[] { 9 }, output.NeedPlan.ToArray());
	}

	[TestMethod]
	public void CoolingGamesGetNoSlotButStillCountPlay() {
		BotState state = new();
		state.Plans[1] = PlanWith(1, DateTime.UtcNow, 0);

		TickOutput output = Engine.Tick(state, new BotSettings(), Input([1], DateTime.UtcNow, cooling: new HashSet<uint> { 1 }));

		Assert.AreEqual(0, output.Slots.Count);
		Assert.IsNull(output.UnlockStep);
		Assert.AreEqual(1, state.Plans[1].ActiveMinutes);
	}

	[TestMethod]
	public void MostOverdueGoesFirst() {
		DateTime now = DateTime.UtcNow;
		BotState state = new();
		state.Plans[1] = PlanWith(1, now.AddDays(-1), 10);
		state.Plans[2] = PlanWith(2, now, 3);
		state.Plans[1].ActiveMinutes = 11;
		state.Plans[2].ActiveMinutes = 40;

		TickOutput output = Engine.Tick(state, new BotSettings(), Input([1, 2], now));

		Assert.AreEqual(2u, output.UnlockAppId);
	}
}

[TestClass]
public sealed class EligibilityTests {
	private static AppFacts Facts(params int[] categories) => new(1, "G", "game", [.. categories], [], ["Dev"], ["Pub"], DateTime.UtcNow);

	private static readonly IReadOnlySet<uint> NoBlacklist = new HashSet<uint>();

	[TestMethod]
	public void BasicVerdicts() {
		BotSettings s = new();

		Assert.AreEqual(Verdict.Ok, Eligibility.Evaluate(1, Facts(Categories.Achievements), s, NoBlacklist, []));
		Assert.AreEqual(Verdict.NoAchievements, Eligibility.Evaluate(1, Facts(), s, NoBlacklist, []));
		Assert.AreEqual(Verdict.Vac, Eligibility.Evaluate(1, Facts(Categories.Achievements, Categories.Vac), s, NoBlacklist, []));
		Assert.AreEqual(Verdict.Online, Eligibility.Evaluate(1, Facts(Categories.Achievements, Categories.OnlineCoop), s, NoBlacklist, []));
		Assert.AreEqual(Verdict.Unknown, Eligibility.Evaluate(1, null, s, NoBlacklist, []));
		Assert.AreEqual(Verdict.MissingDlc, Eligibility.Evaluate(1, Facts(Categories.Achievements), s, NoBlacklist, [5]));
		Assert.AreEqual(Verdict.AsfBlacklisted, Eligibility.Evaluate(1, Facts(Categories.Achievements), s, new HashSet<uint> { 1 }, []));
	}

	[TestMethod]
	public void VacBeatsAllowlistButAllowlistBeatsOnlineAndDlc() {
		BotSettings s = new() { Allowlist = [1] };

		Assert.AreEqual(Verdict.Vac, Eligibility.Evaluate(1, Facts(Categories.Achievements, Categories.Vac), s, NoBlacklist, []));
		Assert.AreEqual(Verdict.Ok, Eligibility.Evaluate(1, Facts(Categories.Achievements, Categories.MultiPlayer), s, NoBlacklist, [5]));
		Assert.AreEqual(Verdict.Ok, Eligibility.Evaluate(1, null, s, NoBlacklist, []));
	}

	[TestMethod]
	public void BlacklistBeatsEverything() {
		BotSettings s = new() { Allowlist = [1], Blacklist = [1] };

		Assert.AreEqual(Verdict.Blacklisted, Eligibility.Evaluate(1, Facts(Categories.Achievements), s, NoBlacklist, []));
	}

	[TestMethod]
	public void DeveloperAndPublisherBlocks() {
		Assert.AreEqual(Verdict.BlockedDeveloper, Eligibility.Evaluate(1, Facts(Categories.Achievements), new BotSettings { BlockedDevelopers = ["dev"] }, NoBlacklist, []));
		Assert.AreEqual(Verdict.BlockedPublisher, Eligibility.Evaluate(1, Facts(Categories.Achievements), new BotSettings { BlockedPublishers = ["PUB"] }, NoBlacklist, []));
	}

	[TestMethod]
	public void TogglesTurnChecksOff() {
		BotSettings s = new() { ExcludeVac = false, ExcludeOnline = false, ProtectDlc = false };

		Assert.AreEqual(Verdict.Ok, Eligibility.Evaluate(1, Facts(Categories.Achievements, Categories.Vac, Categories.MultiPlayer), s, NoBlacklist, [5]));
	}

	[TestMethod]
	public void MissingDlcOnlyCountsUnownedDlcWithAchievements() {
		AppFacts game = new(1, "G", "game", [Categories.Achievements], [10, 11, 12, 13], [], [], DateTime.UtcNow);
		Dictionary<uint, AppFacts> dlcs = new() {
			[10] = new AppFacts(10, "Owned", "dlc", [Categories.Achievements], [], [], [], DateTime.UtcNow),
			[11] = new AppFacts(11, "Soundtrack", "music", [], [], [], [], DateTime.UtcNow),
			[12] = new AppFacts(12, "Expansion", "dlc", [Categories.Achievements], [], [], [], DateTime.UtcNow)
		};

		List<uint> missing = Eligibility.MissingAchievementDlcs(game, dlcs.GetValueOrDefault, new HashSet<uint> { 10 });

		// 12: not owned and has achievements; 13: unknown, assumed risky
		CollectionAssert.AreEquivalent(new uint[] { 12, 13 }, missing);
	}
}

[TestClass]
public sealed class AutoPlayPickerTests {
	[TestMethod]
	public void StartedPlansFirstThenMostPlayed() {
		BotState state = new();
		state.Plans[3] = new GamePlan { AppId = 3, CreatedUtc = DateTime.UtcNow };
		Dictionary<uint, long> playtime = new() { [1] = 10, [2] = 500, [3] = 0, [4] = 100 };

		List<uint> picked = AutoPlayPicker.Pick([1, 2, 3, 4], state, playtime, 3);

		CollectionAssert.AreEqual(new uint[] { 3, 2, 4 }, picked);
	}

	[TestMethod]
	public void CombineKeepsIdleListAndCaps() {
		List<uint> combined = AutoPlayPicker.Combine([1, 2], Enumerable.Range(2, 40).Select(static i => (uint) i));

		Assert.AreEqual(AutoPlayPicker.MaxPlayedGames, combined.Count);
		Assert.AreEqual(1u, combined[0]);
		Assert.AreEqual(combined.Count, combined.Distinct().Count());
	}
}

[TestClass]
public sealed class SettingsTests {
	[TestMethod]
	public void DefaultsAreValid() => Assert.AreEqual(0, new BotSettings().Validate(Clock.IsValidTimeZone).Count);

	[TestMethod]
	public void CatchesBadValues() {
		BotSettings s = new() { PaceScale = 99, ActiveHours = "x", MaxActiveGames = 0, DiscordWebhook = "https://evil.example/api/webhooks/1" };

		Assert.AreEqual(4, s.Validate(Clock.IsValidTimeZone).Count);
	}

	[TestMethod]
	public void NormalizeCleansLists() {
		BotSettings s = new() { Blacklist = [5, 0, 5, 3], BlockedDevelopers = [" Valve ", "valve", ""], Language = "EN" };
		s.Normalize();

		CollectionAssert.AreEqual(new uint[] { 3, 5 }, s.Blacklist);
		CollectionAssert.AreEqual(new[] { "Valve" }, s.BlockedDevelopers);
		Assert.AreEqual("en", s.Language);
	}

	[TestMethod]
	[DataRow("https://discord.com/api/webhooks/1/abc", true)]
	[DataRow("https://discordapp.com/api/webhooks/1/abc", true)]
	[DataRow("http://discord.com/api/webhooks/1/abc", false)]
	[DataRow("https://discord.com.evil.io/api/webhooks/1", false)]
	[DataRow("https://discord.com/other", false)]
	public void WebhookCheck(string url, bool ok) => Assert.AreEqual(ok, BotSettings.IsDiscordWebhook(url));

	[TestMethod]
	public void CloneIsDeep() {
		BotSettings s = new() { Blacklist = [1] };
		BotSettings copy = s.Clone();
		copy.Blacklist.Add(2);

		Assert.AreEqual(1, s.Blacklist.Count);
	}
}

[TestClass]
public sealed class TextsTests {
	[TestMethod]
	public void EveryVerdictHasText() {
		foreach (Verdict verdict in Enum.GetValues<Verdict>()) {
			Assert.AreNotEqual($"verdict.{verdict}", Texts.Verdict("tr", verdict));
			Assert.AreNotEqual($"verdict.{verdict}", Texts.Verdict("en", verdict));
		}
	}

	[TestMethod]
	public void FallsBackToTurkish() => Assert.AreEqual(Texts.Get("tr", "word.on"), Texts.Get("de", "word.on"));
}

[TestClass]
public sealed class SelfTestTests {
	[TestMethod]
	public void AllStepsPassOnTheFullRuntime() => Assert.AreEqual(0, SteadyAchievements.Runtime.SelfTest.Run());
}

[TestClass]
public sealed class ClockTests {
	[TestMethod]
	public void ZonesWorkOnTheFullRuntime() {
		Assert.IsTrue(Clock.IsValidTimeZone("Europe/Istanbul"));
		Assert.IsFalse(Clock.IsValidTimeZone("No/Such_Zone"));
		Assert.IsTrue(Clock.IsValidTimeZone("Europe/Istanbul"), "an unknown id must not switch zones off");

		DateTime utc = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
		Assert.AreEqual(15, Clock.ToLocal(utc, "Europe/Istanbul").Hour);
	}
}
