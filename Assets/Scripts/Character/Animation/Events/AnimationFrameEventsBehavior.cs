using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class AnimationFrameEventsBehavior : PlayableBehaviour
{
    private readonly List<FrameEventEntry> _entries = new();
    private readonly List<FrameEventEntry> _entriesToAdd = new();
    private readonly List<AnimationClipPlayable> _clipsToRemove = new();

    private bool _isIterating = false;

    public void Register(AnimationClipPlayable clip, int fromFrame, int toFrame, 
        Func<float> weightProvider, Action onEnter = null, Action onExit = null, 
        Action onTick = null, float weightThreshold = 0.5f)
    {
        var fps = clip.GetAnimationClip().frameRate;

        var entry = new FrameEventEntry
        {
            Clip            = clip,
            FromTime        = fromFrame / fps,
            ToTime          = toFrame   / fps,
            WeightProvider  = weightProvider,
            OnEnter         = onEnter,
            OnExit          = onExit,
            OnTick          = onTick,
            WeightThreshold = weightThreshold
        };

        if (_isIterating)
            _entriesToAdd.Add(entry);
        else
            _entries.Add(entry);
    }

    public void Unregister(AnimationClipPlayable clip)
    {
        if (_isIterating)
        {
            _clipsToRemove.Add(clip);
        }
        else
        {
            RemoveClipInternal(clip);
        }
    }

    public void UnregisterAll()
    {
        if (_isIterating)
        {
            _clipsToRemove.Clear();
            _clipsToRemove.AddRange(_entries.Select(e => e.Clip));
        }
        else
        {
            foreach (var entry in _entries)
                entry.ForceExit();
            _entries.Clear();
        }
    }

    private void RemoveClipInternal(AnimationClipPlayable clip)
    {
        foreach (var entry in _entries)
        {
            if (entry.Clip.Equals(clip))
                entry.ForceExit();
        }

        _entries.RemoveAll(e => e.Clip.Equals(clip));
    }

    public override void PrepareFrame(Playable playable, FrameData info)
    {
        _isIterating = true;

        foreach (var entry in _entries)
        {
            if (entry != null)
                entry.Evaluate();
        }

        _isIterating = false;

        ApplyPendingChanges();
    }

    private void ApplyPendingChanges()
    {
        if (_entriesToAdd.Count > 0)
        {
            _entries.AddRange(_entriesToAdd);
            _entriesToAdd.Clear();
        }

        if (_clipsToRemove.Count > 0)
        {
            foreach (var clip in _clipsToRemove)
                RemoveClipInternal(clip);

            _clipsToRemove.Clear();
        }
    }
}