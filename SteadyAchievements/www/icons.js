"use strict";

// Small line icons (24×24, drawn with the current text colour). <i data-icon="name"> is replaced by the SVG.
window.ICONS = {
	menu: '<path d="M4 6h16M4 12h16M4 18h16"/>',
	language: '<path d="M3 5h10M8 3v2M5 5c.5 3.5 3 6.5 7 8M11 5c-.6 3.6-3 6.6-7 8"/><path d="M13 21l4-9 4 9M14.4 18h5.2"/>',
	gears: '<circle cx="9" cy="10" r="2.5"/><path d="M9 4.5v1.6M9 13.9v1.6M3.5 10h1.6M12.9 10h1.6M5.1 6.1l1.1 1.1M11.8 12.8l1.1 1.1M5.1 13.9l1.1-1.1M11.8 7.2l1.1-1.1"/><circle cx="17" cy="17" r="1.8"/><path d="M17 13.2v1.1M17 19.7v1.1M13.2 17h1.1M19.7 17h1.1"/>',
	logout: '<path d="M14 4h5v16h-5"/><path d="M9 8l-4 4 4 4M5 12h10"/>',
	gauge: '<path d="M4.2 17.5a9 9 0 1 1 15.6 0"/><path d="M12 13l4-4"/><circle cx="12" cy="13.5" r="1.3"/>',
	gamepad: '<rect x="2.5" y="7" width="19" height="10.5" rx="4.5"/><path d="M7.5 10.5v4M5.5 12.5h4"/><circle cx="15.5" cy="11.5" r=".9"/><circle cx="17.8" cy="13.8" r=".9"/>',
	history: '<path d="M3.5 12a8.5 8.5 0 1 0 2.6-6.1L3.5 8.5"/><path d="M3.5 3.5v5h5"/><path d="M12 7.5V12l3 2"/>',
	sliders: '<path d="M4 6h9M17 6h3M4 12h3M11 12h9M4 18h11M19 18h1"/><circle cx="15" cy="6" r="2"/><circle cx="9" cy="12" r="2"/><circle cx="17" cy="18" r="2"/>',
	help: '<circle cx="12" cy="12" r="9"/><path d="M9.6 9.2a2.5 2.5 0 1 1 3.4 2.3c-.6.3-1 .9-1 1.6v.4"/><path d="M12 16.8v.2"/>',
	back: '<path d="M10 6l-6 6 6 6M4 12h16"/>',
	trophy: '<path d="M8 4h8v5a4 4 0 0 1-8 0z"/><path d="M8 6H5.5a2.5 2.5 0 0 0 2.6 3.9M16 6h2.5a2.5 2.5 0 0 1-2.6 3.9"/><path d="M12 13v3.5M8.5 20h7M9.5 16.5h5"/>',
	clock: '<circle cx="12" cy="12" r="8.5"/><path d="M12 7.5V12l3 2"/>',
	calendar: '<rect x="3.5" y="5" width="17" height="15.5" rx="2"/><path d="M3.5 10h17M8 3v4M16 3v4"/>',
	flask: '<path d="M9.5 3.5h5M10.5 3.5v5.8L5.3 18a2 2 0 0 0 1.7 3h10a2 2 0 0 0 1.7-3l-5.2-8.7V3.5"/><path d="M7.5 15h9"/>',
	code: '<path d="M8 7l-5 5 5 5M16 7l5 5-5 5"/>',
	memory: '<rect x="6" y="6" width="12" height="12" rx="1.5"/><rect x="9.5" y="9.5" width="5" height="5"/><path d="M9 2.5V6M15 2.5V6M9 18v3.5M15 18v3.5M2.5 9H6M2.5 15H6M18 9h3.5M18 15h3.5"/>',
	uptime: '<path d="M7 3h10M7 21h10M8 3v3.5a4 4 0 0 0 1.6 3.2L12 12l2.4-2.3A4 4 0 0 0 16 6.5V3M8 21v-3.5a4 4 0 0 1 1.6-3.2L12 12l2.4 2.3a4 4 0 0 1 1.6 3.2V21"/>',
	branch: '<circle cx="6" cy="5" r="2"/><circle cx="6" cy="19" r="2"/><circle cx="18" cy="7" r="2"/><path d="M6 7v10M18 9c0 5-6 4-11.5 8.5"/>'
};

// Small flags (30×20) for the language menu, drawn here so no image files are needed
(() => {
	const star = (cx, cy, outer, inner, angle = -90) => {
		const points = [];
		for (let i = 0; i < 10; i++) {
			const r = i % 2 === 0 ? outer : inner;
			const a = ((angle + (i * 36)) * Math.PI) / 180;
			points.push(`${(cx + (r * Math.cos(a))).toFixed(2)},${(cy + (r * Math.sin(a))).toFixed(2)}`);
		}
		return `<polygon points="${points.join(" ")}"/>`;
	};
	const bands = (colors, vertical = false) => colors.map((c, i) => vertical
		? `<rect x="${(30 / colors.length) * i}" width="${30 / colors.length + 0.1}" height="20" fill="${c}"/>`
		: `<rect y="${(20 / colors.length) * i}" width="30" height="${20 / colors.length + 0.1}" fill="${c}"/>`).join("");
	const usStripes = Array.from({ length: 13 }, (_, i) => `<rect y="${(i * 20) / 13}" width="30" height="${20 / 13 + 0.05}" fill="${i % 2 ? "#fff" : "#b22234"}"/>`).join("");

	window.FLAGS = {
		"tr-TR": `<rect width="30" height="20" fill="#e30a17"/><circle cx="11" cy="10" r="5" fill="#fff"/><circle cx="12.25" cy="10" r="4" fill="#e30a17"/><g fill="#fff">${star(17.4, 10, 2.3, 0.95, 180)}</g>`,
		"en-US": `${usStripes}<rect width="13" height="${(20 / 13) * 7}" fill="#3c3b6e"/>`,
		"de-DE": bands(["#000", "#dd0000", "#ffce00"]),
		"es-ES": `<rect width="30" height="20" fill="#aa151b"/><rect y="5" width="30" height="10" fill="#f1bf00"/>`,
		"fr-FR": bands(["#002395", "#fff", "#ed2939"], true),
		"pl-PL": bands(["#fff", "#dc143c"]),
		"pt-BR": `<rect width="30" height="20" fill="#009c3b"/><polygon points="15,2 28,10 15,18 2,10" fill="#ffdf00"/><circle cx="15" cy="10" r="4.3" fill="#002776"/>`,
		"ru-RU": bands(["#fff", "#0039a6", "#d52b1e"]),
		"zh-CN": `<rect width="30" height="20" fill="#de2910"/><g fill="#ffde00">${star(5, 5, 3, 1.15)}${star(10, 2, 1, 0.38)}${star(12, 4, 1, 0.38)}${star(12, 7, 1, 0.38)}${star(10, 9, 1, 0.38)}</g>`
	};

	// Nordic cross flags
	const nordic = (bg, cross, outline) => `<rect width="30" height="20" fill="${bg}"/>${outline ? `<rect x="8" width="5.4" height="20" fill="${outline}"/><rect y="7.3" width="30" height="5.4" fill="${outline}"/>` : ""}<rect x="9" width="3.4" height="20" fill="${cross}"/><rect y="8.3" width="30" height="3.4" fill="${cross}"/>`;
	const stripes = (n, a, b) => Array.from({ length: n }, (_, i) => `<rect y="${(i * 20) / n}" width="30" height="${20 / n + 0.05}" fill="${i % 2 ? b : a}"/>`).join("");
	const sixPoint = (cx, cy, r, color) => {
		const tri = rot => [0, 120, 240].map(a => { const rad = ((a + rot) * Math.PI) / 180; return `${(cx + (r * Math.sin(rad))).toFixed(2)},${(cy - (r * Math.cos(rad))).toFixed(2)}`; }).join(" ");
		return `<polygon points="${tri(0)}" fill="none" stroke="${color}" stroke-width=".8"/><polygon points="${tri(180)}" fill="none" stroke="${color}" stroke-width=".8"/>`;
	};

	Object.assign(window.FLAGS, {
		"be-BY": `<rect width="30" height="20" fill="#c8313e"/><rect y="13.3" width="30" height="6.7" fill="#4aa657"/><rect width="4" height="20" fill="#fff"/><path d="M2 1l1.3 2L2 5 .7 3zM2 7l1.3 2L2 11 .7 9zM2 13l1.3 2L2 17 .7 15z" fill="#c8313e"/>`,
		"bg-BG": bands(["#fff", "#00966e", "#d62612"]),
		"bs-BA": `<rect width="30" height="20" fill="#002395"/><polygon points="8,0 22,0 22,20" fill="#fecb00"/><g fill="#fff">${[0, 1, 2, 3, 4, 5].map(i => star(6 + (i * 2.4), 1.5 + (i * 3.4), 1, 0.4)).join("")}</g>`,
		"ca-ES": stripes(9, "#fcdd09", "#da121a"),
		"cs-CZ": `<rect width="30" height="10" fill="#fff"/><rect y="10" width="30" height="10" fill="#d7141a"/><polygon points="0,0 15,10 0,20" fill="#11457e"/>`,
		"da-DK": nordic("#c8102e", "#fff"),
		"el-GR": `${stripes(9, "#0d5eaf", "#fff")}<rect width="11.1" height="11.1" fill="#0d5eaf"/><rect x="4.45" width="2.2" height="11.1" fill="#fff"/><rect y="4.45" width="11.1" height="2.2" fill="#fff"/>`,
		"fa-IR": `${bands(["#239f40", "#fff", "#da0000"])}<circle cx="15" cy="10" r="1.8" fill="none" stroke="#da0000" stroke-width=".8"/>`,
		"fi-FI": nordic("#fff", "#002f6c"),
		"he-IL": `<rect width="30" height="20" fill="#fff"/><rect y="2" width="30" height="2.6" fill="#0038b8"/><rect y="15.4" width="30" height="2.6" fill="#0038b8"/>${sixPoint(15, 10, 3.6, "#0038b8")}`,
		"hr-HR": `${bands(["#ff0000", "#fff", "#171796"])}<rect x="12.5" y="5" width="5" height="6.5" fill="#fff" stroke="#ff0000" stroke-width=".6"/><path d="M12.5 5h1.7v1.6h-1.7zM15.8 5h1.7v1.6h-1.7zM14.2 6.6h1.6v1.6h-1.6zM12.5 8.2h1.7v1.6h-1.7zM15.8 8.2h1.7v1.6h-1.7zM14.2 9.8h1.6v1.7h-1.6z" fill="#ff0000"/>`,
		"hu-HU": bands(["#ce2939", "#fff", "#477050"]),
		"id-ID": bands(["#ce1126", "#fff"]),
		"it-IT": bands(["#009246", "#fff", "#ce2b37"], true),
		"ja-JP": `<rect width="30" height="20" fill="#fff"/><circle cx="15" cy="10" r="6" fill="#bc002d"/>`,
		"ka-GE": `<rect width="30" height="20" fill="#fff"/><rect x="13.3" width="3.4" height="20" fill="#ff0000"/><rect y="8.3" width="30" height="3.4" fill="#ff0000"/><path d="M6 2.5v4M4 4.5h4M24 2.5v4M22 4.5h4M6 13.5v4M4 15.5h4M24 13.5v4M22 15.5h4" stroke="#ff0000" stroke-width="1.1"/>`,
		"ko-KR": `<rect width="30" height="20" fill="#fff"/><path d="M15 5a5 5 0 0 1 0 10 2.5 2.5 0 0 1 0-5 2.5 2.5 0 0 0 0-5z" fill="#0047a0"/><path d="M15 5a2.5 2.5 0 0 1 0 5 2.5 2.5 0 0 0 0 5 5 5 0 0 1 0-10z" fill="#cd2e3a" transform="rotate(180 15 10)"/><g stroke="#000" stroke-width=".9"><path d="M4.5 3.5l3 2M4 4.5l3 2M3.5 5.5l3 2M22.5 13.5l3 2M22 14.5l3 2M21.5 15.5l3 2M22.5 6.5l3-2M22 5.5l3-2M4.5 16.5l3-2M4 15.5l3-2"/></g>`,
		"lt-LT": bands(["#fdb913", "#006a44", "#c1272d"]),
		"lv-LV": `<rect width="30" height="20" fill="#9e3039"/><rect y="8" width="30" height="4" fill="#fff"/>`,
		"nl-NL": bands(["#ae1c28", "#fff", "#21468b"]),
		"no-NO": nordic("#ba0c2f", "#00205b", "#fff"),
		"pt-PT": `<rect width="30" height="20" fill="#ff0000"/><rect width="12" height="20" fill="#006600"/><circle cx="12" cy="10" r="3.6" fill="#ffff00"/><circle cx="12" cy="10" r="2.2" fill="#ff0000" stroke="#fff" stroke-width=".7"/>`,
		"ro-RO": bands(["#002b7f", "#fcd116", "#ce1126"], true),
		"sk-SK": `${bands(["#fff", "#0b4ea2", "#ee1c25"])}<path d="M7 5h6v6.5a3.5 3.5 0 0 1-3 3.3 3.5 3.5 0 0 1-3-3.3z" fill="#ee1c25" stroke="#fff" stroke-width=".7"/><path d="M10 6.2v6.5M8.3 8h3.4M8.8 9.6h2.4" stroke="#fff" stroke-width=".8"/>`,
		"sr-CS": bands(["#c6363c", "#0c4076", "#fff"]),
		"sv-SE": nordic("#006aa7", "#fecc00"),
		"th-TH": `<rect width="30" height="20" fill="#a51931"/><rect y="3.33" width="30" height="13.34" fill="#f4f5f8"/><rect y="6.67" width="30" height="6.66" fill="#2d2a4a"/>`,
		"uk-UA": bands(["#0057b7", "#ffd700"]),
		"vi-VN": `<rect width="30" height="20" fill="#da251d"/><g fill="#ffff00">${star(15, 10.4, 5, 1.95)}</g>`,
		"zh-HK": `<rect width="30" height="20" fill="#de2910"/><g fill="#fff">${[0, 72, 144, 216, 288].map(a => `<ellipse cx="15" cy="6.6" rx="1.6" ry="3.2" transform="rotate(${a} 15 10)"/>`).join("")}</g><circle cx="15" cy="10" r=".8" fill="#de2910"/>`,
		"zh-TW": `<rect width="30" height="20" fill="#fe0000"/><rect width="15" height="10" fill="#000095"/><g fill="#fff">${star(7.5, 5, 3.6, 1.6, -90).replace("<polygon", "<polygon")}</g><circle cx="7.5" cy="5" r="1.9" fill="#000095"/><circle cx="7.5" cy="5" r="1.5" fill="#fff"/>`
	});

	window.flagSvg = code => `<svg class="flag" viewBox="0 0 30 20" aria-hidden="true">${window.FLAGS[code] || ""}</svg>`;
})();

// Filled brand marks (16×16 viewBox). GitHub mark from GitHub's Octicons (MIT).
window.FILLED_ICONS = {
	github: '<path d="M8 0c4.42 0 8 3.58 8 8a8.013 8.013 0 0 1-5.45 7.59c-.4.08-.55-.17-.55-.38 0-.27.01-1.13.01-2.2 0-.75-.25-1.23-.54-1.48 1.78-.2 3.65-.88 3.65-3.95 0-.88-.31-1.59-.82-2.15.08-.2.36-1.02-.08-2.12 0 0-.67-.22-2.2.82-.64-.18-1.32-.27-2-.27-.68 0-1.36.09-2 .27-1.53-1.03-2.2-.82-2.2-.82-.44 1.1-.16 1.92-.08 2.12-.51.56-.82 1.28-.82 2.15 0 3.06 1.86 3.75 3.64 3.95-.23.2-.44.55-.51 1.07-.46.21-1.61.55-2.33-.66-.15-.24-.6-.83-1.23-.82-.67.01-.27.38.01.53.34.19.73.9.82 1.13.16.45.68 1.31 2.69.94 0 .67.01 1.3.01 1.49 0 .21-.15.45-.55.38A7.995 7.995 0 0 1 0 8c0-4.42 3.58-8 8-8Z"/>'
};

window.renderIcons = (root = document) => {
	for (const el of root.querySelectorAll("i[data-icon]")) {
		const name = el.dataset.icon;
		const filled = window.FILLED_ICONS[name];
		const path = window.ICONS[name];
		if ((!filled && !path) || el.dataset.done) continue;
		el.dataset.done = "1";
		el.className = "ico";
		el.setAttribute("aria-hidden", "true");
		el.innerHTML = filled
			? `<svg viewBox="0 0 16 16" fill="currentColor">${filled}</svg>`
			: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">${path}</svg>`;
	}
};
