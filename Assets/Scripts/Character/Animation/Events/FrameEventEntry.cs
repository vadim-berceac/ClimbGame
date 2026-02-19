// FrameEventEntry.cs
using System;
using UnityEngine.Animations;
using UnityEngine.Playables;

internal class FrameEventEntry
{
    public AnimationClipPlayable Clip;
    public float     FromTime;
    public float     ToTime;
    public Func<float> WeightProvider;
    public Action    OnEnter;
    public Action    OnExit;
    public Action    OnTick;
    public float     WeightThreshold;

    private bool  _wasInside;
    private float _prevTime = -1f;

    public void Evaluate()
    {
        if (!Clip.IsValid())
        {
            ForceExit();
            return;
        }

        var currentTime = (float)Clip.GetTime();
        var clipLength  = Clip.GetAnimationClip().length;

        // Детектируем зацикливание: время прыгнуло назад
        if (_prevTime >= 0f && currentTime < _prevTime - 0.001f)
            ForceExit();

        _prevTime = currentTime;

        // Для зацикленных клипов нормализуем время внутри одного цикла
        var evalTime = clipLength > 0f ? currentTime % clipLength : currentTime;

        var weight = WeightProvider?.Invoke() ?? 1f;
        var inside = weight >= WeightThreshold
                     && evalTime >= FromTime
                     && evalTime <  ToTime;

        if (inside && !_wasInside)
        {
            _wasInside = true;
            OnEnter?.Invoke();
        }
        else if (!inside && _wasInside)
        {
            _wasInside = false;
            OnExit?.Invoke();
        }

        if (inside)
            OnTick?.Invoke();
    }

    public void ForceExit()
    {
        if (!_wasInside) return;
        _wasInside = false;
        _prevTime  = -1f;
        OnExit?.Invoke();
    }
}