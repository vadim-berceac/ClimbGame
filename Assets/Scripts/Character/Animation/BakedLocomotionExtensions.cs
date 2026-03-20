using System;
using UnityEngine.Animations;

public static class BakedLocomotionExtensions
{
    public static void RegisterFrameEvent(
        this BakedLocomotion baked,
        AnimationFrameEventsBehavior behavior,
        LocomotionClipType clipType,
        int fromFrame,
        int toFrame,
        Func<float> weightProvider,
        Action onEnter        = null,
        Action onExit         = null,
        Action onTick         = null,
        float weightThreshold = 0.3f)
    {
        baked.GetClip(clipType).RegisterFrameEvent(
            behavior,
            fromFrame, toFrame,
            weightProvider,
            onEnter, onExit, onTick,
            weightThreshold);
    }
}