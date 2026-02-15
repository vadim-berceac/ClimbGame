using UnityEngine;
using Zenject;

public class SceneBaseInstaller : MonoInstaller
{
    [SerializeField] private GameObject playerInputPrefab;
    
    public override void InstallBindings()
    {
        Container.Bind<PlayerInput>().FromComponentInNewPrefab(playerInputPrefab).AsSingle().NonLazy();
    }
}