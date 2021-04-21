using System;
using System.Threading;

namespace Ignis.Containers
{
/// <summary>
/// Represents a container provider which allows resolved objects to retrieve
/// it's parent container without creating a circular dependency.
///
/// Please take note that the side effect of this is that no more than one
/// container can be in 'configuring' state simultaneously.
///
/// If you aren't implementing a new container type yourself, you could pretty
/// much ignore this class since it has no use for you (excluding <see cref="GetInstance()" />).
/// </summary>
public class ContainerProvider<TState>
{
	private static readonly object _syncRoot = new object();
	private static volatile IContainer<TState> _lastInstance = null;

	/// <summary>
	/// Returns value indicating if there is a container configuration still going on.
	/// </summary>
	public static bool IsBusy()
	{
		return Monitor.IsEntered(_syncRoot);
	}

	/// <summary>
	/// Begins creation of the specified container instance.
	/// Should be called by <see cref="Ignis.IContainer<TState>" /> constructor.
	/// </summary>
	/// <param name="instance"></param>
	public static void BeginCreation(IContainer<TState> instance)
	{
		if (!Monitor.TryEnter(_syncRoot, 500))
			throw new AbandonedMutexException("ContainerProvider is locked");
		_lastInstance = instance;
	}

	/// <summary>
	/// Retrieves the container instance that's being configured right now.
	/// </summary>
	/// <returns></returns>
	public IContainer<TState> GetInstance()
	{
		lock (_syncRoot)
		{
			return _lastInstance;
		}
	}

	/// <summary>
	/// Finishes the creation of the currently active container instance.
	/// Should be called by <see cref="Ignis.IContainer<TState>.Build()" />.
	/// </summary>
	public static void EndCreation()
	{
		_lastInstance = null;
		Monitor.Exit(_syncRoot);
	}
}
}