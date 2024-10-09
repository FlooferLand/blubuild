using Godot;
using GodotUtils;
using FileAccess = Godot.FileAccess;

namespace Project;

/** Handles low-level system stuff */
public partial class SystemAutoload : Node {
	public static SystemAutoload Instance { get; private set; }
	public static readonly ArgumentParser Args = new(OS.GetCmdlineArgs());
	public bool IsCommandlineOnly {
		private set {
			RenderingServer.Singleton?.SetRenderLoopEnabled(!value);
			if (GetWindow() is {} window) {
				window.Mode = Window.ModeEnum.Minimized;
				window.Unfocusable = value;
				window.MinSize = Vector2I.One;
				window.Borderless = value;
				
				// Position and size
				window.Position = Vector2I.One;
				window.Size = Vector2I.One;
			}
		}
		get => !RenderingServer.Singleton.RenderLoopEnabled;
	}
	public static string TitleNetworkingType;

	public override void _EnterTree() {
		Instance = this;
		Log.Configure(ConfigureLogging);
		
		// Bluchalk
		if (Args.IsSubcommand("chalk")) {
			IsCommandlineOnly = true;
		}
		
		// Command line exit
		if (IsCommandlineOnly) {
			GetTree().Quit();
		}

		if (OS.IsDebugBuild()) {
			DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Disabled);
			Engine.MaxFps = 100;
		}
	}

	private void ConfigureLogging(out string stabilityWarning) {
		stabilityWarning = "The software may be unstable beyond this point." + "\n" +
                            "" +
                            "Please save any of your work (show tapes, etc) as a different file" + " " +
                            "in case it corrupts the save, then re-launch the program."  + "\n" +
                            "" +
                            "You may continue running the program at your own risk.";
	}

	public override void _Notification(int what) {
		if (what == NotificationCrash) {
			HandleCrash();
		}
	}

	public void HandleCrash() {
		// Reading the crash info
		string log = null;
		if (FileAccess.Open("user://godot.log", FileAccess.ModeFlags.Read) is {} file) {
			log = file.GetPascalString();
			file.Close();
		}

		if (log is {} l)
			Log.FatalError($"Oh no, Blubuild has crashed!\nReason:\n{l}");
		else
			Log.FatalError($"Oh no, Blubuild has crashed for an unknown reason!\nPlease check \"{OS.GetUserDataDir()}\" for a crash log for more info.");
	}

	public string GetWindowTitle() {
		return $"Blubuild v{ProjectSettings.GetSetting("application/config/version")}";
	}
	
	public override void _Process(double delta) {
		GetWindow().Title = $"{GetWindowTitle()} ({TitleNetworkingType})  |  {Engine.GetFramesPerSecond()} FPS";
	}
}
