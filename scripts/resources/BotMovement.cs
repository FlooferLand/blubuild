using Godot;

namespace Project;

[GlobalClass]
public partial class BotMovement : Resource {
    [Export] public string AnimName = "";
    [Export] public float FlowIn = 1.0f;
    [Export] public float FlowOut = 1.0f;
}
