using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace SteadyAchievements.Core;

// Local time for a bot: its TimeZone setting (IANA id) when the runtime supports it, the server's time otherwise.
//
// ASF's trimmed OS-specific builds have no usable TimeZoneInfo (lookups are removed and on Windows the type
// does not even load), so TimeZoneInfo appears only inside the small non-inlined methods at the bottom.
// Their failure switches to the server's UTC offset, which comes from DateTime.Now - DateTime.UtcNow
// (DateTime.Now works everywhere; ASF's own logging relies on it).
internal static class Clock {
	private static bool ZonesUnavailable;
	private static bool LocalConversionUnavailable;

	internal static bool IsValidTimeZone(string? id) {
		if (id == null) {
			return true;
		}

		if (ZonesUnavailable) {
			return false;
		}

		try {
			return ZoneExists(id);
		} catch (Exception) {
			ZonesUnavailable = true;

			return false;
		}
	}

	internal static DateTime ToLocal(DateTime utc, string? timeZone) {
		DateTime value = DateTime.SpecifyKind(utc, DateTimeKind.Utc);

		if ((timeZone != null) && !ZonesUnavailable) {
			try {
				if (ConvertWithZone(value, timeZone) is DateTime converted) {
					return converted;
				}
			} catch (Exception) {
				ZonesUnavailable = true;
			}
		}

		return ServerLocal(value);
	}

	internal static string Describe(string? timeZone) {
		if (timeZone != null) {
			return timeZone;
		}

		if (!LocalConversionUnavailable) {
			try {
				return LocalZoneId();
			} catch (Exception) {
				LocalConversionUnavailable = true;
			}
		}

		TimeSpan offset = ServerOffset();

		return string.Create(CultureInfo.InvariantCulture, $"UTC{(offset < TimeSpan.Zero ? "-" : "+")}{offset.Duration():hh\\:mm}");
	}

	// "yyyy-MM-dd" of the local day, used to count unlocks per day
	internal static string DayKey(DateTime local) => local.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

	private static DateTime ServerLocal(DateTime utc) {
		if (!LocalConversionUnavailable) {
			try {
				return ToLocalCore(utc);
			} catch (Exception) {
				LocalConversionUnavailable = true;
			}
		}

		return DateTime.SpecifyKind(utc + ServerOffset(), DateTimeKind.Local);
	}

	// The server's current UTC offset, rounded to 15 minutes
	private static TimeSpan ServerOffset() => TimeSpan.FromMinutes(Math.Round((DateTime.Now - DateTime.UtcNow).TotalMinutes / 15) * 15);

	// ---- the only places that touch TimeZoneInfo

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static DateTime ToLocalCore(DateTime utc) => utc.ToLocalTime();

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static string LocalZoneId() => TimeZoneInfo.Local.Id;

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static bool ZoneExists(string id) {
		try {
			_ = TimeZoneInfo.FindSystemTimeZoneById(id);

			return true;
		} catch (Exception) {
			// Unknown id (TimeZoneNotFoundException) or a broken zone file
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static DateTime? ConvertWithZone(DateTime utc, string id) {
		TimeZoneInfo zone;

		try {
			zone = TimeZoneInfo.FindSystemTimeZoneById(id);
		} catch (Exception) {
			return null;
		}

		return TimeZoneInfo.ConvertTimeFromUtc(utc, zone);
	}
}
