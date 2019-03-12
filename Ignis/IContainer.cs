namespace Ignis
{
    public interface IContainer
    {
        IEntityManager EntityManager { get; }
        IComponentCollection<T> GetStorageFor<T>() where T : struct;
        T GetSystem<T>() where T : SystemBase;

        IContainer AddComponent<TComponent>()
            where TComponent : struct;

        IContainer AddComponent<TComponent, TStorage>()
            where TComponent : struct
            where TStorage : IComponentCollection<TComponent>;

        IContainer AddSystem<TSystem>()
            where TSystem : SystemBase;

        IContainer Register<TInterface, TImpl>()
            where TInterface : class
            where TImpl : TInterface;

        TInterface Resolve<TInterface>() where TInterface : class;

        IContainer Build();
        void ExecuteSystems();
    }
}