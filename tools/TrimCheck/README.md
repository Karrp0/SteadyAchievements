# TrimCheck

ASF's OS-specific builds (`win-x64`, `linux-x64`, …) are trimmed: .NET members ASF itself never uses are
removed. A plugin calling such a member only fails when that code path runs. TrimCheck lists every
type and member `SteadyAchievements.dll` references that a trimmed ASF does not contain.

```sh
# 1. Extract the assemblies of a trimmed ASF build (single-file bundle)
dotnet tool install --tool-path ./tools-bin sfextract
./tools-bin/sfextract ASF-win-x64/ArchiSteamFarm.exe -o extracted

# 2. Compare
dotnet run --project tools/TrimCheck -c Release -- SteadyAchievements/bin/Release/net10.0/SteadyAchievements.dll extracted
```

Expected output: only `TimeZoneInfo::FindSystemTimeZoneById` and `TimeZoneInfo::ConvertTimeFromUtc`
(they are isolated in `Core/Clock.cs` and fall back to the server's time), plus the plugin's own
generic helper types. Anything else must be replaced.

Static analysis does not catch everything: an exception type in a `catch … when (e is X)` filter that
does not load makes the whole method fail to compile. For that, start a trimmed ASF with the plugin and
the environment variable `STEADY_SELFTEST=1`; the log shows `[Steady self-test] done: N/N passed`.
