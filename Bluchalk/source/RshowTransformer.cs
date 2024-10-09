using Project;

namespace Bluchalk;

/// Defines how to read/write from the Rshow format
public class RshowTransformer : IBaseTransformer {
    public Boolean RespectsFormat() {
        return true;
    }

    public Result<SignalContainer> Read() {
        return Result<SignalContainer>.Err("Not implemented!");
    }
}