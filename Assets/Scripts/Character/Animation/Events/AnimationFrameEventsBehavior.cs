using System;
using System.Collections.Generic;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class AnimationFrameEventsBehavior : PlayableBehaviour
{
    private readonly List<FrameEventEntry> _entries = new List<FrameEventEntry>();

    public void Register(
        AnimationClipPlayable clip,
        int fromFrame,
        int toFrame,
        Func<float> weightProvider,
        Action onEnter         = null,
        Action onExit          = null,
        Action onTick          = null,
        float weightThreshold  = 0.5f)
    {
        var fps = clip.GetAnimationClip().frameRate;
        _entries.Add(new FrameEventEntry
        {
            Clip            = clip,
            FromTime        = fromFrame / fps,
            ToTime          = toFrame   / fps,
            WeightProvider  = weightProvider,
            OnEnter         = onEnter,
            OnExit          = onExit,
            OnTick          = onTick,
            WeightThreshold = weightThreshold
        });
    }

    public void Unregister(AnimationClipPlayable clip)
    {
        // Перед удалением корректно закрываем активные события
        foreach (var entry in _entries)
            if (entry.Clip.Equals(clip))
                entry.ForceExit();

        _entries.RemoveAll(e => e.Clip.Equals(clip));
    }

    public void UnregisterAll()
    {
        foreach (var entry in _entries)
            entry.ForceExit();
        _entries.Clear();
    }

    public override void PrepareFrame(Playable playable, FrameData info)
    {
        foreach (var entry in _entries)
            entry.Evaluate();
    }
}