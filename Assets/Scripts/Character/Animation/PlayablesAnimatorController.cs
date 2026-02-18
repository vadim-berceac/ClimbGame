using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class PlayablesAnimatorController
{
    private PlayableGraph _playableGraph;
    private readonly MonoBehaviour _coroutineRunner;
    private readonly AudioSource _audioSource;
    private readonly ScriptPlayableOutput _scriptPlayableOutput;
    private readonly AnimationMixerPlayable _animationMixerTopLevel;
    private readonly AnimationMixerPlayable _animationMixerLocomotion;
    private readonly BakedLocomotion[] _bakedLocomotions;
    
    private AnimationClipPlayable _oneShotAnimationClip;
    private ScriptPlayable<FootstepsPlayablesBehavior> _footstepsPlayable;

    private Coroutine _blendInHandle;
    private Coroutine _blendOutHandle;
    
    private float _smoothedForward = 0f;
    private float _smoothedStrafe  = 0f;
    
    public PlayablesAnimatorController(MonoBehaviour coroutineRunner, Animator animator, AudioSource audioSource,
        LocomotionConfigs[] locomotionConfigs)
    {
        _coroutineRunner = coroutineRunner;
        
        _audioSource = audioSource;
        
        _playableGraph = PlayableGraph.Create("AnimatorController");
        
        var output = AnimationPlayableOutput.Create(_playableGraph, "Animation", animator);

        _animationMixerTopLevel = AnimationMixerPlayable.Create(_playableGraph, 2);
        
        output.SetSourcePlayable(_animationMixerTopLevel);
        //
        _animationMixerLocomotion = AnimationMixerPlayable.Create(_playableGraph, 5);
        _animationMixerTopLevel.ConnectInput(0, _animationMixerLocomotion, 0);
        _bakedLocomotions = new BakedLocomotion[locomotionConfigs.Length];
        for (var i = 0; i < _bakedLocomotions.Length; i++)
        {
            _bakedLocomotions[i] = this.BakeLocomotion(locomotionConfigs[i], _playableGraph);
        }
        //
        _playableGraph.GetRootPlayable(0).SetInputWeight(0, 1f);
        
        _scriptPlayableOutput = ScriptPlayableOutput.Create(_playableGraph, "Footsteps");
        _footstepsPlayable = ScriptPlayable<FootstepsPlayablesBehavior>.Create(_playableGraph);
        
        _playableGraph.Play();
    }

    public void SetLocomotion(LocomotionType locomotionType)
    {
        var bakedLocomotion = _bakedLocomotions.FirstOrDefault(b => b.Locomotion == locomotionType);
        if(bakedLocomotion.Equals(default(BakedLocomotion))) return;
        this.ConnectToMixer(_animationMixerLocomotion, _playableGraph, bakedLocomotion);
    }

    public void ConnectFootSteps(AudioSet audioSet)
    {
        var behavior = _footstepsPlayable.GetBehaviour();
        behavior.PlayableGraph = _playableGraph;
        behavior.Footsteps = audioSet.Set;
        behavior.AudioSource = new ExposedReference<AudioSource> { defaultValue = _audioSource };
        _scriptPlayableOutput.SetSourcePlayable(_footstepsPlayable);
    }

    public void OnFootsteps()
    {
        _footstepsPlayable.GetBehaviour().PlayFootsteps();
    }
    
    public void UpdateLocomotion(Vector2 input)
    {
        _smoothedForward = Mathf.Lerp(_smoothedForward, input.y, 9f * Time.deltaTime);
        _smoothedStrafe  = Mathf.Lerp(_smoothedStrafe,  input.x, 9f * Time.deltaTime);

        var fwd   = _smoothedForward;
        var strafe = _smoothedStrafe;

        var moveStrength = new Vector2(Mathf.Abs(strafe), Mathf.Abs(fwd)).magnitude;
        moveStrength = Mathf.Clamp01(moveStrength); 

        if (moveStrength < 0.015f)
        {
            SetLocomotionWeights(1f, 0f, 0f, 0f, 0f);
            return;
        }

        var idleWeight = 1f - moveStrength;

        var contribForward  = Mathf.Max(0f, fwd);
        var contribBackward = Mathf.Max(0f, -fwd);
        var contribLeft     = Mathf.Max(0f, -strafe);
        var contribRight    = Mathf.Max(0f, strafe);
       
        var totalContrib = contribForward + contribBackward + contribLeft + contribRight;

        float wFwd = 0f, wBwd = 0f, wLeft = 0f, wRight = 0f;

        if (totalContrib > 0.00001f)
        {
            var scale = moveStrength / totalContrib;

            wFwd   = contribForward  * scale;
            wBwd   = contribBackward * scale;
            wLeft  = contribLeft     * scale;
            wRight = contribRight    * scale;
        }
        
        var sumCheck = idleWeight + wFwd + wBwd + wLeft + wRight;

        if (Mathf.Abs(sumCheck - 1f) > 0.001f)
        {
            var normalize = 1f / Mathf.Max(sumCheck, 0.0001f);
            idleWeight *= normalize;
            wFwd       *= normalize;
            wBwd       *= normalize;
            wLeft      *= normalize;
            wRight     *= normalize;
        }

        SetLocomotionWeights(idleWeight, wFwd, wBwd, wLeft, wRight);
    }
    
    private void SetLocomotionWeights(float idle, float fwd, float bwd, float left, float right)
    {
        _animationMixerLocomotion.SetInputWeight(0, idle);
        _animationMixerLocomotion.SetInputWeight(1, fwd);
        _animationMixerLocomotion.SetInputWeight(2, bwd);
        _animationMixerLocomotion.SetInputWeight(3, left);
        _animationMixerLocomotion.SetInputWeight(4, right);
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

        DisconnectOneShot();
    }

    private void DisconnectOneShot()
    {
        this.DisconnectPlayable(_animationMixerTopLevel, _playableGraph, 1, true);
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
