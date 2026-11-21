"use strict";

(() => {
	const API = "/Api/Steady";
	const ASF_API = "/Api";
	const REFRESH_MS = 30000;
	const MOBILE = window.matchMedia("(max-width: 760px)");

	// Our own keys, plus ASF-ui's keys (same origin): ASF's panel and this one share password, theme, language and menu state
	const KEYS = { bot: "steady.bot" };
	const ASF_KEYS = { password: "asf-ui:ipc-password", theme: "asf-ui:layout:theme", dark: "asf-ui:layout:dark-mode", locale: "asf-ui:locale", small: "asf-ui:layout:small-navigation" };
	const THEMES = [["blue", "#367fa9"], ["red", "#a92616"], ["teal", "#359392"], ["purple", "#504d8e"], ["green", "#00a65a"], ["orange", "#d58d18"]];
	const PAGES = ["overview", "games", "history", "settings", "help"];

	const $ = (sel, root = document) => root.querySelector(sel);
	const $$ = (sel, root = document) => [...root.querySelectorAll(sel)];

	const store = {
		get(key) { try { return localStorage.getItem(key); } catch { return null; } },
		set(key, value) { try { value == null ? localStorage.removeItem(key) : localStorage.setItem(key, value); } catch { /* private mode */ } },
		// ASF-ui stores JSON values
		getJson(key) { const raw = store.get(key); if (raw == null) return null; try { return JSON.parse(raw); } catch { return raw; } },
		setJson(key, value) { store.set(key, value == null ? null : JSON.stringify(value)); }
	};

	function pickLocale(wanted) {
		const codes = LANGUAGES.map(([code]) => code);
		if (!wanted) return null;
		if (codes.includes(wanted)) return wanted;
		const prefix = String(wanted).slice(0, 2).toLowerCase();
		return codes.find(code => code.slice(0, 2).toLowerCase() === prefix) || null;
	}

	const state = {
		locale: pickLocale(store.getJson(ASF_KEYS.locale)) || pickLocale(navigator.language) || "en-US",
		password: String(store.getJson(ASF_KEYS.password) || ""),
		bot: store.get(KEYS.bot) || "",
		page: PAGES.includes(location.hash.slice(1)) ? location.hash.slice(1) : "overview",
		look: { theme: store.getJson(ASF_KEYS.theme) || "blue", darkMode: store.getJson(ASF_KEYS.dark) === true },
		status: null,
		settings: null,
		timer: 0
	};

	// ------------------------------------------------------------ helpers

	function t(key, ...args) {
		const text = (I18N[state.locale] && I18N[state.locale][key]) ?? I18N["en-US"][key] ?? key;
		return args.length ? text.replace(/\{(\d+)\}/g, (_, i) => String(args[Number(i)] ?? "")) : text;
	}

	// Element builder; strings become text nodes, so names coming from Steam are never parsed as HTML
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
		const number = v => new Intl.NumberFormat(state.locale, { maximumFractionDigits: 1 }).format(v);
		if (minutes < 60) return t("time.min", minutes);
		if (minutes < 60 * 48) return t("time.hour", number(minutes / 60));
		return t("time.day", number(minutes / 1440));
	}

	function relative(iso) {
		if (!iso) return "—";
		const diff = (new Date(iso).getTime() - Date.now()) / 60000;
		if (Math.abs(diff) < 1) return t("time.now");
		return diff > 0 ? t("time.in", duration(diff)) : t("time.ago", duration(-diff));
	}

	function dateTime(iso) {
		if (!iso) return "—";
		return new Intl.DateTimeFormat(state.locale, { dateStyle: "medium", timeStyle: "short" }).format(new Date(iso));
	}

	function percent(value) {
		if (value == null) return "";
		return new Intl.NumberFormat(state.locale, { style: "percent", maximumFractionDigits: value < 1 ? 2 : 1 }).format(value / 100);
	}

	// Steam image with fallbacks; at the end a neutral placeholder takes its place
	function steamImage(className, urls, label = "") {
		const sources = urls.filter(Boolean);
		const placeholder = () => h("span", { class: `${className} placeholder`, "aria-hidden": "true" }, (label || "").trim().slice(0, 1).toUpperCase());
		if (!sources.length) return placeholder();
		let index = 0;
		const img = h("img", { class: className, src: sources[0], alt: "", loading: "lazy" });
		img.addEventListener("error", () => {
			index++;
			if (index < sources.length) img.src = sources[index];
			else img.replaceWith(placeholder());
		});
		return img;
	}

	const capsule = (appId, url, name) => steamImage("thumb", [url, `https://cdn.cloudflare.steamstatic.com/steam/apps/${appId}/capsule_231x87.jpg`], name);

	function message(el, text, isError = false) {
		el.textContent = text;
		el.className = isError ? "text-error" : "muted";
	}

	// ------------------------------------------------------------ API

	class ApiError extends Error {
		constructor(status, text) { super(text); this.status = status; }
	}

	async function request(url, options = {}) {
		let response;
		try {
			response = await fetch(url, {
				method: options.method || "GET",
				headers: { "Authentication": state.password, ...(options.body !== undefined ? { "Content-Type": "application/json" } : {}) },
				body: options.body !== undefined ? JSON.stringify(options.body) : undefined,
				cache: "no-store"
			});
		} catch {
			throw new ApiError(0, t("err.network"));
		}

		if (response.status === 401 || response.status === 403) {
			if (!options.quiet) showLogin(response.status === 403 ? t("login.banned") : (state.password ? t("login.wrong") : ""));
			throw new ApiError(response.status, "auth");
		}

		let json = null;
		try { json = await response.json(); } catch { /* empty body */ }

		if (!response.ok || (json && json.Success === false)) {
			throw new ApiError(response.status, (json && json.Message) || response.statusText);
		}

		return json ? json.Result : null;
	}

	const api = (path, options) => request(`${API}/${path}`, options);

	// ------------------------------------------------------------ appearance (shared with ASF-ui)

	function applyLook() {
		const root = document.documentElement;
		const theme = THEMES.some(([name]) => name === state.look.theme) ? state.look.theme : "blue";
		THEMES.forEach(([name]) => root.classList.toggle(`theme-${name}`, name === theme));
		root.classList.toggle("app--dark-mode", Boolean(state.look.darkMode));
		$$(".swatch").forEach(s => s.classList.toggle("active", s.dataset.theme === theme));
		$("#dark-toggle").checked = Boolean(state.look.darkMode);
	}

	// ASF-ui keeps theme and dark mode in ASF's storage API (and in localStorage); read both so both panels match
	async function loadLook() {
		try {
			const saved = await request(`${ASF_API}/Storage/asfui-settings`, { quiet: true });
			if (saved && typeof saved === "object") {
				state.look = { ...state.look, ...saved };
				applyLook();
			}
		} catch { /* not signed in yet, or no saved settings */ }
	}

	async function saveLook(change) {
		state.look = { ...state.look, ...change };
		store.setJson(ASF_KEYS.theme, state.look.theme);
		store.setJson(ASF_KEYS.dark, Boolean(state.look.darkMode));
		applyLook();
		try {
			await request(`${ASF_API}/Storage/asfui-settings`, { method: "POST", body: state.look, quiet: true });
		} catch { /* localStorage still keeps it for this browser */ }
	}

	// ------------------------------------------------------------ navigation

	function setSmallNavigation(small) {
		document.documentElement.classList.toggle("nav--small", small);
		store.setJson(ASF_KEYS.small, small);
	}

	function toggleMenu() {
		if (MOBILE.matches) {
			document.documentElement.classList.toggle("nav--open");
		} else {
			setSmallNavigation(!document.documentElement.classList.contains("nav--small"));
		}
	}

	function closeDropdowns(except) {
		for (const [button, panel] of [["#lang-toggle", "#lang-menu"], ["#look-toggle", "#look-menu"]]) {
			if (button === except) continue;
			$(panel).hidden = true;
			$(button).setAttribute("aria-expanded", "false");
		}
	}

	function toggleDropdown(button, panel) {
		const open = $(panel).hidden;
		closeDropdowns(button);
		$(panel).hidden = !open;
		$(button).setAttribute("aria-expanded", String(open));
	}

	function setLocale(code) {
		state.locale = code;
		// Same key and format as ASF-ui, so ASF's panel follows on its next load
		store.setJson(ASF_KEYS.locale, code);
		applyLanguage();
		closeDropdowns();
	}

	function applyLanguage() {
		document.documentElement.lang = state.locale.slice(0, 2);
		$$("[data-i18n]").forEach(el => { el.textContent = t(el.dataset.i18n); });
		$$("[data-title]").forEach(el => { el.title = t(el.dataset.title); el.setAttribute("aria-label", t(el.dataset.title)); });
		$("#help").innerHTML = t("help.html");
		$$("#lang-menu button").forEach(b => b.classList.toggle("active", b.dataset.locale === state.locale));
		if (state.status) renderStatus();
	}

	function showLogin(text) {
		$("#login").hidden = false;
		$$(".tab").forEach(s => { s.hidden = true; });
		$("#side-stats").hidden = true;
		const error = $("#login-error");
		error.hidden = !text;
		error.textContent = text || "";
		$("#login-password").focus();
	}

	function showPage(page) {
		state.page = PAGES.includes(page) ? page : "overview";
		$$(".side-menu__item[data-tab]").forEach(a => a.classList.toggle("active", a.dataset.tab === state.page));
		$$(".tab").forEach(s => { s.hidden = s.id !== `tab-${state.page}`; });
		document.documentElement.classList.remove("nav--open");

		if (state.page === "settings") loadSettings();
		else refreshStatus();
	}

	// ------------------------------------------------------------ status

	async function refreshStatus() {
		if (!state.bot) return;
		try {
			state.status = await api(encodeURIComponent(state.bot));
			renderStatus();
		} catch (e) {
			if (e.status !== 401 && e.status !== 403) console.warn(e);
		}
	}

	function setText(id, text, cls = "") {
		const el = $(id);
		el.textContent = text;
		el.className = cls;
	}

	function renderStatus() {
		const s = state.status;
		if (!s) return;

		const all = [...s.Games, ...s.Completed];
		const sum = key => all.reduce((total, g) => total + (g[key] || 0), 0);
		const nextEta = s.Games.filter(g => g.InSlot && g.NextEtaUtc).map(g => g.NextEtaUtc).sort()[0];

		// Side menu statistics (like ASF's bot counters)
		$("#side-stats").hidden = false;
		$("#s-unlocked").textContent = s.Simulation ? sum("Simulated") : sum("Done");
		$("#s-unlocked-label").textContent = s.Simulation ? t("stat.simulated") : t("stat.unlocked");
		$("#s-progress").textContent = s.Games.length;
		$("#s-completed").textContent = s.Completed.filter(g => g.Planned > 0).length;
		$("#s-skipped").textContent = sum("Skipped");
		$("#s-today").textContent = `${s.UnlocksToday} / ${s.MaxPerDay}`;
		$("#s-next").textContent = nextEta ? relative(nextEta) : "—";
		$("#s-mode").textContent = s.Simulation ? t("stat.sim") : t("stat.real");
		$("#version").textContent = `SteadyAchievements ${s.PluginVersion}`;

		// Overview
		$("#trimmed").hidden = !s.TrimmedRuntime;
		$("#i-today").textContent = `${s.UnlocksToday} / ${s.MaxPerDay}`;
		$("#i-games").textContent = s.Games.length;
		$("#i-next").textContent = nextEta ? relative(nextEta) : "—";

		setText("#f-mode", s.Simulation ? t("fact.sim") : t("fact.real"), s.Simulation ? "" : "text-error");
		setText("#f-enabled", s.Enabled ? t("fact.on") : t("fact.off"), s.Enabled ? "text-ok" : "text-error");
		setText("#f-hours", `${s.ActiveHours} · ${s.InWindow ? t("fact.inWindow") : t("fact.outWindow")} (${s.LocalTime.slice(11)})`);
		setText("#f-today", t("fact.today", s.UnlocksToday, s.MaxPerDay));
		setText("#f-auto", !s.AutoPlay ? t("fact.off") : s.AutoPlaying.length ? t("fact.autoActive") : t("fact.autoWaiting"));

		const playing = $("#playing");
		playing.replaceChildren();
		if (!s.Playing.length) playing.append(h("p", { class: "empty" }, t("status.noPlaying")));
		for (const p of s.Playing) {
			const reason = p.Reason && I18N["en-US"][`why.${p.Reason}`] ? t(`why.${p.Reason}`, new Intl.NumberFormat(state.locale).format(p.ReasonValue)) : p.Status;
			const [cls, text] = !p.Eligible ? ["text-error", reason]
				: p.Finished ? ["muted", t("play.finished")]
				: p.InSlot ? ["text-ok", t("play.slot")]
				: ["muted", t("play.waiting")];
			playing.append(h("div", { class: `row ${p.Eligible && !p.Finished ? "" : "row--dim"}` },
				capsule(p.AppId, p.Capsule, p.Name),
				h("div", { class: "row__main" }, h("div", { class: "row__name" }, p.Name), h("div", { class: "muted small" }, `AppID ${p.AppId}`)),
				h("div", { class: `row__side ${cls}` }, text)
			));
		}

		// Games
		const games = $("#games");
		games.replaceChildren();
		if (!s.Games.length) games.append(h("p", { class: "empty" }, t("status.noGames")));
		for (const g of s.Games) games.append(gameRow(g, s.Simulation));

		const completed = $("#completed");
		completed.replaceChildren();
		$("#completed-count").textContent = s.Completed.length ? `(${s.Completed.length})` : "";
		if (!s.Completed.length) completed.append(h("p", { class: "empty" }, t("status.noCompleted")));
		for (const g of s.Completed) {
			completed.append(h("div", { class: "row" },
				capsule(g.AppId, g.Capsule, g.Name),
				h("div", { class: "row__main" },
					h("div", { class: "row__name" }, g.Name),
					h("div", { class: "muted small" }, g.Planned === 0
						? `${t("game.nothing")} · ${g.Protected} ${t("game.protected")} · ${g.UnlockedBefore} ${t("game.before")}`
						: `${t("game.completedAt")} ${dateTime(g.CompletedUtc)} · ${g.Done + g.Simulated}/${g.Planned}`)
				),
				h("div", { class: "row__actions" }, h("button", { type: "button", class: "button button--small", onclick: () => openDetail(g.AppId, g.Name) }, t("game.view")))
			));
		}

		renderHistory();
	}

	function gameRow(g, simulation) {
		const progressed = g.Done + g.Simulated + g.Skipped;
		const pct = g.Planned ? Math.round((progressed / g.Planned) * 100) : 100;

		const details = [`${progressed}/${g.Planned}${simulation && g.Simulated ? ` (${t("game.simulated")})` : ""}`, `${duration(g.ActiveMinutes)} / ${duration(g.BudgetMinutes)}`];
		if (g.Protected) details.push(`${g.Protected} ${t("game.protected")}`);
		if (g.UnlockedBefore) details.push(`${g.UnlockedBefore} ${t("game.before")}`);
		if (g.SkippedRare) details.push(`${g.SkippedRare} ${t("game.rare")}`);

		return h("div", { class: `row ${g.InSlot ? "" : "row--dim"}` },
			capsule(g.AppId, g.Capsule, g.Name),
			h("div", { class: "row__main" },
				h("div", { class: "row__name" }, g.Name, !g.InSlot ? h("span", { class: "muted small" }, ` · ${t("game.paused")}`) : null),
				h("div", { class: "progress", title: `${pct}%` }, h("span", { style: `width:${pct}%` })),
				h("div", { class: "muted small" }, details.join(" · ")),
				g.NextName ? h("div", { class: "small" },
					`${t("game.next")}: `, h("strong", {}, g.NextName), ` ${percent(g.NextPercent)}`,
					h("span", { class: "muted" }, g.InSlot ? ` · ${relative(g.NextEtaUtc)} · ${t("game.end")} ${dateTime(g.EndEtaUtc)}` : "")
				) : null
			),
			h("div", { class: "row__actions" },
				h("button", { type: "button", class: "button button--small", onclick: () => openDetail(g.AppId, g.Name) }, t("game.view")),
				h("button", { type: "button", class: "button button--small", onclick: () => replan(g.AppId) }, t("game.replan"))
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
		tools.textContent = "";
		dialog.showModal();

		let detail;
		try {
			detail = await api(`${encodeURIComponent(state.bot)}/Games/${appId}`);
		} catch (e) {
			body.replaceChildren(h("p", { class: "text-error" }, e.status === 401 ? "" : t("ach.failed")));
			return;
		}

		const counts = {};
		for (const a of detail.Achievements) counts[a.State] = (counts[a.State] || 0) + 1;
		tools.textContent = Object.entries(counts).map(([key, n]) => `${t(`ach.${key}`)}: ${n}`).join(" · ");

		const stateClass = { done: "text-ok", unlocked: "text-ok", simulated: "muted", planned: "text-theme" };
		body.replaceChildren(...detail.Achievements.map(a => h("div", { class: `ach ${["protected", "rare", "skipped", "none"].includes(a.State) ? "ach--off" : ""}` },
			steamImage("icon", [a.Icon], "?"),
			h("div", { class: "row__main" },
				h("div", {}, h("strong", {}, a.Name), a.Hidden ? h("span", { class: "muted small" }, ` · ${t("ach.hidden")}`) : null),
				a.Description ? h("div", { class: "muted small" }, a.Description) : null,
				h("div", { class: "small" },
					h("span", { class: stateClass[a.State] || "muted" }, t(`ach.${a.State}`)),
					a.Percent != null ? ` · ${percent(a.Percent)}` : "",
					a.FinishedUtc ? ` · ${dateTime(a.FinishedUtc)}` : "",
					a.EtaUtc ? ` · ${relative(a.EtaUtc)}` : ""
				)
			)
		)));
	}

	function renderHistory() {
		const list = $("#history");
		list.replaceChildren();
		const items = (state.status && state.status.History) || [];
		if (!items.length) {
			list.append(h("p", { class: "empty" }, t("hist.empty")));
			return;
		}
		const kindClass = { Unlocked: "text-ok", Completed: "text-theme", Failed: "text-error", Skipped: "text-warn", Simulated: "muted" };
		for (const item of items) {
			list.append(h("div", { class: "row" },
				h("div", { class: "row__main" },
					h("div", {}, h("span", { class: kindClass[item.Kind] || "" }, t(`hist.${item.Kind}`)), " · ", h("strong", {}, item.Achievement || item.Game)),
					item.Achievement ? h("div", { class: "muted small" }, `${item.Game}${item.Percent ? ` · ${percent(item.Percent)}` : ""}`) : null
				),
				h("div", { class: "row__side muted", title: dateTime(item.Utc) }, relative(item.Utc))
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
			if (e.status !== 401) message($("#save-msg"), e.message, true);
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
		message($("#save-msg"), "");
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
			message(msg, t("set.badHours"), true);
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
			message(msg, t("set.saved"));
			refreshStatus();
		} catch (e) {
			if (e.status !== 401) message(msg, e.message, true);
		}
	}

	async function testDiscord() {
		const msg = $("#save-msg");
		try {
			const ok = await api(`${encodeURIComponent(state.bot)}/TestDiscord`, { method: "POST" });
			message(msg, ok === false ? t("set.testFail") : t("set.testOk"), ok === false);
		} catch (e) {
			if (e.status !== 401) message(msg, t("set.testFail"), true);
		}
	}

	// ------------------------------------------------------------ start

	async function start() {
		let bots;
		try {
			bots = await api("Bots");
		} catch (e) {
			if (e.status !== 401 && e.status !== 403) showLogin(e.message);
			return;
		}

		$("#login").hidden = true;
		loadLook();

		const select = $("#bot");
		select.replaceChildren(...bots.map(name => h("option", { value: name }, name)));
		select.hidden = bots.length < 2;
		if (!bots.includes(state.bot)) state.bot = bots[0] || "";
		select.value = state.bot;

		showPage(state.page);
		clearInterval(state.timer);
		state.timer = setInterval(() => {
			if (!document.hidden && state.page !== "settings") refreshStatus();
		}, REFRESH_MS);
	}

	function build() {
		$("#lang-menu").replaceChildren(...LANGUAGES.map(([code, name]) => h("button", { type: "button", "data-locale": code, onclick: () => setLocale(code) }, name)));
		$("#swatches").replaceChildren(...THEMES.map(([name, color]) => h("button", { type: "button", class: "swatch", "data-theme": name, title: name, style: `background:${color}`, onclick: () => saveLook({ theme: name }) })));
		renderIcons();
	}

	function wire() {
		$("#menu-toggle").addEventListener("click", toggleMenu);
		$("#lang-toggle").addEventListener("click", e => { e.stopPropagation(); toggleDropdown("#lang-toggle", "#lang-menu"); });
		$("#look-toggle").addEventListener("click", e => { e.stopPropagation(); toggleDropdown("#look-toggle", "#look-menu"); });
		$("#look-menu").addEventListener("click", e => e.stopPropagation());
		$("#dark-toggle").addEventListener("change", e => saveLook({ darkMode: e.target.checked }));
		document.addEventListener("click", () => closeDropdowns());
		$(".main-container").addEventListener("click", () => document.documentElement.classList.remove("nav--open"));

		$("#logout").addEventListener("click", () => {
			state.password = "";
			store.set(ASF_KEYS.password, null);
			clearInterval(state.timer);
			showLogin("");
		});

		window.addEventListener("hashchange", () => {
			const page = location.hash.slice(1);
			if (PAGES.includes(page)) showPage(page);
		});

		$("#bot").addEventListener("change", e => {
			state.bot = e.target.value;
			store.set(KEYS.bot, state.bot);
			state.status = null;
			showPage(state.page);
		});

		$("#login-form").addEventListener("submit", e => {
			e.preventDefault();
			state.password = $("#login-password").value;
			// Same key and format as ASF-ui, so signing in here also signs in there (and the other way round)
			store.setJson(ASF_KEYS.password, state.password);
			start();
		});

		$("#settings-form").addEventListener("submit", saveSettings);
		$("#test-discord").addEventListener("click", testDiscord);
		$("#detail-close").addEventListener("click", () => $("#detail").close());
		$("#detail").addEventListener("click", e => { if (e.target.id === "detail") e.target.close(); });
		document.addEventListener("visibilitychange", () => { if (!document.hidden && state.page !== "settings") refreshStatus(); });
		window.addEventListener("storage", e => {
			if (e.key === ASF_KEYS.theme || e.key === ASF_KEYS.dark) {
				state.look = { ...state.look, theme: store.getJson(ASF_KEYS.theme) || state.look.theme, darkMode: store.getJson(ASF_KEYS.dark) === true };
				applyLook();
			}
		});
	}

	if (store.getJson(ASF_KEYS.small) === true) document.documentElement.classList.add("nav--small");
	build();
	applyLook();
	wire();
	applyLanguage();
	start();
})();
