using System.Linq;

namespace Project;

/// Contains a specific single file format and it's variations (ex: jpg/jpeg, wav)
public readonly record struct FileFormat(string[] Extensions, string Description, string MimeType) {
    public readonly string Filter = string.Join(", ", Extensions.Select(e => "*." + e));

    FileFormat(string extension, string description, string mimeType) : this([extension], description, mimeType) {}

    public static readonly FileFormat
        Show = new("blushw", "Blubuild Show", "application/x-7z-compressed"),
        Character = new("bluchar", "Blubuild Character", "application/x-7z-compressed"),
        SpteShow = new("rshw", "RR Engine Show", "application/octet-stream"),

        #region Models
        GltfModel = new(new string[] { "gltf", "glb" }, "GLTF Model", "model/gltf"),
        FbxModel = new("fbx", "FBX Model", "model/fbx"),
        ObjModel = new("obj", "OBJ Model", "model/obj"),
        #endregion

        #region Audio
        WavAudio = new("wav", "WAV Audio", "audio/wave")
        #endregion
    ;
}