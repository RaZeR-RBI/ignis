using System;
using System.Collections.Generic;

namespace Ignis
{
	public interface IContainer : IDisposable
	{
		IEntityManager EntityManager { get; }
		IComponentCollection<T> GetStorageFor<T>() where T : new();
		dynamic GetStorageFor(Type type);
		T GetSystem<T>() where T : class;
		SystemBase GetSystem(Type type);

		IContainer AddComponent<TComponent, TStorage>()
			where TComponent : struct
			where TStorage : class, IComponentCollection<TComponent>;

		IContainer AddComponent<TComponent>()
			where TComponent : struct;

		IContainer AddSystem<TInterface, TSystem>()
			where TInterface : class
			where TSystem : SystemBase, TInterface;

		IContainer AddSystem<TSystem>()
			where TSystem : SystemBase;

		IContainer AddParallelSystems(Type[] interfaces, Type[] implementations);
		IContainer AddParallelSystems(Type[] implementations);

		IContainer Register<T>()
			where T : class;

		IContainer Register(Type type);

		IContainer Register<TInterface, TImpl>()
			where TInterface : class
			where TImpl : class, TInterface;

		IContainer Register(Type @interface, Type impl);

		object Resolve(Type type);
		TInterface Resolve<TInterface>() where TInterface : class;

		IContainer Build();
		bool IsBuilt();
		void InitializeSystems();
		void ExecuteSystems();

		IEnumerable<Type> GetComponentTypes();
		IEnumerable<Type> GetSystemTypes();
	}
}