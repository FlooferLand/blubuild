using System.Threading.Tasks;
using Godot;
using GodotUtils;

namespace Project.Components;

[Tool]
public partial class InteractComponentInner : Area3D, IPlayerInteractable {
    [Export] public required InteractComponent Outer;

    public override void _Ready() {
        if (Outer.Listener is not IPlayerInteractable) {
            Log.Warning($"Listener '{Outer.Listener.Name}' is not {nameof(IPlayerInteractable)}");
        }
    }

    public async ValueTask Interact(InteractContext ctx) {
        if (Outer.Listener is not IPlayerInteractable interactable) return;
        await interactable.Interact(ctx);
    }
}
