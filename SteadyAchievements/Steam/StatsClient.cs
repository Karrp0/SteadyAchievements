using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArchiSteamFarm.Core;
using SteamKit2;
using SteamKit2.Internal;

namespace SteadyAchievements.Steam;

internal sealed record OwnedGame(uint AppId, string Name, long PlaytimeMinutes, bool HasStats);

// Talks to Steam over the bot's own connection: reads a user's stats (with the schema) and stores new stat values.
// One instance per bot, registered through IBotSteamClient.
internal sealed class StatsClient : ClientMsgHandler {
	private Player? PlayerService;

	public override void HandleMsg(IPacketMsg packetMsg) {
		ArgumentNullException.ThrowIfNull(packetMsg);

		switch (packetMsg.MsgType) {
			case EMsg.ClientGetUserStatsResponse: {
				ClientMsgProtobuf<CMsgClientGetUserStatsResponse> response = new(packetMsg);
				Client.PostCallback(new UserStatsCallback(response.TargetJobID, response.Body));

				break;
			}
			case EMsg.ClientStoreUserStatsResponse: {
				ClientMsgProtobuf<CMsgClientStoreUserStatsResponse> response = new(packetMsg);
				Client.PostCallback(new StoreStatsCallback(response.TargetJobID, response.Body));

				break;
			}
		}
	}

	internal async Task<CMsgClientGetUserStatsResponse?> GetUserStats(ulong steamId, uint appId) {
		if (!Client.IsConnected) {
			return null;
		}

		ClientMsgProtobuf<CMsgClientGetUserStats> request = new(EMsg.ClientGetUserStats) {
			SourceJobID = Client.GetNextJobID()
		};

		request.Body.game_id = appId;
		request.Body.steam_id_for_user = steamId;
		request.Body.schema_local_version = -1;

		Client.Send(request);

		try {
			UserStatsCallback callback = await new AsyncJob<UserStatsCallback>(Client, request.SourceJobID).ToLongRunningTask().ConfigureAwait(false);

			return callback.Body;
		} catch (Exception e) when (e is TaskCanceledException or TimeoutException) {
			return null;
		}
	}

	internal async Task<CMsgClientStoreUserStatsResponse?> StoreStats(ulong steamId, uint appId, uint crcStats, IReadOnlyCollection<(uint Id, uint Value)> stats) {
		ArgumentNullException.ThrowIfNull(stats);

		if (!Client.IsConnected) {
			return null;
		}

		ClientMsgProtobuf<CMsgClientStoreUserStats2> request = new(EMsg.ClientStoreUserStats2) {
			SourceJobID = Client.GetNextJobID()
		};

		request.Body.game_id = appId;
		request.Body.settor_steam_id = steamId;
		request.Body.settee_steam_id = steamId;
		request.Body.crc_stats = crcStats;
		request.Body.explicit_reset = false;
		request.Body.stats.AddRange(stats.Select(static s => new CMsgClientStoreUserStats2.Stats { stat_id = s.Id, stat_value = s.Value }));

		Client.Send(request);

		try {
			StoreStatsCallback callback = await new AsyncJob<StoreStatsCallback>(Client, request.SourceJobID).ToLongRunningTask().ConfigureAwait(false);

			return callback.Body;
		} catch (Exception e) when (e is TaskCanceledException or TimeoutException) {
			return null;
		}
	}

	// The account's games with playtime (minutes), from Steam's Player service.
	internal async Task<List<OwnedGame>?> GetOwnedGames(ulong steamId) {
		if (!Client.IsConnected) {
			return null;
		}

		PlayerService ??= Client.GetHandler<SteamUnifiedMessages>()?.CreateService<Player>();

		if (PlayerService == null) {
			return null;
		}

		CPlayer_GetOwnedGames_Request request = new() {
			steamid = steamId,
			include_appinfo = true,
			include_played_free_games = true,
			include_free_sub = false,
			skip_unvetted_apps = false
		};

		try {
			SteamUnifiedMessages.ServiceMethodResponse<CPlayer_GetOwnedGames_Response> response = await PlayerService.GetOwnedGames(request).ToLongRunningTask().ConfigureAwait(false);

			if (response.Result != EResult.OK) {
				return null;
			}

			return response.Body.games.Where(static g => g.appid > 0).Select(static g => new OwnedGame((uint) g.appid, g.name ?? "", g.playtime_forever, g.has_community_visible_stats)).ToList();
		} catch (Exception e) when (e is TaskCanceledException or TimeoutException) {
			return null;
		}
	}

	private sealed class UserStatsCallback : CallbackMsg {
		internal CMsgClientGetUserStatsResponse Body { get; }

		internal UserStatsCallback(JobID jobId, CMsgClientGetUserStatsResponse body) {
			JobID = jobId;
			Body = body;
		}
	}

	private sealed class StoreStatsCallback : CallbackMsg {
		internal CMsgClientStoreUserStatsResponse Body { get; }

		internal StoreStatsCallback(JobID jobId, CMsgClientStoreUserStatsResponse body) {
			JobID = jobId;
			Body = body;
		}
	}
}
