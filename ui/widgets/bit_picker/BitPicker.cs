using Godot;

namespace Project;

public partial class BitPicker : PopupPanel {
	[Signal] public delegate void SelectedEventHandler(short bitId);

	[ExportGroup("Local")]
	[Export] public required Tree Tree;

	public override void _EnterTree() {
		Visible = false;
		Tree.ItemSelected += () => {
			var selected = Tree.GetSelected();
			if (selected == null) return;
			if (short.TryParse(selected.GetText(1), out short result)) {
				EmitSignalSelected(result);
				Close();
			}
		};
	}

	public override void _Input(InputEvent @event) {
		if (@event.IsActionPressed("ui_cancel")) {
			Close();
		}
	}

	public void PopupBitPicker() {
		Tree.Clear();
		Tree.SetColumns(2);

		var rootItem = Tree.CreateItem();

		foreach ((string chartId, var chart) in BitChartRegistry.Instance.Charts) {
			var chartItem = Tree.CreateItem(rootItem);
			chartItem.SetText(0, chartId);
			foreach ((string fixtureId, var fixture) in chart.Fixtures) {
				var fixtureItem = Tree.CreateItem(chartItem);
				fixtureItem.SetText(0, fixtureId);
				foreach ((int id, string action) in fixture.ActionNames) {
					var item = Tree.CreateItem(fixtureItem);
					item.SetText(0, action);
					item.SetText(1, id.ToString());
				}
			}
		}

		var maxSize = GetViewport().GetVisibleRect().Size;
		Size = new Vector2I((int)(maxSize.X * 0.7f), (int)(maxSize.Y * 0.9f));
		Popup();
	}

	public void Close() {
		Hide();
		QueueFree();
	}
}
