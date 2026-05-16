namespace Project;

public readonly record struct InteractContext(
    Player Player,
    InteractType Type,
    InteractState State
);
