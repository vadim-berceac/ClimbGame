using UnityEngine;

[System.Serializable]
public struct AnimationPlayablesConfigs
{
    [field: SerializeField] public AnimationClip Idle0 { get; set; }
    [field: SerializeField] public AnimationClip Walk0 { get; set; }
    [field: SerializeField] public AnimationClip Climb0 { get; set; }
    [field: SerializeField] public AnimationClip Jump0 { get; set; }
}
