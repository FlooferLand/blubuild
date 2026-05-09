using System.Threading.Tasks;
using GodotUtils;

namespace Project;

public record CharacterFileV1 : CharacterFileVersion {
    public override int Version => 1;
    public override async Task<Result<CharacterFile>> Run(CharacterFile file) {
        await using var reader = await OpenFile("models/main.gltf");
        if (reader != null) {
            (var model, string? err) = await Model.Load(reader);
            if (err != null) return Result.Err($"Model could not be loaded: {err}");
            file.Model = model;
        }

        return Result.Ok(file);
    }
}