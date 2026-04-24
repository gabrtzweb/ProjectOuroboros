using UnityEngine;

[System.Serializable]
public class ChunkData
{
    [SerializeField]
    private byte[] blocks;

    public ChunkData()
    {
        blocks = new byte[VoxelData.ChunkVolume];
    }

    public void Populate(Vector3Int worldPosition, int baseTerrainHeight, float slatePercentage, float stonePercentage, float dirtPercentage, float noiseScale, float heightMultiplier, float transitionNoiseScale, float transitionAmplitude, int ringWidthInChunks)
    {
        int maxGlobalX = (ringWidthInChunks * VoxelData.ChunkWidth) - 1;

        for (int z = 0; z < VoxelData.ChunkDepth; z++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                int globalX = (worldPosition.x * VoxelData.ChunkWidth) + x;
                int globalZ = (worldPosition.z * VoxelData.ChunkDepth) + z;

                float surfaceOffset = Mathf.PerlinNoise(globalX * noiseScale, globalZ * noiseScale) * heightMultiplier;
                int surfaceY = Mathf.FloorToInt(baseTerrainHeight + surfaceOffset);

                float layerOffset = (Mathf.PerlinNoise(globalX * transitionNoiseScale, globalZ * transitionNoiseScale) * transitionAmplitude) - (transitionAmplitude / 2f);
                int slateLimit = Mathf.FloorToInt((baseTerrainHeight * slatePercentage) + layerOffset);

                bool isLeftEdge = globalX <= 2;
                bool isRightEdge = globalX >= maxGlobalX - 2;
                bool isEdge = isLeftEdge || isRightEdge;

                for (int y = 0; y < VoxelData.ChunkHeight; y++)
                {
                    int index = VoxelData.ToIndex(x, y, z);
                    int globalY = (worldPosition.y * VoxelData.ChunkHeight) + y;

                    if (globalY < 3)
                    {
                        blocks[index] = (byte)BlockType.FoundationAlloy;
                        continue;
                    }

                    if (isEdge)
                    {
                        if (globalY <= surfaceY + 8)
                        {
                            blocks[index] = (byte)BlockType.FoundationAlloy;
                        }
                        else if (globalY <= surfaceY + 16)
                        {
                            bool isCenterWallBlock = globalX == 1 || globalX == maxGlobalX - 1;
                            blocks[index] = isCenterWallBlock
                                ? (byte)BlockType.FoundationBarrier
                                : (byte)BlockType.Air;
                        }
                        else
                        {
                            blocks[index] = (byte)BlockType.Air;
                        }

                        continue;
                    }

                    if (globalY > surfaceY)
                    {
                        blocks[index] = (byte)BlockType.Air;
                    }
                    else if (globalY == surfaceY)
                    {
                        blocks[index] = (byte)BlockType.Grass;
                    }
                    else if (globalY > surfaceY - 3)
                    {
                        blocks[index] = (byte)BlockType.Dirt;
                    }
                    else if (globalY < slateLimit)
                    {
                        blocks[index] = (byte)BlockType.Deepslate;
                    }
                    else
                    {
                        blocks[index] = (byte)BlockType.Stone;
                    }
                }
            }
        }
    }

    public byte GetBlock(int x, int y, int z)
    {
        if (!VoxelData.IsInBounds(x, y, z))
        {
            return 0;
        }

        return blocks[VoxelData.ToIndex(x, y, z)];
    }
}
