using System.IO;
using System.Threading.Tasks;
using Godot;
using GodotUtils;

namespace Project;

/// Contains 3D model data, any model is automatically converted to GLTF. <br/>
/// All model types inherit from <see cref="GltfState"/> and <see cref="GltfDocument"/>, even FBX models.
public readonly record struct Model(GltfDocument Document, GltfState State) {
    public readonly string Extension = "gltf";

    /// Returns an error, if there's any
    public async Task<string?> Save(string path) {
        if (State == null) return "State is null";
        var state = State;
        var document = new GltfDocument();
        var error = await Task.Run(() => document.WriteToFilesystem(state, path));
        return error != Error.Ok ? error.ToString() : null;
    }

    /// Returns an error, if there's any
    public async Task<string?> Save(Stream stream) {
        if (!stream.CanWrite) return "Stream can't be written to";
        if (State == null) return "State is null";
        var state = State;
        byte[] bytes = await Task.Run(() => {
            var document = new GltfDocument();
            return document.GenerateBuffer(state);
        });
        await stream.WriteAsync(bytes);
        return null;
    }

    public static async Task<Result<Model>> Load(string path) {
        var document = new GltfDocument();
        var state = new GltfState();
        var error = await Task.Run(() => document.AppendFromFile(path, state));
        if (error != Error.Ok) return Result.Err(error.ToString());
        return Result.Ok(new Model(document, state));
    }

    public static async Task<Result<Model>> Load(Stream stream, string basePath = "") {
        var document = new GltfDocument();
        var state = new GltfState();
        var error = await Task.Run(async () => {
            byte[] bytes = new byte[stream.Length];
            int i = await stream.ReadAsync(bytes);
            return document.AppendFromBuffer(bytes, basePath, state);
        });
        if (error != Error.Ok) return Result.Err(error.ToString());
        return Result.Ok(new Model(document, state));
    }
}