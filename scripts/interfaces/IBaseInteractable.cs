namespace Project;
using Godot;

public interface IBaseInteractable {
	public void Interact(RayCast3D ray, InteractState state, InteractButton button);
}

public enum InteractState {
	Press, Hold, Release
}

public enum InteractButton {
	Primary, Secondary
}
