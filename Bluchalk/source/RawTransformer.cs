using Melanchall.DryWetMidi.Core;
using Project;

namespace Bluchalk;

/// Defines how to read/write from raw midi
public class RawTransformer : IBaseTransformer {
    public Boolean RespectsFormat() {
        return true;
    }

    public Result<SignalContainer> Read(Stream stream) {
        var file = MidiFile.Read(stream);
        return Result<SignalContainer>.Err("Not implemented!");
    }
}
