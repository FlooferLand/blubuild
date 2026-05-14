using System;
using System.Text;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using GodotUtils;

namespace Project;

public record CharacterFileV1 : CharacterFileVersion {
    public override int Version => 1;
    public override async Task<Result<CharacterFile>> Run(CharacterFile file, ProgressHolder progress) {
        // Loading the bit mapping
        const string bitmapFile = "bitmap.txt";
        if (HasFile(bitmapFile)) {
            progress.Set($"Reading '{bitmapFile}'");
            await using var stream = await OpenFile(bitmapFile);
            if (stream == null) return Result.Err($"Failed to read {bitmapFile}");
            string text = Encoding.UTF8.GetString(stream.GetBuffer());
            (var data, string? error) = BitMappingData.FromText(text);
            if (error != null) return Result.Err(error);
            file.BitData.SetFrom(data);
        }

        // Searching textures
        var textures = new Dictionary<string, Image>();
        foreach (string key in ListFolder("models")) {
            string filename = key.Replace("models/", "");
            string extension = key.GetExtension().ToLower();
            if (!FileFormatPack.Images.Extensions.Contains(extension)) continue;
            progress.Set($"Reading texture '{key}'");

            byte[] bytes;
            {
                await using var stream = await OpenFile(key);
                if (stream == null) continue;
                bytes = new byte[stream.Length];
                int _ = await stream.ReadAsync(bytes);
            }

            var image = new Image();
            Error error = image.LoadFromBuffer(bytes, extension);
            if (error != Error.Ok) {
                Log.Error($"Failed to load texture '{key}': {error}");
                continue;
            }
            textures.Add(filename, image);
        }

        // Loading the model
        const string modelPath = "models/main.gltf";
        if (HasFile(modelPath)) {
            progress.Set("Opening the model file");
            await using var stream = await OpenFile(modelPath);
            if (stream == null) return Result.Err($"Failed to read {modelPath}");

            (var model, string? err) = await Model.Load(stream, textures, progress);
            if (err != null) return Result.Err($"Model could not be loaded: {err}");
            file.Model = model;
        }

        return Result.Ok(file);
    }
}