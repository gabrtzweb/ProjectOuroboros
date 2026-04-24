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

    public void Populate()
    {
        for (int z = 0; z < VoxelData.ChunkDepth; z++)
        {
            for (int y = 0; y < VoxelData.ChunkHeight; y++)
            {
                for (int x = 0; x < VoxelData.ChunkWidth; x++)
                {
                    int index = VoxelData.ToIndex(x, y, z);
                    blocks[index] = y < 8 ? (byte)1 : (byte)0;
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
