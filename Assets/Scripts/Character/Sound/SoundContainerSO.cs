using System;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundContainerSO", menuName = "ZenjectInstallers/SoundContainerSO")]
public class SoundContainerSO : ScriptableObject
{
    [field: SerializeField] public CharacterSoundContainer SoundContainer { get; private set; }
}

[Serializable]
public struct CharacterSoundContainer
{
    [SerializeField] private AudioSet[] audioSets;

    public AudioSet GetAudioSet(LocomotionType locomotionType)
    {
        return audioSets.FirstOrDefault(a => a.LocomotionType == locomotionType);
    }
}
