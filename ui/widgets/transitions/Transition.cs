using System;
using System.Threading.Tasks;
using Godot;

namespace Project;

public partial class Transition : CanvasLayer {
    [Signal] public delegate void FinishedEventHandler();

    [ExportGroup("Local")]
    [Export] public required ColorRect Rect;
    [Export] public required Label Info;

    ShaderMaterial? material = null;
    Tween? tween = null;
    Action? callback = null;

    double elapsed = 0;

    public override void _Process(double delta) {
        if (Info.Visible && Info.Modulate.A > 0.1) {
            if (tween == null) Info.Modulate = Colors.Transparent;
            elapsed += delta;
            Info.Text = (elapsed % 0.2 == 0) ? "Loading.." : "Loading.";
        }
    }

    public void FadeIn(Action? finished = null, bool loading = false) =>
        Fade(TransitionType.FadeIn, finished, loading);
    public void FadeInAsync(Func<Task>? finished = null, bool loading = false) =>
        Fade(TransitionType.FadeIn, () => { if (finished != null) Task.Run(finished); }, loading);
    public void FadeOut(Action? finished = null) =>
        Fade(TransitionType.FadeOut, finished);

    void Fade(TransitionType type, Action? finished = null, bool loading = false) {
        float finalCutoff = type switch {
            TransitionType.FadeOut => 1.0f,
            TransitionType.FadeIn => 0.0f,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
        float finalPull = type switch {
            TransitionType.FadeOut => 0.0f,
            TransitionType.FadeIn => 1.0f,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
        var finalTextModulate = type switch {
            TransitionType.FadeOut => Colors.Transparent,
            TransitionType.FadeIn => Colors.White,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        elapsed = 0;
        Info.Visible = loading;
        material?.SetShaderParameter("shader_parameter/cutoff", 1.0f - finalCutoff);
        material?.SetShaderParameter("shader_parameter/pull", 1.0f - finalPull);

        const float duration = 0.3f;
        callback = finished;
        tween?.Kill();
        tween = CreateTween();
        tween.SetEase(Tween.EaseType.OutIn);
        tween.Parallel().TweenProperty(material, "shader_parameter/cutoff", finalCutoff, duration);
        tween.Parallel().TweenProperty(material, "shader_parameter/pull", finalPull * 0.05f, duration);
        tween.Parallel().TweenProperty(Info, CanvasItem.PropertyName.Modulate.ToString(), finalTextModulate, duration);
        tween.TweenCallback(Callable.From(() => {
            tween?.Kill();
            EmitSignalFinished();
            callback?.Invoke();
            callback = null;
        }));
    }

    public override void _EnterTree() {
        material = Rect.Material as ShaderMaterial;
        Info.Visible = false;
    }

    enum TransitionType {
        FadeOut,
        FadeIn
    }
}
