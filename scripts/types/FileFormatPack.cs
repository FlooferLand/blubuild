using System.Linq;

namespace Project;

using static FileFormat;

/// Packs several file formats into one
public readonly record struct FileFormatPack(params FileFormat[] Formats) {
    public readonly string[] Filters = Formats.Select(f => f.Filter).ToArray();

    public static readonly FileFormatPack
        Audio = new(WavAudio),
        Shows = new(Show, SpteShow),
        Models = new(GltfModel, FbxModel, ObjModel)
    ;
}