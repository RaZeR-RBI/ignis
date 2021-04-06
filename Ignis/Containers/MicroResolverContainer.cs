using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Ignis.Storage;
using MicroResolver;

namespace Ignis.Containers
{
public class MicroResolverContainer<TState> : IContainer<TState>, IDisposable
{
	public IEntityManager EntityManager { get; }
	private readonly ObjectResolver _resolver = ObjectResolver.Create();
	private readonly List<Type> _registeredTypes = new List<Type>();

	private Action<TState> _executor = _ => { };
	private readonly List<List<Type>> _systemTypes = new List<List<Type>>();

	private readonly List<Type> _registeredComponents = new List<Type>();
	private readonly List<Type> _registeredSystems = new List<Type>();

	public MicroResolverContainer()
	{
		ContainerProvider<TState>.BeginCreation(this);
		Register<ContainerProvider<TState>>();
		EntityManager = new EntityManager(ResolveStorage);
	}

	public void Dispose()
	{
		if (!_alreadyBuilt)
			ContainerProvider<TState>.EndCreation();
		foreach (var systemType in GetSystemTypes())
			GetSystem(systemType).Dispose();
	}

	private IComponentCollectionStorage ResolveStorage(Type componentType)
	{
		var storageType = typeof(IComponentCollection<>).MakeGenericType(componentType);
		var result = _resolver.Resolve(storageType);
		return (IComponentCollectionStorage) result;
	}

	public IContainer<TState> AddComponent<TComponent>() where TComponent : struct
	{
		return AddComponent<TComponent, DoubleListStorage<TComponent>>();
	}

	public IContainer<TState> AddComponent<TComponent, TStorage>()
		where TComponent : struct
		where TStorage : class, IComponentCollection<TComponent>
	{
		Register<IComponentCollection<TComponent>, TStorage>();
		return this;
	}

	private void ThrowIfSystemIsAlreadyRegistered<T>()
	{
		ThrowIfSystemIsAlreadyRegistered(typeof(T));
	}

	private void ThrowIfSystemIsAlreadyRegistered(Type type)
	{
		if (_systemTypes.SelectMany(l => l).Contains(type))
			throw new ArgumentException($"System type {type} is already registered");
	}

	public IContainer<TState> AddSystem<TSystem>() where TSystem : SystemBase<TState>
	{
		return AddSystem<TSystem, TSystem>();
	}

	private void BuildExecutionOrder()
	{
		foreach (var list in _systemTypes)
		{
			var instances = list
			                .Select(t => _resolver.Resolve(t))
			                .Cast<SystemBase<TState>>()
			                .ToList();

			if (instances.Count == 1)
				_executor += (s) => instances[0].Execute(s);
			else
				_executor += (s) =>
					Parallel.ForEach(instances,
					                 (v, _, __) =>
						                 v.Execute(s));
		}
	}

	private bool _alreadyBuilt = false;

	public IContainer<TState> Build()
	{
		if (_alreadyBuilt)
			throw new InvalidOperationException("Container is already built");
		_resolver.Compile();
		foreach (var type in _registeredTypes)
			_resolver.Resolve(type);
		BuildExecutionOrder();
		_alreadyBuilt = true;
		ContainerProvider<TState>.EndCreation();
		return this;
	}

	public void ExecuteSystems(TState state)
	{
		_executor(state);
	}

	public IComponentCollection<T> GetStorageFor<T>() where T : new()
	{
		return _resolver.Resolve<IComponentCollection<T>>();
	}

	public T GetSystem<T>() where T : class
	{
		if (!_registeredSystems.Contains(typeof(T)))
			throw new ArgumentException($"No implementation for system ${typeof(T)} is registered");
		return _resolver.Resolve<T>();
	}

	public IContainer<TState> Register<TInterface, TImpl>()
		where TInterface : class
		where TImpl : class, TInterface
	{
		return Register(typeof(TInterface), typeof(TImpl));
	}

	public TInterface Resolve<TInterface>() where TInterface : class
	{
		return (TInterface) Resolve(typeof(TInterface));
	}

	public dynamic GetStorageFor(Type type)
	{
		return _resolver.Resolve(typeof(IComponentCollection<>).MakeGenericType(type));
	}

	public SystemBase<TState> GetSystem(Type type)
	{
		return _resolver.Resolve(type) as SystemBase<TState>;
	}

	public IEnumerable<Type> GetComponentTypes()
	{
		return _registeredComponents;
	}

	public IEnumerable<Type> GetSystemTypes()
	{
		return _registeredSystems;
	}

	public void InitializeSystems(TState state = default)
	{
		GetSystemTypes()
			.Select(GetSystem)
			.ToList()
			.ForEach(s => s.Initialize(state));
	}

	public IContainer<TState> AddParallelSystems(Type[] interfaces, Type[] implementations)
	{
		foreach (var iface in interfaces)
			ThrowIfSystemIsAlreadyRegistered(iface);
		foreach (var pair in interfaces.Zip(implementations, (k, v) => (k, v)))
			Register(pair.k, pair.v);
		_systemTypes.Add(interfaces.ToList());
		return this;
	}

	public IContainer<TState> Register<T>() where T : class
	{
		return Register<T, T>();
	}

	public IContainer<TState> Register(Type type)
	{
		return Register(type, type);
	}

	public IContainer<TState> Register(Type @interface, Type impl)
	{
		var registeredType = @interface;
		if (typeof(SystemBase<TState>).IsAssignableFrom(impl))
		{
			_resolver.Register(Lifestyle.Singleton, @interface, impl);
			_registeredSystems.Add(@interface);
		}
		else if (typeof(IComponentCollectionStorage).IsAssignableFrom(impl))
		{
			var storeInterface = impl.GetInterfaces()
			                         .FirstOrDefault(
			                         t => t.GetGenericTypeDefinition() ==
			                              typeof(IComponentCollection<>));
			if (storeInterface == null)
				throw new ArgumentException(
				"Object implements IComponentCollectionStorage but not IComponentCollection<T>");
			var componentType = storeInterface.GetGenericArguments()[0];
			registeredType = typeof(IComponentCollection<>).MakeGenericType(componentType);
			_resolver.Register(Lifestyle.Singleton,
			                   registeredType,
			                   impl);
			_registeredComponents.Add(componentType);
		}
		else
		{
			_resolver.Register(Lifestyle.Singleton, @interface, impl);
		}

		_registeredTypes.Add(registeredType);
		return this;
	}

	public object Resolve(Type type)
	{
		return _resolver.Resolve(type);
	}

	public IContainer<TState> AddSystem<TInterface, TSystem>()
		where TInterface : class
		where TSystem : SystemBase<TState>, TInterface
	{
		ThrowIfSystemIsAlreadyRegistered<TInterface>();
		Register<TInterface, TSystem>();
		_systemTypes.Add(new List<Type> {typeof(TInterface)});
		return this;
	}

	public IContainer<TState> AddParallelSystems(Type[] implementations)
	{
		return AddParallelSystems(implementations, implementations);
	}

	public bool IsBuilt()
	{
		return _alreadyBuilt;
	}
}
}