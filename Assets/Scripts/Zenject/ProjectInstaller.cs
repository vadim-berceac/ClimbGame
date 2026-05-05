using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using Zenject;

public class ProjectInstaller : MonoInstaller
{
    [SerializeField] private TextDatabase textDatabase;
    public override void InstallBindings()
    {
        Container
            .Bind<TextDatabase>()
            .FromInstance(textDatabase)
            .AsSingle()
            .NonLazy();
        
        Container.Bind<NetworkManager>()
            .FromComponentInHierarchy()
            .AsSingle()
            .NonLazy();

        Container.Bind<UnityTransport>()
            .FromComponentInHierarchy()
            .AsSingle()
            .NonLazy();
    }
}