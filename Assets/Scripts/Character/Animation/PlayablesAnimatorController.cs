using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class PlayablesAnimatorController
{
    private PlayableGraph _playableGraph;
    private readonly MonoBehaviour _coroutineRunner;
    private readonly AnimationMixerPlayable _animationMixerTopLevel;
    private readonly AnimationMixerPlayable _animationMixerLocomotion;
    
    private AnimationClipPlayable _oneShotAnimationClip;

    private Coroutine _blendInHandle;
    private Coroutine _blendOutHandle;
    
    public PlayablesAnimatorController(MonoBehaviour coroutineRunner, Animator animator,
        AnimationPlayablesConfigs animationPlayablesConfigs)
    {
        _coroutineRunner = coroutineRunner;
        
        _playableGraph = PlayableGraph.Create("AnimatorController");
        
        var output = AnimationPlayableOutput.Create(_playableGraph, "Animation", animator);

        _animationMixerTopLevel = AnimationMixerPlayable.Create(_playableGraph, 2);
        
        output.SetSourcePlayable(_animationMixerTopLevel);
        
        _animationMixerLocomotion = AnimationMixerPlayable.Create(_playableGraph, 3);
        
        _animationMixerTopLevel.ConnectInput(0, _animationMixerLocomotion, 0);
        
        _playableGraph.GetRootPlayable(0).SetInputWeight(0, 1f);

        ConnectClips(animationPlayablesConfigs);
        
        _playableGraph.Play();
    }

    private void ConnectClips(AnimationPlayablesConfigs animationPlayablesConfigs)
    {
        var idle0 = AnimationClipPlayable.Create(_playableGraph, animationPlayablesConfigs.Idle0);
        var walk0 = AnimationClipPlayable.Create(_playableGraph, animationPlayablesConfigs.Walk0);
        var climb0 = AnimationClipPlayable.Create(_playableGraph, animationPlayablesConfigs.Climb0);

        idle0.GetAnimationClip().wrapMode = WrapMode.Loop;
        walk0.GetAnimationClip().wrapMode = WrapMode.Loop;
        climb0.GetAnimationClip().wrapMode = WrapMode.Loop;
        
        _animationMixerLocomotion.ConnectInput(0, idle0, 0);
        _animationMixerLocomotion.ConnectInput(1, walk0, 0);
        _animationMixerLocomotion.ConnectInput(2, climb0, 0);
    }

    public void UpdateLocomotion(float currentSpeed)
    {
        var weight = Mathf.InverseLerp(0f, 0.1f, currentSpeed);
        _animationMixerLocomotion.SetInputWeight(0, 1 - weight);
        _animationMixerLocomotion.SetInputWeight(1, weight);
    }

    public void PlayOneShotAnimationClip(AnimationClip animationClip)
    {
        if (_oneShotAnimationClip.IsValid() && _oneShotAnimationClip.GetAnimationClip() == animationClip)
        {
            return;
        }

        if (_blendInHandle != null && _blendOutHandle != null)
        {
            InterruptOneShotAnimationClip();
        }
        _oneShotAnimationClip = AnimationClipPlayable.Create(_playableGraph, animationClip);
        _animationMixerTopLevel.ConnectInput(1, _oneShotAnimationClip, 0);
        _animationMixerTopLevel.SetInputWeight(1, 1f);
        
        var blendDuration = Mathf.Max(0.1f, Mathf.Min(animationClip.length * 0.1f, animationClip.length / 2));
        
        BlendIn(blendDuration);
        BlendOut(blendDuration, animationClip.length - blendDuration);
    }

    private void BlendIn(float duration)
    {
        _blendInHandle = _coroutineRunner.StartCoroutine(Blend(duration, blendTime =>
        {
            var weight = Mathf.Lerp(1, 0, blendTime);
            _animationMixerTopLevel.SetInputWeight(0, weight);
            _animationMixerTopLevel.SetInputWeight(1, 1 - weight);

        }));
    }

    private void BlendOut(float duration, float delay)
    {
        _blendOutHandle = _coroutineRunner.StartCoroutine(Blend(duration, blendTime =>
        {
            var weight = Mathf.Lerp(0, 1, blendTime);
            _animationMixerTopLevel.SetInputWeight(0, weight);
            _animationMixerTopLevel.SetInputWeight(1, 1 - weight);

        }, delay, DisconnectOneShot));
    }
    
    private static IEnumerator Blend(float duration, Action<float> blendCallback, float delay = 0, Action finish = null)
    {
        float timePassed = 0;
        while (timePassed < delay)
        {
            timePassed += Time.deltaTime;
            yield return null;
        }

        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / duration);
            blendCallback?.Invoke(t);
            yield return null;
        }

        blendCallback?.Invoke(1f);
        finish?.Invoke();
    }

    private void InterruptOneShotAnimationClip()
    {
        _coroutineRunner.StopCoroutine(_blendInHandle);
        _coroutineRunner.StopCoroutine(_blendOutHandle);
        
        _animationMixerTopLevel.SetInputWeight(0, 1f);
        _animationMixerTopLevel.SetInputWeight(1, 0f);

        if (_oneShotAnimationClip.IsValid())
        {
            DisconnectOneShot();
        }
    }

    private void DisconnectOneShot()
    {
        _animationMixerTopLevel.DisconnectInput(1);
        _playableGraph.DestroyPlayable(_oneShotAnimationClip);
    }

    public void Destroy()
    {
        if (_blendInHandle != null)  _coroutineRunner?.StopCoroutine(_blendInHandle);
        if (_blendOutHandle != null) _coroutineRunner?.StopCoroutine(_blendOutHandle);
        
        if (_animationMixerTopLevel.IsValid())
        {
            _animationMixerTopLevel.SetInputWeight(0, 1f);
            _animationMixerTopLevel.SetInputWeight(1, 0f);
        }

        if (_oneShotAnimationClip.IsValid())
        {
            DisconnectOneShot();          
        }

        if (_playableGraph.IsValid())
        {
            _playableGraph.Destroy();
        }

        _blendInHandle = null;
        _blendOutHandle = null;
    }
}
