using Godot;
using Godot.Collections;
using GodotUtils;

namespace Project;

public partial class MainMenu : Control {
    [Export] public required BeatTimer BeatTimer;
    [Export] public required AudioStreamPlayer ThemeSongPlayer;
    [Export] public required AnimationPlayer AnimationPlayer;
    [Export] public required Camera3D ViewportCamera;
    [Export] public required Array<Node3D> Bots { get; set; }

    float initialCamY = 0f;

    public override void _Ready() {
        initialCamY = ViewportCamera.Position.Y;
        AnimationPlayer.Play("intro");
        ThemeSongPlayer.VolumeDb = -6;
        BeatTimer.Beat += _ => PlayRandomAnimation();
    }

    public override void _Process(double delta) {
        ViewportCamera.Position = ViewportCamera.Position.WithY(
            initialCamY + (0.5 - Mathf.Sin(Time.GetTicksMsec() * 0.0005)) * 0.1
        );
    }

    void PlayRandomAnimation() {
        var bot = Bots.PickRandom();
        if (bot.GetNodeOrNull<AnimationPlayer>(nameof(AnimationPlayer)) is { } animPlayer) {
            string[] anims = animPlayer.GetAnimationList();
            string anim = anims[GD.RandRange(0, anims.Length - 1)];
            animPlayer.Play(anim, 0.5);
        }
    }

    // Called by the animation
    public void IntroContinueSong() {
        ThemeSongPlayer.VolumeDb = 0;
        ThemeSongPlayer.Play((float)AnimationPlayer.CurrentAnimationPosition);
    }
}
