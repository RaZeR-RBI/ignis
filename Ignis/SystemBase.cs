using System;
using Ignis.Containers;

namespace Ignis
{
public abstract class SystemBase<TState> : IDisposable
{
	public IContainer<TState> Container => _container;
	protected readonly IContainer<TState> _container;

	public IEntityManager EntityManager => _em;
	protected readonly IEntityManager _em;

	public abstract void Execute(TState state);

	public virtual void Initialize(TState state)
	{
	}

	public SystemBase(ContainerProvider<TState> ownerProvider)
	{
		_container = ownerProvider.GetInstance();
		_em = _container.EntityManager;
	}

	private bool isDisposed = false;

	protected virtual void OnDispose()
	{
	}

	public virtual void Dispose()
	{
		if (isDisposed) return;
		isDisposed = true;
		OnDispose();
	}
}
}