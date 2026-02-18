using System;
using UnityEngine;
using UnityEngine.Animations;

[System.Serializable]
public struct LocomotionConfigs
{
    [field: SerializeField] public LocomotionType Locomotion { get; set; }
    [field: SerializeField] public AnimationClip Idle { get; set; }
    [field: SerializeField] public AnimationClip MoveForward { get; set; }
    [field: SerializeField] public AnimationClip MoveBackward { get; set; }
    [field: SerializeField] public AnimationClip StrafeLeft { get; set; }
    [field: SerializeField] public AnimationClip StrafeRight { get; set; }
    [field: SerializeField] public MoveSpeedData MoveSpeedData { get; set; }
}

public readonly struct BakedLocomotion
{
    public readonly LocomotionType Locomotion;
    public readonly AnimationClipPlayable Idle;
    public readonly AnimationClipPlayable MoveForward;
    public readonly AnimationClipPlayable MoveBackward;
    public readonly AnimationClipPlayable StrafeLeft;
    public readonly AnimationClipPlayable StrafeRight;

    public BakedLocomotion(LocomotionType locomotion, AnimationClipPlayable idle,
        AnimationClipPlayable moveForward, AnimationClipPlayable moveBackward,
        AnimationClipPlayable strafeLeft, AnimationClipPlayable strafeRight)
    {
        Locomotion = locomotion;
        Idle = idle;
        MoveForward = moveForward;
        MoveBackward = moveBackward;
        StrafeLeft = strafeLeft;
        StrafeRight = strafeRight;
    }
    
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
public struct JumpConfigs
{
    [field: SerializeField] public AnimationClip JumpStart0 { get; set; }
    [field: SerializeField] public AnimationClip JumpEnd0 { get; set; }
    [field: SerializeField] public float JumpHeight { get; set; }
}

[System.Serializable]
public struct AudioSet
{
    [field: SerializeField] public AudioClip[] Set { get; set; }
}

public enum LocomotionType
{
    Walk0 = 0,
    Run0 = 1,
    Climb0 = 2,
    Crouch0 = 3,
    Fall0 = 4,
}
