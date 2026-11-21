"use strict";

// Panel languages. Codes match ASF-ui's locale codes, so choosing a language here also switches ASF's panel.
// Any key missing in a language falls back to English.
window.LANGUAGES = [
	["tr-TR", "Türkçe"],
	["en-US", "English"],
	["de-DE", "Deutsch"],
	["es-ES", "Español"],
	["fr-FR", "Français"],
	["pl-PL", "Polski"],
	["pt-BR", "Português (BR)"],
	["ru-RU", "Русский"],
	["zh-CN", "简体中文"]
];

window.I18N = {
	"tr-TR": {
		"top.menu": "Menü",
		"top.language": "Dil",
		"top.appearance": "Görünüm",
		"top.theme": "Tema rengi",
		"top.dark": "Karanlık mod",
		"top.logout": "Çıkış",

		"nav.groupAchievements": "Başarımlar",
		"nav.overview": "Genel Bakış",
		"nav.games": "Oyunlar",
		"nav.history": "Etkinlik Geçmişi",
		"nav.groupConfig": "Yapılandırma",
		"nav.settings": "Eklenti Ayarları",
		"nav.help": "Nasıl Çalışır?",
		"nav.asf": "ASF Paneline Dön",
		"nav.stats": "İstatistikler",

		"stat.unlocked": "Açılan",
		"stat.simulated": "Simüle",
		"stat.inProgress": "İlerleyen",
		"stat.completed": "Biten",
		"stat.skipped": "Atlanan",
		"stat.today": "Bugün",
		"stat.next": "Sıradaki",
		"stat.mode": "Mod",
		"stat.sim": "Simülasyon",
		"stat.real": "Gerçek",

		"info.today": "Bugün açılan",
		"info.games": "İlerleyen oyun",
		"info.next": "Sıradaki başarım",

		"login.title": "Giriş",
		"login.text": "ASF panelinin şifresini (IPCPassword) gir. ASF paneline girdiysen bu sayfa şifreyi oradan alır.",
		"login.button": "Giriş yap",
		"login.wrong": "Şifre yanlış.",
		"login.banned": "Çok fazla yanlış deneme; bir süre bekle.",

		"status.trimmed": "ASF'nin kırpılmış (trimmed) sürümü çalışıyor. Eklenti bu sürümde de çalışır; sadece saat dilimi ayarı kullanılamaz, sunucunun saati geçerlidir.",
		"status.summary": "Özet",
		"status.mode": "Mod",
		"status.plugin": "Eklenti",
		"status.hours": "Aktif saatler",
		"status.today": "Bugün",
		"status.auto": "Otomatik oyun",
		"status.playing": "Şu an oynanan oyunlar",
		"status.noPlaying": "ASF şu an hiçbir oyun oynamıyor (ya da !play / !pause ile duraklatılmış).",
		"status.games": "İlerleyen oyunlar",
		"status.noGames": "Henüz plan yok. Uygun bir oyun oynanmaya başlayınca burada görünecek.",
		"status.completed": "Tamamlananlar",
		"status.noCompleted": "Henüz tamamlanan oyun yok.",

		"fact.sim": "Simülasyon — Steam'e hiçbir şey gönderilmiyor",
		"fact.real": "Gerçek — başarımlar Steam'de açılıyor",
		"fact.on": "Açık",
		"fact.off": "Kapalı",
		"fact.inWindow": "şu an aktif saatte",
		"fact.outWindow": "şu an aktif saat dışında",
		"fact.today": "{0} / {1} başarım",
		"fact.autoActive": "Açık — şu an oyun oynatıyor",
		"fact.autoWaiting": "Açık — ASF boşta kalınca devreye girer",

		"play.slot": "ilerliyor",
		"play.waiting": "sırada",
		"play.finished": "açılacak başarım yok",

		"game.next": "Sıradaki",
		"game.end": "Bitiş",
		"game.view": "Başarımları gör",
		"game.replan": "Planı sıfırla",
		"game.replanAsk": "Bu oyunun planı silinip oyun oynanırken baştan kurulsun mu?",
		"game.protected": "korumalı",
		"game.before": "zaten açıktı",
		"game.rare": "çok nadir",
		"game.paused": "duraklatıldı (oynanmıyor)",
		"game.nothing": "Açılabilecek başarım yok",
		"game.simulated": "simüle",
		"game.completedAt": "Tamamlandı",

		"ach.unlocked": "Açık",
		"ach.done": "Eklenti açtı",
		"ach.simulated": "Simüle edildi",
		"ach.planned": "Planlandı",
		"ach.protected": "Korumalı (sadece oyun sunucusu açar)",
		"ach.skipped": "Atlandı",
		"ach.rare": "Plan dışı (çok nadir)",
		"ach.none": "Plan yok",
		"ach.hidden": "gizli",
		"ach.loading": "Steam'den okunuyor…",
		"ach.failed": "Steam'den okunamadı.",

		"set.run": "Çalışma",
		"set.mode": "Mod",
		"set.sim": "Simülasyon",
		"set.real": "Gerçek",
		"set.modeHelp": "Simülasyon Steam'e hiçbir şey göndermez, sadece neyi ne zaman açacağını kaydeder. Gerçek modda başarımlar Steam'de açılır.",
		"set.enabled": "Eklenti bu bot için açık",
		"set.language": "Discord ve komut dili",
		"set.languageHelp": "Discord mesajları, ASF komut yanıtları ve kayıtlar bu dilde olur (Türkçe veya İngilizce).",
		"set.pace": "Tempo",
		"set.minDays": "Bir oyun en az (gün)",
		"set.maxDays": "En çok (gün)",
		"set.daysHelp": "Her oyunun başarımları bu aralıkta rastgele seçilen bir süreye yayılır.",
		"set.from": "Aktif saat başlangıcı",
		"set.to": "Bitişi",
		"set.hoursHelp": "Başarımlar sadece bu saatler arasında açılır. Gece yarısını geçebilir (ör. 10:00–01:00). İkisi aynıysa bütün gün.",
		"set.timeZone": "Saat dilimi",
		"set.timeZoneHelp": "Boş bırakılırsa sunucunun saati kullanılır.",
		"set.maxActive": "Aynı anda oyun",
		"set.maxPerDay": "Günde en fazla başarım",
		"set.minGap": "İki başarım arası en az (dk)",
		"set.minPlaytime": "Başlamak için gereken oynama (saat)",
		"set.skipRarer": "Şundan nadir olanları atla (%)",
		"set.paceHelp": "Oynama süresi Steam'deki toplam süredir. Nadirlik sınırı 0 ise kapalıdır; örneğin 1 yazarsan oyuncuların %1'inden azının açtığı başarımlar hiç açılmaz.",
		"set.games": "Hangi oyunlar",
		"set.vac": "VAC korumalı oyunları atla (izin listesi bunu aşamaz)",
		"set.online": "Çevrimiçi / çok oyunculu oyunları atla",
		"set.dlc": "Sahip olmadığım başarımlı DLC'si olan oyunları atla",
		"set.asfBlacklist": "ASF'nin kendi kara listesine de uy",
		"set.blacklist": "Kara liste (AppID)",
		"set.blacklistHelp": "Virgülle ayır. Bu oyunlara asla dokunulmaz.",
		"set.allowlist": "İzin listesi (AppID)",
		"set.allowlistHelp": "Çevrimiçi, DLC ve geliştirici kontrollerini atlar; VAC kontrolünü atlamaz.",
		"set.developers": "Engellenen geliştiriciler",
		"set.publishers": "Engellenen yayıncılar",
		"set.namesHelp": "Virgülle ayır, Steam'de yazdığı gibi (ör. Valve).",
		"set.auto": "Otomatik oyun",
		"set.autoPlay": "ASF boştayken kütüphanemden uygun oyunları kendin seç ve oynat",
		"set.autoHelp": "Kart düşürülecek bir şey yoksa ve aktif saatteysek, başarımı kalan uygun oyunları açar. Oyun listen (GamesPlayedWhileIdle) da oynanmaya devam eder. Kart çıkınca kontrol ASF'ye geri döner.",
		"set.rotate": "ASF'ye kart kontrolü için ara ver (her … saatte bir)",
		"set.webhook": "Webhook adresi",
		"set.nCompleted": "Bir oyun bitince haber ver",
		"set.nEach": "Her başarımda haber ver",
		"set.nSummary": "Günlük özet gönder",
		"set.summaryHour": "Özet saati (0–23)",
		"set.nProblems": "Sorun olunca haber ver",
		"set.test": "Deneme mesajı gönder",
		"set.testOk": "Gönderildi, Discord'a bak.",
		"set.testFail": "Gönderilemedi. Önce adresi kaydet ve kontrol et.",
		"set.save": "Kaydet",
		"set.saved": "Kaydedildi.",
		"set.badHours": "Aktif saatleri seç.",

		"real.title": "Gerçek moda geçilsin mi?",
		"real.text": "Başarımlar Steam'de gerçekten açılacak ve geri alınamaz. Başarımları oynamadan açmak Steam kurallarına aykırıdır; risk sana aittir. Simülasyonda yapılan ilerleme silinir, planlar hesabının gerçek durumundan yeniden kurulur.",
		"real.cancel": "Vazgeç",
		"real.ok": "Evet, gerçek moda geç",

		"hist.Unlocked": "Açıldı",
		"hist.Simulated": "Simülasyon",
		"hist.Failed": "Başarısız",
		"hist.Skipped": "Atlandı",
		"hist.Completed": "Oyun bitti",
		"hist.empty": "Henüz kayıt yok.",

		"time.now": "şimdi",
		"time.in": "{0} içinde",
		"time.ago": "{0} önce",
		"time.min": "{0} dk",
		"time.hour": "{0} sa",
		"time.day": "{0} gün",

		"err.network": "Sunucuya ulaşılamadı.",
		"err.server": "Hata: {0}",

		"help.html": `
<h2>Nasıl çalışır?</h2>
<p>SteadyAchievements, ASF'nin <strong>o anda oynadığı</strong> oyunlarda başarımları yavaş ve doğal bir tempoyla açar: kart düşürülen oyunlar, oyun listen (<code>GamesPlayedWhileIdle</code>) ya da açarsan otomatik seçilen oyunlar.</p>
<ul>
<li><strong>Plan:</strong> bir oyun ilk kez uygun şekilde oynandığında, kalan başarımlar seçtiğin gün aralığına yayılır. Yaygın başarımlar genelde önce, nadir olanlar sonra gelir; sıra ve aralıklar her seferinde rastgeledir.</li>
<li><strong>Kademeler:</strong> "30 maç" ve "100 maç" gibi kademeli başarımlar her zaman küçükten büyüğe açılır.</li>
<li><strong>Zaman:</strong> sadece oyun oynanırken ve aktif saatlerdeyken ilerler. Oyun oynanmıyorsa plan bekler.</li>
<li><strong>Sınırlar:</strong> iki başarım arasında en az belirlenen dakika kadar beklenir; günlük sınır aşılmaz.</li>
<li><strong>Güvenlik:</strong> VAC'lı, çevrimiçi ve sahip olmadığın başarımlı DLC'si olan oyunlar varsayılan olarak atlanır. Sadece oyun sunucusunun açabildiği (korumalı) başarımlara hiç dokunulmaz.</li>
<li><strong>Simülasyon:</strong> eklenti simülasyonda başlar; Steam'e hiçbir şey gitmez. Gerçek moda geçmek senin kararın.</li>
</ul>
<h2>ASF komutları</h2>
<p><code>steady</code> (kısaca <code>sa</code>) ve ardından: <code>status</code>, <code>on</code>, <code>off</code>, <code>sim</code>, <code>real</code>, <code>auto on</code>, <code>auto off</code>, <code>plan &lt;appid&gt;</code>, <code>replan &lt;appid&gt;</code>. Sona bot adları eklenebilir.</p>
<h2>Uyarı</h2>
<p>Başarımları oynamadan açmak Steam kurallarına aykırıdır. Eklentiyi kullanmak senin sorumluluğundadır.</p>`
	},

	"en-US": {
		"top.menu": "Menu",
		"top.language": "Language",
		"top.appearance": "Appearance",
		"top.theme": "Theme colour",
		"top.dark": "Dark mode",
		"top.logout": "Sign out",

		"nav.groupAchievements": "Achievements",
		"nav.overview": "Overview",
		"nav.games": "Games",
		"nav.history": "Activity Log",
		"nav.groupConfig": "Configuration",
		"nav.settings": "Plugin Settings",
		"nav.help": "How It Works",
		"nav.asf": "Back to ASF",
		"nav.stats": "Statistics",

		"stat.unlocked": "Unlocked",
		"stat.simulated": "Simulated",
		"stat.inProgress": "In progress",
		"stat.completed": "Completed",
		"stat.skipped": "Skipped",
		"stat.today": "Today",
		"stat.next": "Next",
		"stat.mode": "Mode",
		"stat.sim": "Simulation",
		"stat.real": "Real",

		"info.today": "Unlocked today",
		"info.games": "Games in progress",
		"info.next": "Next achievement",

		"login.title": "Sign in",
		"login.text": "Enter ASF's IPC password. If you are signed in to ASF's panel, this page uses that password.",
		"login.button": "Sign in",
		"login.wrong": "Wrong password.",
		"login.banned": "Too many wrong attempts; wait a while.",

		"status.trimmed": "A trimmed ASF build is running. The plugin works on it; only the time zone setting is unavailable, the server time is used.",
		"status.summary": "Summary",
		"status.mode": "Mode",
		"status.plugin": "Plugin",
		"status.hours": "Active hours",
		"status.today": "Today",
		"status.auto": "Auto play",
		"status.playing": "Games being played",
		"status.noPlaying": "ASF is not playing anything right now (or it was paused with !play / !pause).",
		"status.games": "Games in progress",
		"status.noGames": "No plans yet. Eligible games show up here once they are played.",
		"status.completed": "Completed",
		"status.noCompleted": "No completed games yet.",

		"fact.sim": "Simulation — nothing is sent to Steam",
		"fact.real": "Real — achievements are unlocked on Steam",
		"fact.on": "On",
		"fact.off": "Off",
		"fact.inWindow": "inside active hours now",
		"fact.outWindow": "outside active hours now",
		"fact.today": "{0} / {1} achievements",
		"fact.autoActive": "On — playing games now",
		"fact.autoWaiting": "On — starts when ASF is idle",

		"play.slot": "progressing",
		"play.waiting": "waiting",
		"play.finished": "nothing left to unlock",

		"game.next": "Next",
		"game.end": "Finish",
		"game.view": "View achievements",
		"game.replan": "Reset plan",
		"game.replanAsk": "Delete this game's plan and rebuild it while the game is played?",
		"game.protected": "protected",
		"game.before": "already unlocked",
		"game.rare": "too rare",
		"game.paused": "paused (not being played)",
		"game.nothing": "Nothing left to unlock",
		"game.simulated": "simulated",
		"game.completedAt": "Completed",

		"ach.unlocked": "Unlocked",
		"ach.done": "Unlocked by the plugin",
		"ach.simulated": "Simulated",
		"ach.planned": "Planned",
		"ach.protected": "Protected (only the game's servers unlock it)",
		"ach.skipped": "Skipped",
		"ach.rare": "Not planned (too rare)",
		"ach.none": "No plan",
		"ach.hidden": "hidden",
		"ach.loading": "Reading from Steam…",
		"ach.failed": "Could not read from Steam.",

		"set.run": "How it runs",
		"set.mode": "Mode",
		"set.sim": "Simulation",
		"set.real": "Real",
		"set.modeHelp": "Simulation sends nothing to Steam and only records what it would unlock and when. Real mode unlocks achievements on Steam.",
		"set.enabled": "Plugin enabled for this bot",
		"set.language": "Discord and command language",
		"set.languageHelp": "Discord messages, ASF command replies and log lines use this language (Turkish or English).",
		"set.pace": "Pace",
		"set.minDays": "A game takes at least (days)",
		"set.maxDays": "At most (days)",
		"set.daysHelp": "Each game's achievements are spread over a random duration in this range.",
		"set.from": "Active hours from",
		"set.to": "To",
		"set.hoursHelp": "Achievements only unlock between these hours. It may cross midnight (e.g. 10:00–01:00). Equal times mean all day.",
		"set.timeZone": "Time zone",
		"set.timeZoneHelp": "Leave empty to use the server's time.",
		"set.maxActive": "Games at once",
		"set.maxPerDay": "Max achievements per day",
		"set.minGap": "Minimum gap (min)",
		"set.minPlaytime": "Playtime needed to start (hours)",
		"set.skipRarer": "Skip rarer than (%)",
		"set.paceHelp": "Playtime is the total shown by Steam. A rarity limit of 0 is off; for example 1 never unlocks achievements that fewer than 1% of players have.",
		"set.games": "Which games",
		"set.vac": "Skip VAC protected games (the allowlist cannot override this)",
		"set.online": "Skip online / multiplayer games",
		"set.dlc": "Skip games with achievement DLC I don't own",
		"set.asfBlacklist": "Also respect ASF's own blacklist",
		"set.blacklist": "Blacklist (AppIDs)",
		"set.blacklistHelp": "Comma separated. These games are never touched.",
		"set.allowlist": "Allowlist (AppIDs)",
		"set.allowlistHelp": "Bypasses the online, DLC and developer checks; never the VAC check.",
		"set.developers": "Blocked developers",
		"set.publishers": "Blocked publishers",
		"set.namesHelp": "Comma separated, as written on Steam (e.g. Valve).",
		"set.auto": "Auto play",
		"set.autoPlay": "When ASF is idle, pick eligible games from my library and play them",
		"set.autoHelp": "When there are no cards to farm and we are in active hours, it plays eligible games that still have achievements. Your idle list (GamesPlayedWhileIdle) keeps playing too. When cards show up, ASF takes over again.",
		"set.rotate": "Let ASF check for cards every … hours",
		"set.webhook": "Webhook URL",
		"set.nCompleted": "Notify when a game is complete",
		"set.nEach": "Notify on every achievement",
		"set.nSummary": "Send a daily summary",
		"set.summaryHour": "Summary hour (0–23)",
		"set.nProblems": "Notify about problems",
		"set.test": "Send a test message",
		"set.testOk": "Sent, check Discord.",
		"set.testFail": "Could not send. Save the URL first and check it.",
		"set.save": "Save",
		"set.saved": "Saved.",
		"set.badHours": "Pick the active hours.",

		"real.title": "Switch to real mode?",
		"real.text": "Achievements will really be unlocked on Steam and this cannot be undone. Unlocking achievements without playing is against Steam's rules; the risk is yours. Simulated progress is discarded and plans are rebuilt from your account's real state.",
		"real.cancel": "Cancel",
		"real.ok": "Yes, switch to real mode",

		"hist.Unlocked": "Unlocked",
		"hist.Simulated": "Simulated",
		"hist.Failed": "Failed",
		"hist.Skipped": "Skipped",
		"hist.Completed": "Game complete",
		"hist.empty": "Nothing yet.",

		"time.now": "now",
		"time.in": "in {0}",
		"time.ago": "{0} ago",
		"time.min": "{0} min",
		"time.hour": "{0} h",
		"time.day": "{0} d",

		"err.network": "Could not reach the server.",
		"err.server": "Error: {0}",

		"help.html": `
<h2>How it works</h2>
<p>SteadyAchievements slowly and naturally unlocks achievements in the games ASF is <strong>playing right now</strong>: games being farmed for cards, your idle list (<code>GamesPlayedWhileIdle</code>) or, if enabled, games it picks itself.</p>
<ul>
<li><strong>Plan:</strong> the first time a game is eligibly played, its remaining achievements are spread over your day range. Common achievements tend to come first and rare ones last; order and gaps are random every time.</li>
<li><strong>Tiers:</strong> tiered achievements such as "30 matches" and "100 matches" always unlock from the smallest to the largest.</li>
<li><strong>Time:</strong> it only moves while the game is played and inside the active hours. If the game is not played, the plan waits.</li>
<li><strong>Limits:</strong> there is always a minimum gap between achievements and a daily cap.</li>
<li><strong>Safety:</strong> VAC games, online games and games with achievement DLC you don't own are skipped by default. Achievements only the game's servers can unlock (protected) are never touched.</li>
<li><strong>Simulation:</strong> the plugin starts in simulation; nothing is sent to Steam. Switching to real mode is your call.</li>
</ul>
<h2>ASF commands</h2>
<p><code>steady</code> (or <code>sa</code>) followed by: <code>status</code>, <code>on</code>, <code>off</code>, <code>sim</code>, <code>real</code>, <code>auto on</code>, <code>auto off</code>, <code>plan &lt;appid&gt;</code>, <code>replan &lt;appid&gt;</code>. Bot names may follow.</p>
<h2>Warning</h2>
<p>Unlocking achievements without playing is against Steam's rules. Using this plugin is your own responsibility.</p>`
	},

	"de-DE": {
		"top.menu": "Menü", "top.language": "Sprache", "top.appearance": "Darstellung", "top.theme": "Designfarbe", "top.dark": "Dunkelmodus", "top.logout": "Abmelden",
		"nav.groupAchievements": "Erfolge", "nav.overview": "Übersicht", "nav.games": "Spiele", "nav.history": "Aktivitätsverlauf", "nav.groupConfig": "Konfiguration", "nav.settings": "Plugin-Einstellungen", "nav.help": "So funktioniert's", "nav.asf": "Zurück zu ASF", "nav.stats": "Statistiken",
		"stat.unlocked": "Freigeschaltet", "stat.simulated": "Simuliert", "stat.inProgress": "Laufend", "stat.completed": "Fertig", "stat.skipped": "Übersprungen", "stat.today": "Heute", "stat.next": "Nächster", "stat.mode": "Modus", "stat.sim": "Simulation", "stat.real": "Echt",
		"info.today": "Heute freigeschaltet", "info.games": "Laufende Spiele", "info.next": "Nächster Erfolg",
		"login.title": "Anmelden", "login.text": "Gib das IPC-Passwort von ASF ein. Bist du im ASF-Panel angemeldet, wird dieses Passwort verwendet.", "login.button": "Anmelden", "login.wrong": "Falsches Passwort.", "login.banned": "Zu viele Fehlversuche; bitte warten.",
		"status.trimmed": "Es läuft ein getrimmter ASF-Build. Für Plugins wird der 'generic'-Build empfohlen; die Zeitzonen-Einstellung funktioniert hier evtl. nicht.",
		"status.summary": "Zusammenfassung", "status.mode": "Modus", "status.plugin": "Plugin", "status.hours": "Aktive Stunden", "status.today": "Heute", "status.auto": "Auto-Spielen",
		"status.playing": "Gerade gespielte Spiele", "status.noPlaying": "ASF spielt gerade nichts (oder wurde mit !play / !pause angehalten).", "status.games": "Laufende Spiele", "status.noGames": "Noch keine Pläne. Geeignete Spiele erscheinen hier, sobald sie gespielt werden.", "status.completed": "Abgeschlossen", "status.noCompleted": "Noch keine abgeschlossenen Spiele.",
		"fact.sim": "Simulation — es wird nichts an Steam gesendet", "fact.real": "Echt — Erfolge werden auf Steam freigeschaltet", "fact.on": "An", "fact.off": "Aus", "fact.inWindow": "gerade innerhalb der aktiven Stunden", "fact.outWindow": "gerade außerhalb der aktiven Stunden", "fact.today": "{0} / {1} Erfolge", "fact.autoActive": "An — spielt gerade", "fact.autoWaiting": "An — startet, wenn ASF untätig ist",
		"play.slot": "läuft", "play.waiting": "wartet", "play.finished": "nichts mehr freizuschalten",
		"game.next": "Nächster", "game.end": "Ende", "game.view": "Erfolge ansehen", "game.replan": "Plan zurücksetzen", "game.replanAsk": "Plan dieses Spiels löschen und beim Spielen neu erstellen?", "game.protected": "geschützt", "game.before": "bereits freigeschaltet", "game.rare": "zu selten", "game.paused": "pausiert (wird nicht gespielt)", "game.nothing": "Nichts mehr freizuschalten", "game.simulated": "simuliert", "game.completedAt": "Abgeschlossen",
		"ach.unlocked": "Freigeschaltet", "ach.done": "Vom Plugin freigeschaltet", "ach.simulated": "Simuliert", "ach.planned": "Geplant", "ach.protected": "Geschützt (nur die Spielserver schalten ihn frei)", "ach.skipped": "Übersprungen", "ach.rare": "Nicht geplant (zu selten)", "ach.none": "Kein Plan", "ach.hidden": "versteckt", "ach.loading": "Wird von Steam gelesen…", "ach.failed": "Konnte nicht von Steam gelesen werden.",
		"set.run": "Betrieb", "set.mode": "Modus", "set.sim": "Simulation", "set.real": "Echt", "set.modeHelp": "Die Simulation sendet nichts an Steam und protokolliert nur, was wann freigeschaltet würde. Im echten Modus werden Erfolge auf Steam freigeschaltet.", "set.enabled": "Plugin für diesen Bot aktiviert",
		"set.language": "Sprache für Discord und Befehle", "set.languageHelp": "Discord-Nachrichten, ASF-Befehlsantworten und Logzeilen (Türkisch oder Englisch).",
		"set.pace": "Tempo", "set.minDays": "Ein Spiel dauert mindestens (Tage)", "set.maxDays": "Höchstens (Tage)", "set.daysHelp": "Die Erfolge jedes Spiels werden über eine zufällige Dauer in diesem Bereich verteilt.", "set.from": "Aktive Stunden von", "set.to": "Bis", "set.hoursHelp": "Erfolge werden nur in diesem Zeitraum freigeschaltet. Er darf über Mitternacht gehen (z. B. 10:00–01:00). Gleiche Zeiten bedeuten ganztägig.", "set.timeZone": "Zeitzone", "set.timeZoneHelp": "Leer lassen, um die Serverzeit zu verwenden.",
		"set.maxActive": "Spiele gleichzeitig", "set.maxPerDay": "Max. Erfolge pro Tag", "set.minGap": "Mindestabstand (Min.)", "set.minPlaytime": "Nötige Spielzeit zum Start (Std.)", "set.skipRarer": "Seltener als (%) überspringen", "set.paceHelp": "Die Spielzeit ist die Gesamtzeit laut Steam. Ein Seltenheitslimit von 0 ist aus; z. B. 1 schaltet nie Erfolge frei, die weniger als 1 % der Spieler haben.",
		"set.games": "Welche Spiele", "set.vac": "VAC-geschützte Spiele überspringen (die Erlaubnisliste kann das nicht aufheben)", "set.online": "Online-/Mehrspielerspiele überspringen", "set.dlc": "Spiele mit Erfolgs-DLC überspringen, den ich nicht besitze", "set.asfBlacklist": "Auch ASFs eigene Blacklist beachten", "set.blacklist": "Blacklist (AppIDs)", "set.blacklistHelp": "Durch Kommas getrennt. Diese Spiele werden nie angerührt.", "set.allowlist": "Erlaubnisliste (AppIDs)", "set.allowlistHelp": "Umgeht die Online-, DLC- und Entwicklerprüfung, nie aber die VAC-Prüfung.", "set.developers": "Blockierte Entwickler", "set.publishers": "Blockierte Publisher", "set.namesHelp": "Durch Kommas getrennt, wie auf Steam geschrieben (z. B. Valve).",
		"set.auto": "Auto-Spielen", "set.autoPlay": "Wenn ASF untätig ist, geeignete Spiele aus meiner Bibliothek wählen und spielen", "set.autoHelp": "Gibt es keine Karten zu farmen und sind wir in den aktiven Stunden, werden geeignete Spiele mit offenen Erfolgen gespielt. Deine Leerlaufliste (GamesPlayedWhileIdle) läuft weiter. Tauchen Karten auf, übernimmt ASF wieder.", "set.rotate": "ASF alle … Stunden nach Karten suchen lassen",
		"set.webhook": "Webhook-URL", "set.nCompleted": "Benachrichtigen, wenn ein Spiel fertig ist", "set.nEach": "Bei jedem Erfolg benachrichtigen", "set.nSummary": "Tägliche Zusammenfassung senden", "set.summaryHour": "Uhrzeit der Zusammenfassung (0–23)", "set.nProblems": "Bei Problemen benachrichtigen", "set.test": "Testnachricht senden", "set.testOk": "Gesendet, sieh in Discord nach.", "set.testFail": "Senden fehlgeschlagen. Speichere zuerst die URL und prüfe sie.", "set.save": "Speichern", "set.saved": "Gespeichert.", "set.badHours": "Wähle die aktiven Stunden.",
		"real.title": "In den echten Modus wechseln?", "real.text": "Erfolge werden wirklich auf Steam freigeschaltet; das lässt sich nicht rückgängig machen. Erfolge ohne Spielen freizuschalten verstößt gegen die Steam-Regeln; das Risiko trägst du. Simulierter Fortschritt wird verworfen und die Pläne werden aus dem echten Kontostand neu erstellt.", "real.cancel": "Abbrechen", "real.ok": "Ja, echten Modus aktivieren",
		"hist.Unlocked": "Freigeschaltet", "hist.Simulated": "Simuliert", "hist.Failed": "Fehlgeschlagen", "hist.Skipped": "Übersprungen", "hist.Completed": "Spiel abgeschlossen", "hist.empty": "Noch nichts.",
		"time.now": "jetzt", "time.in": "in {0}", "time.ago": "vor {0}", "time.min": "{0} Min.", "time.hour": "{0} Std.", "time.day": "{0} T.",
		"err.network": "Server nicht erreichbar.", "err.server": "Fehler: {0}",
		"help.html": `<h2>So funktioniert's</h2><p>SteadyAchievements schaltet Erfolge in den Spielen, die ASF <strong>gerade spielt</strong>, langsam und natürlich frei: beim Kartenfarmen, in deiner Leerlaufliste (<code>GamesPlayedWhileIdle</code>) oder, falls aktiviert, in selbst gewählten Spielen.</p><ul><li><strong>Plan:</strong> Die verbleibenden Erfolge werden über deinen Tagesbereich verteilt; häufige meist zuerst, seltene zuletzt, Reihenfolge und Abstände zufällig.</li><li><strong>Stufen:</strong> Gestufte Erfolge wie „30 Matches“ und „100 Matches“ werden immer vom kleinsten zum größten freigeschaltet.</li><li><strong>Zeit:</strong> Fortschritt nur während das Spiel läuft und innerhalb der aktiven Stunden.</li><li><strong>Sicherheit:</strong> VAC-, Online-Spiele und Spiele mit nicht besessenem Erfolgs-DLC werden standardmäßig übersprungen; geschützte Erfolge werden nie angerührt.</li><li><strong>Simulation:</strong> Das Plugin startet in der Simulation; der Wechsel in den echten Modus ist deine Entscheidung.</li></ul><h2>ASF-Befehle</h2><p><code>steady</code> (oder <code>sa</code>) gefolgt von <code>status</code>, <code>on</code>, <code>off</code>, <code>sim</code>, <code>real</code>, <code>auto on</code>, <code>auto off</code>, <code>plan &lt;appid&gt;</code>, <code>replan &lt;appid&gt;</code>.</p><h2>Warnung</h2><p>Erfolge ohne Spielen freizuschalten verstößt gegen die Steam-Regeln. Die Nutzung erfolgt auf eigene Verantwortung.</p>`
	},

	"es-ES": {
		"top.menu": "Menú", "top.language": "Idioma", "top.appearance": "Apariencia", "top.theme": "Color del tema", "top.dark": "Modo oscuro", "top.logout": "Cerrar sesión",
		"nav.groupAchievements": "Logros", "nav.overview": "Resumen", "nav.games": "Juegos", "nav.history": "Historial de actividad", "nav.groupConfig": "Configuración", "nav.settings": "Ajustes del plugin", "nav.help": "Cómo funciona", "nav.asf": "Volver a ASF", "nav.stats": "Estadísticas",
		"stat.unlocked": "Desbloqueados", "stat.simulated": "Simulados", "stat.inProgress": "En curso", "stat.completed": "Terminados", "stat.skipped": "Omitidos", "stat.today": "Hoy", "stat.next": "Siguiente", "stat.mode": "Modo", "stat.sim": "Simulación", "stat.real": "Real",
		"info.today": "Desbloqueados hoy", "info.games": "Juegos en curso", "info.next": "Siguiente logro",
		"login.title": "Iniciar sesión", "login.text": "Introduce la contraseña IPC de ASF. Si ya iniciaste sesión en el panel de ASF, esta página la usa.", "login.button": "Entrar", "login.wrong": "Contraseña incorrecta.", "login.banned": "Demasiados intentos fallidos; espera un poco.",
		"status.trimmed": "Se está ejecutando una compilación recortada de ASF. Para plugins se recomienda la versión 'generic'; el ajuste de zona horaria puede no funcionar aquí.",
		"status.summary": "Resumen", "status.mode": "Modo", "status.plugin": "Plugin", "status.hours": "Horas activas", "status.today": "Hoy", "status.auto": "Juego automático",
		"status.playing": "Juegos en ejecución", "status.noPlaying": "ASF no está jugando nada ahora (o se pausó con !play / !pause).", "status.games": "Juegos en curso", "status.noGames": "Aún no hay planes. Los juegos aptos aparecerán aquí cuando se jueguen.", "status.completed": "Completados", "status.noCompleted": "Aún no hay juegos completados.",
		"fact.sim": "Simulación — no se envía nada a Steam", "fact.real": "Real — los logros se desbloquean en Steam", "fact.on": "Activado", "fact.off": "Desactivado", "fact.inWindow": "ahora dentro de las horas activas", "fact.outWindow": "ahora fuera de las horas activas", "fact.today": "{0} / {1} logros", "fact.autoActive": "Activado — jugando ahora", "fact.autoWaiting": "Activado — empieza cuando ASF está libre",
		"play.slot": "avanzando", "play.waiting": "en espera", "play.finished": "nada por desbloquear",
		"game.next": "Siguiente", "game.end": "Fin", "game.view": "Ver logros", "game.replan": "Reiniciar plan", "game.replanAsk": "¿Borrar el plan de este juego y rehacerlo mientras se juega?", "game.protected": "protegidos", "game.before": "ya desbloqueados", "game.rare": "demasiado raros", "game.paused": "en pausa (no se juega)", "game.nothing": "Nada por desbloquear", "game.simulated": "simulados", "game.completedAt": "Completado",
		"ach.unlocked": "Desbloqueado", "ach.done": "Desbloqueado por el plugin", "ach.simulated": "Simulado", "ach.planned": "Planificado", "ach.protected": "Protegido (solo lo desbloquean los servidores del juego)", "ach.skipped": "Omitido", "ach.rare": "No planificado (demasiado raro)", "ach.none": "Sin plan", "ach.hidden": "oculto", "ach.loading": "Leyendo de Steam…", "ach.failed": "No se pudo leer de Steam.",
		"set.run": "Funcionamiento", "set.mode": "Modo", "set.sim": "Simulación", "set.real": "Real", "set.modeHelp": "La simulación no envía nada a Steam: solo registra qué desbloquearía y cuándo. El modo real desbloquea los logros en Steam.", "set.enabled": "Plugin activado para este bot",
		"set.language": "Idioma de Discord y comandos", "set.languageHelp": "Mensajes de Discord, respuestas a comandos de ASF y registro (turco o inglés).",
		"set.pace": "Ritmo", "set.minDays": "Un juego tarda al menos (días)", "set.maxDays": "Como máximo (días)", "set.daysHelp": "Los logros de cada juego se reparten en una duración aleatoria dentro de este rango.", "set.from": "Horas activas desde", "set.to": "Hasta", "set.hoursHelp": "Los logros solo se desbloquean en este horario. Puede pasar de medianoche (p. ej. 10:00–01:00). Horas iguales significan todo el día.", "set.timeZone": "Zona horaria", "set.timeZoneHelp": "Déjalo vacío para usar la hora del servidor.",
		"set.maxActive": "Juegos a la vez", "set.maxPerDay": "Máx. logros por día", "set.minGap": "Intervalo mínimo (min)", "set.minPlaytime": "Tiempo de juego para empezar (h)", "set.skipRarer": "Omitir más raros que (%)", "set.paceHelp": "El tiempo de juego es el total de Steam. Un límite de rareza 0 está desactivado; por ejemplo, 1 nunca desbloquea logros que tiene menos del 1 % de jugadores.",
		"set.games": "Qué juegos", "set.vac": "Omitir juegos con VAC (la lista de permitidos no puede anularlo)", "set.online": "Omitir juegos en línea / multijugador", "set.dlc": "Omitir juegos con DLC de logros que no tengo", "set.asfBlacklist": "Respetar también la lista negra de ASF", "set.blacklist": "Lista negra (AppID)", "set.blacklistHelp": "Separados por comas. Estos juegos nunca se tocan.", "set.allowlist": "Lista de permitidos (AppID)", "set.allowlistHelp": "Salta las comprobaciones de juego en línea, DLC y desarrollador; nunca la de VAC.", "set.developers": "Desarrolladores bloqueados", "set.publishers": "Distribuidores bloqueados", "set.namesHelp": "Separados por comas, como aparecen en Steam (p. ej. Valve).",
		"set.auto": "Juego automático", "set.autoPlay": "Cuando ASF esté libre, elegir y jugar juegos aptos de mi biblioteca", "set.autoHelp": "Si no hay cromos que conseguir y estamos en horas activas, juega juegos aptos con logros pendientes. Tu lista GamesPlayedWhileIdle sigue jugándose. Cuando aparecen cromos, ASF retoma el control.", "set.rotate": "Dejar que ASF busque cromos cada … horas",
		"set.webhook": "URL del webhook", "set.nCompleted": "Avisar cuando un juego termine", "set.nEach": "Avisar en cada logro", "set.nSummary": "Enviar un resumen diario", "set.summaryHour": "Hora del resumen (0–23)", "set.nProblems": "Avisar de problemas", "set.test": "Enviar mensaje de prueba", "set.testOk": "Enviado, revisa Discord.", "set.testFail": "No se pudo enviar. Guarda primero la URL y compruébala.", "set.save": "Guardar", "set.saved": "Guardado.", "set.badHours": "Elige las horas activas.",
		"real.title": "¿Cambiar al modo real?", "real.text": "Los logros se desbloquearán de verdad en Steam y no se puede deshacer. Desbloquear logros sin jugar va contra las normas de Steam; el riesgo es tuyo. El progreso simulado se descarta y los planes se rehacen desde el estado real de tu cuenta.", "real.cancel": "Cancelar", "real.ok": "Sí, cambiar al modo real",
		"hist.Unlocked": "Desbloqueado", "hist.Simulated": "Simulado", "hist.Failed": "Fallido", "hist.Skipped": "Omitido", "hist.Completed": "Juego completado", "hist.empty": "Nada todavía.",
		"time.now": "ahora", "time.in": "en {0}", "time.ago": "hace {0}", "time.min": "{0} min", "time.hour": "{0} h", "time.day": "{0} d",
		"err.network": "No se pudo contactar con el servidor.", "err.server": "Error: {0}",
		"help.html": `<h2>Cómo funciona</h2><p>SteadyAchievements desbloquea logros de forma lenta y natural en los juegos que ASF <strong>está jugando</strong>: al conseguir cromos, en tu lista <code>GamesPlayedWhileIdle</code> o, si lo activas, en juegos que elige él mismo.</p><ul><li><strong>Plan:</strong> los logros pendientes se reparten en tu rango de días; los comunes suelen ir primero y los raros al final, con orden e intervalos aleatorios.</li><li><strong>Niveles:</strong> logros escalonados como «30 partidas» y «100 partidas» siempre se desbloquean de menor a mayor.</li><li><strong>Tiempo:</strong> solo avanza mientras se juega y dentro de las horas activas.</li><li><strong>Seguridad:</strong> por defecto se omiten juegos con VAC, en línea y con DLC de logros que no tienes; los logros protegidos nunca se tocan.</li><li><strong>Simulación:</strong> el plugin empieza en simulación; pasar al modo real es decisión tuya.</li></ul><h2>Comandos de ASF</h2><p><code>steady</code> (o <code>sa</code>) seguido de <code>status</code>, <code>on</code>, <code>off</code>, <code>sim</code>, <code>real</code>, <code>auto on</code>, <code>auto off</code>, <code>plan &lt;appid&gt;</code>, <code>replan &lt;appid&gt;</code>.</p><h2>Aviso</h2><p>Desbloquear logros sin jugar va contra las normas de Steam. Usas el plugin bajo tu responsabilidad.</p>`
	},

	"fr-FR": {
		"top.menu": "Menu", "top.language": "Langue", "top.appearance": "Apparence", "top.theme": "Couleur du thème", "top.dark": "Mode sombre", "top.logout": "Déconnexion",
		"nav.groupAchievements": "Succès", "nav.overview": "Vue d'ensemble", "nav.games": "Jeux", "nav.history": "Journal d'activité", "nav.groupConfig": "Configuration", "nav.settings": "Réglages du plugin", "nav.help": "Fonctionnement", "nav.asf": "Retour à ASF", "nav.stats": "Statistiques",
		"stat.unlocked": "Débloqués", "stat.simulated": "Simulés", "stat.inProgress": "En cours", "stat.completed": "Terminés", "stat.skipped": "Ignorés", "stat.today": "Aujourd'hui", "stat.next": "Suivant", "stat.mode": "Mode", "stat.sim": "Simulation", "stat.real": "Réel",
		"info.today": "Débloqués aujourd'hui", "info.games": "Jeux en cours", "info.next": "Prochain succès",
		"login.title": "Connexion", "login.text": "Saisis le mot de passe IPC d'ASF. Si tu es connecté au panneau d'ASF, cette page l'utilise.", "login.button": "Se connecter", "login.wrong": "Mot de passe incorrect.", "login.banned": "Trop d'essais ratés ; patiente un peu.",
		"status.trimmed": "Une version réduite (trimmed) d'ASF est utilisée. La version 'generic' est recommandée pour les plugins ; le réglage du fuseau horaire peut ne pas fonctionner ici.",
		"status.summary": "Résumé", "status.mode": "Mode", "status.plugin": "Plugin", "status.hours": "Heures actives", "status.today": "Aujourd'hui", "status.auto": "Jeu automatique",
		"status.playing": "Jeux en cours d'exécution", "status.noPlaying": "ASF ne joue à rien pour le moment (ou a été mis en pause avec !play / !pause).", "status.games": "Jeux en cours", "status.noGames": "Pas encore de plan. Les jeux éligibles apparaissent ici dès qu'ils sont joués.", "status.completed": "Terminés", "status.noCompleted": "Aucun jeu terminé pour l'instant.",
		"fact.sim": "Simulation — rien n'est envoyé à Steam", "fact.real": "Réel — les succès sont débloqués sur Steam", "fact.on": "Activé", "fact.off": "Désactivé", "fact.inWindow": "dans les heures actives", "fact.outWindow": "hors des heures actives", "fact.today": "{0} / {1} succès", "fact.autoActive": "Activé — joue en ce moment", "fact.autoWaiting": "Activé — démarre quand ASF est inactif",
		"play.slot": "en progression", "play.waiting": "en attente", "play.finished": "plus rien à débloquer",
		"game.next": "Suivant", "game.end": "Fin", "game.view": "Voir les succès", "game.replan": "Réinitialiser le plan", "game.replanAsk": "Supprimer le plan de ce jeu et le refaire pendant qu'il est joué ?", "game.protected": "protégés", "game.before": "déjà débloqués", "game.rare": "trop rares", "game.paused": "en pause (non joué)", "game.nothing": "Plus rien à débloquer", "game.simulated": "simulés", "game.completedAt": "Terminé",
		"ach.unlocked": "Débloqué", "ach.done": "Débloqué par le plugin", "ach.simulated": "Simulé", "ach.planned": "Planifié", "ach.protected": "Protégé (seuls les serveurs du jeu le débloquent)", "ach.skipped": "Ignoré", "ach.rare": "Non planifié (trop rare)", "ach.none": "Pas de plan", "ach.hidden": "caché", "ach.loading": "Lecture depuis Steam…", "ach.failed": "Lecture depuis Steam impossible.",
		"set.run": "Fonctionnement", "set.mode": "Mode", "set.sim": "Simulation", "set.real": "Réel", "set.modeHelp": "La simulation n'envoie rien à Steam et note seulement ce qui serait débloqué et quand. Le mode réel débloque les succès sur Steam.", "set.enabled": "Plugin activé pour ce bot",
		"set.language": "Langue de Discord et des commandes", "set.languageHelp": "Messages Discord, réponses aux commandes ASF et journal (turc ou anglais).",
		"set.pace": "Rythme", "set.minDays": "Un jeu dure au moins (jours)", "set.maxDays": "Au plus (jours)", "set.daysHelp": "Les succès de chaque jeu sont répartis sur une durée aléatoire dans cet intervalle.", "set.from": "Heures actives de", "set.to": "À", "set.hoursHelp": "Les succès ne se débloquent que dans cette plage. Elle peut passer minuit (ex. 10:00–01:00). Des heures égales signifient toute la journée.", "set.timeZone": "Fuseau horaire", "set.timeZoneHelp": "Laisser vide pour utiliser l'heure du serveur.",
		"set.maxActive": "Jeux en même temps", "set.maxPerDay": "Succès max. par jour", "set.minGap": "Écart minimum (min)", "set.minPlaytime": "Temps de jeu requis pour commencer (h)", "set.skipRarer": "Ignorer plus rares que (%)", "set.paceHelp": "Le temps de jeu est le total affiché par Steam. Une limite de rareté de 0 est désactivée ; par exemple 1 ne débloque jamais les succès possédés par moins de 1 % des joueurs.",
		"set.games": "Quels jeux", "set.vac": "Ignorer les jeux protégés par VAC (la liste autorisée ne peut pas l'outrepasser)", "set.online": "Ignorer les jeux en ligne / multijoueurs", "set.dlc": "Ignorer les jeux avec un DLC à succès que je ne possède pas", "set.asfBlacklist": "Respecter aussi la liste noire d'ASF", "set.blacklist": "Liste noire (AppID)", "set.blacklistHelp": "Séparés par des virgules. Ces jeux ne sont jamais touchés.", "set.allowlist": "Liste autorisée (AppID)", "set.allowlistHelp": "Contourne les contrôles en ligne, DLC et développeur ; jamais le contrôle VAC.", "set.developers": "Développeurs bloqués", "set.publishers": "Éditeurs bloqués", "set.namesHelp": "Séparés par des virgules, tels qu'écrits sur Steam (ex. Valve).",
		"set.auto": "Jeu automatique", "set.autoPlay": "Quand ASF est inactif, choisir et lancer des jeux éligibles de ma bibliothèque", "set.autoHelp": "S'il n'y a pas de cartes à farmer et que l'on est dans les heures actives, il lance des jeux éligibles avec des succès restants. Ta liste GamesPlayedWhileIdle continue de tourner. Quand des cartes apparaissent, ASF reprend la main.", "set.rotate": "Laisser ASF chercher des cartes toutes les … heures",
		"set.webhook": "URL du webhook", "set.nCompleted": "Prévenir quand un jeu est terminé", "set.nEach": "Prévenir à chaque succès", "set.nSummary": "Envoyer un résumé quotidien", "set.summaryHour": "Heure du résumé (0–23)", "set.nProblems": "Prévenir en cas de problème", "set.test": "Envoyer un message de test", "set.testOk": "Envoyé, regarde Discord.", "set.testFail": "Envoi impossible. Enregistre d'abord l'URL et vérifie-la.", "set.save": "Enregistrer", "set.saved": "Enregistré.", "set.badHours": "Choisis les heures actives.",
		"real.title": "Passer en mode réel ?", "real.text": "Les succès seront réellement débloqués sur Steam, sans retour possible. Débloquer des succès sans jouer est contraire aux règles de Steam ; le risque est le tien. La progression simulée est effacée et les plans sont refaits à partir de l'état réel de ton compte.", "real.cancel": "Annuler", "real.ok": "Oui, passer en mode réel",
		"hist.Unlocked": "Débloqué", "hist.Simulated": "Simulé", "hist.Failed": "Échec", "hist.Skipped": "Ignoré", "hist.Completed": "Jeu terminé", "hist.empty": "Rien pour l'instant.",
		"time.now": "maintenant", "time.in": "dans {0}", "time.ago": "il y a {0}", "time.min": "{0} min", "time.hour": "{0} h", "time.day": "{0} j",
		"err.network": "Serveur injoignable.", "err.server": "Erreur : {0}",
		"help.html": `<h2>Fonctionnement</h2><p>SteadyAchievements débloque lentement et naturellement les succès des jeux qu'ASF <strong>est en train de jouer</strong> : farm de cartes, ta liste <code>GamesPlayedWhileIdle</code> ou, si activé, des jeux qu'il choisit lui-même.</p><ul><li><strong>Plan :</strong> les succès restants sont répartis sur ta plage de jours ; les courants plutôt d'abord, les rares à la fin, ordre et écarts aléatoires.</li><li><strong>Paliers :</strong> les succès à paliers comme « 30 parties » et « 100 parties » se débloquent toujours du plus petit au plus grand.</li><li><strong>Temps :</strong> la progression n'a lieu que pendant le jeu et dans les heures actives.</li><li><strong>Sécurité :</strong> les jeux VAC, en ligne et avec un DLC à succès non possédé sont ignorés par défaut ; les succès protégés ne sont jamais touchés.</li><li><strong>Simulation :</strong> le plugin démarre en simulation ; passer en mode réel est ta décision.</li></ul><h2>Commandes ASF</h2><p><code>steady</code> (ou <code>sa</code>) suivi de <code>status</code>, <code>on</code>, <code>off</code>, <code>sim</code>, <code>real</code>, <code>auto on</code>, <code>auto off</code>, <code>plan &lt;appid&gt;</code>, <code>replan &lt;appid&gt;</code>.</p><h2>Avertissement</h2><p>Débloquer des succès sans jouer est contraire aux règles de Steam. Tu utilises ce plugin à tes risques.</p>`
	},

	"pl-PL": {
		"top.menu": "Menu", "top.language": "Język", "top.appearance": "Wygląd", "top.theme": "Kolor motywu", "top.dark": "Tryb ciemny", "top.logout": "Wyloguj",
		"nav.groupAchievements": "Osiągnięcia", "nav.overview": "Przegląd", "nav.games": "Gry", "nav.history": "Historia aktywności", "nav.groupConfig": "Konfiguracja", "nav.settings": "Ustawienia wtyczki", "nav.help": "Jak to działa", "nav.asf": "Wróć do ASF", "nav.stats": "Statystyki",
		"stat.unlocked": "Odblokowane", "stat.simulated": "Symulowane", "stat.inProgress": "W toku", "stat.completed": "Ukończone", "stat.skipped": "Pominięte", "stat.today": "Dziś", "stat.next": "Następne", "stat.mode": "Tryb", "stat.sim": "Symulacja", "stat.real": "Prawdziwy",
		"info.today": "Odblokowane dziś", "info.games": "Gry w toku", "info.next": "Następne osiągnięcie",
		"login.title": "Logowanie", "login.text": "Wpisz hasło IPC ASF. Jeśli jesteś zalogowany w panelu ASF, ta strona użyje tego hasła.", "login.button": "Zaloguj", "login.wrong": "Złe hasło.", "login.banned": "Zbyt wiele nieudanych prób; odczekaj chwilę.",
		"status.trimmed": "Działa przycięta (trimmed) wersja ASF. Do wtyczek zalecana jest wersja 'generic'; ustawienie strefy czasowej może tu nie działać.",
		"status.summary": "Podsumowanie", "status.mode": "Tryb", "status.plugin": "Wtyczka", "status.hours": "Aktywne godziny", "status.today": "Dziś", "status.auto": "Automatyczne granie",
		"status.playing": "Uruchomione gry", "status.noPlaying": "ASF teraz w nic nie gra (albo wstrzymano go przez !play / !pause).", "status.games": "Gry w toku", "status.noGames": "Brak planów. Kwalifikujące się gry pojawią się tu, gdy będą uruchomione.", "status.completed": "Ukończone", "status.noCompleted": "Brak ukończonych gier.",
		"fact.sim": "Symulacja — nic nie jest wysyłane do Steam", "fact.real": "Prawdziwy — osiągnięcia są odblokowywane na Steam", "fact.on": "Włączona", "fact.off": "Wyłączona", "fact.inWindow": "teraz w aktywnych godzinach", "fact.outWindow": "teraz poza aktywnymi godzinami", "fact.today": "{0} / {1} osiągnięć", "fact.autoActive": "Włączone — teraz gra", "fact.autoWaiting": "Włączone — rusza, gdy ASF jest bezczynny",
		"play.slot": "postępuje", "play.waiting": "czeka", "play.finished": "nic do odblokowania",
		"game.next": "Następne", "game.end": "Koniec", "game.view": "Zobacz osiągnięcia", "game.replan": "Resetuj plan", "game.replanAsk": "Usunąć plan tej gry i utworzyć go od nowa podczas grania?", "game.protected": "chronione", "game.before": "już odblokowane", "game.rare": "zbyt rzadkie", "game.paused": "wstrzymana (nieuruchomiona)", "game.nothing": "Nic do odblokowania", "game.simulated": "symulowane", "game.completedAt": "Ukończono",
		"ach.unlocked": "Odblokowane", "ach.done": "Odblokowane przez wtyczkę", "ach.simulated": "Symulowane", "ach.planned": "Zaplanowane", "ach.protected": "Chronione (odblokowują je tylko serwery gry)", "ach.skipped": "Pominięte", "ach.rare": "Poza planem (zbyt rzadkie)", "ach.none": "Brak planu", "ach.hidden": "ukryte", "ach.loading": "Odczyt ze Steam…", "ach.failed": "Nie udało się odczytać ze Steam.",
		"set.run": "Działanie", "set.mode": "Tryb", "set.sim": "Symulacja", "set.real": "Prawdziwy", "set.modeHelp": "Symulacja nic nie wysyła do Steam, tylko zapisuje, co i kiedy by odblokowała. Tryb prawdziwy odblokowuje osiągnięcia na Steam.", "set.enabled": "Wtyczka włączona dla tego bota",
		"set.language": "Język Discorda i komend", "set.languageHelp": "Wiadomości na Discordzie, odpowiedzi na komendy ASF i dziennik (turecki lub angielski).",
		"set.pace": "Tempo", "set.minDays": "Gra trwa co najmniej (dni)", "set.maxDays": "Najwyżej (dni)", "set.daysHelp": "Osiągnięcia każdej gry są rozłożone na losowy czas w tym zakresie.", "set.from": "Aktywne godziny od", "set.to": "Do", "set.hoursHelp": "Osiągnięcia odblokowują się tylko w tych godzinach. Zakres może przechodzić przez północ (np. 10:00–01:00). Równe godziny oznaczają cały dzień.", "set.timeZone": "Strefa czasowa", "set.timeZoneHelp": "Pozostaw puste, aby użyć czasu serwera.",
		"set.maxActive": "Gier naraz", "set.maxPerDay": "Maks. osiągnięć dziennie", "set.minGap": "Minimalny odstęp (min)", "set.minPlaytime": "Czas gry potrzebny do startu (h)", "set.skipRarer": "Pomiń rzadsze niż (%)", "set.paceHelp": "Czas gry to suma pokazywana przez Steam. Limit rzadkości 0 jest wyłączony; np. 1 nigdy nie odblokuje osiągnięć, które ma mniej niż 1% graczy.",
		"set.games": "Które gry", "set.vac": "Pomiń gry z VAC (lista dozwolonych tego nie zmienia)", "set.online": "Pomiń gry online / wieloosobowe", "set.dlc": "Pomiń gry z DLC z osiągnięciami, którego nie mam", "set.asfBlacklist": "Uwzględniaj też czarną listę ASF", "set.blacklist": "Czarna lista (AppID)", "set.blacklistHelp": "Oddzielone przecinkami. Te gry nigdy nie są ruszane.", "set.allowlist": "Lista dozwolonych (AppID)", "set.allowlistHelp": "Omija sprawdzanie online, DLC i dewelopera; nigdy sprawdzania VAC.", "set.developers": "Zablokowani deweloperzy", "set.publishers": "Zablokowani wydawcy", "set.namesHelp": "Oddzielone przecinkami, tak jak na Steam (np. Valve).",
		"set.auto": "Automatyczne granie", "set.autoPlay": "Gdy ASF jest bezczynny, wybieraj i uruchamiaj gry z mojej biblioteki", "set.autoHelp": "Gdy nie ma kart do zbierania i trwają aktywne godziny, uruchamia kwalifikujące się gry z pozostałymi osiągnięciami. Twoja lista GamesPlayedWhileIdle nadal działa. Gdy pojawią się karty, ASF przejmuje kontrolę.", "set.rotate": "Pozwól ASF szukać kart co … godz.",
		"set.webhook": "Adres webhooka", "set.nCompleted": "Powiadom po ukończeniu gry", "set.nEach": "Powiadom przy każdym osiągnięciu", "set.nSummary": "Wysyłaj dzienne podsumowanie", "set.summaryHour": "Godzina podsumowania (0–23)", "set.nProblems": "Powiadom o problemach", "set.test": "Wyślij wiadomość testową", "set.testOk": "Wysłano, sprawdź Discorda.", "set.testFail": "Nie udało się wysłać. Najpierw zapisz adres i go sprawdź.", "set.save": "Zapisz", "set.saved": "Zapisano.", "set.badHours": "Wybierz aktywne godziny.",
		"real.title": "Przełączyć na tryb prawdziwy?", "real.text": "Osiągnięcia zostaną naprawdę odblokowane na Steam i nie da się tego cofnąć. Odblokowywanie osiągnięć bez grania jest niezgodne z zasadami Steam; ryzyko jest po twojej stronie. Symulowany postęp zostanie usunięty, a plany utworzone od nowa z prawdziwego stanu konta.", "real.cancel": "Anuluj", "real.ok": "Tak, przełącz",
		"hist.Unlocked": "Odblokowano", "hist.Simulated": "Symulacja", "hist.Failed": "Błąd", "hist.Skipped": "Pominięto", "hist.Completed": "Gra ukończona", "hist.empty": "Jeszcze nic.",
		"time.now": "teraz", "time.in": "za {0}", "time.ago": "{0} temu", "time.min": "{0} min", "time.hour": "{0} h", "time.day": "{0} d",
		"err.network": "Brak połączenia z serwerem.", "err.server": "Błąd: {0}",
		"help.html": `<h2>Jak to działa</h2><p>SteadyAchievements powoli i naturalnie odblokowuje osiągnięcia w grach, które ASF <strong>właśnie uruchamia</strong>: przy zbieraniu kart, z listy <code>GamesPlayedWhileIdle</code> lub — jeśli włączone — w grach wybranych samodzielnie.</p><ul><li><strong>Plan:</strong> pozostałe osiągnięcia są rozkładane na twój zakres dni; częste zwykle najpierw, rzadkie na końcu, kolejność i odstępy losowe.</li><li><strong>Poziomy:</strong> stopniowane osiągnięcia jak „30 meczów” i „100 meczów” zawsze odblokowują się od najmniejszego.</li><li><strong>Czas:</strong> postęp tylko podczas gry i w aktywnych godzinach.</li><li><strong>Bezpieczeństwo:</strong> domyślnie pomijane są gry z VAC, online i z nieposiadanym DLC z osiągnięciami; chronione osiągnięcia nigdy nie są ruszane.</li><li><strong>Symulacja:</strong> wtyczka startuje w symulacji; przejście na tryb prawdziwy to twoja decyzja.</li></ul><h2>Komendy ASF</h2><p><code>steady</code> (lub <code>sa</code>) oraz <code>status</code>, <code>on</code>, <code>off</code>, <code>sim</code>, <code>real</code>, <code>auto on</code>, <code>auto off</code>, <code>plan &lt;appid&gt;</code>, <code>replan &lt;appid&gt;</code>.</p><h2>Ostrzeżenie</h2><p>Odblokowywanie osiągnięć bez grania jest niezgodne z zasadami Steam. Używasz wtyczki na własną odpowiedzialność.</p>`
	},

	"pt-BR": {
		"top.menu": "Menu", "top.language": "Idioma", "top.appearance": "Aparência", "top.theme": "Cor do tema", "top.dark": "Modo escuro", "top.logout": "Sair",
		"nav.groupAchievements": "Conquistas", "nav.overview": "Visão geral", "nav.games": "Jogos", "nav.history": "Histórico de atividade", "nav.groupConfig": "Configuração", "nav.settings": "Configurações do plugin", "nav.help": "Como funciona", "nav.asf": "Voltar ao ASF", "nav.stats": "Estatísticas",
		"stat.unlocked": "Desbloqueadas", "stat.simulated": "Simuladas", "stat.inProgress": "Em andamento", "stat.completed": "Concluídos", "stat.skipped": "Ignoradas", "stat.today": "Hoje", "stat.next": "Próxima", "stat.mode": "Modo", "stat.sim": "Simulação", "stat.real": "Real",
		"info.today": "Desbloqueadas hoje", "info.games": "Jogos em andamento", "info.next": "Próxima conquista",
		"login.title": "Entrar", "login.text": "Digite a senha IPC do ASF. Se você já entrou no painel do ASF, esta página usa essa senha.", "login.button": "Entrar", "login.wrong": "Senha incorreta.", "login.banned": "Muitas tentativas erradas; aguarde um pouco.",
		"status.trimmed": "Uma versão reduzida (trimmed) do ASF está em uso. Para plugins recomenda-se a versão 'generic'; o fuso horário pode não funcionar aqui.",
		"status.summary": "Resumo", "status.mode": "Modo", "status.plugin": "Plugin", "status.hours": "Horas ativas", "status.today": "Hoje", "status.auto": "Jogo automático",
		"status.playing": "Jogos em execução", "status.noPlaying": "O ASF não está jogando nada agora (ou foi pausado com !play / !pause).", "status.games": "Jogos em andamento", "status.noGames": "Ainda não há planos. Jogos elegíveis aparecem aqui quando forem jogados.", "status.completed": "Concluídos", "status.noCompleted": "Nenhum jogo concluído ainda.",
		"fact.sim": "Simulação — nada é enviado à Steam", "fact.real": "Real — conquistas são desbloqueadas na Steam", "fact.on": "Ligado", "fact.off": "Desligado", "fact.inWindow": "agora dentro das horas ativas", "fact.outWindow": "agora fora das horas ativas", "fact.today": "{0} / {1} conquistas", "fact.autoActive": "Ligado — jogando agora", "fact.autoWaiting": "Ligado — começa quando o ASF estiver ocioso",
		"play.slot": "avançando", "play.waiting": "na fila", "play.finished": "nada a desbloquear",
		"game.next": "Próxima", "game.end": "Fim", "game.view": "Ver conquistas", "game.replan": "Refazer plano", "game.replanAsk": "Apagar o plano deste jogo e refazê-lo enquanto ele é jogado?", "game.protected": "protegidas", "game.before": "já desbloqueadas", "game.rare": "raras demais", "game.paused": "pausado (não está sendo jogado)", "game.nothing": "Nada a desbloquear", "game.simulated": "simuladas", "game.completedAt": "Concluído",
		"ach.unlocked": "Desbloqueada", "ach.done": "Desbloqueada pelo plugin", "ach.simulated": "Simulada", "ach.planned": "Planejada", "ach.protected": "Protegida (só os servidores do jogo desbloqueiam)", "ach.skipped": "Ignorada", "ach.rare": "Fora do plano (rara demais)", "ach.none": "Sem plano", "ach.hidden": "oculta", "ach.loading": "Lendo da Steam…", "ach.failed": "Não foi possível ler da Steam.",
		"set.run": "Funcionamento", "set.mode": "Modo", "set.sim": "Simulação", "set.real": "Real", "set.modeHelp": "A simulação não envia nada à Steam e só registra o que desbloquearia e quando. O modo real desbloqueia as conquistas na Steam.", "set.enabled": "Plugin ativado para este bot",
		"set.language": "Idioma do Discord e dos comandos", "set.languageHelp": "Mensagens do Discord, respostas de comandos do ASF e registro (turco ou inglês).",
		"set.pace": "Ritmo", "set.minDays": "Um jogo leva no mínimo (dias)", "set.maxDays": "No máximo (dias)", "set.daysHelp": "As conquistas de cada jogo são distribuídas em uma duração aleatória dentro deste intervalo.", "set.from": "Horas ativas de", "set.to": "Até", "set.hoursHelp": "As conquistas só são desbloqueadas neste horário. Pode passar da meia-noite (ex.: 10:00–01:00). Horas iguais significam o dia todo.", "set.timeZone": "Fuso horário", "set.timeZoneHelp": "Deixe vazio para usar a hora do servidor.",
		"set.maxActive": "Jogos ao mesmo tempo", "set.maxPerDay": "Máx. de conquistas por dia", "set.minGap": "Intervalo mínimo (min)", "set.minPlaytime": "Tempo de jogo para começar (h)", "set.skipRarer": "Ignorar mais raras que (%)", "set.paceHelp": "O tempo de jogo é o total mostrado pela Steam. Limite de raridade 0 é desligado; por exemplo, 1 nunca desbloqueia conquistas que menos de 1% dos jogadores têm.",
		"set.games": "Quais jogos", "set.vac": "Ignorar jogos com VAC (a lista de permitidos não muda isso)", "set.online": "Ignorar jogos online / multijogador", "set.dlc": "Ignorar jogos com DLC de conquistas que não tenho", "set.asfBlacklist": "Respeitar também a lista negra do ASF", "set.blacklist": "Lista negra (AppIDs)", "set.blacklistHelp": "Separados por vírgula. Estes jogos nunca são tocados.", "set.allowlist": "Lista de permitidos (AppIDs)", "set.allowlistHelp": "Ignora as verificações de jogo online, DLC e desenvolvedor; nunca a do VAC.", "set.developers": "Desenvolvedores bloqueados", "set.publishers": "Distribuidoras bloqueadas", "set.namesHelp": "Separados por vírgula, como escrito na Steam (ex.: Valve).",
		"set.auto": "Jogo automático", "set.autoPlay": "Quando o ASF estiver ocioso, escolher e jogar jogos elegíveis da minha biblioteca", "set.autoHelp": "Sem cartas para coletar e dentro das horas ativas, ele joga jogos elegíveis com conquistas restantes. Sua lista GamesPlayedWhileIdle continua rodando. Quando aparecem cartas, o ASF retoma o controle.", "set.rotate": "Deixar o ASF procurar cartas a cada … horas",
		"set.webhook": "URL do webhook", "set.nCompleted": "Avisar quando um jogo for concluído", "set.nEach": "Avisar a cada conquista", "set.nSummary": "Enviar resumo diário", "set.summaryHour": "Hora do resumo (0–23)", "set.nProblems": "Avisar sobre problemas", "set.test": "Enviar mensagem de teste", "set.testOk": "Enviado, veja o Discord.", "set.testFail": "Falha ao enviar. Salve a URL primeiro e confira.", "set.save": "Salvar", "set.saved": "Salvo.", "set.badHours": "Escolha as horas ativas.",
		"real.title": "Mudar para o modo real?", "real.text": "As conquistas serão desbloqueadas de verdade na Steam e isso não pode ser desfeito. Desbloquear conquistas sem jogar vai contra as regras da Steam; o risco é seu. O progresso simulado é descartado e os planos são refeitos a partir do estado real da conta.", "real.cancel": "Cancelar", "real.ok": "Sim, mudar para o modo real",
		"hist.Unlocked": "Desbloqueada", "hist.Simulated": "Simulada", "hist.Failed": "Falhou", "hist.Skipped": "Ignorada", "hist.Completed": "Jogo concluído", "hist.empty": "Nada ainda.",
		"time.now": "agora", "time.in": "em {0}", "time.ago": "há {0}", "time.min": "{0} min", "time.hour": "{0} h", "time.day": "{0} d",
		"err.network": "Não foi possível contatar o servidor.", "err.server": "Erro: {0}",
		"help.html": `<h2>Como funciona</h2><p>O SteadyAchievements desbloqueia conquistas de forma lenta e natural nos jogos que o ASF <strong>está jogando</strong>: na coleta de cartas, na sua lista <code>GamesPlayedWhileIdle</code> ou, se ativado, em jogos que ele mesmo escolhe.</p><ul><li><strong>Plano:</strong> as conquistas restantes são distribuídas no seu intervalo de dias; as comuns tendem a vir primeiro e as raras por último, com ordem e intervalos aleatórios.</li><li><strong>Níveis:</strong> conquistas escalonadas como “30 partidas” e “100 partidas” sempre são desbloqueadas da menor para a maior.</li><li><strong>Tempo:</strong> só avança enquanto o jogo roda e dentro das horas ativas.</li><li><strong>Segurança:</strong> por padrão, jogos com VAC, online e com DLC de conquistas que você não tem são ignorados; conquistas protegidas nunca são tocadas.</li><li><strong>Simulação:</strong> o plugin começa em simulação; mudar para o modo real é decisão sua.</li></ul><h2>Comandos do ASF</h2><p><code>steady</code> (ou <code>sa</code>) seguido de <code>status</code>, <code>on</code>, <code>off</code>, <code>sim</code>, <code>real</code>, <code>auto on</code>, <code>auto off</code>, <code>plan &lt;appid&gt;</code>, <code>replan &lt;appid&gt;</code>.</p><h2>Aviso</h2><p>Desbloquear conquistas sem jogar vai contra as regras da Steam. O uso do plugin é de sua responsabilidade.</p>`
	},

	"ru-RU": {
		"top.menu": "Меню", "top.language": "Язык", "top.appearance": "Оформление", "top.theme": "Цвет темы", "top.dark": "Тёмная тема", "top.logout": "Выйти",
		"nav.groupAchievements": "Достижения", "nav.overview": "Обзор", "nav.games": "Игры", "nav.history": "Журнал событий", "nav.groupConfig": "Настройка", "nav.settings": "Настройки плагина", "nav.help": "Как это работает", "nav.asf": "Назад в ASF", "nav.stats": "Статистика",
		"stat.unlocked": "Открыто", "stat.simulated": "Симуляция", "stat.inProgress": "В процессе", "stat.completed": "Готово", "stat.skipped": "Пропущено", "stat.today": "Сегодня", "stat.next": "Следующее", "stat.mode": "Режим", "stat.sim": "Симуляция", "stat.real": "Реальный",
		"info.today": "Открыто сегодня", "info.games": "Игр в процессе", "info.next": "Следующее достижение",
		"login.title": "Вход", "login.text": "Введите IPC-пароль ASF. Если вы вошли в панель ASF, эта страница использует тот же пароль.", "login.button": "Войти", "login.wrong": "Неверный пароль.", "login.banned": "Слишком много неудачных попыток; подождите.",
		"status.trimmed": "Запущена урезанная (trimmed) сборка ASF. Для плагинов рекомендуется сборка 'generic'; настройка часового пояса здесь может не работать.",
		"status.summary": "Сводка", "status.mode": "Режим", "status.plugin": "Плагин", "status.hours": "Активные часы", "status.today": "Сегодня", "status.auto": "Автоигра",
		"status.playing": "Запущенные игры", "status.noPlaying": "ASF сейчас ничего не запускает (или приостановлен через !play / !pause).", "status.games": "Игры в процессе", "status.noGames": "Планов пока нет. Подходящие игры появятся здесь, когда будут запущены.", "status.completed": "Завершённые", "status.noCompleted": "Завершённых игр пока нет.",
		"fact.sim": "Симуляция — в Steam ничего не отправляется", "fact.real": "Реальный — достижения открываются в Steam", "fact.on": "Включён", "fact.off": "Выключен", "fact.inWindow": "сейчас активные часы", "fact.outWindow": "сейчас вне активных часов", "fact.today": "{0} / {1} достижений", "fact.autoActive": "Включена — сейчас играет", "fact.autoWaiting": "Включена — начнёт, когда ASF простаивает",
		"play.slot": "идёт", "play.waiting": "ожидает", "play.finished": "нечего открывать",
		"game.next": "Следующее", "game.end": "Конец", "game.view": "Достижения", "game.replan": "Сбросить план", "game.replanAsk": "Удалить план этой игры и составить заново во время игры?", "game.protected": "защищено", "game.before": "уже открыто", "game.rare": "слишком редкие", "game.paused": "на паузе (не запущена)", "game.nothing": "Нечего открывать", "game.simulated": "симуляция", "game.completedAt": "Завершено",
		"ach.unlocked": "Открыто", "ach.done": "Открыто плагином", "ach.simulated": "Симуляция", "ach.planned": "Запланировано", "ach.protected": "Защищено (открывают только серверы игры)", "ach.skipped": "Пропущено", "ach.rare": "Не запланировано (слишком редкое)", "ach.none": "Нет плана", "ach.hidden": "скрытое", "ach.loading": "Чтение из Steam…", "ach.failed": "Не удалось прочитать из Steam.",
		"set.run": "Работа", "set.mode": "Режим", "set.sim": "Симуляция", "set.real": "Реальный", "set.modeHelp": "Симуляция ничего не отправляет в Steam и только записывает, что и когда было бы открыто. Реальный режим открывает достижения в Steam.", "set.enabled": "Плагин включён для этого бота",
		"set.language": "Язык Discord и команд", "set.languageHelp": "Сообщения Discord, ответы на команды ASF и журнал (турецкий или английский).",
		"set.pace": "Темп", "set.minDays": "Игра длится минимум (дней)", "set.maxDays": "Максимум (дней)", "set.daysHelp": "Достижения каждой игры распределяются на случайный срок в этом диапазоне.", "set.from": "Активные часы с", "set.to": "До", "set.hoursHelp": "Достижения открываются только в эти часы. Интервал может переходить через полночь (напр. 10:00–01:00). Одинаковое время — весь день.", "set.timeZone": "Часовой пояс", "set.timeZoneHelp": "Оставьте пустым, чтобы использовать время сервера.",
		"set.maxActive": "Игр одновременно", "set.maxPerDay": "Макс. достижений в день", "set.minGap": "Мин. интервал (мин)", "set.minPlaytime": "Нужно наиграть для старта (ч)", "set.skipRarer": "Пропускать реже чем (%)", "set.paceHelp": "Время игры — общее время в Steam. Лимит редкости 0 выключен; например, 1 никогда не откроет достижения, которые есть менее чем у 1% игроков.",
		"set.games": "Какие игры", "set.vac": "Пропускать игры с VAC (список разрешённых этого не отменяет)", "set.online": "Пропускать онлайн / многопользовательские игры", "set.dlc": "Пропускать игры с DLC с достижениями, которого у меня нет", "set.asfBlacklist": "Учитывать чёрный список ASF", "set.blacklist": "Чёрный список (AppID)", "set.blacklistHelp": "Через запятую. Эти игры никогда не трогаются.", "set.allowlist": "Разрешённые (AppID)", "set.allowlistHelp": "Обходит проверки онлайна, DLC и разработчика, но не VAC.", "set.developers": "Заблокированные разработчики", "set.publishers": "Заблокированные издатели", "set.namesHelp": "Через запятую, как в Steam (напр. Valve).",
		"set.auto": "Автоигра", "set.autoPlay": "Когда ASF простаивает, выбирать и запускать подходящие игры из библиотеки", "set.autoHelp": "Если нет карточек для фарма и идут активные часы, запускает подходящие игры с оставшимися достижениями. Ваш список GamesPlayedWhileIdle продолжает работать. Когда появляются карточки, управление возвращается ASF.", "set.rotate": "Давать ASF проверить карточки каждые … ч",
		"set.webhook": "URL вебхука", "set.nCompleted": "Сообщать о завершении игры", "set.nEach": "Сообщать о каждом достижении", "set.nSummary": "Ежедневная сводка", "set.summaryHour": "Час сводки (0–23)", "set.nProblems": "Сообщать о проблемах", "set.test": "Отправить тестовое сообщение", "set.testOk": "Отправлено, проверьте Discord.", "set.testFail": "Не удалось отправить. Сначала сохраните URL и проверьте его.", "set.save": "Сохранить", "set.saved": "Сохранено.", "set.badHours": "Выберите активные часы.",
		"real.title": "Перейти в реальный режим?", "real.text": "Достижения будут действительно открыты в Steam, отменить это нельзя. Открытие достижений без игры нарушает правила Steam; риск на вас. Прогресс симуляции будет удалён, а планы составлены заново по реальному состоянию аккаунта.", "real.cancel": "Отмена", "real.ok": "Да, перейти",
		"hist.Unlocked": "Открыто", "hist.Simulated": "Симуляция", "hist.Failed": "Ошибка", "hist.Skipped": "Пропущено", "hist.Completed": "Игра завершена", "hist.empty": "Пока пусто.",
		"time.now": "сейчас", "time.in": "через {0}", "time.ago": "{0} назад", "time.min": "{0} мин", "time.hour": "{0} ч", "time.day": "{0} дн",
		"err.network": "Сервер недоступен.", "err.server": "Ошибка: {0}",
		"help.html": `<h2>Как это работает</h2><p>SteadyAchievements медленно и естественно открывает достижения в играх, которые ASF <strong>сейчас запускает</strong>: при фарме карточек, из списка <code>GamesPlayedWhileIdle</code> или, если включено, в играх, выбранных им самим.</p><ul><li><strong>План:</strong> оставшиеся достижения распределяются на ваш диапазон дней; частые обычно раньше, редкие позже, порядок и интервалы случайны.</li><li><strong>Уровни:</strong> ступенчатые достижения вроде «30 матчей» и «100 матчей» всегда открываются от меньшего к большему.</li><li><strong>Время:</strong> прогресс идёт только во время игры и в активные часы.</li><li><strong>Безопасность:</strong> по умолчанию пропускаются игры с VAC, онлайн-игры и игры с отсутствующим DLC с достижениями; защищённые достижения не трогаются.</li><li><strong>Симуляция:</strong> плагин стартует в симуляции; переход в реальный режим — ваше решение.</li></ul><h2>Команды ASF</h2><p><code>steady</code> (или <code>sa</code>) и затем <code>status</code>, <code>on</code>, <code>off</code>, <code>sim</code>, <code>real</code>, <code>auto on</code>, <code>auto off</code>, <code>plan &lt;appid&gt;</code>, <code>replan &lt;appid&gt;</code>.</p><h2>Предупреждение</h2><p>Открытие достижений без игры нарушает правила Steam. Вы используете плагин на свой страх и риск.</p>`
	},

	"zh-CN": {
		"top.menu": "菜单", "top.language": "语言", "top.appearance": "外观", "top.theme": "主题颜色", "top.dark": "深色模式", "top.logout": "退出",
		"nav.groupAchievements": "成就", "nav.overview": "概览", "nav.games": "游戏", "nav.history": "活动记录", "nav.groupConfig": "配置", "nav.settings": "插件设置", "nav.help": "工作原理", "nav.asf": "返回 ASF", "nav.stats": "统计",
		"stat.unlocked": "已解锁", "stat.simulated": "已模拟", "stat.inProgress": "进行中", "stat.completed": "已完成", "stat.skipped": "已跳过", "stat.today": "今天", "stat.next": "下一个", "stat.mode": "模式", "stat.sim": "模拟", "stat.real": "真实",
		"info.today": "今日解锁", "info.games": "进行中的游戏", "info.next": "下一个成就",
		"login.title": "登录", "login.text": "请输入 ASF 的 IPC 密码。如果已登录 ASF 面板，此页面会使用该密码。", "login.button": "登录", "login.wrong": "密码错误。", "login.banned": "错误次数过多，请稍后再试。",
		"status.trimmed": "正在运行精简版（trimmed）ASF。插件建议使用 'generic' 版本；此处时区设置可能无效。",
		"status.summary": "摘要", "status.mode": "模式", "status.plugin": "插件", "status.hours": "活跃时段", "status.today": "今天", "status.auto": "自动游玩",
		"status.playing": "正在运行的游戏", "status.noPlaying": "ASF 当前没有运行任何游戏（或已通过 !play / !pause 暂停）。", "status.games": "进行中的游戏", "status.noGames": "暂无计划。符合条件的游戏运行后会显示在这里。", "status.completed": "已完成", "status.noCompleted": "暂无已完成的游戏。",
		"fact.sim": "模拟 — 不会向 Steam 发送任何内容", "fact.real": "真实 — 成就会在 Steam 上解锁", "fact.on": "开启", "fact.off": "关闭", "fact.inWindow": "当前处于活跃时段", "fact.outWindow": "当前不在活跃时段", "fact.today": "{0} / {1} 个成就", "fact.autoActive": "开启 — 正在游玩", "fact.autoWaiting": "开启 — ASF 空闲时启动",
		"play.slot": "进行中", "play.waiting": "等待中", "play.finished": "无可解锁成就",
		"game.next": "下一个", "game.end": "完成", "game.view": "查看成就", "game.replan": "重置计划", "game.replanAsk": "删除此游戏的计划并在游玩时重新生成？", "game.protected": "受保护", "game.before": "已解锁", "game.rare": "过于稀有", "game.paused": "已暂停（未运行）", "game.nothing": "无可解锁成就", "game.simulated": "模拟", "game.completedAt": "完成于",
		"ach.unlocked": "已解锁", "ach.done": "由插件解锁", "ach.simulated": "已模拟", "ach.planned": "已计划", "ach.protected": "受保护（仅游戏服务器可解锁）", "ach.skipped": "已跳过", "ach.rare": "未计划（过于稀有）", "ach.none": "无计划", "ach.hidden": "隐藏", "ach.loading": "正在从 Steam 读取…", "ach.failed": "无法从 Steam 读取。",
		"set.run": "运行", "set.mode": "模式", "set.sim": "模拟", "set.real": "真实", "set.modeHelp": "模拟模式不会向 Steam 发送任何内容，只记录将在何时解锁什么。真实模式会在 Steam 上解锁成就。", "set.enabled": "为此机器人启用插件",
		"set.language": "Discord 与命令语言", "set.languageHelp": "Discord 消息、ASF 命令回复和日志使用此语言（土耳其语或英语）。",
		"set.pace": "节奏", "set.minDays": "每个游戏至少（天）", "set.maxDays": "最多（天）", "set.daysHelp": "每个游戏的成就会分布在此范围内随机选择的时长中。", "set.from": "活跃时段开始", "set.to": "结束", "set.hoursHelp": "成就只在此时段内解锁，可跨越午夜（如 10:00–01:00）。开始与结束相同表示全天。", "set.timeZone": "时区", "set.timeZoneHelp": "留空则使用服务器时间。",
		"set.maxActive": "同时进行的游戏", "set.maxPerDay": "每日最多成就", "set.minGap": "最小间隔（分钟）", "set.minPlaytime": "开始所需游戏时长（小时）", "set.skipRarer": "跳过稀有度低于（%）", "set.paceHelp": "游戏时长为 Steam 显示的总时长。稀有度限制为 0 表示关闭；例如填 1 将永不解锁少于 1% 玩家拥有的成就。",
		"set.games": "哪些游戏", "set.vac": "跳过受 VAC 保护的游戏（允许列表无法覆盖）", "set.online": "跳过在线 / 多人游戏", "set.dlc": "跳过含有我未拥有的带成就 DLC 的游戏", "set.asfBlacklist": "同时遵守 ASF 自身的黑名单", "set.blacklist": "黑名单（AppID）", "set.blacklistHelp": "用逗号分隔。这些游戏永远不会被处理。", "set.allowlist": "允许列表（AppID）", "set.allowlistHelp": "跳过在线、DLC 和开发商检查；不会跳过 VAC 检查。", "set.developers": "屏蔽的开发商", "set.publishers": "屏蔽的发行商", "set.namesHelp": "用逗号分隔，与 Steam 上的写法一致（如 Valve）。",
		"set.auto": "自动游玩", "set.autoPlay": "ASF 空闲时，从我的库中选择并运行符合条件的游戏", "set.autoHelp": "没有卡牌可挂且处于活跃时段时，运行仍有成就的符合条件的游戏。你的 GamesPlayedWhileIdle 列表也会继续运行。出现卡牌时由 ASF 接管。", "set.rotate": "每隔 … 小时让 ASF 检查卡牌",
		"set.webhook": "Webhook 地址", "set.nCompleted": "游戏完成时通知", "set.nEach": "每个成就都通知", "set.nSummary": "发送每日摘要", "set.summaryHour": "摘要时间（0–23）", "set.nProblems": "出现问题时通知", "set.test": "发送测试消息", "set.testOk": "已发送，请查看 Discord。", "set.testFail": "发送失败。请先保存并检查地址。", "set.save": "保存", "set.saved": "已保存。", "set.badHours": "请选择活跃时段。",
		"real.title": "切换到真实模式？", "real.text": "成就将真正在 Steam 上解锁且无法撤销。不游玩就解锁成就违反 Steam 规则，风险由你承担。模拟进度将被丢弃，计划将根据账号的真实状态重新生成。", "real.cancel": "取消", "real.ok": "是，切换",
		"hist.Unlocked": "已解锁", "hist.Simulated": "模拟", "hist.Failed": "失败", "hist.Skipped": "已跳过", "hist.Completed": "游戏完成", "hist.empty": "暂无记录。",
		"time.now": "现在", "time.in": "{0}后", "time.ago": "{0}前", "time.min": "{0} 分钟", "time.hour": "{0} 小时", "time.day": "{0} 天",
		"err.network": "无法连接服务器。", "err.server": "错误：{0}",
		"help.html": `<h2>工作原理</h2><p>SteadyAchievements 会在 ASF <strong>正在运行</strong>的游戏中缓慢、自然地解锁成就：挂卡的游戏、你的 <code>GamesPlayedWhileIdle</code> 列表，或（若启用）它自行选择的游戏。</p><ul><li><strong>计划：</strong>剩余成就分布在你设定的天数范围内；常见成就通常在前，稀有成就在后，顺序和间隔每次随机。</li><li><strong>阶梯：</strong>“30 场”与“100 场”这类阶梯成就总是从小到大解锁。</li><li><strong>时间：</strong>只有在游戏运行且处于活跃时段时才会推进。</li><li><strong>安全：</strong>默认跳过 VAC 游戏、在线游戏以及含有未拥有带成就 DLC 的游戏；受保护成就永不处理。</li><li><strong>模拟：</strong>插件以模拟模式启动；是否切换到真实模式由你决定。</li></ul><h2>ASF 命令</h2><p><code>steady</code>（或 <code>sa</code>）后接 <code>status</code>、<code>on</code>、<code>off</code>、<code>sim</code>、<code>real</code>、<code>auto on</code>、<code>auto off</code>、<code>plan &lt;appid&gt;</code>、<code>replan &lt;appid&gt;</code>。</p><h2>警告</h2><p>不游玩就解锁成就违反 Steam 规则。使用本插件的风险由你自行承担。</p>`
	}
};

// Why a game being played is skipped (reason codes sent by the plugin)
(() => {
	const why = {
		"tr-TR": ["mağaza bilgisi alınamadı", "kara listede", "ASF kara listesinde", "VAC korumalı", "oyun değil", "engellenen geliştirici", "engellenen yayıncı", "çevrimiçi / çok oyunculu", "Steam başarımı yok", "sahip olmadığın başarımlı DLC var", "oynama süresi henüz {0} saatin altında"],
		"en-US": ["store info unavailable", "on the blacklist", "on ASF's blacklist", "VAC protected", "not a game", "blocked developer", "blocked publisher", "online / multiplayer", "no Steam achievements", "has achievement DLC you don't own", "playtime still below {0} h"],
		"de-DE": ["Shop-Infos nicht verfügbar", "auf der Blacklist", "auf ASFs Blacklist", "VAC-geschützt", "kein Spiel", "blockierter Entwickler", "blockierter Publisher", "online / Mehrspieler", "keine Steam-Erfolge", "hat Erfolgs-DLC, den du nicht besitzt", "Spielzeit noch unter {0} Std."],
		"es-ES": ["sin datos de la tienda", "en la lista negra", "en la lista negra de ASF", "protegido por VAC", "no es un juego", "desarrollador bloqueado", "distribuidor bloqueado", "en línea / multijugador", "sin logros de Steam", "tiene DLC con logros que no posees", "tiempo de juego aún menor de {0} h"],
		"fr-FR": ["infos boutique indisponibles", "dans la liste noire", "dans la liste noire d'ASF", "protégé par VAC", "pas un jeu", "développeur bloqué", "éditeur bloqué", "en ligne / multijoueur", "pas de succès Steam", "DLC à succès non possédé", "temps de jeu encore sous {0} h"],
		"pl-PL": ["brak danych ze sklepu", "na czarnej liście", "na czarnej liście ASF", "chroniona przez VAC", "to nie gra", "zablokowany deweloper", "zablokowany wydawca", "online / wieloosobowa", "brak osiągnięć Steam", "ma DLC z osiągnięciami, którego nie masz", "czas gry poniżej {0} h"],
		"pt-BR": ["sem dados da loja", "na lista negra", "na lista negra do ASF", "protegido por VAC", "não é um jogo", "desenvolvedor bloqueado", "distribuidora bloqueada", "online / multijogador", "sem conquistas Steam", "tem DLC com conquistas que você não tem", "tempo de jogo ainda abaixo de {0} h"],
		"ru-RU": ["нет данных магазина", "в чёрном списке", "в чёрном списке ASF", "защищено VAC", "не игра", "заблокированный разработчик", "заблокированный издатель", "онлайн / мультиплеер", "нет достижений Steam", "есть DLC с достижениями, которого у вас нет", "наиграно меньше {0} ч"],
		"zh-CN": ["无法获取商店信息", "在黑名单中", "在 ASF 黑名单中", "受 VAC 保护", "不是游戏", "已屏蔽的开发商", "已屏蔽的发行商", "在线 / 多人", "没有 Steam 成就", "含有你未拥有的带成就 DLC", "游戏时长仍不足 {0} 小时"]
	};
	const codes = ["Unknown", "Blacklisted", "AsfBlacklisted", "Vac", "NotAGame", "BlockedDeveloper", "BlockedPublisher", "Online", "NoAchievements", "MissingDlc", "Playtime"];

	for (const [locale, texts] of Object.entries(why)) {
		codes.forEach((code, i) => { window.I18N[locale][`why.${code}`] = texts[i]; });
	}
})();

// Side menu: ASF memory usage and uptime
(() => {
	const extra = {
		"tr-TR": ["Bellek kullanımı", "Çalışma süresi", "G", "SA", "DK", "SN"],
		"en-US": ["Memory usage", "Uptime", "D", "H", "M", "S"],
		"de-DE": ["Speicher", "Laufzeit", "T", "Std", "Min", "S"],
		"es-ES": ["Memoria", "Tiempo activo", "D", "H", "Min", "S"],
		"fr-FR": ["Mémoire", "Temps actif", "J", "H", "Min", "S"],
		"pl-PL": ["Pamięć", "Czas działania", "D", "G", "Min", "S"],
		"pt-BR": ["Memória", "Tempo ativo", "D", "H", "Min", "S"],
		"ru-RU": ["Память", "Время работы", "Д", "Ч", "МИН", "С"],
		"zh-CN": ["内存占用", "运行时间", "天", "时", "分", "秒"]
	};
	const keys = ["stat.memory", "stat.uptime", "up.d", "up.h", "up.m", "up.s"];

	for (const [locale, texts] of Object.entries(extra)) {
		keys.forEach((key, i) => { window.I18N[locale][key] = texts[i]; });
	}
})();

// Hour boosting page and game picker
(() => {
	const keys = ["nav.idle", "idle.title", "idle.help", "idle.add", "idle.remove", "idle.empty", "idle.save", "idle.saved", "idle.limit", "set.pick", "picker.title", "picker.search", "picker.byPlaytime", "picker.byName", "picker.ok", "picker.cancel", "picker.count", "picker.countMax", "picker.played", "picker.none", "picker.failed"];
	const texts = {
		"tr-TR": ["Saat Kasma", "Saat kasılan oyunlar", "ASF, düşürülecek kart kalmadığında bu oyunları oynar ve saat kasar (en fazla 32). Kart düşürülürken bu liste bekler. Bu liste ASF'nin GamesPlayedWhileIdle ayarıdır; burada kaydetmek ASF'nin bot ayarını değiştirir.", "Kütüphaneden oyun ekle", "Kaldır", "Liste boş. Kütüphaneden oyun ekleyebilirsin.", "Kaydet ve ASF'ye uygula", "Kaydedildi. ASF bot ayarını yeniledi; bot birkaç saniyeliğine yeniden bağlanabilir.", "En fazla {0} oyun seçilebilir.", "Kütüphaneden seç", "Kütüphaneden oyun seç", "Oyun adı veya AppID ara…", "En çok oynanan", "Ada göre", "Tamam", "Vazgeç", "{0} seçili", "{0} / {1} seçili", "{0} saat oynandı", "Eşleşen oyun yok.", "Kütüphane Steam'den alınamadı."],
		"en-US": ["Hour Boosting", "Games idled for hours", "When there are no cards left to farm, ASF plays these games to add hours (up to 32). While cards are being farmed this list waits. This is ASF's GamesPlayedWhileIdle setting; saving here changes ASF's bot config.", "Add games from library", "Remove", "The list is empty. Add games from your library.", "Save and apply to ASF", "Saved. ASF reloaded the bot config; the bot may reconnect for a few seconds.", "You can select up to {0} games.", "Pick from library", "Pick games from your library", "Search by name or AppID…", "Most played", "By name", "OK", "Cancel", "{0} selected", "{0} / {1} selected", "{0} h played", "No matching games.", "Could not load the library from Steam."],
		"de-DE": ["Stunden sammeln", "Spiele für Spielstunden", "Wenn keine Karten mehr zu farmen sind, spielt ASF diese Spiele für Spielstunden (bis zu 32). Während Karten gefarmt werden, wartet die Liste. Das ist ASFs Einstellung GamesPlayedWhileIdle; Speichern ändert die Bot-Konfiguration.", "Spiele aus der Bibliothek hinzufügen", "Entfernen", "Die Liste ist leer. Füge Spiele aus deiner Bibliothek hinzu.", "Speichern und in ASF übernehmen", "Gespeichert. ASF hat die Bot-Konfiguration neu geladen; der Bot verbindet sich evtl. kurz neu.", "Du kannst bis zu {0} Spiele wählen.", "Aus Bibliothek wählen", "Spiele aus deiner Bibliothek wählen", "Nach Name oder AppID suchen…", "Meistgespielt", "Nach Name", "OK", "Abbrechen", "{0} ausgewählt", "{0} / {1} ausgewählt", "{0} Std. gespielt", "Keine passenden Spiele.", "Bibliothek konnte nicht von Steam geladen werden."],
		"es-ES": ["Horas de juego", "Juegos para sumar horas", "Cuando no quedan cromos, ASF juega estos juegos para sumar horas (hasta 32). Mientras consigue cromos, la lista espera. Es el ajuste GamesPlayedWhileIdle de ASF; guardar aquí cambia la configuración del bot.", "Añadir juegos de la biblioteca", "Quitar", "La lista está vacía. Añade juegos de tu biblioteca.", "Guardar y aplicar en ASF", "Guardado. ASF recargó la configuración del bot; puede reconectarse unos segundos.", "Puedes elegir hasta {0} juegos.", "Elegir de la biblioteca", "Elige juegos de tu biblioteca", "Buscar por nombre o AppID…", "Más jugados", "Por nombre", "Aceptar", "Cancelar", "{0} seleccionados", "{0} / {1} seleccionados", "{0} h jugadas", "No hay juegos que coincidan.", "No se pudo cargar la biblioteca de Steam."],
		"fr-FR": ["Heures de jeu", "Jeux pour cumuler des heures", "Quand il n'y a plus de cartes à farmer, ASF lance ces jeux pour cumuler des heures (jusqu'à 32). Pendant le farm de cartes, la liste attend. C'est le réglage GamesPlayedWhileIdle d'ASF ; enregistrer ici modifie la configuration du bot.", "Ajouter des jeux de la bibliothèque", "Retirer", "La liste est vide. Ajoute des jeux de ta bibliothèque.", "Enregistrer et appliquer à ASF", "Enregistré. ASF a rechargé la configuration du bot ; il peut se reconnecter quelques secondes.", "Tu peux choisir jusqu'à {0} jeux.", "Choisir dans la bibliothèque", "Choisis des jeux de ta bibliothèque", "Rechercher par nom ou AppID…", "Les plus joués", "Par nom", "OK", "Annuler", "{0} sélectionnés", "{0} / {1} sélectionnés", "{0} h jouées", "Aucun jeu correspondant.", "Impossible de charger la bibliothèque depuis Steam."],
		"pl-PL": ["Nabijanie godzin", "Gry do nabijania godzin", "Gdy nie ma już kart do zebrania, ASF uruchamia te gry, by nabijać godziny (maks. 32). Podczas zbierania kart lista czeka. To ustawienie GamesPlayedWhileIdle w ASF; zapis zmienia konfigurację bota.", "Dodaj gry z biblioteki", "Usuń", "Lista jest pusta. Dodaj gry z biblioteki.", "Zapisz i zastosuj w ASF", "Zapisano. ASF przeładował konfigurację bota; bot może na chwilę połączyć się ponownie.", "Możesz wybrać maksymalnie {0} gier.", "Wybierz z biblioteki", "Wybierz gry z biblioteki", "Szukaj po nazwie lub AppID…", "Najczęściej grane", "Po nazwie", "OK", "Anuluj", "Wybrano {0}", "Wybrano {0} / {1}", "{0} h gry", "Brak pasujących gier.", "Nie udało się pobrać biblioteki ze Steam."],
		"pt-BR": ["Horas de jogo", "Jogos para acumular horas", "Quando não há mais cartas para coletar, o ASF roda estes jogos para acumular horas (até 32). Enquanto coleta cartas, a lista espera. É a opção GamesPlayedWhileIdle do ASF; salvar aqui altera a configuração do bot.", "Adicionar jogos da biblioteca", "Remover", "A lista está vazia. Adicione jogos da sua biblioteca.", "Salvar e aplicar no ASF", "Salvo. O ASF recarregou a configuração do bot; ele pode reconectar por alguns segundos.", "Você pode escolher até {0} jogos.", "Escolher da biblioteca", "Escolha jogos da sua biblioteca", "Buscar por nome ou AppID…", "Mais jogados", "Por nome", "OK", "Cancelar", "{0} selecionados", "{0} / {1} selecionados", "{0} h jogadas", "Nenhum jogo encontrado.", "Não foi possível carregar a biblioteca da Steam."],
		"ru-RU": ["Набивка часов", "Игры для набивки часов", "Когда карточки для фарма закончились, ASF запускает эти игры для набивки часов (до 32). Во время фарма карточек список ждёт. Это настройка GamesPlayedWhileIdle в ASF; сохранение здесь меняет конфигурацию бота.", "Добавить игры из библиотеки", "Убрать", "Список пуст. Добавьте игры из библиотеки.", "Сохранить и применить в ASF", "Сохранено. ASF перезагрузил конфигурацию бота; бот может переподключиться на несколько секунд.", "Можно выбрать не больше {0} игр.", "Выбрать из библиотеки", "Выберите игры из библиотеки", "Поиск по названию или AppID…", "Самые наигранные", "По названию", "ОК", "Отмена", "Выбрано: {0}", "Выбрано: {0} / {1}", "наиграно {0} ч", "Ничего не найдено.", "Не удалось загрузить библиотеку из Steam."],
		"zh-CN": ["挂时长", "挂时长的游戏", "没有卡牌可挂时，ASF 会运行这些游戏来增加时长（最多 32 个）。挂卡期间此列表等待。这是 ASF 的 GamesPlayedWhileIdle 设置；在此保存会修改 ASF 的机器人配置。", "从库中添加游戏", "移除", "列表为空。可从库中添加游戏。", "保存并应用到 ASF", "已保存。ASF 已重新加载机器人配置，机器人可能会短暂重连。", "最多可选择 {0} 个游戏。", "从库中选择", "从你的库中选择游戏", "按名称或 AppID 搜索…", "最常玩", "按名称", "确定", "取消", "已选 {0} 个", "已选 {0} / {1}", "已玩 {0} 小时", "没有匹配的游戏。", "无法从 Steam 加载游戏库。"]
	};

	for (const [locale, list] of Object.entries(texts)) {
		keys.forEach((key, i) => { window.I18N[locale][key] = list[i]; });
	}
})();
