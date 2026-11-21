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
