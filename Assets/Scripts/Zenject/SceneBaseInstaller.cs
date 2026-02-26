using UnityEngine;
using Zenject;

public class SceneBaseInstaller : MonoInstaller
{
    [SerializeField] private GameObject mainCameraPrefab;
    [SerializeField] private GameObject selectorPrefab;
    [SerializeField] private PlayerInputSO playerInputSO;
    [SerializeField] private AnimationContainerSO animationContainerSO;
    [SerializeField] private SoundContainerSO soundContainerSO;
    
    public override void InstallBindings()
    {
        Container.Bind<Camera>().FromComponentInNewPrefab(mainCameraPrefab).AsSingle().NonLazy();
        Container.Bind<CharacterSelector>().FromComponentInNewPrefab(selectorPrefab).AsSingle().NonLazy();
        
        Container.BindInterfacesAndSelfTo<PlayerInputSO>()
            .FromScriptableObject(playerInputSO)
            .AsSingle();
        
        Container
            .Bind<CharacterAnimationContainer>()
            .FromInstance(animationContainerSO.AnimationContainer)
            .AsSingle();
        
        Container
            .Bind<CharacterSoundContainer>()
            .FromInstance(soundContainerSO.SoundContainer)
            .AsSingle();
    }
}