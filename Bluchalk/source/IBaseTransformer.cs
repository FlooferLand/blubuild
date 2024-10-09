using Project;

namespace Bluchalk;

/// An interface for file formats like <c>rshw</c> to define how to read/write to the format. <br/>
/// Mostly used for conversion between formats.
public interface IBaseTransformer {
    /// Returns <c>true</c> if the buffer matches the specifications for this format
    public Boolean RespectsFormat();
    
    /// Creates a file from a buffer. <br/>
    /// Only called if <see cref="RespectsFormat"/> returns <c>true</c>.
    public Result<SignalContainer> Read();
}