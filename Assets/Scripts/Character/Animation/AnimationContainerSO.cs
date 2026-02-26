using System;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "AnimationContainerSO", menuName = "ZenjectInstallers/AnimationContainerSO")]
public class AnimationContainerSO : ScriptableObject
{
    [field: SerializeField] public CharacterAnimationContainer AnimationContainer {get; private set;}
}

[Serializable]
public struct CharacterAnimationContainer
{
    [Header("Locomotion")]
    [SerializeField] private LocomotionConfigs[] locomotionConfigs;
    [SerializeField] private LocomotionType defaultLocomotion;

    public LocomotionConfigs[] LocomotionConfigs => locomotionConfigs;
    public LocomotionType DefaultLocomotion => defaultLocomotion;

    public MoveSpeedData GetMoveSpeedData(LocomotionType locomotionType)
    {
        return locomotionConfigs.FirstOrDefault(l => l.Locomotion == locomotionType).MoveSpeedData;
    }
}