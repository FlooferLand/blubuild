using BenchmarkDotNet.Attributes;
using Bluchalk.shows;

namespace Blutest;

[MemoryDiagnoser]
public class RshwPerformance {
    readonly RshwFormat format = new();
    readonly List<string> showPaths = [];

    [GlobalSetup]
    public void Setup() {
        bool oneFileFlag = Environment.GetCommandLineArgs().Contains("--one");
        string showDir = Environment.GetEnvironmentVariable("TEST_SHOW_DIR")!;
        var bigShows = new DirectoryInfo(showDir)
            .EnumerateFiles("*.rshw", new EnumerationOptions { IgnoreInaccessible = true })
            .OrderByDescending(f => f.Length)
            .Take(oneFileFlag ? 1 : 5)
            .Select(f => f.FullName);
        showPaths.Clear();
        showPaths.AddRange(bigShows);
    }

    [Benchmark]
    public void Read() {
        foreach (string showPath in showPaths) {
            var data = format.ReadFile(showPath);
            if (data.LetErr(out string err)) {
                throw new Exception($"Error reading rshw: {err}");
            }
        }
    }
}