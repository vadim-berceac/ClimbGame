using System.Linq;
using UnityEngine;

public class SoundContainer : MonoBehaviour
{
    [SerializeField] private AudioSet[] audioSets;

    public AudioSet GetAudioSet(LocomotionType locomotionType)
    {
        return audioSets.FirstOrDefault(a => a.LocomotionType == locomotionType);
    }
}
    