![Stability: alpha](https://img.shields.io/badge/stability-alpha-orange.svg)
![Line coverage](docs/coverage/badge_linecoverage.png) ![Branch coverage](docs/coverage/badge_branchcoverage.png)

# Ignis - a simple and lightweight Entity Component System (ECS)

Entity Component System is an architectural pattern that's mostly used in game
development.

In short, you describe your game entities as composition of **components** like
'Position', 'Velocity', 'Model' and so on, and **systems** operate on them,
decoupling the game logic from the entities and allowing a wide range of behaviors
to be combined easily. **Entities** are represented as simple types (like **int**) and carry zero overhead.

This library provides a simple and fast implementation for such a
pattern.

[Interested? Read more about ECS](https://en.wikipedia.org/wiki/Entity_component_system)

## Documentation
- [API Docs](https://razer-rbi.github.io/ignis/api)
- [Samples](https://github.com/RaZeR-RBI/ignis/tree/master/Samples)

## Goals
- Easy to use API
- Minimal or zero heap allocations in hot paths
- Simple architecture
- Minimal boilerplate code - [MicroResolver](https://github.com/neuecc/MicroResolver) is used as dependency injection container, which allows easy and fast injection of needed services and objects, keeping your code clean

## Non-goals
To reduce both code and usage complexity the following trade-offs were made:
- Single instance of component per entity
- Order of execution is defined by the user

## Definitions
- **Entity** - represented as **int**, components are attached to them.
- **Component** - a plain data struct with fields and properties - basically a data container. Although classes can be used instead of structs, it's recommended to use structs to avoid heap allocations.
- **System** - a class that implements some part of logic related to specific components.
- **Container** - the root of the whole system. Encapsulates systems, component
storage and an entity manager.
- **Entity manager** - manages the entity IDs and the component associations,
allowing for creation and deletion of entities and their components, querying
and so on. There is one entity manager per container.
- **Component collection (storage)** - an object that stores the specific component values
for each entity that has that component. There is one component collection per one registered component type.
- **Entity view** - an automatically updated collection of entity IDs that have all
components listed in it's filter (sometimes it's called an **archetype**).

## Tutorial
[Full code can be found here](https://github.com/RaZeR-RBI/ignis/tree/master/Samples/ConsoleSample).

Let's try to do a simple thing - make objects fly around on our screen and bounce
off the screen edges. For simplicity, we will be rendering everything to terminal using the [ANSITerm](https://github.com/razer-rbi/ansiterm-net) library.

There are various approaches to designing your game logic in an ECS system -
perhaps the most easiest one is to start with **components**. As a starting point,
just ask yourself - what are the most basic things that every entity needs to have?

We need a way to position it and something to draw.
Since we're working with positions and velocities at once, let's make a
**PhysicsObject** component:
```csharp
public struct PhysicsObject
{
	public Vector2 Position, Velocity;
	/* constructor omitted */
}
```

---

**TIP:** You can go even more granular and separate Position and Velocity into separate
components - but we wouldn't go that way here to make things simpler.
If you're familiar with Unity, you should have worked with components already -
perhaps the most simplest one is the Unity **Transform** component, which you
can also easily implement yourself.

---

Since we need to draw those entities, let's make a **Drawable** component
(Color16 and ColorValue come from [ANSITerm](https://github.com/razer-rbi/ansiterm-net) library):
```csharp
public struct Drawable
{
	public char Symbol;
	public ColorValue Color;
	/* constructor omitted */
}
```

The second thing that we need is a **State** parameter - use it to encapsulate
the information that's needed for every system run. Almost always it will have at least a way to measure frame time to advance computations - in our case, we will use **seconds since the last frame** (`DeltaSeconds`).

It's strongly recommended to define it as a `class`, not as `struct` to allow
systems and other objects to alter it.

We will add additional properties to it to assist us with rendering:
```csharp
public class GameState
{
	public float DeltaSeconds { get; set; }
	public int ScreenWidth { get; set; }
	public int ScreenHeight { get; set; }
	public IConsoleBackend Backend { get; set; }
	/* constructor omitted */
}
```

Okay, now we need to get things moving. Let's implement our first **system**.
Since the movement logic doesn't depend on drawing stuff, the only
component we need in it is the **PhysicsObject**.
Let's make a **PhysicsSystem**:
```csharp
public class PhysicsSystem : SystemBase<GameState>
{
	[Inject] // this attribute comes from MicroResolver
	private readonly IComponentCollection<PhysicsObject> _objects;

	public PhysicsSystem(ContainerProvider<GameState> ownerProvider) : base(ownerProvider)
	{
	}

	public override void Execute(GameState state)
	{
		/* we need to do something here */
	}
}
```

We will get our IComponentCollection<PhysicsObject> injected into our system
automatically later on.
All systems should derive from [SystemBase<T>](https://razer-rbi.github.io/ignis/api/Ignis.SystemBase_TState_.html).

The heart of all systems is their **Execute(state)** method - it gets executed on each **ExecuteSystems(state)** call (we'll see it later).

---

**TIP:** If you need to do some initialization logic after the system has been constructed, feel free to override [Initialize(state)](https://razer-rbi.github.io/ignis/api/Ignis.SystemBase_TState_.Initialize(TState).html) method.
If you need to dispose something when the whole ECS gets disposed, override the
[Dispose()](https://razer-rbi.github.io/ignis/api/Ignis.SystemBase_TState_.Dispose().html) method.

---

**TIP:** The whole ECS container is based on [MicroResolver](https://github.com/neuecc/MicroResolver) library, so you are free to inject the needed
storages, systems and other objects in any way that's supported by that
library - personally I like the attribute method.

---

Now we need to process the **PhysicsObject** components.
[IComponentCollection<T>](https://razer-rbi.github.io/ignis/api/Ignis.IComponentCollection_T_.html) provides several methods to help us with it,
and the most straightforward one is the ```Process(Func<int, T, T>)```.

It runs a callback on each 'entity ID'-'component value' pair, you pass
a new component value, and it saves it.

The second one is the ```ForEach(Action<int, T, U>, U param)``` - it doesn't
save the new component value automatically and also allows you to pass an
additional parameter of any type.

---

**TIP:** Lambda functions that use (capture) variables
from 'outside' ([here is a good writeup on implicit allocations](https://devblogs.microsoft.com/pfxteam/know-thine-implicit-allocations/)) allocate additional memory on the heap, consider using ```ForEach``` instead of ```Process``` when you need to access anything other than the entity ID and the
component value to reduce memory allocations and GC pressure (which in turn
increases performance).

---

**TIP:** [ClrHeapAllocationAnalyzer](https://github.com/microsoft/RoslynClrHeapAllocationAnalyzer) is a very useful tool that lets you keep an
eye on heap allocations in your code.

---

Since we need to access the outer state to do our calculations, let's use the ```ForEach``` method.

```csharp
public override void Execute(GameState state)
{
	// pass current state and instance as parameter
	var param = (this, state);
	// process each component
	_objects.ForEach((id, obj, p) => Move(id, obj, p), param);
}
```

Let's implement our ```Move``` method:
```csharp
// note - it's static on purpose, to prevent accidental closure creation

private static void Move(int id, PhysicsObject obj, (PhysicsSystem, GameState) param)
{
	// unpack the parameters
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
```

If you want to update a component value for a specific entity ID, use the `Update(int, T)` method.

Now let's get to the rendering part. To draw stuff we need both position and
a description of what to draw - in other words, we need both the **PhysicsObject** and **Drawable** components.

A set of components of an entity is often called an **archetype** - we can work
with them through the **entity views** ([IEntityView](https://razer-rbi.github.io/ignis/api/Ignis.IEntityView.html)).

There is two ways of retrieving an `IEntityView` - `IEntityManager` (accessible from all systems and from the whole container) has a `GetView(Type[])` method, and each `IComponentCollection<T>` has `GetView()` method that returns `IEntityView` with the single component that it stores.

We need two components, so we need to request it from the `EntityManager`:
```csharp
public class RenderingSystem : SystemBase<GameState>
{
	[Inject]
	private IComponentCollection<PhysicsObject> _physObjects = null;

	[Inject]
	private IComponentCollection<Drawable> _drawables = null;

	// our entity view
	private readonly IEntityView _drawableIds;

	public RenderingSystem(ContainerProvider<GameState> ownerProvider) : base(ownerProvider)
	{
		// retrieve an entity view
		_drawableIds = EntityManager.GetView<PhysicsObject, Drawable>();
	}

	public override void Execute(GameState state)
	{
		/* implement logic here */
	}
```

`IEntityView` implements `IEnumerable<int>`, so working with it boils down to
using `foreach`:
```csharp
foreach (var id in _drawableIds)
{
	/* work with entity ID here */
}
```

To retrieve a component from IComponentCollection for a specific entity ID use the `Get(int)` method.

Let's implement the `Execute` method:
```csharp
public override void Execute(GameState state)
{
		var term = state.Backend;
		term.BackgroundColor = new ColorValue(Color16.Black);
		term.Clear();
		// enumerate through entity view
		foreach (var id in _drawableIds)
		{
			var position = _physObjects.Get(id).Position;
			position.X = MathF.Round(position.X);
			position.Y = MathF.Round(position.Y);
			var drawable = _drawables.Get(id);

			// draw it
			term.SetCursorPosition((int)position.X, (int)position.Y);
			term.ForegroundColor = drawable.Color;
			term.Write(drawable.Symbol.ToString());
		}
	}
```

Now we have all of our components and systems in place, let's configure the **ECS container**.

To do that you need to use the [ContainerFactory](https://razer-rbi.github.io/ignis/api/Ignis.Containers.ContainerFactory.html):
```csharp
private static IContainer<GameState> ConfigureECS()
{
	return ContainerFactory.CreateMicroResolverContainer<GameState>()
	                       .AddComponent<PhysicsObject>()
	                       .AddComponent<Drawable>()
	                       .AddSystem<PhysicsSystem>()
	                       .AddSystem<RenderingSystem>()
	                       .Build();
}
```

Don't forget to call the **Build** method after configuring all your components, systems and objects (those are configured using `Register` method).
There is also overloads that accept interface type along with the implementing type.

The order of registration of the systems should be the same as the order you want them to be run in - in other words, `PhysicsSystem` will be run first,
and the `RenderingSystem` will be run second.

---

**NOTE:** Some ECS users advocate against coupling systems, but I personally find it hard
to express some things with a pure non-coupled ECS way, so when I find out a that some part of system logic can be used in another other system, I abstract it through interfaces. That way the systems depend on abstractions and not concrete types, and they can be freely swapped and replaced.

---

After initializing our container we can spawn some entities:

```csharp
private static int SpawnRandomEntity(IContainer<GameState> ecs, IConsoleBackend term)
{
	var em = ecs.EntityManager;
	var id = em.Create(); // create an entity ID
	var screenSize = new Vector2(term.WindowWidth, term.WindowHeight);
	var maxSpeed = new Vector2(10f);

	// add physics component
	var position = RandomVector(Vector2.Zero, screenSize);
	var velocity = RandomVector(-maxSpeed, maxSpeed);
	ecs.AddComponent(id, new PhysicsObject(position, velocity));

	// add drawable component
	var symbol = RandomElement(s_symbols);
	var color = RandomEnumValue<Color16>();
	ecs.AddComponent(id, new Drawable(symbol, color));

	return id;
}
```

Now let's implement our game loop:
```csharp
private static void RunLoop(IConsoleBackend term, IContainer<GameState> ecs)
{
	const int msPerFrame = 20;
	var width = term.WindowWidth;
	var height = term.WindowHeight;
	var state = new GameState(msPerFrame / 1000f, width, height, term);
	// initialize
	ecs.InitializeSystems(state);

	// run
	while (true)
	{
		// Note - in a real game update and rendering logic
		// is often separated - there is no requirement
		// for putting the rendering logic inside ECS -
		// since it's a DI container, you can freely
		// resolve needed components and systems from
		// external code and use them in any other way
		ecs.ExecuteSystems(state);
		Thread.Sleep(20);
		if (term.KeyAvailable)
			break; // exit on any key press
	}
}
```

---
### TL;DR
---
1. Define your components as structs
2. Define your game/app state as a class (it will be passed to each system)
3. Define your systems by inheriting them from `SystemBase<T>` where T is the **state type**
4. Configure their types in an **ECS container**, then `Build()` it
5. Initialize the systems through the container and then run them as you wish

---

[Full code can be found here](https://github.com/RaZeR-RBI/ignis/tree/master/Samples/ConsoleSample).