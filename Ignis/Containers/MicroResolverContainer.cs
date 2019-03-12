namespace Ignis.Containers
{
    public class MicroResolverContainer : IContainer
    {
        public IEntityManager EntityManager => throw new System.NotImplementedException();

        public IContainer AddComponent<TComponent>() where TComponent : struct
        {
            throw new System.NotImplementedException();
        }

        public IContainer AddComponent<TComponent, TStorage>()
            where TComponent : struct
            where TStorage : IComponentCollection<TComponent>
        {
            throw new System.NotImplementedException();
        }

        public IContainer AddSystem<TSystem>() where TSystem : SystemBase
        {
            throw new System.NotImplementedException();
        }

        public IContainer Build()
        {
            throw new System.NotImplementedException();
        }

        public void ExecuteSystems()
        {
            throw new System.NotImplementedException();
        }

        public IComponentCollection<T> GetStorageFor<T>() where T : struct
        {
            throw new System.NotImplementedException();
        }

        public T GetSystem<T>() where T : SystemBase
        {
            throw new System.NotImplementedException();
        }

        public IContainer Register<TInterface, TImpl>()
            where TInterface : class
            where TImpl : TInterface
        {
            throw new System.NotImplementedException();
        }

        public TInterface Resolve<TInterface>() where TInterface : class
        {
            throw new System.NotImplementedException();
        }
    }
}