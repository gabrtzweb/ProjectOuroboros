# Ouroboros Voxel Engine (Ringworld)

A compact procedural voxel engine prototype for Unity that simulates a centrifugal ringworld habitat. Focused on performance, multithreaded chunk generation, and an interior-facing gravity model.

The current terrain and chunk system are compatible with an inner-surface world, later work I will add curved coordinate mapping, gravity vectors per-chunk, and multi-scale LOD for very large ring.

## Current Features

- Chunked voxel terrain generation using layered Perlin noise (configurable scale, height, offsets).
- Procedural block placement with multiple block types (Grass, Dirt, Stone, Deepslate, Sand, Gravel, Water, Leaves).
- Chunk mesh generation with three submeshes: opaque, water (transparent), and cutout (leaves/grass).
- Simple player controller using `CharacterController`: walking, sprinting, and jumping.
- Interaction input hooks (primary/secondary) present as stubs for block breaking/placing.
- WorldManager exposes generation parameters (render distance, sea level, noise offsets) for fast iteration.

## Controls

- W/A/S/D — Move
- Mouse — Look
- Space — Jump
- Left Shift — Sprint
- Left Ctrl — Crouch
