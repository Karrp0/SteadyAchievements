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

	private static BotSettings Settings() => new() { MinDays = 30, MaxDays = 45, MinGapMinutes = 15 };

	[TestMethod]
	public void ExcludesProtectedAndUnlocked() {
		List<AchievementDef> schema = [Def("a1"), Def("a2", unlocked: true), Def("a3", isProtected: true), Def("a4")];
		GamePlan plan = Planner.Create(1, "G", schema, new Dictionary<string, double>(), Settings(), 900, new Random(1), DateTime.UtcNow);

		CollectionAssert.AreEquivalent(new[] { "a1", "a4" }, plan.Steps.Select(static s => s.ApiName).ToArray());
		Assert.AreEqual(1, plan.ProtectedCount);
		Assert.AreEqual(1, plan.UnlockedBefore);
		Assert.IsFalse(plan.IsCompleted);
	}

	[TestMethod]
	public void NothingToDoIsCompleted() {
		GamePlan plan = Planner.Create(1, "G", [Def("a1", isProtected: true), Def("a2", unlocked: true)], new Dictionary<string, double>(), Settings(), 900, new Random(1), DateTime.UtcNow);

		Assert.AreEqual(0, plan.Steps.Count);
		Assert.IsTrue(plan.IsCompleted);
	}

	[TestMethod]
	public void BudgetWithinDaysAndGapsRespected() {
		List<AchievementDef> schema = Enumerable.Range(0, 40).Select(static i => Def($"a{i}")).ToList();
		Dictionary<string, double> percents = schema.Select((a, i) => (a.ApiName, P: 1.0 + (i * 2))).ToDictionary(static x => x.ApiName, static x => x.P);

		for (int seed = 0; seed < 50; seed++) {
			GamePlan plan = Planner.Create(1, "G", schema, percents, Settings(), 900, new Random(seed), DateTime.UtcNow);
			List<long> due = plan.Steps.Select(static s => s.DueMinute).ToList();

			Assert.IsTrue(plan.BudgetMinutes is >= 30 * 900 and <= 45 * 900, $"budget {plan.BudgetMinutes}");
			Assert.IsTrue(due[0] >= 15);
			Assert.IsTrue(due.Zip(due.Skip(1)).All(static p => p.Second - p.First >= 14), "min gap");

			// Last one lands close to the budget (min-gap clamping may push it a bit further)
			Assert.IsTrue(due[^1] >= plan.BudgetMinutes * 0.95 && due[^1] <= plan.BudgetMinutes * 1.15, $"last {due[^1]} budget {plan.BudgetMinutes}");
		}
	}

	[TestMethod]
	public void HalfDoneGameGetsAboutHalfTheTime() {
		List<AchievementDef> schema = Enumerable.Range(0, 20).Select(i => Def($"a{i}", unlocked: i < 10)).ToList();
		GamePlan plan = Planner.Create(1, "G", schema, new Dictionary<string, double>(), new BotSettings { MinDays = 30, MaxDays = 30, MinGapMinutes = 15 }, 600, new Random(3), DateTime.UtcNow);

		Assert.AreEqual(30 * 600 / 2, plan.BudgetMinutes);
	}

	[TestMethod]
	public void CommonAchievementsTendToComeFirst() {
		List<AchievementDef> schema = [Def("a1"), Def("a2")];
		Dictionary<string, double> percents = new() { ["a1"] = 80, ["a2"] = 2 };
		int commonFirst = 0;

		for (int seed = 0; seed < 1000; seed++) {
			GamePlan plan = Planner.Create(1, "G", schema, percents, Settings(), 900, new Random(seed), DateTime.UtcNow);

			if (plan.Steps.OrderBy(static s => s.DueMinute).First().ApiName == "a1") {
				commonFirst++;
			}
		}

		Assert.IsTrue(commonFirst > 950, $"common first {commonFirst}/1000");
	}

	[TestMethod]
	public void RareGapsAreLonger() {
		List<AchievementDef> schema = Enumerable.Range(0, 2).Select(static i => Def($"a{i}")).ToList();
		Dictionary<string, double> percents = new() { ["a0"] = 90, ["a1"] = 0.5 };
		double rareGap = 0;
		double commonGap = 0;

		for (int seed = 0; seed < 500; seed++) {
			GamePlan plan = Planner.Create(1, "G", schema, percents, new BotSettings { MinDays = 30, MaxDays = 30, MinGapMinutes = 5 }, 900, new Random(seed), DateTime.UtcNow);
			List<PlanStep> ordered = plan.Steps.OrderBy(static s => s.DueMinute).ToList();
			long first = ordered[0].DueMinute;
			long second = ordered[1].DueMinute - first;

			if (ordered[0].ApiName == "a0") {
				commonGap += first;
				rareGap += second;
			}
		}

		Assert.IsTrue(rareGap > commonGap * 2, $"rare {rareGap} common {commonGap}");
	}

	[TestMethod]
	public void SkipsTooRareOnlyWhenPercentKnown() {
		List<AchievementDef> schema = [Def("a1"), Def("a2"), Def("a3")];
		Dictionary<string, double> percents = new() { ["a1"] = 0.3, ["a2"] = 12 };
		BotSettings settings = Settings();
		settings.SkipRarerThan = 1;

		GamePlan plan = Planner.Create(1, "G", schema, percents, settings, 900, new Random(1), DateTime.UtcNow);

		CollectionAssert.AreEquivalent(new[] { "a2", "a3" }, plan.Steps.Select(static s => s.ApiName).ToArray());
		Assert.AreEqual(1, plan.SkippedRare);
	}

	[TestMethod]
	public void ExtendAppendsAfterLastStep() {
		BotSettings settings = Settings();
		GamePlan plan = Planner.Create(1, "G", [Def("a1"), Def("a2")], new Dictionary<string, double>(), settings, 900, new Random(1), DateTime.UtcNow);
		long last = plan.Steps.Max(static s => s.DueMinute);

		int added = Planner.Extend(plan, [Def("a1"), Def("a2"), Def("a3"), Def("a4", isProtected: true)], new Dictionary<string, double>(), settings, 900, new Random(2));

		Assert.AreEqual(1, added);
		Assert.IsTrue(plan.Steps.Single(static s => s.ApiName == "a3").DueMinute > last);
	}
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
	public void OutsideWindowDoesNothing() {
		BotState state = new();
		state.Plans[1] = PlanWith(1, DateTime.UtcNow, 0);

		TickOutput output = Engine.Tick(state, new BotSettings(), Input([1], DateTime.UtcNow, inWindow: false));

		Assert.IsNull(output.UnlockStep);
		Assert.AreEqual(0, state.Plans[1].ActiveMinutes);
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
	public void CoolingGamesAreSkipped() {
		BotState state = new();
		state.Plans[1] = PlanWith(1, DateTime.UtcNow, 0);

		TickOutput output = Engine.Tick(state, new BotSettings(), Input([1], DateTime.UtcNow, cooling: new HashSet<uint> { 1 }));

		Assert.AreEqual(0, output.Slots.Count);
		Assert.AreEqual(0, state.Plans[1].ActiveMinutes);
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
		BotSettings s = new() { MinDays = 40, MaxDays = 10, ActiveHours = "x", MaxActiveGames = 0, DiscordWebhook = "https://evil.example/api/webhooks/1" };

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
