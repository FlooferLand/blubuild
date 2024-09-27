using System;

namespace Project.Components;
using Godot;

/*
 * Low-level info about the animatronic
 */
public partial class BotInfoComp : Node {
	[ExportCategory("Name")]
	[Export] public string DisplayName;

	[ExportCategory("Name")]
	[Export] public string IdNamespace;
	[Export] public string IdName;

	[ExportCategory("Nodes")]
	[Export] public Node3D Mesh;

	/* Mainly used as a hash to be able to store the animatronic */
	public string GetId() {
		return $"{IdNamespace}:{IdName}";
	}

	public override bool Equals(object obj) {
		string id = GetId();
		return obj switch {
			BotInfoComp botInfo =>
				id == botInfo.GetId(),
			string str =>
				id == str,
			_ => false
		};
	}
	public override int GetHashCode() {
		return HashCode.Combine(IdNamespace, IdName);
	}
}
