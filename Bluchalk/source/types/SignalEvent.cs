namespace Bluchalk.types;

public abstract class SignalEvent {
    public readonly TimeSpan TimeStamp;
    public readonly EventType EventId;

    public SignalEvent(TimeSpan timestamp, EventType eventId) {
        TimeStamp = timestamp;
        EventId = eventId;
    }
    
    public enum EventType {
        Bit,
        Other
    }
}