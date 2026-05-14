using System.Globalization;
using Godot;

namespace Project;

[Tool]
public partial class NumberEdit : Container {
    [Signal] public delegate void ValueChangedEventHandler(float value);

    [Export] public float Value {
        get;
        set {
            float val = Mathf.Clamp(value, MinValue, MaxValue);
            field = val;
            if (IsInsideTree() && Slider != null && LineEdit != null) {
                Slider.SetValueNoSignal(val);
                LineEdit.Text = val.ToString("0.0#");
                EmitSignalValueChanged(Value);
            }
        }
    } = 0.0f;
    [Export] public float MinValue {
        get;
        set {
            field = value;
            if (IsInsideTree() && Slider != null) {
                Slider.SetMin(value);
            }
        }
    } = 0.0f;
    [Export] public float MaxValue {
        get;
        set {
            field = value;
            if (IsInsideTree() && Slider != null) {
                Slider.SetMax(value);
            }
        }
    } = 1.0f;

    [ExportGroup("Local")]
    [Export] public LineEdit? LineEdit;
    [Export] public Slider? Slider;

    public const float DragStartDistance = 3f;
    public const float DragSensitivity = 0.05f;

    bool dragging = false;
    Vector2 clickPos = Vector2.Zero;

    public override void _Ready() {
        if (Engine.IsEditorHint()) return;
        Slider?.SetValueNoSignal(Value);
        Slider?.MinValue = MinValue;
        Slider?.MaxValue = MaxValue;

        LineEdit?.GuiInput += LineEditGuiInput;
        LineEdit?.TextSubmitted += text => {
            Value = float.TryParse(text, out float value) ? value : 0f;
        };
        Slider?.ValueChanged += value => {
            Value = (float) value;
        };
    }

    void LineEditGuiInput(InputEvent @event) {
        switch (@event) {
            case InputEventMouseButton { ButtonIndex: MouseButton.Left } button:
                switch (button.Pressed) {
                    case true:
                        dragging = false;
                        clickPos = GetViewport().GetMousePosition();
                        break;
                    case false:
                        if (!dragging) {
                            LineEdit?.Edit();
                        } else {
                            Input.MouseMode = Input.MouseModeEnum.Visible;
                            GetViewport().WarpMouse(clickPos);
                        }
                        dragging = false;
                        break;
                }
                break;
            case InputEventMouseMotion motion when Input.IsMouseButtonPressed(MouseButton.Left):
                switch (dragging) {
                    case false when motion.Position.DistanceTo(clickPos) > DragStartDistance:
                        dragging = true;
                        Input.MouseMode = Input.MouseModeEnum.Captured;
                        break;
                    case true:
                        Value += motion.Relative.X * DragSensitivity;
                        break;
                }
                break;
        }
    }
}
