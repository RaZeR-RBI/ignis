using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Ignis.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Ignis.Containers
{
internal class MEDIContainer<TState> : IContainer<TState>, IDisposable
{
	public IEntityManager EntityManager { get; }
	private readonly List<Type> _registeredTypes = new List<Type>();

	private readonly Dictionary<Type, Type> _registeredComponents = new Dictionary<Type, Type>();
	private readonly List<Type> _registeredSystems = new List<Type>();

	private readonly List<SystemBase<TState>> _systemInstances = new List<SystemBase<TState>>();

	private IServiceProvider _provider;
	private IServiceCollection _services;

	public MEDIContainer()
	{
		_services = new ServiceCollection();
		ContainerProvider<TState>.BeginCreation(this);
		Register<ContainerProvider<TState>>();
		EntityManager = new EntityManager(ResolveStorage);
	}

	public void Dispose()
	{
		if (!_alreadyBuilt)
			ContainerProvider<TState>.EndCreation();
		foreach (var systemType in _registeredSystems)
			GetSystem(systemType).Dispose();
	}

#pragma warning disable HAA0101 // rare call, don't care about params allocation
	private IComponentCollectionStorage ResolveStorage(Type componentType)
	{
		var storageType = _registeredComponents[componentType];
		var result = Resolve(storageType);
		return (IComponentCollectionStorage) result;
	}
#pragma warning restore

	public IContainer<TState> AddComponent<TComponent>() where TComponent : struct
	{
		return AddComponent<TComponent, DoubleListStorage<TComponent>>();
	}

	public IContainer<TState> AddComponent<TComponent, TStorage>()
		where TComponent : struct
		where TStorage : class, IComponentCollection<TComponent>
	{
		_registeredComponents.Add(typeof(TComponent), typeof(IComponentCollection<TComponent>));
		_registeredTypes.Add(typeof(IComponentCollection<TComponent>));
		_services.AddSingleton<IComponentCollection<TComponent>, TStorage>();
		return this;
	}

	private void ThrowIfSystemIsAlreadyRegistered<T>()
	{
		ThrowIfSystemIsAlreadyRegistered(typeof(T));
	}

	private void ThrowIfSystemIsAlreadyRegistered(Type type)
	{
		if (_registeredSystems.Contains(type))
			throw new ArgumentException($"System type {type} is already registered");
	}

	public IContainer<TState> AddSystem<TSystem>() where TSystem : SystemBase<TState>
	{
		return AddSystem<TSystem, TSystem>();
	}

	private void CreateSystemInstances()
	{
		foreach (var type in _registeredSystems)
			_systemInstances.Add(GetSystem(type));
	}

	private bool _alreadyBuilt = false;

	[UnconditionalSuppressMessage("Aot", "IL3050:RequiresDynamicCode", Justification = "The unfriendly method is not reachable with AOT")]
	public IContainer<TState> Build()
	{
		if (_alreadyBuilt)
			throw new InvalidOperationException("Container is already built");
		_provider = _services.BuildServiceProvider();
		foreach (var type in _registeredTypes)
			Resolve(type);
		CreateSystemInstances();
		_alreadyBuilt = true;
		ContainerProvider<TState>.EndCreation();
		return this;
	}

	public void ExecuteSystems(TState state)
	{
		foreach (var item in _systemInstances)
			item.Execute(state);
	}

	public IComponentCollection<T> GetStorageFor<T>() where T : new()
	{
		return Resolve<IComponentCollection<T>>();
	}

	public T GetSystem<T>() where T : class
	{
		if (!_registeredSystems.Contains(typeof(T)))
			throw new ArgumentException($"No implementation for system ${typeof(T)} is registered");
		return Resolve<T>();
	}

	public IContainer<TState> Register<TInterface, TImpl>()
		where TInterface : class
		where TImpl : class, TInterface
	{
		_services.AddSingleton<TInterface, TImpl>();
		_registeredTypes.Add(typeof(TInterface));
		return this;
	}

	public TInterface Resolve<TInterface>() where TInterface : class
	{
		return (TInterface) Resolve(typeof(TInterface));
	}

#pragma warning disable HAA0101 // params call is ok because it's intended mostly for testing purposes
	public IComponentCollection GetStorageFor(Type type)
	{
		var storageType = _registeredComponents[type];
		var result = Resolve(storageType);
		return (IComponentCollection) result;
	}
#pragma warning restore

	public SystemBase<TState> GetSystem(Type type)
	{
		return Resolve(type) as SystemBase<TState>;
	}

	public IEnumerable<Type> GetComponentTypes()
	{
		return _registeredComponents.Keys;
	}

	public IEnumerable<Type> GetSystemTypes()
	{
		return _registeredSystems;
	}

	public void InitializeSystems(TState state = default)
	{
		foreach (var type in _registeredSystems)
		{
			var system = GetSystem(type);
			system.Initialize(state);
		}
	}

	public IContainer<TState> Register<T>() where T : class
	{
		return Register<T, T>();
	}

	public IContainer<TState> Register(
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
		Type type)
	{
		return Register(type, type);
	}

	public IContainer<TState> Register(
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
		Type @interface,
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
		Type impl)
	{
		var registeredType = @interface;
		if (typeof(SystemBase<TState>).IsAssignableFrom(impl))
		{
			_services.AddSingleton(@interface, impl);
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
			_services.AddSingleton(registeredType, impl);
			_registeredComponents.Add(componentType, registeredType);
		}
		else
		{
			_services.AddSingleton(@interface, impl);
		}

		_registeredTypes.Add(registeredType);
		return this;
	}

	public object Resolve(Type type)
	{
		if (_provider is null)
			throw new InvalidOperationException("Container must be built first");
		return _provider.GetRequiredService(type);
	}

	public IContainer<TState> AddSystem<TInterface, TSystem>()
		where TInterface : class
		where TSystem : SystemBase<TState>, TInterface
	{
		ThrowIfSystemIsAlreadyRegistered<TInterface>();
		Register<TInterface, TSystem>();
		_registeredSystems.Add(typeof(TInterface));
		return this;
	}

	public bool IsBuilt()
	{
		return _alreadyBuilt;
	}
}
}