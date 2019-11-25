using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Ignis.Storage;
using MicroResolver;

namespace Ignis.Containers
{
	public class MicroResolverContainer : IContainer, IDisposable
	{
		public IEntityManager EntityManager { get; }
		private ObjectResolver _resolver = ObjectResolver.Create();
		private List<Type> _registeredTypes = new List<Type>();

		private List<Action> _executionOrder = new List<Action>();
		private List<List<Type>> _systemTypes = new List<List<Type>>();

		private List<Type> _registeredComponents = new List<Type>();
		private List<Type> _registeredSystems = new List<Type>();

		public MicroResolverContainer()
		{
			ContainerProvider.BeginCreation(this);
			Register<ContainerProvider, ContainerProvider>();
			EntityManager = new EntityManager(ResolveStorage);
		}

		public void Dispose()
		{
			if (!_alreadyBuilt)
				ContainerProvider.EndCreation();
			foreach (var systemType in GetSystemTypes())
				GetSystem(systemType).Dispose();
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

		private void ThrowIfSystemIsAlreadyRegistered<T>() =>
			ThrowIfSystemIsAlreadyRegistered(typeof(T));

		private void ThrowIfSystemIsAlreadyRegistered(Type type)
		{
			if (_systemTypes.SelectMany(l => l).Contains(type))
				throw new ArgumentException($"System type {type} is already registered");
		}

		public IContainer AddSystem<TSystem>() where TSystem : SystemBase =>
			AddSystem<TSystem, TSystem>();

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
			foreach (var type in _registeredTypes)
				_resolver.Resolve(type);
			BuildExecutionOrder();
			_alreadyBuilt = true;
			ContainerProvider.EndCreation();
			return this;
		}

		public void ExecuteSystems() =>
			_executionOrder.ForEach(a => a.Invoke());

		public IComponentCollection<T> GetStorageFor<T>() where T : new() =>
			_resolver.Resolve<IComponentCollection<T>>();

		public T GetSystem<T>() where T : class
		{
			if (!_registeredSystems.Contains(typeof(T)))
				throw new ArgumentException($"No implementation for system ${typeof(T)} is registered");
			return _resolver.Resolve<T>();
		}

		public IContainer Register<TInterface, TImpl>()
			where TInterface : class
			where TImpl : class, TInterface =>
				Register(typeof(TInterface), typeof(TImpl));

		public TInterface Resolve<TInterface>() where TInterface : class =>
			(TInterface)Resolve(typeof(TInterface));

		public dynamic GetStorageFor(Type type) =>
			_resolver.Resolve(typeof(IComponentCollection<>).MakeGenericType(type));

		public SystemBase GetSystem(Type type) => _resolver.Resolve(type) as SystemBase;

		public IEnumerable<Type> GetComponentTypes() => _registeredComponents;

		public IEnumerable<Type> GetSystemTypes() => _registeredSystems;

		public void InitializeSystems() =>
			GetSystemTypes()
				.Select(GetSystem)
				.ToList()
				.ForEach(s => s.Initialize());

		public IContainer AddParallelSystems(Type[] interfaces, Type[] implementations)
		{
			foreach (var iface in interfaces)
				ThrowIfSystemIsAlreadyRegistered(iface);
			foreach (var pair in interfaces.Zip(implementations, (k, v) => (k, v)))
				Register(pair.k, pair.v);
			_systemTypes.Add(interfaces.ToList());
			return this;
		}

		public IContainer Register<T>() where T : class =>
			Register<T, T>();

		public IContainer Register(Type type) =>
			Register(type, type);

		public IContainer Register(Type @interface, Type impl)
		{
			var registeredType = @interface;
			if (typeof(SystemBase).IsAssignableFrom(impl))
			{
				_resolver.Register(Lifestyle.Singleton, @interface, impl);
				_registeredSystems.Add(@interface);
			}
			else if (typeof(IComponentCollectionStorage).IsAssignableFrom(impl))
			{
				var storeInterface = impl.GetInterfaces()
					.FirstOrDefault(t => t.GetGenericTypeDefinition() == typeof(IComponentCollection<>));
				if (storeInterface == null)
					throw new ArgumentException("Object implements IComponentCollectionStorage but not IComponentCollection<T>");
				var componentType = storeInterface.GetGenericArguments()[0];
				registeredType = typeof(IComponentCollection<>).MakeGenericType(componentType);
				_resolver.Register(Lifestyle.Singleton,
					registeredType,
					impl);
				_registeredComponents.Add(componentType);
			}
			else
				_resolver.Register(Lifestyle.Singleton, @interface, impl);
			_registeredTypes.Add(registeredType);
			return this;
		}

		public object Resolve(Type type) =>
			_resolver.Resolve(type);

		public IContainer AddSystem<TInterface, TSystem>()
			where TInterface : class
			where TSystem : SystemBase, TInterface
		{
			ThrowIfSystemIsAlreadyRegistered<TInterface>();
			Register<TInterface, TSystem>();
			_systemTypes.Add(new List<Type> { typeof(TInterface) });
			return this;
		}

		public IContainer AddParallelSystems(Type[] implementations) =>
			AddParallelSystems(implementations, implementations);

		public bool IsBuilt() => _alreadyBuilt;
	}
}