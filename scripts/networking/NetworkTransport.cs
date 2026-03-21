using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Godot;

namespace Project;

// WORK IN PROGRESS
// SHOULD PROBABLY NOT USE

public partial class NetworkTransport : Node {
    static readonly List<PacketHandlerRaw> handlers = new();
    static readonly Dictionary<Type, byte> ids = new();

    const int BufPacketId = 0;

    static NetworkTransport instance = null!;

    public override void _EnterTree() {
        instance = this;

        /*if (Multiplayer.IsServer()) {
            // TODO: Simplify this the way Minecraft does it; Add an extra parameter that gives me the Player directly.
            NetworkTransport.Handle<PlayerInputPacket>((packet, senderId) => {
                if (NetworkManager.Players.TryGetValue(senderId, out var player)) {
                    player.PlayerInput.InputDirection = packet.InputDirection;
                    player.PlayerInput.InputMouse = packet.InputMouse;
                }
            });
        }*/
    }

    public static void Handle<T>(PacketHandler<T> handler) where T : unmanaged, IPacket {
        if (ids.ContainsKey(typeof(T))) {
            GD.PushError($"Packet '{typeof(T)}' was already registered");
            return;
        }

        ids[typeof(T)] = (byte) handlers.Count;
        handlers.Add((data, senderId) => handler(MemoryMarshal.Read<T>(data.AsSpan(BufPacketId + 1)), senderId));
    }

    public static void Send<T>(in T packet, long targetPeer = MultiplayerPeer.TargetPeerBroadcast) where T: unmanaged, IPacket {
        var multiplayer = ((SceneMultiplayer) instance.Multiplayer);
        if (multiplayer.MultiplayerPeer == null || multiplayer.MultiplayerPeer.GetConnectionStatus() != MultiplayerPeer.ConnectionStatus.Connected) {
            return;
        }

        if (IdOf<T>() is not { } id) {
            // GD.PushError($"Failed to get ID for packet '{typeof(T)}'");
            return;
        }

        byte[] buffer = new byte[1 + Unsafe.SizeOf<T>()];
        buffer[0] = id;
        MemoryMarshal.Write(buffer.AsSpan(1), in packet);

        // Calling directly on integrated servers, else out the network
        if (targetPeer == multiplayer.GetUniqueId() || NetworkManager.IsIntegratedServer) {
            OnPeerPacket((int) targetPeer, buffer);
        } else {
            multiplayer.SendBytes(buffer, (int)targetPeer, packet.Mode);
        }
    }

    public static byte? IdOf<T>() => ids.GetValueOrNullS(typeof(T));

    public static void OnPeerPacket(long senderId, byte[] data) =>
        handlers[data[BufPacketId]].Invoke(data, senderId);

    public delegate void PacketHandler<in T>(T packet, long id) where T: unmanaged, IPacket;
    public delegate void PacketHandlerRaw(byte[] data, long id);
}