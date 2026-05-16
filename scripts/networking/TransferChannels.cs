namespace Project;

/// Class to organize all the multiplayer transfer channels for the project
public static class TransferChannels {
    public const int Default = 0;

    /** High bandwidth show data */
    public const int ShowSignal = 1;

    /** High bandwidth show data */
    public const int ShowAudio = 2;

    /** Low bandwidth show info */
    public const int ShowMessage = 3;

    /** Low bandwidth player messages. For instance an interaction packet, or actual player text messages. */
    public const int PlayerMessages = 4;
}