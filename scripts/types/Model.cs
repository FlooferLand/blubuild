using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using GodotUtils;

namespace Project;

/// Contains 3D model data, any model is automatically converted to GLTF. <br/>
/// All model types inherit from <see cref="GltfState"/> and <see cref="GltfDocument"/>, even FBX models.
public readonly record struct Model(GltfDocument Document, GltfState State) {
    public readonly string Extension = "gltf";
    public long Size => State.GlbData.Length;

    /// Returns an error, if there's any
    public async ValueTask<string?> Save(Stream stream, ProgressHolder progress) {
        if (!stream.CanWrite) return "Stream can't be written to";
        if (State == null) return "State is null";
        var (document, state) = (Document, State);

        progress.Set("Generating buffer (will take a while)", indeterminate:true);
        byte[] bytes = await Task.Run(() => document.GenerateBuffer(state));
        await stream.WriteAsync(bytes);
        return null;
    }

    public async ValueTask<Result<Node3D>> CreateScene() {
        var (document, state) = (Document, State);
        var scene = await Task.Run(() => document.GenerateScene(state, bakeFps: 60f, trimming: true));
        if (scene is not Node3D root)
            return Result.Err("Failed to create model scene");
        return Result.Ok(root);
    }

    /// Loads a model from the filesystem along with any textures in its folder
    public static async ValueTask<Result<Model>> Load(string path, ProgressHolder progress) {
        var document = new GltfDocument();
        var state = new GltfState();
        progress.Set("Reading the model file");
        var error = await Task.Run(() => document.AppendFromFile(path, state, basePath:path.GetBaseDir()));
        if (error != Error.Ok) return Result.Err(error.ToString());
        return Result.Ok(new Model(document, state));
    }

    /// Loads a model from a stream. Requires the textures and other data since there's no other way to get them
    public static async ValueTask<Result<Model>> Load(Stream stream, Dictionary<string, Image> textures, ProgressHolder progress) {
        var document = new GltfDocument();
        var state = new GltfState();

        progress.Set("Reading the model stream");
        byte[] bytes = new byte[stream.Length];
        int _ = await stream.ReadAsync(bytes);

        bool isGlb = bytes.AsSpan(0, 4).SequenceEqual("glTF"u8);
        if (!isGlb) return Result.Err("Only glb models are supported");

        progress.Set("Parsing the model");
        var error = await Task.Run(() => document.AppendFromBuffer(bytes, "", state));
        if (error != Error.Ok) return Result.Err(error.ToString());
        return Result.Ok(new Model(document, state));
    }
}