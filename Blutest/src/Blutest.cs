using BenchmarkDotNet.Running;

namespace Blutest;

public static class Entry {
    static void Main() {
        {
            var rshw = new RshwPerformance();
            rshw.Setup();
            rshw.Read();
        }

        if (Environment.GetCommandLineArgs().Contains("--benchmark")) {
            Console.WriteLine("Running benchmark");
            BenchmarkRunner.Run<RshwPerformance>();
        }
    }
}