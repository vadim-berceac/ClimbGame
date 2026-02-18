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

    private readonly AnimationMixerPlayable _animationMixerLocomotionPrev;
    private readonly AnimationMixerPlayable _animationMixerLocomotionBlend;
    private BakedLocomotion _currentBakedLocomotion;
    private Coroutine _locomotionBlendHandle;
    private const float LocomotionBlendDuration = 0.25f;

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

        _animationMixerLocomotion      = AnimationMixerPlayable.Create(_playableGraph, 5);
        _animationMixerLocomotionPrev  = AnimationMixerPlayable.Create(_playableGraph, 5);
        _animationMixerLocomotionBlend = AnimationMixerPlayable.Create(_playableGraph, 2);

        _animationMixerLocomotionBlend.ConnectInput(0, _animationMixerLocomotionPrev, 0);
        _animationMixerLocomotionBlend.ConnectInput(1, _animationMixerLocomotion,     0);
        _animationMixerLocomotionBlend.SetInputWeight(0, 0f);
        _animationMixerLocomotionBlend.SetInputWeight(1, 1f);

        _animationMixerTopLevel.ConnectInput(0, _animationMixerLocomotionBlend, 0);

        _bakedLocomotions = new BakedLocomotion[locomotionConfigs.Length];
        for (var i = 0; i < _bakedLocomotions.Length; i++)
            _bakedLocomotions[i] = this.BakeLocomotion(locomotionConfigs[i], _playableGraph);

        _playableGraph.GetRootPlayable(0).SetInputWeight(0, 1f);

        _scriptPlayableOutput = ScriptPlayableOutput.Create(_playableGraph, "Footsteps");
        _footstepsPlayable = ScriptPlayable<FootstepsPlayablesBehavior>.Create(_playableGraph);

        _playableGraph.Play();
    }

    public void SetLocomotion(LocomotionType locomotionType)
    {
        var next = _bakedLocomotions.FirstOrDefault(b => b.Locomotion == locomotionType);
        if (next.Equals(default(BakedLocomotion))) return;
        if (next.Equals(_currentBakedLocomotion))  return;

        if (_locomotionBlendHandle != null)
        {
            _coroutineRunner.StopCoroutine(_locomotionBlendHandle);
            _locomotionBlendHandle = null;
        }

        var weights = new float[5];
        for (var i = 0; i < 5; i++)
            weights[i] = _animationMixerLocomotion.GetInputWeight(i);
        
        for (var i = 0; i < 5; i++)
        {
            this.DisconnectPlayable(_animationMixerLocomotion,     _playableGraph, i);
            this.DisconnectPlayable(_animationMixerLocomotionPrev, _playableGraph, i);
        }

        if (!_currentBakedLocomotion.Equals(default(BakedLocomotion)))
        {
            this.ConnectToMixer(_animationMixerLocomotionPrev, _playableGraph, _currentBakedLocomotion);
            for (var i = 0; i < 5; i++)
                _animationMixerLocomotionPrev.SetInputWeight(i, weights[i]);
        }

        this.ConnectToMixer(_animationMixerLocomotion, _playableGraph, next);
        for (var i = 0; i < 5; i++)
            _animationMixerLocomotion.SetInputWeight(i, weights[i]);

        _currentBakedLocomotion = next;

        _animationMixerLocomotionBlend.SetInputWeight(0, 1f);
        _animationMixerLocomotionBlend.SetInputWeight(1, 0f);

        _locomotionBlendHandle = _coroutineRunner.StartCoroutine(BlendLocomotion(LocomotionBlendDuration));
    }

    private IEnumerator BlendLocomotion(float duration)
    {
        var elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var currWeight = Mathf.Clamp01(elapsed / duration);
            _animationMixerLocomotionBlend.SetInputWeight(0, 1f - currWeight);
            _animationMixerLocomotionBlend.SetInputWeight(1, currWeight);
            yield return null;
        }
        _animationMixerLocomotionBlend.SetInputWeight(0, 0f);
        _animationMixerLocomotionBlend.SetInputWeight(1, 1f);
        _locomotionBlendHandle = null;
    }

    public void ConnectFootSteps(AudioSet audioSet)
    {
        var behavior = _footstepsPlayable.GetBehaviour();
        behavior.PlayableGraph = _playableGraph;
        behavior.Footsteps = audioSet.Set;
        behavior.AudioSource = new ExposedReference<AudioSource> { defaultValue = _audioSource };
        _scriptPlayableOutput.SetSourcePlayable(_footstepsPlayable);
    }

    public void OnFootsteps() => _footstepsPlayable.GetBehaviour().PlayFootsteps();

    public void UpdateLocomotion(Vector2 input)
    {
        _smoothedForward = Mathf.Lerp(_smoothedForward, input.y, 9f * Time.deltaTime);
        _smoothedStrafe  = Mathf.Lerp(_smoothedStrafe,  input.x, 9f * Time.deltaTime);

        var fwd    = _smoothedForward;
        var strafe = _smoothedStrafe;

        var moveStrength = Mathf.Clamp01(new Vector2(Mathf.Abs(strafe), Mathf.Abs(fwd)).magnitude);

        if (moveStrength < 0.015f)
        {
            SetLocomotionWeights(1f, 0f, 0f, 0f, 0f);
            return;
        }

        var idleWeight = 1f - moveStrength;

        var contribForward  = Mathf.Max(0f,  fwd);
        var contribBackward = Mathf.Max(0f, -fwd);
        var contribLeft     = Mathf.Max(0f, -strafe);
        var contribRight    = Mathf.Max(0f,  strafe);
        var totalContrib    = contribForward + contribBackward + contribLeft + contribRight;

        float wFwd = 0f, wBwd = 0f, wLeft = 0f, wRight = 0f;
        if (totalContrib > 0.00001f)
        {
            var scale = moveStrength / totalContrib;
            wFwd   = contribForward  * scale;
            wBwd   = contribBackward * scale;
            wLeft  = contribLeft     * scale;
            wRight = contribRight    * scale;
        }

        var sum = idleWeight + wFwd + wBwd + wLeft + wRight;
        if (Mathf.Abs(sum - 1f) > 0.001f)
        {
            var n = 1f / Mathf.Max(sum, 0.0001f);
            idleWeight *= n; wFwd *= n; wBwd *= n; wLeft *= n; wRight *= n;
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
            return;

        if (_blendInHandle != null && _blendOutHandle != null)
            InterruptOneShotAnimationClip();

        _oneShotAnimationClip = AnimationClipPlayable.Create(_playableGraph, animationClip);
        _animationMixerTopLevel.ConnectInput(1, _oneShotAnimationClip, 0);
        _animationMixerTopLevel.SetInputWeight(1, 1f);

        var blendDuration = Mathf.Max(0.1f, Mathf.Min(animationClip.length * 0.1f, animationClip.length / 2));
        BlendIn(blendDuration);
        BlendOut(blendDuration, animationClip.length - blendDuration);
    }

    private void BlendIn(float duration)
    {
        _blendInHandle = _coroutineRunner.StartCoroutine(Blend(duration, t =>
        {
            _animationMixerTopLevel.SetInputWeight(0, Mathf.Lerp(1f, 0f, t));
            _animationMixerTopLevel.SetInputWeight(1, Mathf.Lerp(0f, 1f, t));
        }));
    }

    private void BlendOut(float duration, float delay)
    {
        _blendOutHandle = _coroutineRunner.StartCoroutine(Blend(duration, t =>
        {
            _animationMixerTopLevel.SetInputWeight(0, Mathf.Lerp(0f, 1f, t));
            _animationMixerTopLevel.SetInputWeight(1, Mathf.Lerp(1f, 0f, t));
        }, delay, DisconnectOneShot));
    }

    private static IEnumerator Blend(float duration, Action<float> onTick,
        float delay = 0f, Action onFinish = null)
    {
        for (var t = 0f; t < delay; t += Time.deltaTime) yield return null;

        var elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            onTick?.Invoke(Mathf.Clamp01(elapsed / duration));
            yield return null;
        }
        onTick?.Invoke(1f);
        onFinish?.Invoke();
    }

    private void InterruptOneShotAnimationClip()
    {
        _coroutineRunner.StopCoroutine(_blendInHandle);
        _coroutineRunner.StopCoroutine(_blendOutHandle);
        _animationMixerTopLevel.SetInputWeight(0, 1f);
        _animationMixerTopLevel.SetInputWeight(1, 0f);
        DisconnectOneShot();
    }

    private void DisconnectOneShot() =>
        this.DisconnectPlayable(_animationMixerTopLevel, _playableGraph, 1, true);

    public void Destroy()
    {
        if (_blendInHandle         != null) _coroutineRunner?.StopCoroutine(_blendInHandle);
        if (_blendOutHandle        != null) _coroutineRunner?.StopCoroutine(_blendOutHandle);
        if (_locomotionBlendHandle != null) _coroutineRunner?.StopCoroutine(_locomotionBlendHandle);

        if (_animationMixerTopLevel.IsValid())
        {
            _animationMixerTopLevel.SetInputWeight(0, 1f);
            _animationMixerTopLevel.SetInputWeight(1, 0f);
        }

        if (_oneShotAnimationClip.IsValid()) DisconnectOneShot();
        if (_playableGraph.IsValid()) _playableGraph.Destroy();

        _blendInHandle = _blendOutHandle = _locomotionBlendHandle = null;
    }
}