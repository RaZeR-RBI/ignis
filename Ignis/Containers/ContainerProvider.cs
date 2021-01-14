using System;
using System.Threading;

namespace Ignis.Containers
{
public class ContainerProvider<TState>
{
	private static object _syncRoot = new object();
	private static volatile IContainer<TState> _lastInstance = null;

	public static bool IsBusy()
	{
		return Monitor.IsEntered(_syncRoot);
	}

	public static void BeginCreation(IContainer<TState> instance)
	{
		if (!Monitor.TryEnter(_syncRoot, 500))
			throw new AbandonedMutexException("ContainerProvider is locked");
		_lastInstance = instance;
	}

	public IContainer<TState> GetInstance()
	{
		lock (_syncRoot)
		{
			return _lastInstance;
		}
	}

	public static void EndCreation()
	{
		_lastInstance = null;
		Monitor.Exit(_syncRoot);
	}
}
}