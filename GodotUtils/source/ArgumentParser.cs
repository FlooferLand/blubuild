using Project;

namespace GodotUtils;

public class ArgumentParser {
    public static Dictionary<string, Values.Base> Arguments = new();

    // Argument value types
    public static class Values {
        public record Base;
        public record Subcommand : Base;
        public record Flag : Base;
        public record BoolFlag : Flag;
        public record ValueFlag<T>(T Value) : Flag;
    }

    public ArgumentParser(string[] args) {
        foreach (string entry in args) {
            string[] equalSplit = entry.Split(new [] {'='}, 2);
            if (entry.StartsWith("--") && equalSplit.Length > 0) {
                string name = equalSplit[0].Replace("-", "");
                string value = equalSplit.Length > 1 ? equalSplit[1] : "";
                if (value.Split(',') is { Length: > 1 } values) {
                    Arguments.TryAdd(name, new Values.ValueFlag<string[]>(values));
                } else {
                    if (value is "true" or "1b" or "") {
                        Arguments.TryAdd(name, new Values.BoolFlag());
                    } else if (int.TryParse(value, out int i)) {
                        Arguments.TryAdd(name, new Values.ValueFlag<Int32>(i));
                    } else {
                        Arguments.TryAdd(name, new Values.ValueFlag<String>(value));
                    }
                }
            } else if (!entry.StartsWith("-") && equalSplit.Length == 1) {
                Arguments.TryAdd(entry, new Values.Subcommand());
            }
        }
    }

    public bool IsSubcommand(string name) {
        return Arguments.TryGetValue(name, out var basic) && basic is Values.Subcommand;
    }

    public Option<T> GetValueFlag<T>(string name) {
        if (Arguments.TryGetValue(name, out var basic) && basic is Values.ValueFlag<T> value)
            return Option<T>.Some(value.Value);
        return Option<T>.None();
    }

    public bool GetBoolFlag(string name) {
        return Arguments.TryGetValue(name, out var basic) && basic is Values.BoolFlag;
    }
}