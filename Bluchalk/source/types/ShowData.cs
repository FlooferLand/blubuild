namespace Bluchalk.types;

/// Holds info about the show, including the signal
public class ShowData {
    public readonly string Name;
    public readonly TimeSpan Length;
    public readonly SignalContainer Signal;

    public ShowData(string name, TimeSpan length, SignalContainer signal) {
        Name = name;
        Length = length;
        Signal = signal;
    }
}