using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ArchiSteamFarm.Core;
using ArchiSteamFarm.Plugins.Interfaces;
using ArchiSteamFarm.Steam;
using JetBrains.Annotations;
using SteadyAchievements.Runtime;
using SteadyAchievements.Steam;
using SteamKit2;

namespace SteadyAchievements;

#pragma warning disable CA1812 // ASF creates this class at runtime
[UsedImplicitly]
internal sealed class SteadyAchievementsPlugin : IBot, IBotConnection, IBotSteamClient, IBotCommand2, IWebInterface {
	private static readonly ConcurrentDictionary<string, BotWorker> Workers = new(StringComparer.Ordinal);

	// The official linux/windows builds are trimmed single-file apps; the "generic" build runs through the dotnet host
	internal static bool TrimmedRuntime { get; } = !string.Equals(Path.GetFileNameWithoutExtension(Environment.ProcessPath), "dotnet", StringComparison.OrdinalIgnoreCase);

	public string Name => nameof(SteadyAchievements);
	public Version Version => typeof(SteadyAchievementsPlugin).Assembly.GetName().Version ?? throw new InvalidOperationException(nameof(Version));

	public string PhysicalPath => "www";
	public string WebPath => "/steady";

	public Task OnLoaded() {
		ASF.ArchiLogger.LogGenericInfo($"{Name} {Version} loaded. Panel: /steady/");

		if (TrimmedRuntime) {
			ASF.ArchiLogger.LogGenericInfo($"[{Name}] Trimmed ASF build detected; the TimeZone setting is not available here, the server time is used.");
		}

		if (SelfTest.Enabled) {
			try {
				_ = SelfTest.Run();
			} catch (Exception e) {
				ASF.ArchiLogger.LogGenericWarning($"[Steady self-test] could not run: {e.GetType().Name}: {e.Message}");
			}
		}

		return Task.CompletedTask;
	}

	internal static BotWorker? GetWorker(Bot bot) => Workers.GetValueOrDefault(bot.BotName);

	internal static BotWorker? GetWorker(string botName) => Workers.GetValueOrDefault(botName);

	internal static IEnumerable<string> WorkerNames => Workers.Keys;

	public Task OnBotInit(Bot bot) => Task.CompletedTask;

	public Task OnBotDestroy(Bot bot) {
		ArgumentNullException.ThrowIfNull(bot);

		if (Workers.TryRemove(bot.BotName, out BotWorker? worker)) {
			worker.Dispose();
		}

		return Task.CompletedTask;
	}

	public async Task<IReadOnlyCollection<ClientMsgHandler>?> OnBotSteamHandlersInit(Bot bot) {
		ArgumentNullException.ThrowIfNull(bot);

		StatsClient client = new();
		BotWorker worker = await BotWorker.Create(bot, client).ConfigureAwait(false);

		if (Workers.TryRemove(bot.BotName, out BotWorker? old)) {
			old.Dispose();
		}

		Workers[bot.BotName] = worker;
		worker.Start();

		return [client];
	}

	public Task OnBotSteamCallbacksInit(Bot bot, CallbackManager callbackManager) => Task.CompletedTask;

	public Task OnBotLoggedOn(Bot bot) {
		ArgumentNullException.ThrowIfNull(bot);

		GetWorker(bot)?.OnLoggedOn();

		return Task.CompletedTask;
	}

	public Task OnBotDisconnected(Bot bot, EResult reason) => Task.CompletedTask;

	public Task<string?> OnBotCommand(Bot bot, EAccess access, string message, string[] args, ulong steamID = 0) => Commands.Handle(bot, access, args, steamID);
}
#pragma warning restore CA1812
