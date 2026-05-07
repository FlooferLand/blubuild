using System;
using Godot;

namespace Project;

public partial class Transition : CanvasLayer {
    [Signal] public delegate void FinishedEventHandler();

    [ExportGroup("Local")]
    [Export] public required ColorRect Rect;

    ShaderMaterial? material = null;
    Tween? tween = null;
    Action? callback = null;

    public void FadeIn(Action? finished = null) =>
        Fade(TransitionType.FadeOut, finished);
    public void FadeOut(Action? finished = null) =>
        Fade(TransitionType.FadeIn, finished);

    void Fade(TransitionType type, Action? finished = null) {
        float final = type switch {
            TransitionType.FadeIn => 1.0f,
            TransitionType.FadeOut => 0.0f
        };

        material?.SetShaderParameter("shader_parameter/cutoff", 1.0f - final);

        callback = finished;
        tween?.Kill();
        tween = CreateTween();
        tween.SetEase(Tween.EaseType.OutIn);
        tween.SetTrans(Tween.TransitionType.Quad);
        tween.TweenProperty(material, "shader_parameter/cutoff", final, 0.3f);
        tween.TweenCallback(Callable.From(() => {
            tween?.Kill();

            EmitSignalFinished();
            callback?.Invoke();
            callback = null;
        }));
    }

    public override void _EnterTree() {
        material = Rect.Material as ShaderMaterial;
    }

    enum TransitionType {
        FadeIn,
        FadeOut
    }
}
