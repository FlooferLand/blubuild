using Godot;
using Godot.Collections;

namespace Project;

[GlobalClass]
public partial class BotData : Resource {
	[Export] public required string BotName;
    [Export] public Dictionary<int, BotMovement> BitMapping = new();
}
