namespace GodotUtils;
using Godot;

/// Prints debug stuff, similar to Unreal Engine
/// Automatically registers itself as an autoload
public partial class DebugPrinter : Node {
	public static DebugPrinter? SelfCached = null;

	int addsThisFrame = 0;
	Dictionary<string, Label> tracked = new();
	CanvasLayer canvasLayer = new();
	VBoxContainer container = new();

	public override void _Ready() {
		// Creating the container
		container.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		container.MouseFilter = Control.MouseFilterEnum.Ignore;

		// Creating the canvas layer
		canvasLayer.Layer = 1000;
			
		// Adding to the scene
		canvasLayer.AddChild(container);
		AddChild(canvasLayer);
	}

	public override void _Process(double delta) {
		addsThisFrame = 0;
		if (tracked.Count > 20) {
			tracked.Clear();
		}
	}

	public static void Track(Node parent, string messageId, object obj, string tag="") {
		if (!OS.IsDebugBuild()) return;
		var debugPrinter = GetObject();
		string id = $"{(parent.GetScript().Obj as Script)?.GetGlobalName()??parent.GetName()}_{messageId}";
		string tagString = tag != "" ? $"{tag}: " : "";
		string value = $"{tagString}{obj}";
		
		if (debugPrinter.tracked.TryGetValue(id, out var label)) {
			// Updating the existing label
			label.Text = value;
		} else {
			// Adding a new label
			var created = CreateLabel(value, Colors.LightSeaGreen);
			debugPrinter.container.AddChild(created);
			debugPrinter.container.MoveChild(created, debugPrinter.tracked.Count);
			debugPrinter.tracked.Add(id, created);
		}
	}

	/// More convenient than writing the full thing for short temporary stuff
	public static void TrackTemp(Node parent, object obj, string tag="") {
		Track(parent, $"{parent.GetInstanceId()}_{obj.GetType().FullName}", obj, tag);
	}
	
	/// More convenient than writing the full thing; Used to test if a function gets called or not
	public static void TrackTest(Node parent, string messageId) {
		Track(parent, $"test_{messageId}", $"Got called at: {Time.GetTicksMsec() / 1000f}");
	}

	public static void Print(object obj, float time=1.0f) {
		if (!OS.IsDebugBuild()) return;

		var debugPrinter = GetObject();
		if (debugPrinter.addsThisFrame > 20) return;
		
		// Label
		var label = CreateLabel(obj.ToString() ?? "null", Colors.White);
		
		// Tween
		var tween = debugPrinter.CreateTween();
		tween.BindNode(label);
		tween.TweenProperty(label, "modulate", Colors.Gray, time);
		tween.TweenCallback(Callable.From(() => {
			label.QueueFree();
		}));
		
		// Adding the new message
		debugPrinter.container.AddChild(label);
		debugPrinter.addsThisFrame += 1;
	}

	public static Label CreateLabel(string text, Color fontColor) {
		var created = new Label();
		created.Text = text;
		created.AddThemeColorOverride("font_color", fontColor);
		created.AddThemeColorOverride("font_outline_color", Colors.Black);
		created.AddThemeConstantOverride("outline_size", 10);
		created.AddThemeFontSizeOverride("font_size", 24);
		return created;
	}
	
	public static DebugPrinter GetObject() {
		var mainLoop = (SceneTree) Engine.GetMainLoop();
		var root = mainLoop.Root;
		string nodeName = nameof(DebugPrinter);

		if (SelfCached == null) {
			SelfCached = new DebugPrinter();
			SelfCached.Name = nodeName;
			root.CallDeferred(Node.MethodName.AddChild, SelfCached, true);
		}
		return SelfCached;
	}
}
