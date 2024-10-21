using Bluchalk.types;
using Project;

namespace Bluchalk;

public class BluTransformer : RawMidiTransformer {
    public new Result<ShowData> Read(Stream stream) {
        Console.WriteLine("Blu transformer is unimplemented");
        return base.Read(stream);
    }
}