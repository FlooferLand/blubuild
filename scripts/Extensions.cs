using System.Collections.Generic;
using Godot;

namespace Project;

public static class Extensions {
    /// Returns true if it's a client OR an integrated server
    public static bool IsClient(this MultiplayerApi multiplayer) => !multiplayer.IsServer() || NetworkManager.IsIntegratedServer;

    /// Returns true if the current client is controlling this object. <br/>
    /// False if running on the server
    public static bool ClientOwns(this SceneTree tree, Node node) {
        return tree.GetMultiplayer().IsClient()
               && node.IsMultiplayerAuthority();
    }

    // Methods because C# sucks balls
    public static V? GetValueOrNullC<K, V>(this IReadOnlyDictionary<K, V> self, K key) where V: class =>
        self.TryGetValue(key, out var value) ? value : null;
    public static V? GetValueOrNullS<K, V>(this IReadOnlyDictionary<K, V> self, K key) where V: struct =>
        self.TryGetValue(key, out var value) ? value : null;
}