# Ouroboros Voxel Engine (Ringworld)

A compact procedural voxel engine prototype for Unity that simulates a centrifugal ringworld habitat. Focused on performance, modular architecture, and an interior-facing gravity model. At the end of this readme there is our outlines of the mathematical foundation and world-building constraints for the current ringworld project.

The current terrain and chunk system are compatible with an inner-surface world. Future updates will introduce curved coordinate mapping, gravity vectors per-chunk, and multi-scale LOD for very large ring structures.

## Current Features

- **Procedural Generation:** Chunked voxel terrain using layered Perlin noise with distinct surface and underground layers. Fully configurable via `WorldConfiguration` ScriptableObjects.
- **Block Variants:** Multiple block types (Grass, Dirt, Stone, Deepslate, Sand, Gravel, Water, Leaves) with procedural texture variation.
- **Advanced Meshing:** Optimized greedy/face mesh generation using three distinct submeshes: Opaque, Transparent (Water), and Cutout (Leaves/Grass). Uses dynamic face culling and reuses memory buffers to prevent GC spikes.
- **Player Controller:** Robust kinematic movement using a central wrapper. Features walking, sprinting, crouching, crawling, sneak edge-detection, a double-tap creative flight mode, and void-fall rescue.
- **World Interaction:** Fast continuous block breaking and placing using camera-centered raycasting. Includes bounding-box checks to prevent placing blocks inside the player and bedrock-level restrictions.
- **Dynamic Camera System:** Cinemachine integration allowing seamless toggling between First-Person and Third-Person perspectives.
- **Modular Input:** Centralized input handling using Unity's new Input System.
- **Debug UI:** Real-time TextMeshPro overlay displaying FPS metrics, global block coordinates, and cardinal facing direction.

## Controls

- **W/A/S/D** — Move
- **Mouse** — Look around
- **Space** — Jump (Double-tap to toggle Flight mode)
- **Left Shift** — Sprint (Also increases Flight speed)
- **Left Ctrl** — Crouch (Prevents falling off edges, moves down while flying)
- **C** — Crawl
- **V** — Toggle Perspective (1st/3rd Person)
- **Left Click** — Primary Action (Hold for fast block breaking)
- **Right Click** — Secondary Action (Hold for fast block placing)
- **Middle Mouse** — Pick Block

## Roadmap

1. **Flora & Cross-Meshing:** Update the meshing algorithm to render foliage (tall grass, bushes) as intersecting cross-planes instead of solid cubes.
2. **Voxel Raycast:** Implement a custom mathematical raycast to bypass Unity's physics, allowing players to walk through foliage while still being able to target and break it.
3. **Animated Shaders:** Implement vertex displacement in Shader Graph to create dynamic, animated water waves without distorting the texture atlas.
4. **3D Noise Subterranean:** Integrate FastNoiseLite to carve out volumetric caves, overhangs, and complex underground networks.
5. **Multithreading:** Offload chunk voxel population and mesh data calculation to background threads/Unity Jobs to enable seamless infinite exploration.
6. **Ringworld Physics:** Implement the centrifugal gravity model, curved coordinate rendering, and visual distortion for the final habitat aesthetic.

## Ringworld Dimensions & Specifications

1. Physical Scale
    - **Ring Radius:** 651.9 Meters (Surface baseline at Y=0)
    - **Ring Length:** 256 Chunks (Total circumference: 4,096 Meters)
    - **Ring Width:** 16 Chunks (Total lateral span: 256 Meters)
    - **Ring Height:** 8 Chunks Total (128 Blocks of vertical volume)

2. Coordinate Boundaries & Movement Logic
    - **Min Chunk Y:** -4 (Dig depth of -64 blocks below the surface)
    - **Max Chunk Y:** 3 (Maximum altitude of +64 blocks above the surface)
    - **North / South (Longitudinal):** Represents movement along the main curve of the ring. Walking North or South moves the player along the 256-chunk circumference, returning to the starting point after 4,096 meters.
    - **East / West (Lateral):** Represents movement across the 16-chunk width of the ring. Moving East or West travels across the 256-meter "highway" toward the foundation barriers or the open edges of the world.

3. Visibility & Performance
    - **Active Chunks:** 16 Chunks (High-detail physics and interaction radius around the player)
    - **LOD View Distance:** 128 Chunks (Ensures the full loop of the ring is always visible from any point)

4. World Metrics
    - **Walking Speed:** ~16 minutes for a full 4,096m lap.
    - **Voxel Distortion:** Maximum ~10% at the extreme top/bottom boundaries (within acceptable visual limits).
