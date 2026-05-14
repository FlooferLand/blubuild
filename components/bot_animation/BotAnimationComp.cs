using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Project;

// TODO: Clean up this catastrophy
//       This is what working with AnimationTree does to one

/**
 * Replacement for AnimationPlayer that uses math to animate basic physics.
 * It also allows for stacking of animations
 */
public partial class BotAnimationComp : Node3D {
    record struct AnimState(BitData Data) {
        public bool Active = false;
        public float Weight = 0.0f;
    }

    [Export] public required AnimationPlayer AnimPlayer;
    [Export] public required Skeleton3D Skeleton;

    [ExportGroup("Local")]
    [Export] public required AudioStreamPlayer3D PneumaticFire;
    [Export] public required AudioStreamPlayer3D PneumaticRelease;

    public required CharacterFile File = null!;

    AnimationTree tree = null!;
    AnimationNodeBlendTree root = null!;
    Dictionary<string, AnimState> animStates = new();

    static string NodeAnimName(string movement) => $"anim_{movement}";
    static string NodeSpeedName(string movement) => $"speed_{movement}";
    static string NodeSeekName(string movement) => $"seek_{movement}";
    static string NodeAddName(string movement) => $"add_{movement}";

    public override void _Ready() {
        tree = new AnimationTree { AnimPlayer = AnimPlayer.GetPath() };
        AddChild(tree);

        root = new AnimationNodeBlendTree();
        tree.TreeRoot = root;
        tree.Active = true;

        AnimPlayer.SpeedScale = 0.0f;

        // States
        foreach (var (_, data) in File.BitData.BitsToData) {
            var state = new AnimState(data);
            animStates[data.Anim] = state;
        }

        // AnimationTree
        string firstAnim = AnimPlayer.GetAnimationList().FirstOrDefault("RESET");
        string prevName = NodeAnimName(firstAnim);
        foreach ((string name, int i) in AnimPlayer.GetAnimationList().Select((name, i) => (anim: name, i))) {
            string animName = NodeAnimName(name);
            var animNode = new AnimationNodeAnimation { Animation = name };
            root.AddNode(animName, animNode);

            string speedName = NodeSpeedName(name);
            var speedNode = new AnimationNodeTimeScale();
            root.AddNode(speedName, speedNode);
            tree.Set($"parameters/{speedName}/scale", 0.0);
            root.ConnectNode(speedName, 0, animName);

            string seekName = NodeSeekName(name);
            var seekNode = new AnimationNodeTimeSeek();
            root.AddNode(seekName, seekNode);
            tree.Set($"parameters/{seekName}/seek_request", 0.0);
            root.ConnectNode(seekName, 0, speedName);

            if (i == 0) {
                prevName = seekName;
            } else {
                string addName = NodeAddName(name);
                var addNode = new AnimationNodeAdd2();
                root.AddNode(addName, addNode);
                tree.Set($"parameters/{addName}/add_amount", 1.0);
                root.ConnectNode(addName, 0, prevName);
                root.ConnectNode(addName, 1, seekName);
                prevName = addName;
            }
        }
        root.ConnectNode("output", 0, prevName);
    }

    public override void _Process(double delta) {
        foreach ((string key, var state) in animStates) {
            var s = state;
            s.Weight = state.Active
                ? Mathf.Clamp(state.Weight + (float) delta * state.Data.Flows.In,  0f, 1f)
                : Mathf.Clamp(state.Weight - (float) delta * state.Data.Flows.Out, 0f, 1f);
            animStates[key] = s;
            tree.Set($"parameters/{NodeSeekName(key)}/seek_request", s.Weight);
        }
    }

    public void SetBit(MappedBit bit, bool on) {
        if (!File.BitData.BitsToData.TryGetValue(bit, out var data)) return;
        if (!animStates.TryGetValue(data.Anim, out var state)) return;
        state.Active = on;
        animStates[data.Anim] = state;
    }
}