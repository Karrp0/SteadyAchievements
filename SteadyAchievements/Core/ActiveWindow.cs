using System;
using System.Globalization;

namespace SteadyAchievements.Core;

// Daily window "HH:mm-HH:mm" in local time. It may cross midnight; equal start and end means the whole day.
internal readonly record struct ActiveWindow(int StartMinute, int EndMinute) {
	internal const int MinutesInDay = 24 * 60;

	internal bool IsAllDay => StartMinute == EndMinute;

	internal int MinutesPerDay => IsAllDay ? MinutesInDay : (EndMinute - StartMinute + MinutesInDay) % MinutesInDay;

	internal bool Contains(int minuteOfDay) {
		if (IsAllDay) {
			return true;
		}

		return StartMinute < EndMinute ? (minuteOfDay >= StartMinute) && (minuteOfDay < EndMinute) : (minuteOfDay >= StartMinute) || (minuteOfDay < EndMinute);
	}

	internal bool Contains(DateTime localTime) => Contains((localTime.Hour * 60) + localTime.Minute);

	internal static bool TryParse(string? text, out ActiveWindow window) {
		window = default;

		if (string.IsNullOrWhiteSpace(text)) {
			return false;
		}

		string[] parts = text.Split('-');

		if ((parts.Length != 2) || !TryParseTime(parts[0], out int start) || !TryParseTime(parts[1], out int end)) {
			return false;
		}

		window = new ActiveWindow(start, end);

		return true;
	}

	private static bool TryParseTime(string text, out int minuteOfDay) {
		minuteOfDay = 0;
		string[] hm = text.Trim().Split(':');

		if ((hm.Length != 2) || (hm[0].Length is < 1 or > 2) || (hm[1].Length != 2)) {
			return false;
		}

		if (!int.TryParse(hm[0], NumberStyles.None, CultureInfo.InvariantCulture, out int hours) || !int.TryParse(hm[1], NumberStyles.None, CultureInfo.InvariantCulture, out int minutes)) {
			return false;
		}

		if (hours is < 0 or > 23 || minutes is < 0 or > 59) {
			return false;
		}

		minuteOfDay = (hours * 60) + minutes;

		return true;
	}
}
