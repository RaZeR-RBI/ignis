using ConsoleSample.Components;
using Ignis;
using Ignis.Containers;

namespace ConsoleSample.Systems
{
public class PhysicsSystem : SystemBase<GameState>
{
	private readonly IComponentCollection<PhysicsObject> _objects;

	public PhysicsSystem(ContainerProvider<GameState> ownerProvider,
	                     IComponentCollection<PhysicsObject> objects) : base(ownerProvider)
	{
		_objects = objects;
	}

	public override void Execute(GameState state)
	{
		// pass current state and instance as parameter to avoid heap allocations
		var param = (Self: this, State: state);
		// process each component
		_objects.ForEach((id, obj, p) => p.Self.Move(id, obj, p.State), param);
	}

	private void Move(int id, PhysicsObject obj, GameState state)
	{
		// move object
		obj.Position += obj.Velocity * state.DeltaSeconds;

		// reflect off screen edges
		if (obj.Position.X < 0 || obj.Position.X >= state.ScreenWidth)
		{
			obj.Velocity.X *= -1;
			obj.Position.X = obj.Velocity.X > 0 ? 0 : state.ScreenWidth - 1;
		}

		if (obj.Position.Y < 0 || obj.Position.Y >= state.ScreenHeight)
		{
			obj.Velocity.Y *= -1;
			obj.Position.Y = obj.Velocity.Y > 0 ? 0 : state.ScreenHeight - 1;
		}

		// update component value
		_objects.UpdateCurrent(obj);
	}
}
}