using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SteadyAchievements.Core;
using SteadyAchievements.Steam;
using SteamKit2;

namespace SteadyAchievements.Tests;

[TestClass]
public sealed class SchemaParserTests {
	private static KeyValue Node(string name, string? value = null, params KeyValue[] children) {
		KeyValue node = new(name, value);
		node.Children.AddRange(children);

		return node;
	}

	private static KeyValue Schema() => Node(
		"730",
		null,
		Node(
			"stats",
			null,
			Node(
				"1",
				null,
				Node("type", "4"),
				Node(
					"bits",
					null,
					Node("0", null, Node("name", "ACH_A"), Node("display", null, Node("name", null, Node("english", "First"), Node("turkish", "İlk")), Node("desc", null, Node("english", "Do it")), Node("icon", "a.jpg"), Node("icon_gray", "a_g.jpg"))),
					Node("1", null, Node("name", "ACH_SERVER"), Node("permission", "2"), Node("display", null, Node("name", null, Node("english", "Server")))),
					Node("2", null, Node("name", "ACH_PROGRESS"), Node("progress", null, Node("value", null, Node("operand1", "kills")), Node("max_val", "100")), Node("display", null, Node("name", null, Node("english", "Hundred")))),
					Node("3", null, Node("name", "ACH_PROTECTED_STAT"), Node("progress", null, Node("value", null, Node("operand1", "wins")), Node("max_val", "5")), Node("display", null, Node("name", null, Node("english", "Wins")), Node("hidden", "1")))
				)
			),
			Node("7", null, Node("type", "INT"), Node("name", "kills")),
			Node("8", null, Node("type", "1"), Node("name", "wins"), Node("permission", "2"))
		)
	);

	[TestMethod]
	public void ParsesAchievements() {
		List<AchievementDef> list = SchemaParser.Parse(Schema(), new Dictionary<uint, uint> { [1] = 0b0001 }, "tr");

		Assert.AreEqual(4, list.Count);

		AchievementDef a = list.Single(static x => x.ApiName == "ACH_A");
		Assert.AreEqual("İlk", a.Name);
		Assert.AreEqual("Do it", a.Description);
		Assert.IsTrue(a.Unlocked);
		Assert.IsFalse(a.Protected);
		Assert.AreEqual("a_g.jpg", a.IconGray);

		Assert.IsTrue(list.Single(static x => x.ApiName == "ACH_SERVER").Protected);

		AchievementDef progress = list.Single(static x => x.ApiName == "ACH_PROGRESS");
		Assert.IsFalse(progress.Unlocked);
		Assert.IsFalse(progress.Protected);
		Assert.AreEqual(7u, progress.ProgressStatId);
		Assert.AreEqual(100u, progress.ProgressMax);
		Assert.AreEqual(2, progress.Bit);

		AchievementDef wins = list.Single(static x => x.ApiName == "ACH_PROTECTED_STAT");
		Assert.IsTrue(wins.Protected);
		Assert.IsTrue(wins.Hidden);
	}

	[TestMethod]
	public void EnglishNames() => Assert.AreEqual("First", SchemaParser.Parse(Schema(), new Dictionary<uint, uint>(), "en").Single(static x => x.ApiName == "ACH_A").Name);
}

[TestClass]
public sealed class AppInfoTests {
	[TestMethod]
	public void ParsesPicsKeyValues() {
		KeyValue root = new("appinfo");
		KeyValue common = new("common");
		common.Children.Add(new KeyValue("name", "Some Game"));
		common.Children.Add(new KeyValue("type", "Game"));
		KeyValue category = new("category");
		category.Children.Add(new KeyValue("category_22", "1"));
		category.Children.Add(new KeyValue("category_8", "1"));
		category.Children.Add(new KeyValue("category_1", "0"));
		common.Children.Add(category);
		KeyValue associations = new("associations");
		KeyValue dev = new("0");
		dev.Children.Add(new KeyValue("type", "developer"));
		dev.Children.Add(new KeyValue("name", "Studio"));
		associations.Children.Add(dev);
		common.Children.Add(associations);
		KeyValue extended = new("extended");
		extended.Children.Add(new KeyValue("listofdlc", "11, 12,12"));
		extended.Children.Add(new KeyValue("publisher", "Pub"));
		root.Children.Add(common);
		root.Children.Add(extended);

		AppFacts facts = AppInfo.Parse(5, root, DateTime.UtcNow);

		Assert.AreEqual("game", facts.Type);
		Assert.AreEqual("Some Game", facts.Name);
		CollectionAssert.AreEquivalent(new[] { 22, 8 }, facts.Categories.ToArray());
		CollectionAssert.AreEqual(new uint[] { 11, 12 }, facts.Dlcs.ToArray());
		CollectionAssert.AreEqual(new[] { "Studio" }, facts.Developers.ToArray());
		CollectionAssert.AreEqual(new[] { "Pub" }, facts.Publishers.ToArray());
	}
}

[TestClass]
public sealed class GlobalPercentsTests {
	[TestMethod]
	public void ParsesStringsAndNumbers() {
		using JsonDocument doc = JsonDocument.Parse("""{"achievementpercentages":{"achievements":[{"name":"A","percent":"12.5"},{"name":"B","percent":3.25},{"name":"C","percent":"x"},{"percent":"1"}]}}""");

		Dictionary<string, double> result = GlobalPercents.Parse(doc.RootElement);

		Assert.AreEqual(2, result.Count);
		Assert.AreEqual(12.5, result["A"], 1e-9);
		Assert.AreEqual(3.25, result["B"], 1e-9);
	}

	[TestMethod]
	public void EmptyOnUnexpectedShape() {
		using JsonDocument doc = JsonDocument.Parse("[]");

		Assert.AreEqual(0, GlobalPercents.Parse(doc.RootElement).Count);
	}
}
