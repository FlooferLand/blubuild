using Godot;
using Bluchalk;
using SharpCompress.Common;

namespace Project.CharacterEditing;

public partial class InitCanvas : CanvasLayer {
	[Export] public required CharacterEditor Parent;
	[Export] public required Button NewButton;
	[Export] public required Button LoadButton;
	[Export] public required ProgressBar Progress;
	[Export] public required Label Error;

	public override void _Ready() {
		Progress.Visible = false;
		Error.Visible = false;

		NewButton.Pressed += () => {
			var dialog = MakeDialog();
			dialog.FileMode = FileDialog.FileModeEnum.SaveFile;
			dialog.DialogText = "Save your character";
			dialog.FileSelected += async path => await Parent.NewCharacter(path);
			AddChild(dialog);
			dialog.PopupFileDialog();
		};
		LoadButton.Pressed += () => {
			var dialog = MakeDialog();
			dialog.FileMode = FileDialog.FileModeEnum.OpenFile;
			dialog.DialogText = "Load a character";
			dialog.FileSelected += async path => await Parent.LoadCharacter(path);
			AddChild(dialog);
			dialog.PopupFileDialog();
		};
	}

	public void SetError(string? error = null) {
		Error.Text = error ?? "";
		Error.Visible = error != null;
	}

	public void HandleProgress(ProgressReport report) {
		Progress.Value = (float)(report.PercentComplete ?? 0.0);
		Progress.Visible = report is { TotalBytes: not null, PercentComplete: not null };
	}

	static FileDialog MakeDialog() => new() {
		Access = FileDialog.AccessEnum.Filesystem,
		Filters = new []{ FileFormat.Character.Filter },
		UseNativeDialog = true
	};
}
