using Bluchalk.types;

namespace Bluchalk;

// TODO: Add reel bit drawer selection for channels
// TODO: Rethink how signal stuff is stored

/// Holds event signals and has some handy methods
public class SignalContainer {
    private List<SignalEvents.BitEvent> events = new();
    
    public void Add(SignalEvents.BitEvent signal) {
        events.Add(signal);
    }
    
    public SignalEvents.BitEvent? Get(int i) {
        return i < events.Count ? events[i] : null;
    }
    
    public int Count() {
        return events.Count;
    }

    public IEnumerable<SignalEvents.BitEvent> Events() {
        return events;
    }
}