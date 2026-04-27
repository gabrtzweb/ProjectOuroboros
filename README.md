# Ouroboros Voxel Engine (Ringworld)

A compact procedural voxel engine prototype for Unity that simulates a centrifugal ringworld habitat. Focused on performance, modular architecture, and an interior-facing gravity model.

The current terrain and chunk system are compatible with an inner-surface world. Future updates will introduce curved coordinate mapping, gravity vectors per-chunk, and multi-scale LOD for very large ring structures.

## Current Features

- **Procedural Generation:** Chunked voxel terrain using layered Perlin noise (configurable scale, height curves, offsets) with distinct surface and underground layers.
- **Block Variants:** Multiple block types (Grass, Dirt, Stone, Deepslate, Sand, Gravel, Water, Leaves) with procedural texture variation.
- **Advanced Meshing:** Optimized greedy/face mesh generation using three distinct submeshes: Opaque, Transparent (Water), and Cutout (Leaves/Grass). Mesh objects are cached and cleared to prevent GC spikes.
- **Player Controller:** Kinematic character movement using `CharacterController` featuring walking, sprinting, exact-height jumping, crouching, and crawling with smooth hitbox transitions.
- **Dynamic Camera System:** Cinemachine integration allowing seamless toggling between First-Person and Third-Person perspectives, including auto-obstacle avoidance.
- **Modular Input:** Centralized input handling using Unity's new Input System.
- **Debug UI:** Real-time TextMeshPro overlay displaying global block coordinates and cardinal facing direction.

## Controls

- **W/A/S/D** — Move
- **Mouse** — Look around
- **Space** — Jump
- **Left Shift** — Sprint
- **Left Ctrl** — Crouch
- **C** — Crawl
- **V** — Toggle Perspective (1st/3rd Person)
- **Left Click** — Primary Action (Ready for block breaking)
- **Right Click** — Secondary Action (Ready for block placing)

## Roadmap

1. **World Interaction:** Implement camera-centered Raycasting for breaking and placing blocks, triggering chunk mesh updates.
2. **Procedural Flora:** Add surface structures like trees, tall grass, and flowers generated after the base terrain.
3. **3D Noise Subterranean:** Integrate FastNoiseLite to carve out volumetric caves, overhangs, and complex underground networks.
4. **Multithreading:** Offload chunk voxel population and mesh data calculation to background threads/Unity Jobs to enable seamless infinite exploration.
5. **Ringworld Physics:** Implement the centrifugal gravity model and curved coordinate rendering for the final habitat aesthetic.
