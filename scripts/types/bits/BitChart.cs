using System;
using System.Collections.Generic;
using Godot;
using GodotUtils;

namespace Project;

public readonly record struct BitChart(Dictionary<string, BitChart.Fixture> Fixtures) {
    public readonly record struct Fixture(Dictionary<Bit, string> ActionNames);

    public static Result<BitChart> Load(string path) {
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        if (file == null) return Result.Err($"Failed to read bitchart at '{path}': {FileAccess.GetOpenError()}");

        // Parsing the CSV
        (var csv, string? err) = CsvData.ReadFromFile(file);
        file.Close();
        if (err != null) return Result.Err(err);
        if (csv.Headers.Length < 3) return Result.Err("CSV doesn't contain all necessary fields (Bit G#, Fixture, Action)");

        // Parsing the header
        int bitIndex = GetId("bit", 0);
        int fixtureIndex = GetId("fixture", 1);
        int actionIndex = GetId("action", 2);

        // Processing
        var fixtures = new Dictionary<string, Fixture>();
        foreach (string[] entry in csv.Entries) {
            if (!short.TryParse(entry[bitIndex], out short bitId)) continue;

            string fixtureName = entry.Length > fixtureIndex ? entry[fixtureIndex] : "";
            string actionName = entry.Length > actionIndex ? entry[actionIndex] : "";

            if (!fixtures.TryGetValue(fixtureName, out var fixture)) {
                fixture = new Fixture(new Dictionary<Bit, string>());
                fixtures[fixtureName] = fixture;
            }
            fixture.ActionNames[bitId] = actionName;
        }

        return Result.Ok(new BitChart(fixtures));
        int GetId(string match, int @default) =>
            Array.FindIndex(csv.Headers, h => h.Contains(match, StringComparison.CurrentCultureIgnoreCase)) is var i && i != -1 ? i : @default;
    }
}