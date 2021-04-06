using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Ignis.Storage
{
[DebuggerDisplay("{EntityID} => {ComponentValue}")]
internal struct EntityValuePair<T>
	where T : new()
{
	public int EntityID;
	public T ComponentValue;

	public EntityValuePair(int entityId)
	{
		(EntityID, ComponentValue) = (entityId, new T());
	}

	public EntityValuePair(int entityId, T value)
	{
		(EntityID, ComponentValue) = (entityId, value);
	}
}
}