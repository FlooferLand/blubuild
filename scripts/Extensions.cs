using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Project;

public static class Extensions {
    /// Returns true if it's a client OR an integrated server
    public static bool IsClientOrIntegrated(this MultiplayerApi multiplayer) => !multiplayer.IsServer() || NetworkManager.IsIntegratedServer;

    /// Returns true if the current client is controlling this object. <br/>
    /// False if running on the server
    public static bool ClientOwns(this SceneTree tree, Node node) {
        return tree.GetMultiplayer().IsClientOrIntegrated()
               && node.IsMultiplayerAuthority();
    }

    /// Short for checking if I'm on the server, and sending the data to every ID expect the server one
    public static void ServerRpcToClients(this Node node, StringName method, params Variant[] args) {
        if (!node.Multiplayer.IsServer()) return;
        foreach (long id in NetworkManager.Players.Keys.Where(id => id != 1)) {
            node.RpcId(id, method, args);
        }
    }

    public static Error LoadFromBuffer(this Image image, byte[] buffer, string fileExtension) => fileExtension switch {
        "png" => image.LoadPngFromBuffer(buffer),
        "jpg" or "jpeg" => image.LoadJpgFromBuffer(buffer),
        "webp" => image.LoadWebpFromBuffer(buffer),
        "exr" => image.LoadExrFromBuffer(buffer),
        "svg" => image.LoadSvgFromBuffer(buffer),
        "bmp" => image.LoadBmpFromBuffer(buffer),
        _ => Error.FileUnrecognized
    };

    extension(AudioStream stream) {
        public double RealBpm => stream switch {
            AudioStreamOggVorbis s => s.Bpm,
            AudioStreamSynchronized s => s.GetSyncStream(0).RealBpm,
            _ => stream._GetBpm()
        };
    }

    // Methods because C# sucks balls
    public static V? GetValueOrNullC<K, V>(this IReadOnlyDictionary<K, V> self, K key) where V: class =>
        self.TryGetValue(key, out var value) ? value : null;
    public static V? GetValueOrNullS<K, V>(this IReadOnlyDictionary<K, V> self, K key) where V: struct =>
        self.TryGetValue(key, out var value) ? value : null;
}