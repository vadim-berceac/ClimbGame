using UnityEngine;
using Zenject;

public class CharacterSelfInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<AIInput>().FromInstance(GetComponentInChildren<AIInput>()).AsSingle();
        
        Container.Bind<CharacterCore>().FromInstance(GetComponentInParent<CharacterCore>()).AsSingle();
        
        Container.Bind<CharacterController>().FromInstance(GetComponentInParent<CharacterController>()).AsSingle();
        
        Container.Bind<Animator>().FromInstance(GetComponentInParent<Animator>()).AsSingle();
        
        Container.Bind<AudioSource>().FromInstance(GetComponentInParent<AudioSource>()).AsSingle();
    }
}