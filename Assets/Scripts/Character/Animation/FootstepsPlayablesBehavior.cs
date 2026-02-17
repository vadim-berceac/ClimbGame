using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Playables;

public class FootstepsPlayablesBehavior : PlayableBehaviour
{
    public AudioClip[] Footsteps;
    public ExposedReference<AudioSource> AudioSource;
    public PlayableGraph PlayableGraph;
    
    private AudioSource _resolvedAudioSource;
    private AudioMixerPlayable _mixerPlayable;
    private AudioClipPlayable _currentAudioClip;

    public override void OnGraphStart(Playable playable)
    {
        var resolver = playable.GetGraph().GetResolver();
        _resolvedAudioSource = AudioSource.Resolve(resolver);

        _mixerPlayable = AudioMixerPlayable.Create(PlayableGraph, 1);
        var audioOutput = AudioPlayableOutput.Create(PlayableGraph, "Footsteps", _resolvedAudioSource);
        audioOutput.SetSourcePlayable(_mixerPlayable);
    }

    public void PlayFootsteps()
    {
        if(_resolvedAudioSource == null || Footsteps == null || Footsteps.Length == 0) return;
        
       var audioClip = Footsteps[Random.Range(0, Footsteps.Length)];
       var playableAudioCLip = AudioClipPlayable.Create(PlayableGraph, audioClip, false);

       if (_currentAudioClip.IsValid())
       {
           _mixerPlayable.DisconnectInput(0);
           PlayableGraph.DestroyPlayable(_currentAudioClip);
       }
       
       PlayableGraph.Connect(playableAudioCLip, 0, _mixerPlayable, 0);
       _mixerPlayable.SetInputWeight(0, 1.0f);
       
       _currentAudioClip = playableAudioCLip;
    }
}
