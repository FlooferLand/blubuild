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
    struct AnimState {
        public bool Active = false;
        public float Weight = 0.0f;
        public BotMovement Movement = null!;
        public AnimState(BotMovement movement) {
            Movement = movement;
        }
    }

    [Export] public required AnimationPlayer AnimPlayer;
    [Export] public required Skeleton3D Skeleton;
    [Export] public required BotData BotData;

    [ExportGroup("Local")]
    [Export] public required AudioStreamPlayer3D PneumaticFire;
    [Export] public required AudioStreamPlayer3D PneumaticRelease;

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
        foreach ((int bitId, var movement) in BotData.BitMapping) {
            var state = new AnimState(movement);
            animStates[movement.AnimName] = state;
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
                ? Mathf.Clamp(state.Weight + (float) delta * state.Movement.FlowIn,  0f, 1f)
                : Mathf.Clamp(state.Weight - (float) delta * state.Movement.FlowOut, 0f, 1f);
            animStates[key] = s;
            tree.Set($"parameters/{NodeSeekName(key)}/seek_request", s.Weight);
        }
    }

    public void SetBit(int bitId, bool on) {
        if (!BotData.BitMapping.TryGetValue(bitId, out var movement)) return;
        if (!animStates.TryGetValue(movement.AnimName, out var state)) return;
        state.Active = on;
        animStates[movement.AnimName] = state;
    }
}