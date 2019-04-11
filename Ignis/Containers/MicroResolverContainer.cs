using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ignis.Storage;
using MicroResolver;

namespace Ignis.Containers
{
    public class MicroResolverContainer : IContainer
    {
        public IEntityManager EntityManager { get; }
        private ObjectResolver _resolver = ObjectResolver.Create();

        private List<Action> _executionOrder = new List<Action>();
        private List<List<Type>> _systemTypes = new List<List<Type>>();

        public MicroResolverContainer()
        {
            ContainerProvider.BeginCreation(this);
            Register<ContainerProvider, ContainerProvider>();
            EntityManager = new EntityManager(ResolveStorage);
        }

        private IComponentCollectionStorage ResolveStorage(Type componentType)
        {
            var storageType = typeof(IComponentCollection<>).MakeGenericType(componentType);
            var result = _resolver.Resolve(storageType);
            return (IComponentCollectionStorage)result;
        }

        public IContainer AddComponent<TComponent>() where TComponent : struct =>
            AddComponent<TComponent, DoubleListStorage<TComponent>>();

        public IContainer AddComponent<TComponent, TStorage>()
            where TComponent : struct
            where TStorage : class, IComponentCollection<TComponent>
        {
            Register<IComponentCollection<TComponent>, TStorage>();
            return this;
        }

        private void ThrowIfSystemIsAlreadyRegistered<T>()
        {
            if (_systemTypes.SelectMany(l => l).Contains(typeof(T)))
                throw new ArgumentException($"System type {typeof(T)} is already registered");
        }

        public IContainer AddParallelSystems<T1, T2>()
            where T1 : SystemBase
            where T2 : SystemBase
        {
            ThrowIfSystemIsAlreadyRegistered<T1>();
            ThrowIfSystemIsAlreadyRegistered<T2>();
            Register<T1, T1>();
            Register<T2, T2>();
            _systemTypes.Add(new List<Type> { typeof(T1), typeof(T2) });
            return this;
        }

        public IContainer AddParallelSystems<T1, T2, T3>()
            where T1 : SystemBase
            where T2 : SystemBase
            where T3 : SystemBase
        {
            ThrowIfSystemIsAlreadyRegistered<T1>();
            ThrowIfSystemIsAlreadyRegistered<T2>();
            ThrowIfSystemIsAlreadyRegistered<T3>();
            Register<T1, T1>();
            Register<T2, T2>();
            Register<T3, T3>();
            _systemTypes.Add(new List<Type> { typeof(T1), typeof(T2), typeof(T3) });
            return this;
        }

        public IContainer AddParallelSystems<T1, T2, T3, T4>()
            where T1 : SystemBase
            where T2 : SystemBase
            where T3 : SystemBase
            where T4 : SystemBase
        {
            ThrowIfSystemIsAlreadyRegistered<T1>();
            ThrowIfSystemIsAlreadyRegistered<T2>();
            ThrowIfSystemIsAlreadyRegistered<T3>();
            ThrowIfSystemIsAlreadyRegistered<T4>();
            Register<T1, T1>();
            Register<T2, T2>();
            Register<T3, T3>();
            Register<T4, T4>();
            _systemTypes.Add(new List<Type> { typeof(T1), typeof(T2), typeof(T3), typeof(T4) });
            return this;
        }

        public IContainer AddSystem<TSystem>() where TSystem : SystemBase
        {
            ThrowIfSystemIsAlreadyRegistered<TSystem>();
            Register<TSystem, TSystem>();
            _systemTypes.Add(new List<Type> { typeof(TSystem) });
            return this;
        }

        private void BuildExecutionOrder()
        {
            foreach (var list in _systemTypes)
            {
                var instances = list
                    .Select(t => _resolver.Resolve(t))
                    .Cast<SystemBase>()
                    .ToList();

                if (instances.Count == 1)
                    _executionOrder.Add(() => instances[0].Execute());
                else
                    _executionOrder.Add(() =>
                        Parallel.ForEach(instances,
                        (instance, state, i) => instance.Execute()));
            }
        }

        private bool _alreadyBuilt = false;

        public IContainer Build()
        {
            if (_alreadyBuilt)
                throw new InvalidOperationException("Container is already built");
            _resolver.Compile();
            BuildExecutionOrder();
            ContainerProvider.EndCreation();
            _alreadyBuilt = true;
            return this;
        }

        public void ExecuteSystems() =>
            _executionOrder.ForEach(a => a.Invoke());

        public IComponentCollection<T> GetStorageFor<T>() where T : new() =>
            _resolver.Resolve<IComponentCollection<T>>();

        public T GetSystem<T>() where T : SystemBase => _resolver.Resolve<T>();

        public IContainer Register<TInterface, TImpl>()
            where TInterface : class
            where TImpl : class, TInterface
        {
            _resolver.Register<TInterface, TImpl>(Lifestyle.Singleton);
            return this;
        }

        public TInterface Resolve<TInterface>() where TInterface : class =>
            _resolver.Resolve<TInterface>();

        public dynamic GetStorageFor(Type type) =>
            _resolver.Resolve(typeof(IComponentCollection<>).MakeGenericType(type));

        public SystemBase GetSystem(Type type) => _resolver.Resolve(type) as SystemBase;
    }
}