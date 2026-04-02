namespace Bluchalk;

public struct WavHeader {
    public int Channels;
    public int SampleRate;
    public int Bits;
    public int DataStart;

    public static WavHeader Read(byte[] data) => new() {
        Channels = BitConverter.ToInt16(data, 22),
        SampleRate = BitConverter.ToInt32(data, 24),
        Bits = BitConverter.ToInt16(data, 34),
        DataStart = 44
    };
}