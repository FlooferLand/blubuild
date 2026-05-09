using Godot;

namespace Project;

public partial class FilePickerButton : Control {
	[Signal] public delegate void FileSelectedEventHandler(string path);
	[Signal] public delegate void CanceledEventHandler();

	[ExportGroup("Local")]
	[Export] Button Button = null!;
	[Export] FileDialog Dialog = null!;

	public string InitialText = "Pick a file";
	public string PickingText = "Picking..";

	public override void _Ready() {
		Button.Text = InitialText;
		Button.Pressed += () => {
			Button.Text = PickingText;
			Button.Disabled = true;
			Dialog.PopupFileDialog();
		};
		Dialog.FileSelected += path => {
			Button.Text = path.GetFile();
			Button.Disabled = false;
			EmitSignalFileSelected(path);
		};
		Dialog.Canceled += () => {
			Button.Disabled = false;
			Button.Text = InitialText;
			EmitSignalCanceled();
		};
	}

	public void AddFilter(FileFormat format) {
		Dialog.AddFilter(format.Filter, format.Description, format.MimeType);
	}
	public void AddFilters(FileFormatPack pack) {
		foreach (var format in pack.Formats)
			Dialog.AddFilter(format.Filter, format.Description, format.MimeType);
	}
}
