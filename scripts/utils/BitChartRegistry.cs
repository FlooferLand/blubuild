using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using GodotUtils;

namespace Project;

// TODO: Add multiplayer support for bit charts.
//       Allow the server to register new ones, and apply them to the client on join.
public partial class BitChartRegistry : Node {
    [Signal] public delegate void UpdateEventHandler();

    public static BitChartRegistry Instance { get; private set; } = null!;

    public Dictionary<string, BitChart> Charts { get; private set; } = new();

    public BitChartRegistry() {
        Instance = this;
    }

    public override async void _EnterTree() {
        await RegisterDefault();
        EmitSignalUpdate();
    }

    public async Task Clear() {
        Charts.Clear();
        await RegisterDefault();
    }

    /// Returns an error, if any
    public async Task<string?> Register(string id, BitChart chart) {
        if (!Charts.TryAdd(id, chart)) return $"The chart '{id}' already exists";
        Log.Info($"Registered bit chart '{id}'! ({chart.Fixtures.Count} fixtures)");
        EmitSignalUpdate();
        return null;
    }

    async Task RegisterDefault() {
        await Register("rae", BitChart.Load("res://assets/builtin/bitcharts/rae.csv").Unwrap());
    }
}