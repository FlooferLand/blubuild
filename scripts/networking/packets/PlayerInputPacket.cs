using Godot;

namespace Project;

// Client to server
public struct PlayerInputPacket : IPacket {
    public MultiplayerPeer.TransferModeEnum Mode => MultiplayerPeer.TransferModeEnum.UnreliableOrdered;

    public Vector2 InputDirection;
    public Vector2 InputMouse;
}