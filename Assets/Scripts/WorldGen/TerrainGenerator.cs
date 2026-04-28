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

                    if (globalY <= VoxelConstants.WorldBottomLevel + 2) {
                        if (globalY == VoxelConstants.WorldBottomLevel) {
                            chunk.SetBlockType(x, y, z, BlockType.Bedrock);
                        } else {
                            // 60% chance of Bedrock at block -63, 20% at block -62
                            int bedrockChance = globalY == VoxelConstants.WorldBottomLevel + 1 ? 6 : 2;
                            bool isBedrock = (Mathf.Abs(hash) % 10) < bedrockChance;
                            chunk.SetBlockType(x, y, z, isBedrock ? BlockType.Bedrock : BlockType.Deepslate);
                        }
                    }
                    else if (globalY > column.SurfaceHeight) {
                        if (globalY <= worldManager.seaLevel) {
                            chunk.SetBlockType(x, y, z, BlockType.Water);
                        } 
                        // Check if it is the first block above ground
                        else if (globalY == column.SurfaceHeight + 1 && column.SurfaceHeight > worldManager.seaLevel) {
                            bool isBeach = column.SurfaceHeight <= worldManager.seaLevel + VoxelConstants.TerrainSandBeachOffset;
                            
                            if (isBeach) {
                                // Dry grass generation on sand
                                float dryPlantNoise = Mathf.PerlinNoise(globalX * 0.1f, globalZ * 0.1f);
                                if (dryPlantNoise > 0.6f) { // 40% of the beach has dry grass patches
                                    int plantHash = (globalX * 12345) ^ (globalZ * 67890);
                                    if ((Mathf.Abs(plantHash) % 100) < 15) { // 15% density inside patch
                                        chunk.SetBlockType(x, y, z, BlockType.ShortDryGrass);
                                    } else chunk.SetBlockType(x, y, z, BlockType.Air);
                                } else chunk.SetBlockType(x, y, z, BlockType.Air);
                            } else {
                                // Standard generation for grass and bushes on dirt biomes
                                float patchNoise = Mathf.PerlinNoise(globalX * 0.05f, globalZ * 0.05f);
                                if (patchNoise > 0.55f) {
                                    int plantHash = (globalX * 12345) ^ (globalZ * 67890);
                                    int rand = Mathf.Abs(plantHash) % 100;
                                    
                                    if (rand < 40) chunk.SetBlockType(x, y, z, BlockType.ShortGrass);
                                    else if (rand < 48) chunk.SetBlockType(x, y, z, BlockType.ShortBush);
                                    else chunk.SetBlockType(x, y, z, BlockType.Air);
                                } else chunk.SetBlockType(x, y, z, BlockType.Air);
                            }
                        }
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
