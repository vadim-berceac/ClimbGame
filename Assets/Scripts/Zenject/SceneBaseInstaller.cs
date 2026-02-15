using UnityEngine;
using Zenject;

public class SceneBaseInstaller : MonoInstaller
{
    [SerializeField] private GameObject playerInputPrefab;
    [SerializeField] private GameObject mainCameraPrefab;
    [SerializeField] private GameObject selectorPrefab;
    
    public override void InstallBindings()
    {
        Container.Bind<PlayerInput>().FromComponentInNewPrefab(playerInputPrefab).AsSingle().NonLazy();
        Container.Bind<Camera>().FromComponentInNewPrefab(mainCameraPrefab).AsSingle().NonLazy();
        Container.Bind<CharacterSelector>().FromComponentInNewPrefab(selectorPrefab).AsSingle().NonLazy();
    }
}