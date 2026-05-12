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
    [SerializeField] private LocomotionConfigsSO[] locomotionConfigsSO;

    public LocomotionConfigsSO[] LocomotionConfigs => locomotionConfigsSO;

    public MoveSpeedData GetMoveSpeedData(LocomotionType locomotionType)
    {
        return locomotionConfigsSO.FirstOrDefault(l => l.LocomotionConfigs.Locomotion == locomotionType).LocomotionConfigs.MoveSpeedData;
    }

    public LocomotionConfigs GetLocomotionConfigs(LocomotionType locomotionType)
    {
        return locomotionConfigsSO.FirstOrDefault(l => l.LocomotionConfigs.Locomotion == locomotionType).LocomotionConfigs;
    }
}