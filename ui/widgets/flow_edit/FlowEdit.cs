using Godot;

namespace Project;

public partial class FlowEdit : PanelContainer {
    [Signal] public delegate void ChangedEventHandler();

    [ExportGroup("Local")]
    [Export] public required NumberEdit InEdit;
    [Export] public required NumberEdit OutEdit;

    public Flow Value {
        get;
        set {
            field = value;
            InEdit.Value = value.In;
            OutEdit.Value = value.Out;
        }
    } = new();

    public override void _EnterTree() {
        InEdit.Value = Value.In;
        InEdit.ValueChanged += v => {
            Value.In = v;
            EmitSignalChanged();
        };

        OutEdit.Value = Value.Out;
        OutEdit.ValueChanged += v => {
            Value.Out = v;
            EmitSignalChanged();
        };
    }
}
