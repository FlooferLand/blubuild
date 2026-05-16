using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Project;

public partial class BitPicker : PopupPanel {
	[Signal] public delegate void SelectedEventHandler();

	[ExportGroup("Local")]
	[Export] public required LineEdit Search;
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
		Search.TextChanged += FilterTree;
	}

	public override void _Input(InputEvent @event) {
		if (@event.IsActionPressed("ui_cancel")) Close();
	}

	void FilterTree(string text) {
		if (Tree.GetRoot() is not { } root) return;
		foreach (var chartRoot in root.GetChildren()) {
			foreach (var fixtureRoot in chartRoot.GetChildren()) {
				int visibleCount = 0;
				foreach (var bitItem in fixtureRoot.GetChildren()) {
					// Bit filtering
					if (text.All(char.IsDigit)) {
						bitItem.Visible = bitItem.GetText(1).Contains(text) || bitItem.GetText(2).Contains(text);
					} else {
						// Basic pattern matching
						bitItem.Visible = bitItem.GetText(0).Contains(text, StringComparison.OrdinalIgnoreCase)
						                  || bitItem.GetText(1).Contains(text, StringComparison.OrdinalIgnoreCase)
						                  || bitItem.GetText(2).Contains(text, StringComparison.OrdinalIgnoreCase);
					}
					if (bitItem.Visible) visibleCount += 1;
				}
				fixtureRoot.Visible = (visibleCount != 0);
			}
		}
	}

	void SetupTree() {
		Tree.SetColumns(3);
		Tree.SetColumnTitle(0, "Name");
		Tree.SetColumnTitle(1, "Global Bit");
		Tree.SetColumnTitle(2, "Drawered Bit");
		Tree.SetColumnExpand(0, true);
		Tree.SetColumnExpand(1, true);
		Tree.SetColumnExpand(2, true);
		Tree.SetColumnExpandRatio(0, 4);
		Tree.SetColumnExpandRatio(1, 1);
		Tree.SetColumnExpandRatio(2, 1);
	}

	void BuildTree() {
		var rootItem = Tree.CreateItem();
		foreach ((string chartId, var chart) in BitChartRegistry.Charts) {
			var chartItem = Tree.CreateItem(rootItem);
			chartItem.SetText(0, chartId);
			foreach ((string fixtureId, var fixture) in chart.Fixtures) {
				var fixtureItem = Tree.CreateItem(chartItem);
				fixtureItem.SetText(0, OrUnused(fixtureId));
				foreach ((Bit bitId, string bitAction) in fixture.ActionNames) {
					var item = Tree.CreateItem(fixtureItem);
					item.SetText(0, OrUnused(bitAction));
					item.SetText(1, bitId.ToString());
					item.SetText(2, Drawer.FormatBit(bitId));
					Data[item] = new MappedBit(chartId, bitId);
				}
			}
		}
		return;
		static string OrUnused(string value) => value.Length != 0 ? value : "Unused";
	}

	public void PopupBitPicker() {
		Data.Clear();
		Tree.Clear();
		Search.Clear();
		SetupTree();
		BuildTree();

		var maxSize = DisplayServer.WindowGetSize();
		var minSize = new Vector2I((int)(maxSize.X * 0.6f), (int)(maxSize.Y * 0.85f));
		PopupCentered(minSize);
		Search.GrabFocus();
	}

	public void Close() {
		Hide();
		Data.Clear();
	}
}
