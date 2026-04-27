using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class ChunkData : MonoBehaviour {
    MeshFilter meshFilter;
    MeshCollider meshCollider; 
    Mesh mesh;
    
    BlockType[] voxelMap = new BlockType[VoxelData.ChunkVolume];

    WorldManager worldManager;
    Vector3Int chunkCoord;

    public WorldManager WorldManager => worldManager;
    public Vector3Int ChunkCoord => chunkCoord;

    public void InitData(WorldManager world, Vector3Int coord) {
        worldManager = world;
        chunkCoord = coord;
        
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>(); 

        mesh = new Mesh();
        mesh.MarkDynamic();
        meshFilter.sharedMesh = mesh;
        meshCollider.sharedMesh = mesh;
        
        TerrainGenerator.PopulateVoxelMap(this);
    }

    public void GenerateMesh() {
        MeshBuilder.BuildMesh(this, mesh);
        meshFilter.sharedMesh = mesh;
        meshCollider.sharedMesh = mesh;
    }

    public void SetBlockType(int x, int y, int z, BlockType type) {
        int index = VoxelData.Get1DIndex(x, y, z);
        voxelMap[index] = type;
    }

    public BlockType GetBlockType(int x, int y, int z) {
        int index = VoxelData.Get1DIndex(x, y, z);
        return voxelMap[index];
    }

    public BlockType GetVoxelType(Vector3Int localPos) {
        if (localPos.x < 0 || localPos.x >= VoxelData.ChunkWidth || 
            localPos.y < 0 || localPos.y >= VoxelData.ChunkHeight || 
            localPos.z < 0 || localPos.z >= VoxelData.ChunkWidth) {
            
            Vector3Int globalPos = new Vector3Int(
                localPos.x + (chunkCoord.x * VoxelData.ChunkWidth),
                localPos.y + (chunkCoord.y * VoxelData.ChunkHeight),
                localPos.z + (chunkCoord.z * VoxelData.ChunkWidth)
            );
            
            return worldManager.GetBlockFromGlobal(globalPos);
        }

        return GetBlockType(localPos.x, localPos.y, localPos.z);
    }
}
