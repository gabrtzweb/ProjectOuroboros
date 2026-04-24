# Ouroboros Voxel Engine (Ringworld)

A compact procedural voxel engine prototype for Unity that simulates a centrifugal ringworld habitat. Focused on performance, multithreaded chunk generation, and an interior-facing gravity model.

## Quick Start

- Open the folder in Unity Hub and load the `ProjectOuroboros` project.
- Press Play in the Scene named `Main` or the scene in `Assets/Scenes` to start.

## Highlights

- Multithreaded chunk generation using background tasks to avoid main-thread stalls.
- Custom centrifugal gravity so players can walk the inner surface of a ring.
- Concurrent data structures for safe chunk access across threads.
- Radial chunk loading around the player to prioritize nearby terrain.
- Texture atlas support for compact block textures.

## Features

- Procedural terrain generation (Perlin noise).
- Layered geology: grass, dirt, stone, and deeper layers.
- Creative-style movement: walk, sprint, crouch, jump, and toggle flight.
- Dynamic spawn height based on generated surface.

## Controls

- W/A/S/D — Move
- Mouse — Look
- Space — Jump (double-tap to toggle flight)
- Left Shift — Sprint
- Left Ctrl — Crouch

## Development Notes

- Code is primarily under `Assets/Scripts`.
- Check `ProjectSettings/ProjectVersion.txt` for the Unity editor version used to create the project.
- Use the Unity Profiler and Jobs/Tasks diagnostics when tuning chunk generation.

## Contributing

- Fork the repo, make changes on a feature branch, and open a pull request with a clear description.

## Upcoming Features

- Raycast-based block breaking and placing.
- Static voxel sun and dynamic day/night mechanical shutter.
- Triplanar mapped LODs for distant chunk rendering.
