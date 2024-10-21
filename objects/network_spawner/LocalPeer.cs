using System.Diagnostics.CodeAnalysis;
using Godot;

namespace Project;

/// Keeps track of what the local instance of the game is (is it a client, is it a server, etc.)
[SuppressMessage("ReSharper", "InconsistentNaming")]
public static class LocalPeer {
    private static bool _isClient = false;
    public static bool IsClient => _isClient;

    private static bool _isServer = false;
    public static bool IsServer => _isServer;
    
    private static bool _integratedServer = false;
    public static bool IntegratedServer => _integratedServer;
    
    public static long PeerId = -1;

    public static void Init(bool server, bool client, bool integratedServer) {
        _isServer = server;
        _isClient = client;
        _integratedServer = integratedServer;
    }

    /// Returns true if the current client is controlling this object
    public static bool ThisClientOwns(Node node) {
        return IsClient && PeerId == node.Multiplayer.MultiplayerPeer.GetUniqueId();
    }
}