using System;

namespace Ignis
{
    public interface IContainer
    {
        IEntityManager EntityManager { get; }
        IComponentCollection<T> GetStorageFor<T>() where T : new();
        dynamic GetStorageFor(Type type);
        T GetSystem<T>() where T : SystemBase;
        SystemBase GetSystem(Type type);

        IContainer AddComponent<TComponent>()
            where TComponent : struct;

        IContainer AddComponent<TComponent, TStorage>()
            where TComponent : struct
            where TStorage : class, IComponentCollection<TComponent>;

        IContainer AddSystem<TSystem>()
            where TSystem : SystemBase;

        IContainer AddParallelSystems<T1, T2>()
            where T1 : SystemBase
            where T2 : SystemBase;

        IContainer AddParallelSystems<T1, T2, T3>()
            where T1 : SystemBase
            where T2 : SystemBase
            where T3 : SystemBase;

        IContainer AddParallelSystems<T1, T2, T3, T4>()
            where T1 : SystemBase
            where T2 : SystemBase
            where T3 : SystemBase
            where T4 : SystemBase;

        IContainer Register<TInterface, TImpl>()
            where TInterface : class
            where TImpl : class, TInterface;

        TInterface Resolve<TInterface>() where TInterface : class;

        IContainer Build();
        void ExecuteSystems();
    }
}