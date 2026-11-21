using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ArchiSteamFarm.Core;
using ArchiSteamFarm.Steam;
using SteadyAchievements.Core;
using SteadyAchievements.Runtime;
using AsfCommands = ArchiSteamFarm.Steam.Interaction.Commands;

namespace SteadyAchievements;

// steady [status|on|off|sim|real|auto on|auto off|plan <appid>|replan <appid>] [bots]
internal static class Commands {
	private static readonly HashSet<string> Prefixes = new(["STEADY", "SA"], StringComparer.OrdinalIgnoreCase);

	internal static async Task<string?> Handle(Bot bot, EAccess access, string[] args, ulong steamID) {
		ArgumentNullException.ThrowIfNull(bot);
		ArgumentNullException.ThrowIfNull(args);

		if ((args.Length == 0) || !Prefixes.Contains(args[0])) {
			return null;
		}

		if (access < EAccess.Master) {
			return null;
		}

		// Parse: subcommand, optional "on/off" (auto), optional appid (plan/replan), the rest are bot names
		List<string> rest = args.Skip(1).ToList();
		string sub = rest.Count > 0 ? rest[0].ToUpperInvariant() : "STATUS";

		if (rest.Count > 0) {
			rest.RemoveAt(0);
		}

		string? flag = null;
		uint appId = 0;

		if (sub == "AUTO") {
			if ((rest.Count == 0) || (rest[0].ToUpperInvariant() is not ("ON" or "OFF"))) {
				return Usage(bot);
			}

			flag = rest[0].ToUpperInvariant();
			rest.RemoveAt(0);
		} else if (sub is "PLAN" or "REPLAN") {
			if ((rest.Count == 0) || !uint.TryParse(rest[0], NumberStyles.None, CultureInfo.InvariantCulture, out appId) || (appId == 0)) {
				return Usage(bot);
			}

			rest.RemoveAt(0);
		} else if (sub is not ("STATUS" or "ON" or "OFF" or "SIM" or "REAL")) {
			return Usage(bot);
		}

		if (rest.Count == 0) {
			return bot.Commands.FormatBotResponse(await Run(bot, sub, flag, appId).ConfigureAwait(false));
		}

		HashSet<Bot>? bots = Bot.GetBots(string.Join(',', rest));

		if ((bots == null) || (bots.Count == 0)) {
			return access >= EAccess.Owner ? AsfCommands.FormatStaticResponse($"? {string.Join(',', rest)}") : null;
		}

		IList<string?> responses = await Utilities.InParallel(
			bots.OrderBy(static b => b.BotName, StringComparer.OrdinalIgnoreCase).Select(
				async target => AsfCommands.GetProxyAccess(target, access, steamID) >= EAccess.Master ? target.Commands.FormatBotResponse(await Run(target, sub, flag, appId).ConfigureAwait(false)) : null
			)
		).ConfigureAwait(false);

		List<string> valid = responses.OfType<string>().ToList();

		return valid.Count > 0 ? string.Join(Environment.NewLine, valid) : null;
	}

	private static string Usage(Bot bot) => bot.Commands.FormatBotResponse(Texts.Get(LanguageOf(bot), "cmd.usage"));

	private static string LanguageOf(Bot bot) => SteadyAchievementsPlugin.GetWorker(bot)?.Language ?? Texts.Turkish;

	private static async Task<string> Run(Bot bot, string sub, string? flag, uint appId) {
		BotWorker? worker = SteadyAchievementsPlugin.GetWorker(bot);

		if (worker == null) {
			return Texts.Get(Texts.Turkish, "cmd.notConnected");
		}

		string lang = (await worker.GetSettings().ConfigureAwait(false)).Language;

		switch (sub) {
			case "STATUS":
				return $"{await worker.TextStatus().ConfigureAwait(false)}{Environment.NewLine}{Texts.Get(lang, "cmd.panel")}";
			case "ON":
				await worker.SetFlags(enabled: true).ConfigureAwait(false);

				return Texts.Get(lang, "cmd.enabled");
			case "OFF":
				await worker.SetFlags(enabled: false).ConfigureAwait(false);

				return Texts.Get(lang, "cmd.disabled");
			case "SIM":
				await worker.SetFlags(simulation: true).ConfigureAwait(false);

				return Texts.Get(lang, "cmd.simOn");
			case "REAL":
				await worker.SetFlags(simulation: false).ConfigureAwait(false);

				return Texts.Get(lang, "cmd.realOn");
			case "AUTO":
				bool on = flag == "ON";
				await worker.SetFlags(autoPlay: on).ConfigureAwait(false);

				return Texts.Get(lang, on ? "cmd.autoOn" : "cmd.autoOff");
			case "PLAN":
				return await worker.TextPlan(appId).ConfigureAwait(false) ?? Texts.Format(lang, "cmd.noPlan", appId);
			case "REPLAN":
				await worker.Replan(appId).ConfigureAwait(false);

				return Texts.Format(lang, "cmd.replanned", appId);
			default:
				return Texts.Get(lang, "cmd.usage");
		}
	}
}
