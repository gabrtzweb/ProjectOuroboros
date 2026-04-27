using UnityEngine;

public static class TerrainGenerator {
    struct ColumnData {
        public int SurfaceHeight;
        public int BaseDirtBoundary;
        public int BaseCoarseBoundary;
        public int BaseGravelBoundary;
        public int BaseDeepslateBoundary;
        public float MixNoise;
    }

    public static void PopulateVoxelMap(ChunkData chunk) {
        WorldManager worldManager = chunk.WorldManager;

        for (int x = 0; x < VoxelData.ChunkWidth; x++) {
            for (int z = 0; z < VoxelData.ChunkWidth; z++) {
                int globalX = x + (chunk.ChunkCoord.x * VoxelData.ChunkWidth);
                int globalZ = z + (chunk.ChunkCoord.z * VoxelData.ChunkWidth);
                ColumnData column = SampleColumnData(worldManager, globalX, globalZ);

                for (int y = 0; y < VoxelData.ChunkHeight; y++) {
                    int globalY = y + (chunk.ChunkCoord.y * VoxelData.ChunkHeight);
                    int hash = (globalX * 37476139) ^ (globalY * 66826521) ^ (globalZ * 25497383);
                    hash = (hash ^ (hash >> 13)) * 12741261;

                    int dither = (Mathf.Abs(hash) % VoxelConstants.TerrainDitherModulus) - VoxelConstants.TerrainDitherOffset;

                    int dirtBoundary = column.BaseDirtBoundary + dither;
                    int coarseBoundary = column.BaseCoarseBoundary + dither;
                    int gravelBoundary = column.BaseGravelBoundary + dither;
                    int deepslateDitheredBoundary = column.BaseDeepslateBoundary + dither;

                    if (globalY > column.SurfaceHeight) {
                        chunk.SetBlockType(x, y, z, globalY <= worldManager.seaLevel ? BlockType.Water : BlockType.Air);
                    }
                    else if (globalY == column.SurfaceHeight) {
                        if (globalY == worldManager.seaLevel + VoxelConstants.TerrainSandBeachOffset) {
                            chunk.SetBlockType(x, y, z, BlockType.Sand);
                        }
                        else if (globalY <= worldManager.seaLevel) {
                            if (globalY >= worldManager.seaLevel - VoxelConstants.TerrainShallowWaterDepth) {
                                if (column.MixNoise > VoxelConstants.TerrainShallowWaterMixThreshold) {
                                    chunk.SetBlockType(x, y, z, BlockType.Gravel);
                                }
                                else if (column.MixNoise > VoxelConstants.TerrainMidWaterMixThreshold) {
                                    chunk.SetBlockType(x, y, z, BlockType.Sand);
                                }
                                else {
                                    chunk.SetBlockType(x, y, z, BlockType.Dirt);
                                }
                            }
                            else {
                                chunk.SetBlockType(x, y, z, column.MixNoise > VoxelConstants.TerrainDeepWaterMixThreshold ? BlockType.Gravel : BlockType.Dirt);
                            }
                        }
                        else {
                            chunk.SetBlockType(x, y, z, BlockType.Grass);
                        }
                    }
                    else if (globalY >= dirtBoundary) {
                        chunk.SetBlockType(x, y, z, BlockType.Dirt);
                    }
                    else if (globalY >= coarseBoundary) {
                        chunk.SetBlockType(x, y, z, BlockType.CoarseDirt);
                    }
                    else if (globalY >= gravelBoundary) {
                        chunk.SetBlockType(x, y, z, BlockType.Gravel);
                    }
                    else if (globalY <= deepslateDitheredBoundary) {
                        chunk.SetBlockType(x, y, z, BlockType.Deepslate);
                    }
                    else {
                        chunk.SetBlockType(x, y, z, BlockType.Stone);
                    }
                }
            }
        }
    }

    static ColumnData SampleColumnData(WorldManager worldManager, int globalX, int globalZ) {
        int surfaceHeight = worldManager.CalculateSurfaceHeight(globalX, globalZ);

        float layerWave = Mathf.PerlinNoise(
            (globalX + worldManager.noiseOffset) * VoxelConstants.TerrainLayerWaveScale,
            (globalZ + worldManager.noiseOffset) * VoxelConstants.TerrainLayerWaveScale
        ) * VoxelConstants.TerrainLayerWaveAmplitude;

        int baseDirtBoundary = surfaceHeight - VoxelConstants.TerrainDirtBoundaryOffset - Mathf.FloorToInt(layerWave * 0.5f);
        int baseCoarseBoundary = surfaceHeight - VoxelConstants.TerrainCoarseBoundaryOffset - Mathf.FloorToInt(layerWave);
        int baseGravelBoundary = surfaceHeight - VoxelConstants.TerrainGravelBoundaryOffset - Mathf.FloorToInt(layerWave);

        float deepslateWave = Mathf.PerlinNoise(
            (globalX + worldManager.noiseOffset) * VoxelConstants.TerrainDeepslateWaveScale,
            (globalZ + worldManager.noiseOffset) * VoxelConstants.TerrainDeepslateWaveScale
        ) * VoxelConstants.TerrainDeepslateWaveAmplitude;

        int baseDeepslateBoundary = worldManager.deepslateTransitionLevel + Mathf.FloorToInt(deepslateWave);

        float mixNoise = Mathf.PerlinNoise(
            (globalX + worldManager.noiseOffset) * VoxelConstants.TerrainUnderwaterMixNoiseScale,
            (globalZ + worldManager.noiseOffset) * VoxelConstants.TerrainUnderwaterMixNoiseScale
        );

        return new ColumnData {
            SurfaceHeight = surfaceHeight,
            BaseDirtBoundary = baseDirtBoundary,
            BaseCoarseBoundary = baseCoarseBoundary,
            BaseGravelBoundary = baseGravelBoundary,
            BaseDeepslateBoundary = baseDeepslateBoundary,
            MixNoise = mixNoise
        };
    }
}