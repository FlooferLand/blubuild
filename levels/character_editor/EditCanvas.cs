using System;
using System.Threading.Tasks;
using Godot;

namespace Project.CharacterEditing;

public partial class EditCanvas : CanvasLayer {
	[Export] public required CharacterEditor Parent;
	[Export] public required LineEdit DisplayName;
	[Export] public required LineEdit Authors;
	[Export] public required FilePickerButton ModelPicker;
	[Export] public required Label Error;
	[Export] public required Label Info;

	[Export] public required Button SaveButton;
	[Export] public required ProgressBar SaveProgressBar;
	[Export] public required Label SaveError;
	[Export] public required Label SaveInfo;

	// TODO: Add a file watcher and reload everything when the character file changes externally

	public override void _EnterTree() {
		Error.Visible = false;
		Info.Visible = false;
		SaveError.Visible = false;
		SaveInfo.Visible = false;
		SaveButton.Pressed += async () => await Parent.SaveCharacter();
		SaveProgressBar.Value = 0;
		ModelPicker.AddFilter(FileFormat.GltfModel);

		Parent.CharacterLoaded += () => {
			if (Parent.File is not { } file) return;
			DisplayName.Text = file.DisplayName;
			Authors.Text = file.Authors.Join();
		};
		ModelPicker.FileSelected += path => Commit(async _ => await Parent.LoadModel(path));
		DisplayName.TextSubmitted += text => Commit(f => f.DisplayName = text);
		Authors.TextSubmitted += text => Commit(f => f.Authors = text.Split(',', StringSplitOptions.TrimEntries));
	}

	public void SetError(string? error = null) {
		Error.Text = error ?? "";
		Error.Visible = error != null;
	}

	public void SetInfo(string? info = null) {
		Info.Text = info ?? "";
		Info.Visible = info != null;
		SetError();
	}

	async void Commit(Func<CharacterFile, Task> action) {
		if (Parent.File is not { } file) return;
		await action(file);
		// if (AutosaveCheckButton.ButtonPressed)
		// 	Parent.SaveCharacter();
	}

	void Commit(Action<CharacterFile> action) {
		Commit(f => {
			action(f);
			return Task.CompletedTask;
		});
	}
}
