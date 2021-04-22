using ConsoleSample.Components;
using Ignis;
using Ignis.Containers;
using MicroResolver;

namespace ConsoleSample.Systems
{
public class PhysicsSystem : SystemBase<GameState>
{
	[Inject] // this attribute comes from MicroResolver
	private IComponentCollection<PhysicsObject> _objects = null;

	public PhysicsSystem(ContainerProvider<GameState> ownerProvider) : base(ownerProvider)
	{
	}

	public override void Execute(GameState state)
	{
		// pass current state and instance as parameter to avoid heap allocations
		var param = (this, state);
		// process each component
		_objects.ForEach((id, obj, p) => Move(id, obj, p), param);
	}

	// note - it's static on purpose, to prevent accidental closure creation
	private static void Move(int id, PhysicsObject obj, (PhysicsSystem, GameState) param)
	{
		// unpack them back
		var (@this, state) = param;

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
		@this._objects.UpdateCurrent(obj);
	}
}
}