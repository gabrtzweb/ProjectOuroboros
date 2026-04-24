# Project Ouroboros

## Project Overview

Project Ouroboros is a Unity 6 procedural voxel engine prototype focused on generating a finite, looping ringworld cylinder. The current implementation establishes the core chunk data model and a baseline mesh pipeline for visible voxel faces.

## Architecture

The project separates voxel storage and world rules from rendering logic.

- Data layer: `VoxelData` defines chunk dimensions and index math, and `ChunkData` stores block IDs in a 1D byte array.
- Mesh layer: `ChunkRenderer` reads `ChunkData`, performs neighbor visibility checks, and builds Unity mesh buffers (vertices and triangles).

This separation keeps generation rules independent of Unity mesh construction details.
