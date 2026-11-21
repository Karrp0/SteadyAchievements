using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using SteadyAchievements.Core;

namespace SteadyAchievements.Storage;

// Enums are stored by name. System.Text.Json's JsonStringEnumConverter is not available in ASF's trimmed
// builds, so these small converters do the same for the plugin's own enums (and still read numbers).
internal sealed class StepStateConverter : JsonConverter<StepState> {
	public override StepState Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => (StepState) EnumText.Read(ref reader, ["Pending", "Done", "Simulated", "Skipped"]);

	public override void Write(Utf8JsonWriter writer, StepState value, JsonSerializerOptions options) {
		ArgumentNullException.ThrowIfNull(writer);
		writer.WriteStringValue(value switch {
			StepState.Done => "Done",
			StepState.Simulated => "Simulated",
			StepState.Skipped => "Skipped",
			_ => "Pending"
		});
	}
}

internal sealed class HistoryKindConverter : JsonConverter<HistoryKind> {
	public override HistoryKind Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => (HistoryKind) EnumText.Read(ref reader, ["Unlocked", "Simulated", "Failed", "Skipped", "Completed"]);

	public override void Write(Utf8JsonWriter writer, HistoryKind value, JsonSerializerOptions options) {
		ArgumentNullException.ThrowIfNull(writer);
		writer.WriteStringValue(value switch {
			HistoryKind.Simulated => "Simulated",
			HistoryKind.Failed => "Failed",
			HistoryKind.Skipped => "Skipped",
			HistoryKind.Completed => "Completed",
			_ => "Unlocked"
		});
	}
}

internal static class EnumText {
	// Index of the name in names, or the number as written; unknown text reads as the first value
	internal static int Read(ref Utf8JsonReader reader, string[] names) {
		if (reader.TokenType == JsonTokenType.Number) {
			return reader.GetInt32();
		}

		string? text = reader.GetString();

		for (int i = 0; i < names.Length; i++) {
			if (string.Equals(names[i], text, StringComparison.OrdinalIgnoreCase)) {
				return i;
			}
		}

		return 0;
	}
}
