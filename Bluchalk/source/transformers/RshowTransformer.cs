using Bluchalk.types;
using Project;

namespace Bluchalk;

/// Defines how to read/write from the Rshow format
public class RshowTransformer : IBaseTransformer {
    public Boolean RespectsFormat(Stream stream) {
        return true;
    }

    public Result<ShowData> Read(Stream stream) {
        return Result<ShowData>.Err("Not implemented!");
    }
}