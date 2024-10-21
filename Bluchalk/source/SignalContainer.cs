using Bluchalk.types;

namespace Bluchalk;

// TODO: Add reel bit drawer selection for channels
// TODO: Rethink how signal stuff is stored

/// Holds event signals and has some handy methods
public class SignalContainer {
    private Dictionary<int, SignalChannel> channels = new();
    
    public void AddChannel(int channelId, SignalChannel channel) {
        channels.Add(channelId, channel);
    }
    
    public void AddEvent(int channelId, SignalEvents.BitEvent signalEvent) {
        if (!Exists(channelId)) {
            AddChannel(channelId, new SignalChannel());
        }
        Get(channelId).AddEvent(signalEvent);
    }
    
    public SignalChannel? Get(int channelId) {
        return Exists(channelId) ? channels[channelId] : null;
    }
    
    public bool Exists(int channelId) {
        return channels.ContainsKey(channelId);
    }
    
    public int Count() {
        return channels.Count;
    }
    
    public IEnumerable<KeyValuePair<int, SignalChannel>> Channels() {
        var e = channels.GetEnumerator();
        while (e.MoveNext()) {
            yield return e.Current;
        }
    }
}