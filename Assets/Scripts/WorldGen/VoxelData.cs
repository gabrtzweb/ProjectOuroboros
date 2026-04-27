using UnityEngine;

public static class VoxelData {
    public static readonly int ChunkWidth = 16;
    public static readonly int ChunkHeight = 16;
    public static readonly int ChunkVolume = ChunkWidth * ChunkHeight * ChunkWidth;

    public static readonly int TextureAtlasSizeInBlocks = 16;
    
    public static float NormalizedBlockTextureSize {
        get { return 1f / (float)TextureAtlasSizeInBlocks; }
    }

    public static readonly Vector3[] voxelVerts = new Vector3[8] {
        new Vector3(0,0,0), new Vector3(1,0,0), new Vector3(1,1,0), new Vector3(0,1,0),
        new Vector3(0,0,1), new Vector3(1,0,1), new Vector3(1,1,1), new Vector3(0,1,1)
    };

    public static readonly Vector3Int[] faceChecks = new Vector3Int[6] {
        new Vector3Int(0, 0, -1), new Vector3Int(0, 0, 1),
        new Vector3Int(0, 1, 0), new Vector3Int(0, -1, 0),
        new Vector3Int(-1, 0, 0), new Vector3Int(1, 0, 0)
    };

    public static readonly int[,] voxelTris = new int[6,4] {
        {0, 3, 1, 2}, 
        {5, 6, 4, 7}, 
        {3, 7, 2, 6}, 
        {1, 5, 0, 4}, 
        {4, 7, 0, 3}, 
        {1, 2, 5, 6}  
    };

    public static int Get1DIndex(int x, int y, int z) {
        return x + (y * ChunkWidth) + (z * ChunkWidth * ChunkHeight);
    }
}

public enum BlockType : byte {
    Air = 0,
    Grass = 1,
    Dirt = 2,
    CoarseDirt = 3,
    Stone = 4,
    Deepslate = 5,
    Gravel = 6
}
