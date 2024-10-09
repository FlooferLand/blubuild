using Godot;
namespace GodotUtils;

public static class Log {
	private static string stabilityWarning = "The game may be unstable from now on.";
	
	public delegate void LogConfigurator(out string stabilityWarning);
	public static void Configure(LogConfigurator configurator) {
		configurator.Invoke(out string warning);
		stabilityWarning = warning;
	}
	
	public static void Debug(params object?[] what) {
		#if DEBUG
		GD.Print(what);
		#endif
	}
	
	public static void Info(string message) {
		GD.Print(message);
	}
	
	public static void Warning(string message) {
		GD.PushWarning(message);
	}

	public static void Error(Error error) {
		Error($"Internal name: {error.ToString()}");
	}
	
	public static void Error(string reason) {
		GD.PushError(reason);
		OS.Alert(reason, "Error!");
	}
	
	public static void FatalError(string reason) {
		Input.MouseMode = Input.MouseModeEnum.Visible;

		var tree = (SceneTree) Engine.GetMainLoop();
		var dialog = new ConfirmationDialog();
		dialog.Title = "Fatal error!";
		dialog.DialogText = $"Reason: {reason}\n\n{stabilityWarning}";
		dialog.CancelButtonText = "Quit";
		dialog.OkButtonText = "Continue regardless";
		dialog.Transient = true;
		dialog.GetOkButton().Pressed += () => Warning("Advice ignored. Continuing the game in an unstable state");
		dialog.GetCancelButton().Pressed += () => tree.Quit(-1);
		tree.Root.CallDeferred(Node.MethodName.AddChild, dialog);
		dialog.CallDeferred(Window.MethodName.PopupCentered);
		dialog.Show();
	}
}
