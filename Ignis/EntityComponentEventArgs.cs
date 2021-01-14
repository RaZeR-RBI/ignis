using System;
using System.Diagnostics.CodeAnalysis;

namespace Ignis
{
[ExcludeFromCodeCoverage]
public struct EntityComponentEventArgs
{
	public int EntityID { get; }
	public Type ComponentType { get; }

	public EntityComponentEventArgs(int id, Type component)
	{
		(EntityID, ComponentType) = (id, component);
	}
}
}