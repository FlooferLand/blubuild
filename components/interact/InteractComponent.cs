using Godot;
using GodotUtils;

namespace Project.Components;

[Tool]
public partial class InteractComponent : Node3D {
    [Export] public required Node3D Listener;

    [Export(PropertyHint.Range, "0.1,5.0,0.005")]
    public Vector3 Size {
        get;
        set {
            field = value;
            CallDeferred(nameof(UpdateShape));
        }
    } = Vector3.Zero;

    [ExportGroup("Local")]
    [Export] public required InteractComponentInner Inner;
    [Export] public required CollisionShape3D Collision;

    public override void _EnterTree() {
        UpdateShape();
        VisibilityChanged += () => {
            Collision.Disabled = !Visible;
        };
    }

    void UpdateShape() {
        if (Size.IsZeroApprox()) {
            Log.Warning($"{nameof(InteractComponent)} on {Listener.Name} has a size of zero");
        }
        if (Collision.Shape is BoxShape3D shape) {
            shape.SetSize(Size);
        }
    }
}
