"use strict";

(() => {
	const API = "/Api/Steady";
	const STORE = { password: "steady.ipc", lang: "steady.lang", bot: "steady.bot", tab: "steady.tab" };
	const REFRESH_MS = 30000;

	const $ = (sel, root = document) => root.querySelector(sel);
	const $$ = (sel, root = document) => [...root.querySelectorAll(sel)];

	const store = {
		get(key) { try { return localStorage.getItem(key); } catch { return null; } },
		set(key, value) { try { value == null ? localStorage.removeItem(key) : localStorage.setItem(key, value); } catch { /* private mode */ } }
	};

	const state = {
		lang: store.get(STORE.lang) || ((navigator.language || "tr").toLowerCase().startsWith("tr") ? "tr" : "en"),
		password: store.get(STORE.password) || "",
		bot: store.get(STORE.bot) || "",
		tab: store.get(STORE.tab) || "status",
		status: null,
		settings: null,
		timer: 0
	};

	// ------------------------------------------------------------ helpers

	function t(key, ...args) {
		const text = (I18N[state.lang] && I18N[state.lang][key]) ?? I18N.tr[key] ?? key;
		return args.length ? text.replace(/\{(\d+)\}/g, (_, i) => String(args[Number(i)] ?? "")) : text;
	}

	// Small element builder; strings become text nodes, so Steam-provided names are never parsed as HTML
	function h(tag, attrs = {}, ...children) {
		const el = document.createElement(tag);
		for (const [key, value] of Object.entries(attrs || {})) {
			if (value == null || value === false) continue;
			if (key === "class") el.className = value;
			else if (key.startsWith("on")) el.addEventListener(key.slice(2), value);
			else el.setAttribute(key, value === true ? "" : value);
		}
		for (const child of children.flat()) {
			if (child == null || child === false) continue;
			el.append(child instanceof Node ? child : document.createTextNode(String(child)));
		}
		return el;
	}

	function duration(minutes) {
		minutes = Math.max(0, Math.round(minutes));
		if (minutes < 60) return t("time.min", minutes);
		if (minutes < 60 * 48) return t("time.hour", Math.round(minutes / 6) / 10);
		return t("time.day", Math.round(minutes / 144) / 10);
	}

	function relative(iso) {
		if (!iso) return "—";
		const diff = (new Date(iso).getTime() - Date.now()) / 60000;
		if (Math.abs(diff) < 1) return t("time.now");
		return diff > 0 ? t("time.in", duration(diff)) : t("time.ago", duration(-diff));
	}

	function dateTime(iso) {
		if (!iso) return "—";
		return new Intl.DateTimeFormat(state.lang === "tr" ? "tr-TR" : "en-GB", { dateStyle: "medium", timeStyle: "short" }).format(new Date(iso));
	}

	function percent(value) {
		if (value == null) return "";
		const number = value < 1 ? value.toFixed(2) : value.toFixed(1);
		return state.lang === "tr" ? `%${number.replace(".", ",")}` : `${number}%`;
	}

	function headerImage(appId) {
		return `https://cdn.cloudflare.steamstatic.com/steam/apps/${appId}/capsule_231x87.jpg`;
	}

	function toast(el, text, isError = false) {
		el.textContent = text;
		el.classList.toggle("error", isError);
	}

	// ------------------------------------------------------------ API

	class ApiError extends Error {
		constructor(status, message) { super(message); this.status = status; }
	}

	async function api(path, options = {}) {
		let response;
		try {
			response = await fetch(`${API}/${path}`, {
				method: options.method || "GET",
				headers: { "Authentication": state.password, ...(options.body ? { "Content-Type": "application/json" } : {}) },
				body: options.body ? JSON.stringify(options.body) : undefined,
				cache: "no-store"
			});
		} catch {
			throw new ApiError(0, t("err.network"));
		}

		if (response.status === 401 || response.status === 403) {
			showLogin(response.status === 403 ? t("login.banned") : (state.password ? t("login.wrong") : ""));
			throw new ApiError(response.status, "auth");
		}

		let json = null;
		try { json = await response.json(); } catch { /* empty body */ }

		if (!response.ok || (json && json.Success === false)) {
			throw new ApiError(response.status, (json && json.Message) || response.statusText);
		}

		return json ? json.Result : null;
	}

	// ------------------------------------------------------------ layout

	function applyLanguage() {
		document.documentElement.lang = state.lang;
		$$("[data-i18n]").forEach(el => { el.textContent = t(el.dataset.i18n); });
		$("#tab-help").innerHTML = t("help.html");
		$$(".lang button").forEach(b => b.classList.toggle("active", b.dataset.lang === state.lang));
		$("#login-password").placeholder = "IPCPassword";
		if (state.status) renderStatus();
		if (state.tab === "history" && state.status) renderHistory();
	}

	function showLogin(message) {
		$("#login").hidden = false;
		$$(".tab").forEach(s => { s.hidden = true; });
		$(".tabs").hidden = true;
		const error = $("#login-error");
		error.hidden = !message;
		error.textContent = message || "";
		$("#login-password").focus();
	}

	function hideLogin() {
		$("#login").hidden = true;
		$(".tabs").hidden = false;
	}

	function selectTab(tab) {
		state.tab = tab;
		store.set(STORE.tab, tab);
		$$(".tabs button").forEach(b => b.setAttribute("aria-selected", String(b.dataset.tab === tab)));
		$$(".tab").forEach(s => { s.hidden = s.id !== `tab-${tab}`; });

		if (tab === "settings") loadSettings();
		if (tab === "history") renderHistory();
		if (tab === "status" || tab === "history") refreshStatus();
	}

	// ------------------------------------------------------------ status

	async function refreshStatus() {
		if (!state.bot) return;
		try {
			state.status = await api(encodeURIComponent(state.bot));
			renderStatus();
			if (state.tab === "history") renderHistory();
		} catch (e) {
			if (e.status !== 401 && e.status !== 403) console.warn(e);
		}
	}

	function badge(el, text, kind) {
		el.textContent = text;
		el.className = `badge ${kind || ""}`;
		el.hidden = !text;
	}

	function renderStatus() {
		const s = state.status;
		if (!s) return;

		$("#trimmed").hidden = !s.TrimmedRuntime;
		badge($("#mode-badge"), s.Simulation ? t("badge.sim") : t("badge.real"), s.Simulation ? "sim" : "real");
		badge($("#enabled-badge"), s.Enabled ? t("badge.on") : t("badge.off"), s.Enabled ? "ok" : "off");
		badge($("#window-badge"), s.InWindow ? t("badge.inWindow") : t("badge.outWindow"), s.InWindow ? "ok" : "off");
		badge($("#auto-badge"), s.AutoPlay ? (s.AutoPlaying.length ? t("badge.autoActive") : t("badge.auto")) : "", "info");
		$("#today").textContent = `${s.UnlocksToday} / ${s.MaxPerDay}`;
		$("#hours").textContent = s.ActiveHours;
		$("#local-time").textContent = s.LocalTime.slice(11);

		const playing = $("#playing");
		playing.replaceChildren();
		if (!s.Playing.length) {
			playing.append(h("p", { class: "empty" }, t("status.noPlaying")));
		}
		for (const p of s.Playing) {
			playing.append(h("div", { class: `row-item ${p.Eligible ? "" : "dim"}` },
				h("img", { class: "thumb", src: headerImage(p.AppId), alt: "", loading: "lazy", onerror: e => { e.target.style.visibility = "hidden"; } }),
				h("div", { class: "grow" }, h("strong", {}, p.Name), h("div", { class: "muted small" }, `AppID ${p.AppId}`)),
				h("span", { class: `chip ${p.Eligible ? (p.InSlot ? "ok" : "") : "off"}` }, p.Eligible ? (p.InSlot ? t("play.slot") : t("play.waiting")) : p.Status)
			));
		}

		const games = $("#games");
		games.replaceChildren();
		if (!s.Games.length) {
			games.append(h("p", { class: "empty" }, t("status.noGames")));
		}
		for (const g of s.Games) games.append(gameCard(g, s.Simulation));

		const completed = $("#completed");
		completed.replaceChildren();
		$("#completed-count").textContent = s.Completed.length ? `(${s.Completed.length})` : "";
		if (!s.Completed.length) completed.append(h("p", { class: "empty" }, t("status.noCompleted")));
		for (const g of s.Completed) {
			const progressed = g.Done + g.Simulated;
			completed.append(h("div", { class: "row-item" },
				h("img", { class: "thumb", src: headerImage(g.AppId), alt: "", loading: "lazy", onerror: e => { e.target.style.visibility = "hidden"; } }),
				h("div", { class: "grow" },
					h("strong", {}, g.Name),
					h("div", { class: "muted small" }, g.Planned === 0
						? `${t("game.nothing")} · ${g.Protected} ${t("game.protected")} · ${g.UnlockedBefore} ${t("game.before")}`
						: `${t("game.completedAt")} ${dateTime(g.CompletedUtc)} · ${progressed}/${g.Planned}`)
				),
				h("button", { type: "button", class: "link", onclick: () => openDetail(g.AppId, g.Name) }, t("game.view"))
			));
		}
	}

	function gameCard(g, simulation) {
		const progressed = g.Done + g.Simulated + g.Skipped;
		const pct = g.Planned ? Math.round((progressed / g.Planned) * 100) : 100;
		const timePct = g.BudgetMinutes ? Math.min(100, Math.round((g.ActiveMinutes / g.BudgetMinutes) * 100)) : 0;

		const meta = [];
		if (g.Protected) meta.push(`${g.Protected} ${t("game.protected")}`);
		if (g.UnlockedBefore) meta.push(`${g.UnlockedBefore} ${t("game.before")}`);
		if (g.SkippedRare) meta.push(`${g.SkippedRare} ${t("game.rare")}`);

		return h("article", { class: `game card ${g.InSlot ? "" : "paused"}` },
			h("img", { class: "cover", src: `https://cdn.cloudflare.steamstatic.com/steam/apps/${g.AppId}/header.jpg`, alt: "", loading: "lazy", onerror: e => { e.target.remove(); } }),
			h("div", { class: "game-body" },
				h("div", { class: "game-title" }, h("strong", {}, g.Name), !g.InSlot ? h("span", { class: "chip off" }, t("game.paused")) : null),
				h("div", { class: "progress", title: `${progressed}/${g.Planned}` }, h("span", { style: `width:${pct}%` })),
				h("div", { class: "muted small" }, `${progressed} / ${g.Planned}${simulation && g.Simulated ? ` (${g.Simulated} ${t("game.simulated")})` : ""} · ${duration(g.ActiveMinutes)} / ${duration(g.BudgetMinutes)} (${timePct}%)`),
				meta.length ? h("div", { class: "muted small" }, meta.join(" · ")) : null,
				g.NextName ? h("div", { class: "next" },
					h("span", { class: "label" }, t("game.next")),
					h("strong", {}, g.NextName), " ", h("span", { class: "muted" }, percent(g.NextPercent)),
					h("div", { class: "muted small" }, `${t("game.eta")}: ${g.InSlot ? relative(g.NextEtaUtc) : "—"} · ${t("game.end")}: ${g.InSlot ? dateTime(g.EndEtaUtc) : "—"}`)
				) : null,
				h("div", { class: "actions" },
					h("button", { type: "button", onclick: () => openDetail(g.AppId, g.Name) }, t("game.view")),
					h("button", { type: "button", class: "link", onclick: () => replan(g.AppId) }, t("game.replan"))
				)
			)
		);
	}

	async function replan(appId) {
		if (!confirm(t("game.replanAsk"))) return;
		try {
			await api(`${encodeURIComponent(state.bot)}/Replan/${appId}`, { method: "POST" });
			refreshStatus();
		} catch (e) {
			if (e.status !== 401) alert(t("err.server", e.message));
		}
	}

	async function openDetail(appId, name) {
		const dialog = $("#detail");
		$("#detail-title").textContent = name;
		const body = $("#detail-body");
		const tools = $("#detail-tools");
		body.replaceChildren(h("p", { class: "muted" }, t("ach.loading")));
		tools.replaceChildren();
		dialog.showModal();

		let detail;
		try {
			detail = await api(`${encodeURIComponent(state.bot)}/Games/${appId}`);
		} catch (e) {
			body.replaceChildren(h("p", { class: "error" }, e.status === 401 ? "" : t("ach.failed")));
			return;
		}

		const counts = {};
		for (const a of detail.Achievements) counts[a.State] = (counts[a.State] || 0) + 1;
		tools.replaceChildren(...Object.entries(counts).map(([key, n]) => h("span", { class: `chip st-${key}` }, `${t(`ach.${key}`)}: ${n}`)));

		body.replaceChildren(...detail.Achievements.map(a => h("div", { class: `ach st-${a.State}` },
			a.Icon ? h("img", { src: a.Icon, alt: "", loading: "lazy" }) : h("span", { class: "noicon" }),
			h("div", { class: "grow" },
				h("div", {}, h("strong", {}, a.Name), a.Hidden ? h("span", { class: "muted small" }, ` · ${t("ach.hidden")}`) : null),
				a.Description ? h("div", { class: "muted small" }, a.Description) : null,
				h("div", { class: "small" },
					h("span", { class: "state" }, t(`ach.${a.State}`)),
					a.Percent != null ? ` · ${percent(a.Percent)}` : "",
					a.FinishedUtc ? ` · ${dateTime(a.FinishedUtc)}` : "",
					a.EtaUtc ? ` · ${relative(a.EtaUtc)}` : ""
				)
			)
		)));
	}

	// ------------------------------------------------------------ history

	function renderHistory() {
		const list = $("#history");
		list.replaceChildren();
		const items = (state.status && state.status.History) || [];
		if (!items.length) {
			list.append(h("p", { class: "empty" }, t("hist.empty")));
			return;
		}
		for (const item of items) {
			list.append(h("div", { class: "row-item" },
				h("span", { class: `chip k-${item.Kind}` }, t(`hist.${item.Kind}`)),
				h("div", { class: "grow" },
					h("strong", {}, item.Achievement || item.Game),
					h("div", { class: "muted small" }, item.Achievement ? `${item.Game}${item.Percent ? ` · ${percent(item.Percent)}` : ""}` : "")
				),
				h("span", { class: "muted small", title: dateTime(item.Utc) }, relative(item.Utc))
			));
		}
	}

	// ------------------------------------------------------------ settings

	const LIST_NUMBERS = ["Blacklist", "Allowlist"];
	const LIST_TEXT = ["BlockedDevelopers", "BlockedPublishers"];

	async function loadSettings() {
		if (!state.bot) return;
		try {
			state.settings = await api(`${encodeURIComponent(state.bot)}/Settings`);
			fillSettings(state.settings);
		} catch (e) {
			if (e.status !== 401) toast($("#save-msg"), e.message, true);
		}
	}

	function fillSettings(s) {
		const form = $("#settings-form");
		for (const el of form.elements) {
			if (!el.name || !(el.name in s)) continue;
			const value = s[el.name];
			if (el.type === "checkbox") el.checked = Boolean(value);
			else if (LIST_NUMBERS.includes(el.name) || LIST_TEXT.includes(el.name)) el.value = (value || []).join(", ");
			else el.value = value ?? "";
		}
		form.elements.mode.value = s.Simulation ? "sim" : "real";
		const [from, to] = (s.ActiveHours || "10:00-01:00").split("-");
		form.elements.from.value = from.padStart(5, "0");
		form.elements.to.value = to.padStart(5, "0");
		toast($("#save-msg"), "");
	}

	function collectSettings() {
		const form = $("#settings-form");
		const s = { ...state.settings };
		for (const el of form.elements) {
			if (!el.name || !(el.name in s)) continue;
			if (el.type === "checkbox") s[el.name] = el.checked;
			else if (LIST_NUMBERS.includes(el.name)) s[el.name] = el.value.split(/[\s,;]+/).map(Number).filter(n => Number.isInteger(n) && n > 0);
			else if (LIST_TEXT.includes(el.name)) s[el.name] = el.value.split(",").map(x => x.trim()).filter(Boolean);
			else if (el.type === "number") s[el.name] = el.value === "" ? 0 : Number(el.value);
			else s[el.name] = el.value.trim() === "" && el.name === "TimeZone" ? null : el.value.trim();
		}
		s.Simulation = form.elements.mode.value !== "real";
		s.ActiveHours = `${form.elements.from.value}-${form.elements.to.value}`;
		return s;
	}

	function confirmReal() {
		const dialog = $("#confirm-real");
		return new Promise(resolve => {
			const done = value => { dialog.close(); resolve(value); };
			$$("button", dialog).forEach(b => { b.onclick = () => done(b.value === "ok"); });
			dialog.oncancel = () => resolve(false);
			dialog.showModal();
		});
	}

	async function saveSettings(event) {
		event.preventDefault();
		const msg = $("#save-msg");
		const form = $("#settings-form");
		if (!form.elements.from.value || !form.elements.to.value) {
			toast(msg, t("set.badHours"), true);
			return;
		}

		const next = collectSettings();
		if (state.settings.Simulation && !next.Simulation && !(await confirmReal())) {
			form.elements.mode.value = "sim";
			return;
		}

		try {
			await api(`${encodeURIComponent(state.bot)}/Settings`, { method: "POST", body: next });
			state.settings = next;
			toast(msg, t("set.saved"));
			refreshStatus();
		} catch (e) {
			if (e.status !== 401) toast(msg, e.message, true);
		}
	}

	async function testDiscord() {
		const msg = $("#save-msg");
		try {
			const ok = await api(`${encodeURIComponent(state.bot)}/TestDiscord`, { method: "POST" });
			toast(msg, ok === false ? t("set.testFail") : t("set.testOk"), ok === false);
		} catch (e) {
			if (e.status !== 401) toast(msg, t("set.testFail"), true);
		}
	}

	// ------------------------------------------------------------ start

	async function start() {
		let bots;
		try {
			bots = await api("Bots");
		} catch (e) {
			if (e.status !== 401 && e.status !== 403) {
				showLogin(e.message);
			}
			return;
		}

		hideLogin();
		const select = $("#bot");
		select.replaceChildren(...bots.map(name => h("option", { value: name }, name)));
		select.hidden = bots.length < 2;
		if (!bots.includes(state.bot)) state.bot = bots[0] || "";
		select.value = state.bot;

		selectTab(state.tab);
		clearInterval(state.timer);
		state.timer = setInterval(() => {
			if (!document.hidden && (state.tab === "status" || state.tab === "history")) refreshStatus();
		}, REFRESH_MS);
	}

	function wire() {
		$$(".lang button").forEach(b => b.addEventListener("click", () => {
			state.lang = b.dataset.lang;
			store.set(STORE.lang, state.lang);
			applyLanguage();
		}));

		$$(".tabs button").forEach(b => b.addEventListener("click", () => selectTab(b.dataset.tab)));

		$("#bot").addEventListener("change", e => {
			state.bot = e.target.value;
			store.set(STORE.bot, state.bot);
			state.status = null;
			selectTab(state.tab);
		});

		$("#login-form").addEventListener("submit", e => {
			e.preventDefault();
			state.password = $("#login-password").value;
			store.set(STORE.password, state.password);
			start();
		});

		$("#settings-form").addEventListener("submit", saveSettings);
		$("#test-discord").addEventListener("click", testDiscord);
		$("#detail-close").addEventListener("click", () => $("#detail").close());
		$("#detail").addEventListener("click", e => { if (e.target.id === "detail") e.target.close(); });
		document.addEventListener("visibilitychange", () => { if (!document.hidden && state.tab === "status") refreshStatus(); });
	}

	wire();
	applyLanguage();
	start();
})();
