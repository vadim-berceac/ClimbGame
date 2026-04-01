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
    }
}