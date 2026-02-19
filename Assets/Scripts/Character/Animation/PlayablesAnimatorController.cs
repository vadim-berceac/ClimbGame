using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class PlayablesAnimatorController
{
    #region Fields

    private readonly MonoBehaviour _coroutineRunner;
    private readonly AudioSource   _audioSource;
    private          PlayableGraph _playableGraph;

    // Graph nodes
    private readonly AnimationMixerPlayable _animationMixerTopLevel;
    private readonly AnimationMixerPlayable _animationMixerLocomotionBlend;
    private readonly AnimationMixerPlayable _animationMixerLocomotion;
    private readonly AnimationMixerPlayable _animationMixerLocomotionPrev;

    // Footsteps
    private readonly ScriptPlayableOutput                       _scriptPlayableOutput;
    private          ScriptPlayable<FootstepsPlayablesBehavior> _footstepsPlayable;

    // Frame events
    private ScriptPlayable<AnimationFrameEventsBehavior>        _frameEventsPlayable;
    private Func<string, (Action onEnter, Action onExit, Action onTick)> _eventTagResolver;

    // Locomotion state
    private readonly BakedLocomotion[] _bakedLocomotions;
    private          BakedLocomotion   _currentBakedLocomotion;
    private          Coroutine         _locomotionBlendHandle;
    private const    float             LocomotionBlendDuration = 0.25f;

    // OneShot state
    private AnimationClipPlayable _oneShotClip;
    private Coroutine             _blendInHandle;
    private Coroutine             _blendOutHandle;

    // Locomotion smoothing
    private float _smoothedForward;
    private float _smoothedStrafe;

    #endregion

    #region Constructor

    public PlayablesAnimatorController(
        MonoBehaviour       coroutineRunner,
        Animator            animator,
        AudioSource         audioSource,
        LocomotionConfigs[] locomotionConfigs)
    {
        _coroutineRunner = coroutineRunner;
        _audioSource     = audioSource;

        _playableGraph = PlayableGraph.Create("AnimatorController");

        var output = AnimationPlayableOutput.Create(_playableGraph, "Animation", animator);

        _animationMixerTopLevel = AnimationMixerPlayable.Create(_playableGraph, 3);
        output.SetSourcePlayable(_animationMixerTopLevel);

        (_animationMixerLocomotionBlend,
         _animationMixerLocomotion,
         _animationMixerLocomotionPrev) = CreateLocomotionGraph();

        _animationMixerTopLevel.ConnectInput(0, _animationMixerLocomotionBlend, 0, 1f);

        _bakedLocomotions = BakeAllLocomotions(locomotionConfigs);

        _scriptPlayableOutput = ScriptPlayableOutput.Create(_playableGraph, "Footsteps");
        _footstepsPlayable    = ScriptPlayable<FootstepsPlayablesBehavior>.Create(_playableGraph);

        _frameEventsPlayable  = ScriptPlayable<AnimationFrameEventsBehavior>.Create(_playableGraph);
        _animationMixerTopLevel.ConnectInput(2, _frameEventsPlayable, 0, 0f);

        _playableGraph.Play();
    }

    private (AnimationMixerPlayable blend, AnimationMixerPlayable curr, AnimationMixerPlayable prev)
        CreateLocomotionGraph()
    {
        var curr  = AnimationMixerPlayable.Create(_playableGraph, 5);
        var prev  = AnimationMixerPlayable.Create(_playableGraph, 5);
        var blend = AnimationMixerPlayable.Create(_playableGraph, 2);

        blend.ConnectInput(0, prev, 0, 0f);
        blend.ConnectInput(1, curr, 0, 1f);

        return (blend, curr, prev);
    }

    private BakedLocomotion[] BakeAllLocomotions(LocomotionConfigs[] configs)
    {
        var result = new BakedLocomotion[configs.Length];
        for (var i = 0; i < configs.Length; i++)
            result[i] = this.BakeLocomotion(configs[i], _playableGraph);
        return result;
    }

    #endregion

    #region Locomotion

    /// <summary>
    /// Привязывает действия к тегам анимационных событий персонажа.
    /// Должен быть установлен до первого вызова SetLocomotion.
    /// </summary>
    public void SetEventTagResolver(Func<string, (Action onEnter, Action onExit, Action onTick)> resolver)
    {
        _eventTagResolver = resolver;
    }

    public void SetLocomotion(LocomotionType locomotionType)
    {
        var next = FindBakedLocomotion(locomotionType);
        if (next == null || next.Value == _currentBakedLocomotion) return;

        StopLocomotionBlend();
        SwapLocomotionSets(next.Value);
        StartLocomotionBlend();
        ApplyLocomotionFrameEvents(next.Value);
    }

    private void ApplyLocomotionFrameEvents(BakedLocomotion baked)
    {
        if (_eventTagResolver == null) return;

        foreach (var config in baked.CreateEventConfigs(_eventTagResolver))
            RegisterLocomotionFrameEventInternal(baked, config);
    }

    private void StopLocomotionBlend()
    {
        if (_locomotionBlendHandle == null) return;
        _coroutineRunner.StopCoroutine(_locomotionBlendHandle);
        _locomotionBlendHandle = null;
    }

    private void SwapLocomotionSets(BakedLocomotion next)
    {
        var weights = ReadLocomotionWeights();
        DisconnectBothLocomotionMixers();

        if (!_currentBakedLocomotion.Equals(default(BakedLocomotion)))
        {
            this.ConnectToMixer(_animationMixerLocomotionPrev, _playableGraph, _currentBakedLocomotion);
            ApplyWeightsToMixer(_animationMixerLocomotionPrev, weights);
        }

        this.ConnectToMixer(_animationMixerLocomotion, _playableGraph, next);
        ApplyWeightsToMixer(_animationMixerLocomotion, weights);

        _currentBakedLocomotion = next;
    }

    private void StartLocomotionBlend()
    {
        _animationMixerLocomotionBlend.SetInputWeight(0, 1f);
        _animationMixerLocomotionBlend.SetInputWeight(1, 0f);
        _locomotionBlendHandle = _coroutineRunner.StartCoroutine(BlendLocomotion(LocomotionBlendDuration));
    }

    private void DisconnectBothLocomotionMixers()
    {
        for (var i = 0; i < 5; i++)
        {
            this.DisconnectPlayable(_animationMixerLocomotion,     _playableGraph, i);
            this.DisconnectPlayable(_animationMixerLocomotionPrev, _playableGraph, i);
        }
    }

    private float[] ReadLocomotionWeights()
    {
        var weights = new float[5];
        for (var i = 0; i < 5; i++)
            weights[i] = _animationMixerLocomotion.GetInputWeight(i);
        return weights;
    }

    private static void ApplyWeightsToMixer(AnimationMixerPlayable mixer, float[] weights)
    {
        for (var i = 0; i < weights.Length; i++)
            mixer.SetInputWeight(i, weights[i]);
    }

    private IEnumerator BlendLocomotion(float duration)
    {
        var elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / duration);
            _animationMixerLocomotionBlend.SetInputWeight(0, 1f - t);
            _animationMixerLocomotionBlend.SetInputWeight(1, t);
            yield return null;
        }
        _animationMixerLocomotionBlend.SetInputWeight(0, 0f);
        _animationMixerLocomotionBlend.SetInputWeight(1, 1f);
        _locomotionBlendHandle = null;
    }

    public void UpdateLocomotion(Vector2 input)
    {
        _smoothedForward = Mathf.Lerp(_smoothedForward, input.y, 9f * Time.deltaTime);
        _smoothedStrafe  = Mathf.Lerp(_smoothedStrafe,  input.x, 9f * Time.deltaTime);

        var moveStrength = Mathf.Clamp01(
            new Vector2(Mathf.Abs(_smoothedStrafe), Mathf.Abs(_smoothedForward)).magnitude);

        if (moveStrength < 0.015f)
        {
            SetLocomotionWeights(1f, 0f, 0f, 0f, 0f);
            return;
        }

        var idleWeight = 1f - moveStrength;
        var (wFwd, wBwd, wLeft, wRight) = ComputeDirectionWeights(moveStrength);

        NormalizeLocomotionWeights(ref idleWeight, ref wFwd, ref wBwd, ref wLeft, ref wRight);
        SetLocomotionWeights(idleWeight, wFwd, wBwd, wLeft, wRight);
    }

    private (float fwd, float bwd, float left, float right) ComputeDirectionWeights(float moveStrength)
    {
        var contribFwd   = Mathf.Max(0f,  _smoothedForward);
        var contribBwd   = Mathf.Max(0f, -_smoothedForward);
        var contribLeft  = Mathf.Max(0f, -_smoothedStrafe);
        var contribRight = Mathf.Max(0f,  _smoothedStrafe);
        var total        = contribFwd + contribBwd + contribLeft + contribRight;

        if (total < 0.00001f) return (0f, 0f, 0f, 0f);

        var scale = moveStrength / total;
        return (contribFwd * scale, contribBwd * scale, contribLeft * scale, contribRight * scale);
    }

    private static void NormalizeLocomotionWeights(
        ref float idle, ref float fwd, ref float bwd, ref float left, ref float right)
    {
        var sum = idle + fwd + bwd + left + right;
        if (Mathf.Abs(sum - 1f) <= 0.001f) return;

        var n = 1f / Mathf.Max(sum, 0.0001f);
        idle *= n; fwd *= n; bwd *= n; left *= n; right *= n;
    }

    private void SetLocomotionWeights(float idle, float fwd, float bwd, float left, float right)
    {
        _animationMixerLocomotion.SetInputWeight(0, idle);
        _animationMixerLocomotion.SetInputWeight(1, fwd);
        _animationMixerLocomotion.SetInputWeight(2, bwd);
        _animationMixerLocomotion.SetInputWeight(3, left);
        _animationMixerLocomotion.SetInputWeight(4, right);
    }

    #endregion

    #region OneShot

    public void PlayOneShotAnimationClip(AnimationClip animationClip, params FrameEventConfig[] frameEvents)
    {
        if (IsAlreadyPlaying(animationClip)) return;
        if (OneShotIsActive()) InterruptOneShotAnimationClip();

        ConnectOneShotClip(animationClip);

        var blendDuration = ComputeBlendDuration(animationClip.length);
        BlendIn(blendDuration);
        BlendOut(blendDuration, animationClip.length - blendDuration);

        foreach (var config in frameEvents)
            RegisterOneShotFrameEvent(config);
    }

    private bool IsAlreadyPlaying(AnimationClip clip) =>
        _oneShotClip.IsValid() && _oneShotClip.GetAnimationClip() == clip;

    private bool OneShotIsActive() =>
        _blendInHandle != null && _blendOutHandle != null;

    private void ConnectOneShotClip(AnimationClip animationClip)
    {
        _oneShotClip = AnimationClipPlayable.Create(_playableGraph, animationClip);
        _animationMixerTopLevel.ConnectInput(1, _oneShotClip, 0);
        _animationMixerTopLevel.SetInputWeight(1, 0f);
    }

    private static float ComputeBlendDuration(float clipLength) =>
        Mathf.Max(0.1f, Mathf.Min(clipLength * 0.1f, clipLength / 2f));

    private void BlendIn(float duration)
    {
        _blendInHandle = _coroutineRunner.StartCoroutine(Blend(duration, t =>
        {
            _animationMixerTopLevel.SetInputWeight(0, 1f - t);
            _animationMixerTopLevel.SetInputWeight(1, t);
        }));
    }

    private void BlendOut(float duration, float delay)
    {
        _blendOutHandle = _coroutineRunner.StartCoroutine(Blend(duration, t =>
        {
            _animationMixerTopLevel.SetInputWeight(0, t);
            _animationMixerTopLevel.SetInputWeight(1, 1f - t);
        }, delay, DisconnectOneShot));
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

    private static IEnumerator Blend(float duration, Action<float> onTick, float delay = 0f, Action onFinish = null)
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

    #endregion

    #region Frame Events

    private void RegisterOneShotFrameEvent(FrameEventConfig config)
    {
        _oneShotClip.RegisterFrameEvent(
            _frameEventsPlayable.GetBehaviour(),
            config.FromFrame,
            config.ToFrame,
            weightProvider:  () => _animationMixerTopLevel.GetInputWeight(1),
            onEnter:         config.OnEnter,
            onExit:          config.OnExit,
            onTick:          config.OnTick,
            weightThreshold: config.WeightThreshold);
    }

    private void RegisterLocomotionFrameEventInternal(BakedLocomotion baked, LocomotionFrameEventConfig config)
    {
        var slotIndex = (int)config.ClipType;
        Func<float> weightProvider = () =>
        {
            var isCurr      = _currentBakedLocomotion.Equals(baked);
            var blendWeight = _animationMixerLocomotionBlend.GetInputWeight(isCurr ? 1 : 0);
            var mixer       = isCurr ? _animationMixerLocomotion : _animationMixerLocomotionPrev;
            return blendWeight * mixer.GetInputWeight(slotIndex);
        };

        baked.RegisterFrameEvent(
            _frameEventsPlayable.GetBehaviour(),
            config.ClipType,
            config.EventConfig.FromFrame,
            config.EventConfig.ToFrame,
            weightProvider,
            config.EventConfig.OnEnter,
            config.EventConfig.OnExit,
            config.EventConfig.OnTick,
            config.EventConfig.WeightThreshold);
    }

    #endregion

    #region Footsteps

    public void ConnectFootSteps(AudioSet audioSet)
    {
        var behavior = _footstepsPlayable.GetBehaviour();
        behavior.PlayableGraph = _playableGraph;
        behavior.Footsteps     = audioSet.Set;
        behavior.AudioSource   = new ExposedReference<AudioSource> { defaultValue = _audioSource };
        _scriptPlayableOutput.SetSourcePlayable(_footstepsPlayable);
    }

    public void OnFootsteps() => _footstepsPlayable.GetBehaviour().PlayFootsteps();

    #endregion

    #region Lifecycle

    public void Destroy()
    {
        _frameEventsPlayable.GetBehaviour().UnregisterAll();

        if (_blendInHandle         != null) _coroutineRunner?.StopCoroutine(_blendInHandle);
        if (_blendOutHandle        != null) _coroutineRunner?.StopCoroutine(_blendOutHandle);
        if (_locomotionBlendHandle != null) _coroutineRunner?.StopCoroutine(_locomotionBlendHandle);

        if (_animationMixerTopLevel.IsValid())
        {
            _animationMixerTopLevel.SetInputWeight(0, 1f);
            _animationMixerTopLevel.SetInputWeight(1, 0f);
        }

        if (_oneShotClip.IsValid()) DisconnectOneShot();
        if (_playableGraph.IsValid()) _playableGraph.Destroy();

        _blendInHandle = _blendOutHandle = _locomotionBlendHandle = null;
    }

    #endregion

    #region Helpers

    private BakedLocomotion? FindBakedLocomotion(LocomotionType type)
    {
        var result = _bakedLocomotions.FirstOrDefault(b => b.Locomotion == type);
        return result.Equals(default(BakedLocomotion)) ? null : result;
    }

    #endregion
}