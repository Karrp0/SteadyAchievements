using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Globalization;

namespace SteadyAchievements.Core;

// Turkish / English strings for logs, commands and Discord. The web panel has its own copy in www/i18n.js.
internal static class Texts {
	internal const string Turkish = "tr";
	internal const string English = "en";

	private static readonly FrozenDictionary<string, (string Tr, string En)> Strings = new Dictionary<string, (string Tr, string En)>(StringComparer.Ordinal) {
		// Validation
		["val.pace"] = ("Tempo {0} ile {1} arasında olmalı.", "Pace must be between {0} and {1}."),
		["val.activeHours"] = ("Aktif saatler SS:dd-SS:dd biçiminde olmalı (ör. 10:00-01:00).", "Active hours must look like HH:mm-HH:mm (e.g. 10:00-01:00)."),
		["val.timeZone"] = ("Saat dilimi bulunamadı: {0}", "Unknown time zone: {0}"),
		["val.maxActiveGames"] = ("Aynı anda oyun sayısı 1 ile {0} arasında olmalı.", "Games at once must be between 1 and {0}."),
		["val.minGap"] = ("İki başarım arası en az 5, en çok 1440 dakika olmalı.", "Minimum gap must be between 5 and 1440 minutes."),
		["val.maxPerDay"] = ("Günlük sınır 1 ile 200 arasında olmalı.", "Daily limit must be between 1 and 200."),
		["val.minPlaytime"] = ("Gereken oynama süresi 0 ile 1000 saat arasında olmalı.", "Required playtime must be between 0 and 1000 hours."),
		["val.skipRarer"] = ("Nadirlik sınırı 0 ile 50 arasında olmalı.", "Rarity limit must be between 0 and 50."),
		["val.rotate"] = ("Otomatik oyun yenileme 1 ile 48 saat arasında olmalı.", "Auto play refresh must be between 1 and 48 hours."),
		["val.summaryHour"] = ("Özet saati 0 ile 23 arasında olmalı.", "Summary hour must be between 0 and 23."),
		["val.webhook"] = ("Discord webhook adresi https://discord.com/api/webhooks/... biçiminde olmalı.", "Discord webhook must look like https://discord.com/api/webhooks/..."),

		// Verdicts
		["verdict.Ok"] = ("uygun", "eligible"),
		["verdict.Unknown"] = ("mağaza bilgisi alınamadı", "store info unavailable"),
		["verdict.Blacklisted"] = ("kara listede", "on the blacklist"),
		["verdict.AsfBlacklisted"] = ("ASF kara listesinde", "on ASF's blacklist"),
		["verdict.Vac"] = ("VAC korumalı", "VAC protected"),
		["verdict.NotAGame"] = ("oyun değil", "not a game"),
		["verdict.BlockedDeveloper"] = ("engellenen geliştirici", "blocked developer"),
		["verdict.BlockedPublisher"] = ("engellenen yayıncı", "blocked publisher"),
		["verdict.Online"] = ("çevrimiçi / çok oyunculu", "online / multiplayer"),
		["verdict.NoAchievements"] = ("Steam başarımı yok", "no Steam achievements"),
		["verdict.MissingDlc"] = ("sahip olmadığın başarımlı DLC var", "has achievement DLC you don't own"),
		["verdict.Playtime"] = ("oynama süresi henüz {0} saatin altında", "playtime still below {0} h"),

		// Log
		["log.started"] = ("Başladı ({0} plan yüklendi, mod: {1}).", "Started ({0} plans loaded, mode: {1})."),
		["log.excluded"] = ("{0} ({1}) atlandı: {2}.", "Skipping {0} ({1}): {2}."),
		["log.planCreated"] = ("{0} için plan hazır: {1} başarım, toplam ~{2} saat oynamaya yayıldı ({3} korumalı, {4} zaten açık, {5} çok nadir atlandı).", "Plan ready for {0}: {1} achievements spread over ~{2} hours of play in total ({3} protected, {4} already unlocked, {5} too rare skipped)."),
		["log.replanModel"] = ("{0} planı yeni tempo modeline göre yeniden kurulacak.", "The plan for {0} will be rebuilt with the new pace model."),
		["log.planFailed"] = ("{0} ({1}) için plan yapılamadı; 30 dakika sonra tekrar denenecek.", "Could not plan {0} ({1}); retrying in 30 minutes."),
		["log.simulated"] = ("[Simülasyon] Açılacaktı: \"{0}\" — {1} (%{2}).", "[Simulation] Would unlock \"{0}\" — {1} ({2}%)."),
		["log.unlocked"] = ("Açıldı: \"{0}\" — {1} (%{2}).", "Unlocked \"{0}\" — {1} ({2}%)."),
		["log.alreadyUnlocked"] = ("\"{0}\" — {1} zaten açılmış (oyunda açılmış olabilir).", "\"{0}\" — {1} was already unlocked (perhaps in game)."),
		["log.unlockFailed"] = ("\"{0}\" — {1} açılamadı ({2}); oyun 30 dakika bekleyecek.", "Failed to unlock \"{0}\" — {1} ({2}); game waits 30 minutes."),
		["log.stepSkipped"] = ("\"{0}\" — {1} art arda başarısız oldu, atlanıyor.", "\"{0}\" — {1} kept failing, skipping it."),
		["log.completed"] = ("{0} tamamlandı.", "{0} is complete."),
		["log.extended"] = ("{0}: {1} yeni başarım plana eklendi.", "{0}: added {1} new achievements to the plan."),
		["log.modeReal"] = ("Gerçek moda geçildi; simülasyon planları silinip gerçek durumdan yeniden kurulacak.", "Switched to real mode; simulated plans will be rebuilt from the real state."),
		["log.autoPlay"] = ("Otomatik oyun: {0}", "Auto play: {0}"),
		["log.autoStop"] = ("Otomatik oyun bırakıldı, kontrol ASF'de.", "Auto play released, ASF is back in control."),
		["log.tickError"] = ("Beklenmeyen hata: {0}", "Unexpected error: {0}"),

		// Commands
		["cmd.usage"] = ("Kullanım: steady [status|on|off|sim|real|auto on|auto off|plan <appid>|replan <appid>] [botlar]", "Usage: steady [status|on|off|sim|real|auto on|auto off|plan <appid>|replan <appid>] [bots]"),
		["cmd.notConnected"] = ("Bot bağlı değil.", "Bot is not connected."),
		["cmd.enabled"] = ("Açıldı.", "Enabled."),
		["cmd.disabled"] = ("Kapatıldı.", "Disabled."),
		["cmd.simOn"] = ("Simülasyon modu: Steam'de hiçbir şey açılmaz.", "Simulation mode: nothing is unlocked on Steam."),
		["cmd.realOn"] = ("GERÇEK mod: başarımlar Steam'de açılacak.", "REAL mode: achievements will be unlocked on Steam."),
		["cmd.autoOn"] = ("Otomatik oyun açık.", "Auto play on."),
		["cmd.autoOff"] = ("Otomatik oyun kapalı.", "Auto play off."),
		["cmd.noPlan"] = ("{0} için plan yok.", "No plan for {0}."),
		["cmd.replanned"] = ("{0} planı silindi; oyun oynanırken yeniden kurulacak.", "Plan for {0} deleted; it will be rebuilt while the game is played."),
		["cmd.status"] = ("Mod: {0} | {1} | aktif saatler {2} ({3}) | bugün {4}/{5}", "Mode: {0} | {1} | active hours {2} ({3}) | today {4}/{5}"),
		["cmd.game"] = ("• {0}: {1}/{2}, sıradaki \"{3}\" ~{4}", "• {0}: {1}/{2}, next \"{3}\" ~{4}"),
		["cmd.noGames"] = ("Şu an ilerleyen oyun yok.", "No game in progress."),
		["cmd.panel"] = ("Panel: /steady/", "Panel: /steady/"),

		// Words
		["word.simulation"] = ("Simülasyon", "Simulation"),
		["word.real"] = ("GERÇEK", "REAL"),
		["word.on"] = ("açık", "on"),
		["word.off"] = ("kapalı", "off"),
		["word.inWindow"] = ("aktif saatte", "in active hours"),
		["word.outWindow"] = ("aktif saat dışında", "outside active hours"),
		["word.done"] = ("tamam", "done"),
		["word.hours"] = ("{0} sa", "{0} h"),
		["word.minutes"] = ("{0} dk", "{0} min"),
		["word.serverTime"] = ("sunucu saati", "server time"),

		// Discord
		["dc.title.unlock"] = ("Başarım açıldı", "Achievement unlocked"),
		["dc.title.sim"] = ("Başarım açılacaktı (simülasyon)", "Would unlock (simulation)"),
		["dc.title.completed"] = ("Oyun tamamlandı 🎉", "Game complete 🎉"),
		["dc.title.summary"] = ("Günlük özet", "Daily summary"),
		["dc.title.problem"] = ("Sorun", "Problem"),
		["dc.title.test"] = ("Deneme bildirimi", "Test notification"),
		["dc.test"] = ("SteadyAchievements bildirimleri çalışıyor.", "SteadyAchievements notifications work."),
		["dc.unlock"] = ("**{0}** — {1} (%{2})", "**{0}** — {1} ({2}%)"),
		["dc.completed"] = ("**{0}**: planlanan tüm başarımlar açıldı.", "**{0}**: every planned achievement is unlocked."),
		["dc.summary"] = ("Bugün {0} başarım ({1}). İlerleyen oyunlar: {2}.", "Today {0} achievements ({1}). Games in progress: {2}."),
		["dc.summaryNone"] = ("Bugün başarım açılmadı. İlerleyen oyunlar: {0}.", "No achievements today. Games in progress: {0}."),
		["dc.problem.failures"] = ("{0}: \"{1}\" art arda açılamadı ve atlandı.", "{0}: \"{1}\" kept failing and was skipped."),
		["dc.problem.error"] = ("Eklentide hata: {0}", "Plugin error: {0}"),
		["dc.footer.sim"] = ("Simülasyon modu", "Simulation mode"),
		["dc.footer.real"] = ("Gerçek mod", "Real mode")
	}.ToFrozenDictionary(StringComparer.Ordinal);

	internal static string NormalizeLanguage(string? language) => string.Equals(language?.Trim(), English, StringComparison.OrdinalIgnoreCase) ? English : Turkish;

	internal static string Get(string language, string key) {
		if (!Strings.TryGetValue(key, out (string Tr, string En) text)) {
			return key;
		}

		return NormalizeLanguage(language) == English ? text.En : text.Tr;
	}

	internal static string Format(string language, string key, params object?[] args) => string.Format(CultureInfo.InvariantCulture, Get(language, key), args);

	internal static string Verdict(string language, Verdict verdict) => Get(language, $"verdict.{verdict}");

	internal static string Percent(double percent) => percent.ToString(percent < 1 ? "0.##" : "0.#", CultureInfo.InvariantCulture);

	internal static string Duration(string language, double minutes) {
		if (minutes < 60) {
			return Format(language, "word.minutes", Math.Max(0, (int) Math.Round(minutes)));
		}

		return Format(language, "word.hours", (minutes / 60).ToString("0.#", CultureInfo.InvariantCulture));
	}
}
