using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ArchiSteamFarm.IPC.Controllers.Api;
using ArchiSteamFarm.IPC.Responses;
using Microsoft.AspNetCore.Mvc;
using SteadyAchievements.Core;
using SteadyAchievements.Runtime;

namespace SteadyAchievements.IPC;

// REST API behind ASF's IPC password, used by the /steady/ panel.
[Route("Api/Steady")]
public sealed class SteadyController : ArchiController {
	[HttpGet("Bots")]
	[ProducesResponseType<GenericResponse<IReadOnlyList<string>>>((int) HttpStatusCode.OK)]
	public ActionResult<GenericResponse> Bots() => Ok(new GenericResponse<IReadOnlyList<string>>(SteadyAchievementsPlugin.WorkerNames.Order().ToList()));

	[HttpGet("{botName:required}")]
	[ProducesResponseType<GenericResponse<StatusDto>>((int) HttpStatusCode.OK)]
	public async Task<ActionResult<GenericResponse>> Status(string botName) {
		BotWorker? worker = SteadyAchievementsPlugin.GetWorker(botName);

		return worker == null ? NotFoundBot(botName) : Ok(new GenericResponse<StatusDto>(await worker.BuildStatus().ConfigureAwait(false)));
	}

	[HttpGet("{botName:required}/Settings")]
	[ProducesResponseType<GenericResponse<BotSettings>>((int) HttpStatusCode.OK)]
	public async Task<ActionResult<GenericResponse>> GetSettings(string botName) {
		BotWorker? worker = SteadyAchievementsPlugin.GetWorker(botName);

		return worker == null ? NotFoundBot(botName) : Ok(new GenericResponse<BotSettings>(await worker.GetSettings().ConfigureAwait(false)));
	}

	[HttpPost("{botName:required}/Settings")]
	[ProducesResponseType<GenericResponse>((int) HttpStatusCode.OK)]
	[ProducesResponseType<GenericResponse>((int) HttpStatusCode.BadRequest)]
	public async Task<ActionResult<GenericResponse>> SaveSettings(string botName, [FromBody] BotSettings? settings) {
		BotWorker? worker = SteadyAchievementsPlugin.GetWorker(botName);

		if (worker == null) {
			return NotFoundBot(botName);
		}

		if (settings == null) {
			return BadRequest(new GenericResponse(false, "settings"));
		}

		List<string> errors = await worker.UpdateSettings(settings).ConfigureAwait(false);

		return errors.Count == 0 ? Ok(new GenericResponse(true)) : BadRequest(new GenericResponse(false, string.Join(" ", errors)));
	}

	[HttpGet("{botName:required}/Library")]
	[ProducesResponseType<GenericResponse<IReadOnlyList<LibraryGameDto>>>((int) HttpStatusCode.OK)]
	public async Task<ActionResult<GenericResponse>> Library(string botName) {
		BotWorker? worker = SteadyAchievementsPlugin.GetWorker(botName);

		if (worker == null) {
			return NotFoundBot(botName);
		}

		List<LibraryGameDto>? games = await worker.GetLibrary().ConfigureAwait(false);

		return games == null ? StatusCode((int) HttpStatusCode.ServiceUnavailable, new GenericResponse(false, "Steam")) : Ok(new GenericResponse<IReadOnlyList<LibraryGameDto>>(games));
	}

	[HttpGet("{botName:required}/Games/{appId:required}")]
	[ProducesResponseType<GenericResponse<GameDetailDto>>((int) HttpStatusCode.OK)]
	public async Task<ActionResult<GenericResponse>> Game(string botName, uint appId) {
		BotWorker? worker = SteadyAchievementsPlugin.GetWorker(botName);

		if (worker == null) {
			return NotFoundBot(botName);
		}

		if (!worker.Bot.IsConnectedAndLoggedOn) {
			return StatusCode((int) HttpStatusCode.ServiceUnavailable, new GenericResponse(false, Texts.Get(worker.Language, "cmd.notConnected")));
		}

		GameDetailDto? detail = await worker.GetGameDetail(appId).ConfigureAwait(false);

		return detail == null ? StatusCode((int) HttpStatusCode.ServiceUnavailable, new GenericResponse(false, "Steam")) : Ok(new GenericResponse<GameDetailDto>(detail));
	}

	[HttpPost("{botName:required}/Replan/{appId:required}")]
	[ProducesResponseType<GenericResponse>((int) HttpStatusCode.OK)]
	public async Task<ActionResult<GenericResponse>> Replan(string botName, uint appId) {
		BotWorker? worker = SteadyAchievementsPlugin.GetWorker(botName);

		if (worker == null) {
			return NotFoundBot(botName);
		}

		await worker.Replan(appId).ConfigureAwait(false);

		return Ok(new GenericResponse(true));
	}

	[HttpPost("{botName:required}/TestDiscord")]
	[ProducesResponseType<GenericResponse>((int) HttpStatusCode.OK)]
	public async Task<ActionResult<GenericResponse>> TestDiscord(string botName) {
		BotWorker? worker = SteadyAchievementsPlugin.GetWorker(botName);

		return worker == null ? NotFoundBot(botName) : Ok(new GenericResponse(await worker.SendTestNotice().ConfigureAwait(false)));
	}

	private NotFoundObjectResult NotFoundBot(string botName) => NotFound(new GenericResponse(false, $"Bot not found: {botName}"));
}
