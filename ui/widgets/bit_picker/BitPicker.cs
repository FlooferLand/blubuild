using System.Collections.Generic;
using Godot;

namespace Project;

public partial class BitPicker : PopupPanel {
	[Signal] public delegate void SelectedEventHandler();

	[ExportGroup("Local")]
	[Export] public required Tree Tree;

	public MappedBit SelectedBit = new();
	readonly Dictionary<TreeItem, MappedBit> Data = new();

	public override void _EnterTree() {
		Visible = false;
		Tree.ItemSelected += () => {
			var selected = Tree.GetSelected();
			if (!Data.TryGetValue(selected, out var bit)) return;
			SelectedBit = bit;
			EmitSignalSelected();
			Close();
		};
	}

	public override void _Input(InputEvent @event) {
		if (@event.IsActionPressed("ui_cancel")) {
			Close();
		}
	}

	public void PopupBitPicker() {
		Data.Clear();

		Tree.Clear();
		Tree.SetColumns(3);
		Tree.SetColumnTitle(0, "Name");
		Tree.SetColumnTitle(1, "Drawered Bit");
		Tree.SetColumnTitle(2, "Global Bit");
		Tree.SetColumnExpand(0, true);
		Tree.SetColumnExpand(1, false);
		Tree.SetColumnExpand(2, false);

		var rootItem = Tree.CreateItem();

		foreach ((string chartId, var chart) in BitChartRegistry.Instance.Charts) {
			var chartItem = Tree.CreateItem(rootItem);
			chartItem.SetText(0, chartId);
			foreach ((string fixtureId, var fixture) in chart.Fixtures) {
				var fixtureItem = Tree.CreateItem(chartItem);
				fixtureItem.SetText(0, (fixtureId.Length != 0) ? fixtureId : "Unused");
				foreach ((Bit bitId, string bitAction) in fixture.ActionNames) {
					var item = Tree.CreateItem(fixtureItem);
					item.SetText(0, (bitAction.Length != 0) ? bitAction : "Unused");
					item.SetText(1, bitId.FormatDrawered());
					item.SetText(2, bitId.ToString());
					Data[item] = new MappedBit(chartId, bitId);
				}
			}
		}

		var maxSize = GetViewport().GetVisibleRect().Size;
		Size = new Vector2I((int)(maxSize.X * 0.7f), (int)(maxSize.Y * 0.9f));
		Popup();
	}

	public void Close() {
		Hide();
		Data.Clear();
	}
}
