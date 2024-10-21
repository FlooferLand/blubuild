namespace Bluchalk.types;

public class SignalChannel {
    private readonly List<SignalEvents.BitEvent> events = new();

    public void AddEvent(SignalEvents.BitEvent signal) {
        events.Add(signal);
    }
    
    public SignalEvents.BitEvent? GetEvent(int i) {
        return i < events.Count ? events[i] : null;
    }
    
    public int CountEvents() {
        return events.Count;
    }

    public IEnumerable<SignalEvents.BitEvent> Events() {
        return events;
    }
}