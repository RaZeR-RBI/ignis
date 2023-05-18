using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Ignis
{
/// <summary>
/// Describes an ECS container which incapsulates an entity manager, systems,
/// components and their storage.
/// Basically it's a dependency injection container with ECS-specific additions where every object is a singleton.
/// There is two types of members available - 'builder' methods (AddSystem, AddComponent, Register, etc.)
/// and regular members that can be used after <see cref="Build()" /> has been called.
/// The systems will be called in order they've been registered with.
/// </summary>
/// <typeparam name="TState">Type of state parameter that gets passed to systems.</typeparam>
public interface IContainer<TState> : IDisposable
{
	/// <summary>
	/// Gets the entity manager for this container.
	/// </summary>
	IEntityManager EntityManager { get; }

	/// <summary>
	/// Gets the storage for the specified component type.
	/// </summary>
	/// <typeparam name="T">Component type</typeparam>
	IComponentCollection<T> GetStorageFor<T>() where T : new();

	/// <summary>
	/// Gets the storage for the specified component type.
	/// </summary>
	/// <param name="type">Component type</param>
	IComponentCollection GetStorageFor(Type type);

	/// <summary>
	/// Gets the system of the specified type.
	/// </summary>
	/// <typeparam name="T">System type</typeparam>
	T GetSystem<T>() where T : class;

	/// <summary>
	/// Gets the system of the specified type.
	/// </summary>
	/// <param name="type">System type</param>
	SystemBase<TState> GetSystem(Type type);

	/// <summary>
	/// Registers a component type with the specified storage type.
	/// </summary>
	/// <typeparam name="TComponent">Component type</typeparam>
	/// <typeparam name="TStorage">Storage type</typeparam>
	/// <seealso cref="Ignis.Storage.DoubleListStorage<T>" />
	/// <seealso cref="Ignis.Storage.ListOfPairsStorage<T>" />
	/// <seealso cref="Ignis.Storage.NullStorage<T>" />
	IContainer<TState> AddComponent<TComponent, TStorage>()
		where TComponent : struct
		where TStorage : class, IComponentCollection<TComponent>;

	/// <summary>
	/// Registers a component type.
	/// </summary>
	/// <typeparam name="TComponent">Component type</typeparam>
	IContainer<TState> AddComponent<TComponent>()
		where TComponent : struct;

	/// <summary>
	/// Registers a system implementation conforming to the supplied interface.
	/// </summary>
	/// <typeparam name="TInterface">Interface type (will be used for <see cref="Resolve(Type)" />)</typeparam>
	/// <typeparam name="TSystem">System type that implements the interface</typeparam>
	/// <returns></returns>
	IContainer<TState> AddSystem<TInterface, TSystem>()
		where TInterface : class
		where TSystem : SystemBase<TState>, TInterface;

	/// <summary>
	/// Registers a system type.
	/// </summary>
	/// <typeparam name="TSystem">System type</typeparam>
	IContainer<TState> AddSystem<TSystem>()
		where TSystem : SystemBase<TState>;

	/// <summary>
	/// Register an object type in this container.
	/// </summary>
	/// <typeparam name="T">Object type</typeparam>
	IContainer<TState> Register<T>()
		where T : class;

	/// <summary>
	/// Register an object type in this container.
	/// </summary>
	/// <param name="type">Object type</param>
	IContainer<TState> Register([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
	                            Type type);

	/// <summary>
	/// Register an interface implementation in this container.
	/// </summary>
	/// <typeparam name="TInterface">Interface type (will be used for <see cref="Resolve(Type)" /></typeparam>
	/// <typeparam name="TImpl">Implementing type</typeparam>
	IContainer<TState> Register<TInterface, TImpl>()
		where TInterface : class
		where TImpl : class, TInterface;

	/// <summary>
	/// Register an interface implementation in this container.
	/// </summary>
	IContainer<TState> Register(
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
		Type @interface,
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
		Type impl);

	/// <summary>
	/// Resolves an object of the specified type.
	/// </summary>
	/// <param name="type"></param>
	object Resolve(Type type);

	/// <summary>
	/// Resolves an object of the specified type or implementing the specified interface.
	/// </summary>
	/// <typeparam name="TInterface">Object type or interface</typeparam>
	TInterface Resolve<TInterface>() where TInterface : class;

	/// <summary>
	/// Builds the container.
	/// </summary>
	/// <seealso cref="AddSystem<TInterface, TSystem>()" />
	/// <seealso cref="AddComponent<TComponent>()" />
	/// <seealso cref="Register<TInterface, TImpl>()" />
	IContainer<TState> Build();

	/// <summary>
	/// Indicates whether this container is built or still in configuration stage.
	/// </summary>
	bool IsBuilt();

	/// <summary>
	/// Initializes the systems.
	/// </summary>
	/// <param name="state">State value passed to systems</param>
	/// <seealso cref="Ignis.SystemBase<TState>.Initialize(TState)" />
	void InitializeSystems(TState state = default);

	/// <summary>
	/// Executes the systems.
	/// </summary>
	/// <param name="state">State value passed to systems</param>
	/// <seealso cref="Ignis.SystemBase<TState>.Execute(TState)" />
	void ExecuteSystems(TState state = default);

	/// <summary>
	/// Gets the component types registered in this container.
	/// </summary>
	IEnumerable<Type> GetComponentTypes();

	/// <summary>
	/// Gets the system types registered in this container.
	/// </summary>
	IEnumerable<Type> GetSystemTypes();
}
}