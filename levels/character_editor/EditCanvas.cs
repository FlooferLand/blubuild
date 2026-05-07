using System;
using System.Linq;
using Godot;

namespace Project.CharacterEditing;

public partial class EditCanvas : CanvasLayer {
	[Export] public required CharacterEditor Parent;
	[Export] public required LineEdit DisplayName;
	[Export] public required LineEdit Authors;

	[Export] public required Button SaveButton;
	[Export] public required CheckButton AutosaveCheckButton;
	[Export] public required ProgressBar SaveProgressBar;
	[Export] public required Label SaveError;

	// TODO: Add a file watcher and reload everything when the character file changes externally

	public override void _EnterTree() {
		SaveError.Visible = false;
		SaveButton.Pressed += Parent.SaveCharacter;

		Parent.CharacterLoaded += () => {
			if (Parent.File is not { } file) return;
			DisplayName.Text = file.DisplayName;
			Authors.Text = file.Authors.Join();
		};
		DisplayName.TextSubmitted += text => Commit(f => f.DisplayName = text);
		Authors.TextSubmitted += text => Commit(f => f.Authors = text.Split(',', StringSplitOptions.TrimEntries));
	}

	void Commit(Action<CharacterFile> action) {
		if (Parent.File is not {} file) return;
		action(file);
		if (AutosaveCheckButton.ButtonPressed)
			Parent.SaveCharacter();
	}
}
