using UnityEngine;
using Zenject;

public class CharacterSelfInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<AIInput>().FromInstance(transform.parent.GetComponentInChildren<AIInput>()).AsSingle();
        
        Container.Bind<AnimatedModelTag>().FromInstance(transform.parent.GetComponentInChildren<AnimatedModelTag>()).AsSingle();
        
        Container.Bind<CoreController>().FromInstance(GetComponentInParent<CoreController>()).AsSingle();
        
        Container.Bind<CharacterController>().FromInstance(GetComponentInParent<CharacterController>()).AsSingle();
        
        Container.Bind<Animator>().FromInstance(GetComponentInParent<Animator>()).AsSingle();
        
        Container.Bind<AudioSource>().FromInstance(GetComponentInParent<AudioSource>()).AsSingle();
        
        Container.Bind<CharacterAnimationEvents>().FromInstance(GetComponentInParent<CharacterAnimationEvents>()).AsSingle();
        
        Container.Bind<CharacterEventsContainer>().FromInstance(GetComponentInParent<CharacterEventsContainer>()).AsSingle();
        
        Container.Bind<CharacterPresetLoader>().FromInstance(GetComponentInParent<CharacterPresetLoader>()).AsSingle();
        
        Container.Bind<CharacterSlots>().AsSingle();
        
        Container.Bind<EquipmentManager>().AsSingle();
        
        Container.Bind<Inventory>().AsSingle();
    }
}