using UnityEngine;

[System.Serializable]
public struct LocomotionConfigs
{
    [field: SerializeField] public AnimationClip Idle { get; set; }
    [field: SerializeField] public AnimationClip MoveForward { get; set; }
    [field: SerializeField] public AnimationClip MoveBackward { get; set; }
    [field: SerializeField] public AnimationClip StrafeLeft { get; set; }
    [field: SerializeField] public AnimationClip StrafeRight { get; set; }
}

[System.Serializable]
public struct JumpConfigs
{
    [field: SerializeField] public AnimationClip Jump0 { get; set; }
}
