using System;

public class FrameEventConfig
{
    public readonly int    FromFrame;
    public readonly int    ToFrame;
    public Action OnEnter;
    public Action OnExit;
    public Action OnTick;
    public readonly float  WeightThreshold;

    public FrameEventConfig(
        int    fromFrame,
        int    toFrame,
        Action onEnter        = null,
        Action onExit         = null,
        Action onTick         = null,
        float  weightThreshold = 0.5f)
    {
        FromFrame       = fromFrame;
        ToFrame         = toFrame;
        OnEnter         = onEnter;
        OnExit          = onExit;
        OnTick          = onTick;
        WeightThreshold = weightThreshold;
    }
}

public class LocomotionFrameEventConfig
{
    public readonly LocomotionClipType ClipType;
    public readonly FrameEventConfig   EventConfig;

    public LocomotionFrameEventConfig(LocomotionClipType clipType, FrameEventConfig eventConfig)
    {
        ClipType    = clipType;
        EventConfig = eventConfig;
    }
}