using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using GodotUtils;

namespace Project;

// TODO: Add multiplayer support for bit charts.
//       Allow the server to register new ones, and apply them to the client on join.
public partial class BitChartRegistry : Node {
    [Signal] public delegate void UpdateEventHandler();

    public static BitChartRegistry Instance { get; private set; } = null!;

    public static Dictionary<string, BitChart> Charts { get; } = new();
    public static Dictionary<string, string> ExtensionToId { get; } = new();

    public BitChartRegistry() {
        Instance = this;
    }

    public override async void _EnterTree() {
        await RegisterDefault();
        EmitSignalUpdate();
    }

    public async ValueTask Clear() {
        Charts.Clear();
        ExtensionToId.Clear();
        await RegisterDefault();
    }

    /// Returns an error, if any
    public async ValueTask<string?> Register(string id, BitChart chart) {
        if (!Charts.TryAdd(id, chart)) return $"The chart '{id}' already exists";
        foreach (string ext in chart.ShowExtensions) ExtensionToId.Add(ext, id);
        Log.Info($"Registered bit chart '{id}'! ({chart.Fixtures.Count} fixtures)");
        EmitSignalUpdate();
        return null;
    }

    async Task RegisterDefault() {
        await Register("rae", BitChart.Load("res://assets/builtin/bitcharts/rae.csv", ["rshw"]).Unwrap());
    }
}