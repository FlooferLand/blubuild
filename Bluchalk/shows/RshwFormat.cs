using System.Collections;
using System.Formats.Nrbf;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using GodotUtils;

namespace Bluchalk.shows;

// TODO: Port the low-level BizlibNative stuff here so C++ support for the library can be achieved

public class RshwFormat : ShowFormat<RshwFormat.RshwData> {
    const string SignalField = "<signalData>k__BackingField";
    const string AudioField = "<audioData>k__BackingField";
    const string VideoField = "<videoData>k__BackingField";
    const string SignalKeyword = "signal";
    const string AudioKeyword = "audio";
    const string VideoKeyword = "video";

    static Result<RshwData> _Read(Stream stream) {
        var result = new RshwData();
        try {
            var root = NrbfDecoder.DecodeClassRecord(stream);

            var errors = new ArrayList();
            void ProcessArray<T>(string fieldKeyword, string field, ref T[] target, bool optional = false) where T: unmanaged {
                // Finding the name
                string? name = null;
                if (root.HasMember(field)) {
                    name = field;
                } else if (!optional) {
                    name = root.MemberNames.FirstOrDefault(m => m.Contains(fieldKeyword));
                }
                if (name == null) {
                    if (!optional) errors.Add($"Failed to find a {fieldKeyword} field");
                    return;
                }

                if (root.GetArrayRecord(name) is SZArrayRecord<T> record) {
                    target = Unsafe.As<T[]>(record.GetArray(allowNulls:false));
                } else target = [];
            }

            ProcessArray(SignalKeyword, SignalField, ref result.signal);
            ProcessArray(AudioKeyword, AudioField, ref result.audio);
            ProcessArray(VideoKeyword, VideoField, ref result.video, optional: true);
            if (errors.Count > 0) {
                return Result<RshwData>.Err($"{errors.Count} unknown error(s) encountered while reading fields for rshw:\n- {string.Join("\n- ", errors)}");
            }
        } catch (Exception e) {
            return Result<RshwData>.Err($"BizlibNative encountered an exception: {e}");
        }
        return Result<RshwData>.Ok(result);
    }

    #region Safe API
    [StructLayout(LayoutKind.Sequential)]
    public struct RshwData {
        public int[] signal = [];
        public byte[] audio = [];
        public byte[] video = [];
        public RshwData() {}
    }

    protected override Result<RshwData> Read(Stream stream) => _Read(stream);
    protected override Result<Unit> Write(Stream stream) {
        throw new NotImplementedException("Writing Rshw files not implemented yet");
    }
    #endregion
}