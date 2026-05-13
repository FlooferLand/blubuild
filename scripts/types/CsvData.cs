using System.Collections.Generic;
using Godot;
using GodotUtils;

namespace Project;

public readonly record struct CsvData(string[] Headers, List<string[]> Entries) {
    public static Result<CsvData> ReadFromFile(FileAccess file, string delim = ",") {
        string[] headers = file.GetCsvLine();
        if (headers.IsEmpty()) return Result.Err("CSV header wasn't found");

        var entries = new List<string[]>();
        while (!file.EofReached()) {
            entries.Add(file.GetCsvLine());
        }
        file.Close();
        if (entries.Count == 0) return Result.Err("CSV body wasn't found");

        return Result.Ok(new CsvData(headers, entries));
    }
}