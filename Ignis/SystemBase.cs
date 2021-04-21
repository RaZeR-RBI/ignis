using System;
using Ignis.Containers;

namespace Ignis
{
/// <summary>
/// Base class for ECS systems. Implement your own systems by deriving from it.
/// </summary>
/// <typeparam name="TState">Type of state parameter that gets passed to <see cref="Execute(TState)" /></typeparam>
public abstract class SystemBase<TState> : IDisposable
{
	/// <summary>
	/// Gets the container in which this system belongs too.
	/// </summary>
	public IContainer<TState> Container => _container;
	protected readonly IContainer<TState> _container;

	/// <summary>
	/// Gets the entity manager belonging to this system's container.
	/// </summary>
	public IEntityManager EntityManager => _em;
	protected readonly IEntityManager _em;

	/// <summary>
	/// Main method that gets executed when <see cref="Ignis.IContainer<TState>.ExecuteSystems(TState)" /> gets called.
	/// Implement your system's logic here.
	/// </summary>
	/// <param name="state">State parameter</param>
	public abstract void Execute(TState state);

	/// <summary>
	/// Gets called by <see cref="Ignis.IContainer<TState>.InitializeSystems(TState)" />.
	/// Override it to implement post-construction logic - for example, to do preparations before
	/// execution or to resolve circular dependencies.
	/// </summary>
	/// <param name="state">State parameter</param>
	public virtual void Initialize(TState state)
	{
	}

	/// <summary>
	/// Base constructor.
	/// </summary>
	/// <param name="ownerProvider">Container provider</param>
	public SystemBase(ContainerProvider<TState> ownerProvider)
	{
		_container = ownerProvider.GetInstance();
		_em = _container.EntityManager;
	}

	private bool isDisposed = false;

	protected virtual void OnDispose()
	{
	}

	/// <summary>
	/// Disposes the system. Overridable.
	/// </summary>
	public virtual void Dispose()
	{
		if (isDisposed) return;
		isDisposed = true;
		OnDispose();
	}
}
}