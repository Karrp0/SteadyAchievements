using System;

namespace SteadyAchievements.Core;

// Local time for a bot: its TimeZone setting (IANA id) when the runtime knows it, the server's zone otherwise.
internal static class Clock {
	internal static bool IsValidTimeZone(string? id) => (id == null) || (Find(id) != null);

	internal static DateTime ToLocal(DateTime utc, string? timeZone) {
		TimeZoneInfo zone = (timeZone != null ? Find(timeZone) : null) ?? TimeZoneInfo.Local;

		return TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), zone);
	}

	internal static string Describe(string? timeZone) => timeZone ?? TimeZoneInfo.Local.Id;

	// "yyyy-MM-dd" of the local day, used to count unlocks per day
	internal static string DayKey(DateTime local) => local.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);

	private static TimeZoneInfo? Find(string id) {
		try {
			return TimeZoneInfo.FindSystemTimeZoneById(id);
		} catch (Exception e) when (e is TimeZoneNotFoundException or InvalidTimeZoneException or ArgumentException) {
			return null;
		}
	}
}
