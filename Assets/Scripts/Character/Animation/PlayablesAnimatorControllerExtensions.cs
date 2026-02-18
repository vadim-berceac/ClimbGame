using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public static class PlayablesAnimatorControllerExtensions
{
    public static BakedLocomotion BakeLocomotion(this PlayablesAnimatorController controller,
        LocomotionConfigs locomotionConfigs, PlayableGraph playableGraph)
    {
        var idle = AnimationClipPlayable.Create(playableGraph, locomotionConfigs.Idle);
        var moveForward = AnimationClipPlayable.Create(playableGraph, locomotionConfigs.MoveForward);
        var moveBackward = AnimationClipPlayable.Create(playableGraph, locomotionConfigs.MoveBackward);
        var strafeLeft = AnimationClipPlayable.Create(playableGraph, locomotionConfigs.StrafeLeft);
        var strafeRight = AnimationClipPlayable.Create(playableGraph, locomotionConfigs.StrafeRight);

        idle.GetAnimationClip().wrapMode = WrapMode.Loop;
        moveForward.GetAnimationClip().wrapMode = WrapMode.Loop;
        moveBackward.GetAnimationClip().wrapMode = WrapMode.Loop;
        strafeLeft.GetAnimationClip().wrapMode = WrapMode.Loop;
        strafeRight.GetAnimationClip().wrapMode = WrapMode.Loop;
        
        return new BakedLocomotion(locomotionConfigs.Locomotion, idle, moveForward, moveBackward, strafeLeft, strafeRight);
    }

    public static void ConnectToMixer(this PlayablesAnimatorController controller, AnimationMixerPlayable mixer, PlayableGraph graph,
        BakedLocomotion locomotion)
    {
        controller.DisconnectPlayable(mixer, graph, 0);
        controller.DisconnectPlayable(mixer, graph, 1);
        controller.DisconnectPlayable(mixer, graph, 2);
        controller.DisconnectPlayable(mixer, graph, 3);
        controller.DisconnectPlayable(mixer, graph, 4);
        
        mixer.ConnectInput(0, locomotion.Idle, 0);
        mixer.ConnectInput(1, locomotion.MoveForward, 0);
        mixer.ConnectInput(2, locomotion.MoveBackward, 0);
        mixer.ConnectInput(3, locomotion.StrafeLeft, 0);
        mixer.ConnectInput(4, locomotion.StrafeRight, 0);
    }
    
    public static void DisconnectPlayable(this PlayablesAnimatorController controller,
        AnimationMixerPlayable mixer, PlayableGraph graph, int inputId, bool destroy = false)
    {
        if (inputId < 0 || inputId >= mixer.GetInputCount()) return;
        
        var clip = mixer.GetInput(inputId);
        
        if(!clip.IsValid()) return;
        
        mixer.DisconnectInput(inputId);
        
        if(!destroy) return;
        
        graph.DestroyPlayable(clip);
    }
}
