using System;
using System.Collections;
using System.Collections.Generic;

namespace Ignis
{
internal interface IComponentCollectionStorage
{
	bool RemoveComponentFromStorage(int entityId);
	bool StoreComponentForEntity(int entityId);
}

public interface IComponentCollection<T>
	where T : new()
{
	void UpdateCurrent(T value);
	void Update(int entityId, T value);
	void Update(int entityId, object value);
	T Get(int entityId);
	void ForEach(Action<int, T> action);
	IEntityView GetView();
	int GetCount();
	IEnumerable<T> GetValues();
}
}