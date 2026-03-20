using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Events;

[System.Serializable]
public struct LocomotionConfigs
{
    [field: SerializeField] public LocomotionType     Locomotion    { get; set; }
    [field: SerializeField] public LocomotionDirection Direction    { get; set; }
    [field: SerializeField] public AnimationClip      Idle          { get; set; }
    [field: SerializeField] public AnimationClip      MoveForward   { get; set; }
    [field: SerializeField] public AnimationClip      MoveBackward  { get; set; }
    [field: SerializeField] public AnimationClip      StrafeLeft    { get; set; }
    [field: SerializeField] public AnimationClip      StrafeRight   { get; set; }
    [field: SerializeField] public MoveSpeedData      MoveSpeedData { get; set; }
    
    [field: SerializeField] public LocomotionClipFrameRange[] FrameRanges { get; set; }
}

public readonly struct BakedLocomotion
{
    public readonly LocomotionType        Locomotion;
    public readonly LocomotionDirection   Direction;
    public readonly LocomotionConfigs     Configs;       // ← храним исходный конфиг
    public readonly AnimationClipPlayable Idle;
    public readonly AnimationClipPlayable MoveForward;
    public readonly AnimationClipPlayable MoveBackward;
    public readonly AnimationClipPlayable StrafeLeft;
    public readonly AnimationClipPlayable StrafeRight;

    public BakedLocomotion(
        LocomotionConfigs     configs,
        AnimationClipPlayable idle,
        AnimationClipPlayable moveForward,
        AnimationClipPlayable moveBackward,
        AnimationClipPlayable strafeLeft,
        AnimationClipPlayable strafeRight)
    {
        Configs      = configs;
        Locomotion   = configs.Locomotion;
        Direction    = configs.Direction;
        Idle         = idle;
        MoveForward  = moveForward;
        MoveBackward = moveBackward;
        StrafeLeft   = strafeLeft;
        StrafeRight  = strafeRight;
    }

    /// <summary>
    /// Создаёт конфиги событий из сохранённых диапазонов фреймов,
    /// привязывая к ним действия через actionProvider.
    /// </summary>
    /// <param name="actionProvider">
    /// Принимает ClipType и диапазон, возвращает (onEnter, onExit, onTick).
    /// Если вернул null-экшены — диапазон пропускается.
    /// </param>
    public LocomotionFrameEventConfig[] CreateEventConfigs(
        Func<LocomotionClipType, LocomotionClipFrameRange, (Action onEnter, Action onExit, Action onTick)> actionProvider)
    {
        if (Configs.FrameRanges == null || Configs.FrameRanges.Length == 0)
            return Array.Empty<LocomotionFrameEventConfig>();

        var result = new List<LocomotionFrameEventConfig>(Configs.FrameRanges.Length);

        foreach (var range in Configs.FrameRanges)
        {
            var (onEnter, onExit, onTick) = actionProvider(range.ClipType, range);
            if (onEnter == null && onExit == null && onTick == null) continue;

            result.Add(new LocomotionFrameEventConfig(
                range.ClipType,
                new FrameEventConfig(
                    range.FromFrame,
                    range.ToFrame,
                    onEnter,
                    onExit,
                    onTick,
                    range.WeightThreshold)));
        }

        return result.ToArray();
    }
    
    public LocomotionFrameEventConfig[] CreateEventConfigs(
        Func<string, (Action onEnter, Action onExit, Action onTick)> tagResolver)
    {
        if (Configs.FrameRanges == null || Configs.FrameRanges.Length == 0)
            return Array.Empty<LocomotionFrameEventConfig>();

        var result = new List<LocomotionFrameEventConfig>(Configs.FrameRanges.Length);
        foreach (var range in Configs.FrameRanges)
        {
            var (onEnter, onExit, onTick) = tagResolver(range.EventTag);
            if (onEnter == null && onExit == null && onTick == null) continue;

            result.Add(new LocomotionFrameEventConfig(
                range.ClipType,
                new FrameEventConfig(
                    range.FromFrame,
                    range.ToFrame,
                    onEnter,
                    onExit,
                    onTick,
                    range.WeightThreshold)));
        }
        return result.ToArray();
    }

    public AnimationClipPlayable GetClip(LocomotionClipType type) => type switch
    {
        LocomotionClipType.Idle         => Idle,
        LocomotionClipType.MoveForward  => MoveForward,
        LocomotionClipType.MoveBackward => MoveBackward,
        LocomotionClipType.StrafeLeft   => StrafeLeft,
        LocomotionClipType.StrafeRight  => StrafeRight,
        _                               => Idle
    };
    
    public override bool Equals(object obj)
    {
        return obj is BakedLocomotion other && Equals(other);
    }
    public bool Equals(BakedLocomotion other)
    {
        return Locomotion == other.Locomotion &&
               Idle.Equals(other.Idle) &&
               MoveForward.Equals(other.MoveForward) &&
               MoveBackward.Equals(other.MoveBackward) &&
               StrafeLeft.Equals(other.StrafeLeft) &&
               StrafeRight.Equals(other.StrafeRight);
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(
            Locomotion,
            Idle,
            MoveForward,
            MoveBackward,
            StrafeLeft,
            StrafeRight);
    }
    public static bool operator ==(BakedLocomotion left, BakedLocomotion right)
    {
        return left.Equals(right);
    }
    public static bool operator !=(BakedLocomotion left, BakedLocomotion right)
    {
        return !(left == right);
    }
}

[System.Serializable]
public struct AudioSet
{
    [field: SerializeField] public LocomotionType LocomotionType { get; set; }
    [field: SerializeField] public AudioClip[] Set { get; set; }
}

[Serializable]
public struct LocomotionClipFrameRange
{
    [field: SerializeField] public string             EventTag        { get; set; }
    [field: SerializeField] public LocomotionClipType ClipType        { get; set; }
    [field: SerializeField] public int                FromFrame       { get; set; }
    [field: SerializeField] public int                ToFrame         { get; set; }
    [field: SerializeField] public float              WeightThreshold { get; set; }
}

public enum LocomotionType
{
    Walk0 = 0,
    Run0 = 1,
    Climb0 = 2,
    Crouch0 = 3,
    Fall0 = 4,
    Jump0 = 5
}

public enum LocomotionClipType
{
    Idle        = 0,
    MoveForward = 1,
    MoveBackward = 2,
    StrafeLeft  = 3,
    StrafeRight = 4
}

public enum LocomotionDirection
{
    Horizontal = 0,
    Vertical = 1,
    Spherical = 2 // для плавания и полета
}
