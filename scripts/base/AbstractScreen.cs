using Godot;

namespace Project;

public abstract partial class AbstractScreen : Control {
    public AbstractScreen? Parent = null;

    protected abstract void OnScreenOpen();
    protected abstract void OnScreenRemove();

    public override void _EnterTree() {
        if (!Multiplayer.IsClient()) {
            SetProcess(false);
            SetPhysicsProcess(false);
            SetProcessInput(false);
        }
    }

    public bool ScreenIsActive() => BlubuildClient.CurrentScreen == this;

    public void ScreenToggle() {
        if (ScreenIsActive())
            ScreenDeactivate();
        else
            ScreenActivate();
    }

    public void ScreenActivate() {
        BlubuildClient.CurrentScreen?.ScreenDeactivate();
        BlubuildClient.CurrentScreen = this;
        OnScreenOpen();
    }

    public void ScreenDeactivate() {
        Parent?.ScreenActivate();
        BlubuildClient.CurrentScreen = Parent;
        OnScreenRemove();
    }
}