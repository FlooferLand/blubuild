using Godot;
using System;

namespace Project;

public partial class TitleWidget : PanelContainer {
    [Export] public required BeatTimer BeatTimer;

    [ExportGroup("Local")]
    [Export] public required Control TitleScale;

    float titleZoom = 0f;

    public override void _Ready() {
        BeatTimer.Step += step => {
            if (step % 2 == 0) titleZoom = 1f;
        };
    }

    public override void _Process(double delta) {
        TitleScale.Scale = Vector2.One + (Vector2.One * titleZoom) * 0.2f;
        titleZoom = Mathf.Lerp(titleZoom, 0f, 1.5f * (float) delta);
    }
}
