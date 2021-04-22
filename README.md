![Stability: alpha](https://img.shields.io/badge/stability-alpha-orange.svg)

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
- *TODO: Add samples*

## Goals
- Easy to use API
- Minimal or zero heap allocations in hot paths
- Simple architecture

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

*TODO: Add samples*