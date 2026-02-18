using System;
using UnityEngine.Animations;

public static class AnimationClipPlayableExtensions
{
    public static void RegisterFrameEvent(
        this AnimationClipPlayable clip,
        AnimationFrameEventsBehavior behavior,
        int fromFrame,
        int toFrame,
        Func<float> weightProvider,
        Action onEnter        = null,
        Action onExit         = null,
        Action onTick         = null,
        float weightThreshold = 0.5f)
    {
        behavior.Register(clip, fromFrame, toFrame, weightProvider, onEnter, onExit, onTick, weightThreshold);
    }

    public static void UnregisterFrameEvents(
        this AnimationClipPlayable clip,
        AnimationFrameEventsBehavior behavior)
    {
        behavior.Unregister(clip);
    }
}