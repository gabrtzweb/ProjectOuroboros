# Ouroboros Voxel Engine (Ringworld)

A compact procedural voxel engine prototype for Unity that simulates a centrifugal ringworld habitat. Focused on performance, modular architecture, and an interior-facing gravity model. At the end of this readme there is our outlines of the mathematical foundation and world-building constraints for the current ringworld project.

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

## Ringworld Dimensions & Specifications

1. Physical Scale

- **Ring Radius:** 651.9 Meters (Surface baseline at Y=0)
- **Ring Length:** 256 Chunks (Total circumference: 4,096 Meters)
- **Ring Width:** 16 Chunks (Total lateral span: 256 Meters)
- **Ring Height:** 8 Chunks Total (128 Blocks of vertical volume)

2. Coordinate Boundaries

- **Min Chunk Y:** -4 (Dig depth of -64 blocks below the surface)
- **Max Chunk Y:** 3 (Maximum altitude of +64 blocks above the surface)

3. Visibility & Performance

- **Active Chunks:** 16 Chunks (High-detail physics and interaction radius around the player)
- **LOD View Distance:** 128 Chunks (Ensures the full loop of the ring is always visible from any point)

4. World Metrics

- **Walking Speed:** ~16 minutes for a full 4,096m lap.
- **Voxel Distortion:** Maximum ~10% at the extreme top/bottom boundaries (within acceptable visual limits).
