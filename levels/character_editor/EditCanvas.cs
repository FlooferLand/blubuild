using System;
using System.Threading.Tasks;
using Godot;

namespace Project.CharacterEditing;

public partial class EditCanvas : CanvasLayer {
	[Export] public required CharacterEditor Parent;
	[Export] public required TabContainer TabContainer;
	[Export] public required Label Error;
	[Export] public required Label Info;
	[Export] public required Button SaveButton;
	[Export] public required ProgressBar SaveProgressBar;

	[Export] public required PackedScene BitPickerScene;
	[Export] public required PackedScene BitPickerButtonScene;
	[Export] public required PackedScene FlowEditScene;

	[ExportGroup("Info")]
	[Export] public required LineEdit DisplayName;
	[Export] public required LineEdit Authors;

	[ExportGroup("Model")]
	[Export] public required FilePickerButton ModelPicker;

	[ExportGroup("Animations")]
	[Export] public required GridContainer AnimationContainer;

	// TODO: Add a file watcher and reload everything when the character file changes externally

	public override void _EnterTree() {
		Error.Visible = false;
		Info.Visible = false;
		SaveButton.Pressed += async () => await Parent.SaveCharacter();
		SaveProgressBar.Value = 0;
		ModelPicker.AddFilter(FileFormat.GltfModel);
		TabContainer.CurrentTab = 0;

		Parent.CharacterLoaded += () => {
			if (Parent.File is not { } file) return;
			DisplayName.Text = file.DisplayName;
			Authors.Text = file.Authors.Join();

			AnimationContainer.QueueFreeChildren();
			foreach (string text in new string[] { "Animation", "Bit", "Flows" }) {
				AnimationContainer.AddChild(new Label { Text = text, SizeFlagsHorizontal = Control.SizeFlags.ExpandFill });
			}
			if (Parent.ModelAnimationPlayer is {} animPlayer) {
				foreach (var animation in animPlayer.GetAllAnimations()) {
					// Animation name
					var nameButton = new Button { Text = animation };
					nameButton.Pressed += () => {
						animPlayer.Stop();
						animPlayer.Play(nameButton.Text);
					};

					// Flow selector
					var flowEdit = FlowEditScene.Instantiate<FlowEdit>();

					// Bit selector
					var bitButton = BitPickerButtonScene.Instantiate<BitPickerButton>();
					bitButton.Picker.Selected += () => {
						var selected = bitButton.Picker.SelectedBit;
						file.BitData.Add(selected, new BitData(animation, flowEdit.Value));
					};

					// Adding (has to be in a different order)
					if (file.BitData.AnimToBits.TryGetValue(animation, out var bit)) {
						bitButton.SetBit(bit);
						if (file.BitData.BitsToData.TryGetValue(bit, out var data)) {
							flowEdit.Value = data.Flows;
						}
					}
					flowEdit.Changed += () => {
						var selected = bitButton.Picker.SelectedBit;
						var flow = flowEdit.Value;
						if (file.BitData.BitsToData.TryGetValue(selected, out var data)) {
							data.Flows.Set(flow);
						}
					};

					nameButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
					AnimationContainer.AddChild(nameButton);
					AnimationContainer.AddChild(bitButton);
					AnimationContainer.AddChild(flowEdit);
				}
			}
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
