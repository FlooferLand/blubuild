using Godot;

namespace Project;

public interface IPacket {
    public MultiplayerPeer.TransferModeEnum Mode { get; }
}