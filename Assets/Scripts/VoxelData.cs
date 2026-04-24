using UnityEngine;

public static class VoxelData
{
    public const int ChunkWidth = 16;
    public const int ChunkHeight = 16;
    public const int ChunkDepth = 16;

    public const int ChunkVolume = ChunkWidth * ChunkHeight * ChunkDepth;

    public static int ToIndex(Vector3Int localPosition)
    {
        return ToIndex(localPosition.x, localPosition.y, localPosition.z);
    }

    public static int ToIndex(int x, int y, int z)
    {
        return x + ChunkWidth * (y + ChunkHeight * z);
    }

    public static bool IsInBounds(Vector3Int localPosition)
    {
        return IsInBounds(localPosition.x, localPosition.y, localPosition.z);
    }

    public static bool IsInBounds(int x, int y, int z)
    {
        return x >= 0 && x < ChunkWidth
            && y >= 0 && y < ChunkHeight
            && z >= 0 && z < ChunkDepth;
    }
}
