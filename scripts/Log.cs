namespace Project;
using Godot;

public static class Log {
	public static void Debug(params object[] what) {
		GD.Print(what);
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
		// GD.PushError(reason);

		var tree = (SceneTree) Engine.GetMainLoop();
		const string stabilityWarning = "The software may be unstable beyond this point." + "\n" +
		                                "" +
		                                "Please save any of your work (show tapes, etc) as a different file" + " " +
		                                "in case it corrupts the save, then re-launch the program."  + "\n" +
		                                "" +
		                                "You may continue running the program at your own risk.";
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
