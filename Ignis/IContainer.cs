using System;
using System.Collections.Generic;

namespace Ignis
{
public interface IContainer<TState> : IDisposable
{
	IEntityManager EntityManager { get; }
	IComponentCollection<T> GetStorageFor<T>() where T : new();
	dynamic GetStorageFor(Type type);
	T GetSystem<T>() where T : class;
	SystemBase<TState> GetSystem(Type type);

	IContainer<TState> AddComponent<TComponent, TStorage>()
		where TComponent : struct
		where TStorage : class, IComponentCollection<TComponent>;

	IContainer<TState> AddComponent<TComponent>()
		where TComponent : struct;

	IContainer<TState> AddSystem<TInterface, TSystem>()
		where TInterface : class
		where TSystem : SystemBase<TState>, TInterface;

	IContainer<TState> AddSystem<TSystem>()
		where TSystem : SystemBase<TState>;

	IContainer<TState> AddParallelSystems(Type[] interfaces, Type[] implementations);
	IContainer<TState> AddParallelSystems(Type[] implementations);

	IContainer<TState> Register<T>()
		where T : class;

	IContainer<TState> Register(Type type);

	IContainer<TState> Register<TInterface, TImpl>()
		where TInterface : class
		where TImpl : class, TInterface;

	IContainer<TState> Register(Type @interface, Type impl);

	object Resolve(Type type);
	TInterface Resolve<TInterface>() where TInterface : class;

	IContainer<TState> Build();
	bool IsBuilt();
	void InitializeSystems(TState state = default(TState));
	void ExecuteSystems(TState state = default(TState));

	IEnumerable<Type> GetComponentTypes();
	IEnumerable<Type> GetSystemTypes();
}
}