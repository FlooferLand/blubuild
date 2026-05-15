using Godot;

namespace Project;

public partial class GraphicsAutoload : Node {
    public static GraphicsAutoload Instance { get; private set; } = null!;
    public GraphicsAutoload() {
        Instance = this;
    }

    [Signal] public delegate void UpdateViewportEventHandler();
    [Signal] public delegate void ChangeEventHandler(Environment env);
    public static Presets Preset = Presets.Good;
    public enum Presets { Good, Bad, Shit }

    public static Environment? ActiveWorldEnv = null;
    public static Environment? ShitWorldEnv = null;
    public static Environment? BadWorldEnv = null;
    public static Environment? GoodWorldEnv = null;

    public override void _EnterTree() {
        ShitWorldEnv = GD.Load<Environment>("uid://cxuluoiw8vor8");
        BadWorldEnv = GD.Load<Environment>("uid://c3j0plr6u7bxi");
        GoodWorldEnv = GD.Load<Environment>("uid://c43b6hm83w2gs");
    }

    public static void SetupViewport(Viewport viewport) {
        switch (Preset) {
            case Presets.Good:
                viewport.Msaa3D = Viewport.Msaa.Msaa2X;
                viewport.Scaling3DScale = 1.0f;
                break;
            case Presets.Bad:
                viewport.Msaa3D = Viewport.Msaa.Msaa2X;
                viewport.Scaling3DScale = 0.6f;
                break;
            case Presets.Shit:
                viewport.Msaa3D = Viewport.Msaa.Disabled;
                viewport.Scaling3DScale = 0.2f;
                break;
        }
    }

    public static void SetPreset(Presets preset) {
        Preset = preset;
        switch (preset) {
            case Presets.Good:
                ActiveWorldEnv = GoodWorldEnv;
                break;
            case Presets.Bad:
                ActiveWorldEnv = BadWorldEnv;
                break;
            case Presets.Shit:
                ActiveWorldEnv = ShitWorldEnv;
                break;
        }
        Instance.EmitSignalChange(ActiveWorldEnv);
        Instance.EmitSignalUpdateViewport();
    }
}