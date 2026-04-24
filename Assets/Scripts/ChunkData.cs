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

    public void Populate(Vector3Int worldPosition, int baseTerrainHeight, float slatePercentage, float stonePercentage, float dirtPercentage, float noiseScale, float heightMultiplier)
    {
        for (int z = 0; z < VoxelData.ChunkDepth; z++)
        {
            for (int y = 0; y < VoxelData.ChunkHeight; y++)
            {
                for (int x = 0; x < VoxelData.ChunkWidth; x++)
                {
                    int index = VoxelData.ToIndex(x, y, z);
                    int globalX = (worldPosition.x * VoxelData.ChunkWidth) + x;
                    int globalY = (worldPosition.y * VoxelData.ChunkHeight) + y;
                    int globalZ = (worldPosition.z * VoxelData.ChunkDepth) + z;

                    float surfaceOffset = Mathf.PerlinNoise(globalX * noiseScale, globalZ * noiseScale) * heightMultiplier;
                    int surfaceY = Mathf.FloorToInt(baseTerrainHeight + surfaceOffset);

                    if (globalY > surfaceY)
                    {
                        blocks[index] = 0;
                    }
                    else if (globalY == surfaceY)
                    {
                        blocks[index] = 1;
                    }
                    else if (globalY < baseTerrainHeight * slatePercentage)
                    {
                        blocks[index] = 4;
                    }
                    else if (globalY < baseTerrainHeight * (slatePercentage + stonePercentage))
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
