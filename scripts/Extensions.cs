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

    extension (Node node) {
        /// Short for checking if I'm on the server, and sending the data to every ID expect the server one
        /// TODO: Check if this is even required. Rpc(..) seems to work fine
        public void ServerRpcToClients(StringName method, params Variant[] args) {
            if (!node.Multiplayer.IsServer()) return;
            foreach (long id in node.Multiplayer.GetPeers().Where(id => id != 1)) {
                node.RpcId(id, method, args);
            }
        }
        public Error RpcToServer(StringName method, params Variant[] args) {
            return node.RpcId(1, method, args);
        }
    }

    public static IEnumerable<StringName> GetAllAnimations(this AnimationPlayer animPlayer) {
        return animPlayer.GetAnimationLibraryList().SelectMany(library => animPlayer.GetAnimationLibrary(library).GetAnimationList());
    }
    public static void QueueFreeChildren(this Node node) {
        foreach (var child in node.GetChildren()) child?.QueueFree();
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