using Melanchall.DryWetMidi.Interaction;

namespace Bluchalk.types;

public class SignalEvents {
    public class BitEvent : SignalEvent {
        public readonly bool Enabled;
        public readonly Note RawNote;
        
        public BitEvent(TimeSpan timestamp, Note rawNote, bool enabled) : base(timestamp, EventType.Bit) {
            Enabled = enabled;
            RawNote = rawNote;
        }
    }
}