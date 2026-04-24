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
                int stoneLimit = Mathf.FloorToInt((baseTerrainHeight * (slatePercentage + stonePercentage)) + layerOffset);

                for (int y = 0; y < VoxelData.ChunkHeight; y++)
                {
                    int index = VoxelData.ToIndex(x, y, z);
                    int globalY = (worldPosition.y * VoxelData.ChunkHeight) + y;

                    if (globalX == 0 || globalX == maxGlobalX)
                    {
                        if (globalY <= surfaceY + 4)
                        {
                            blocks[index] = 4;
                        }
                        else
                        {
                            blocks[index] = 0;
                        }
                    }
                    else if (globalY > surfaceY)
                    {
                        blocks[index] = 0;
                    }
                    else if (globalY == surfaceY)
                    {
                        blocks[index] = 1;
                    }
                    else if (globalY < slateLimit)
                    {
                        blocks[index] = 4;
                    }
                    else if (globalY < stoneLimit)
                    {
                        blocks[index] = 3;
                    }
                    else
                    {
                        blocks[index] = 2;
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
