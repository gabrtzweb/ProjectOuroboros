using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour {
    public Material chunkMaterial;
    public int renderDistance = 16;
    
    // This value represents how many chunks extend in each vertical direction.
    // Setting it to 4 means the world goes from -4 to 3 (8 chunks total, matching -64 to +64).
    public int chunkBounds = 4;

    public Dictionary<Vector3Int, ChunkData> chunks = new Dictionary<Vector3Int, ChunkData>();

    void Start() {
        GenerateWorld();
    }

    void GenerateWorld() {
        for (int x = -renderDistance; x <= renderDistance; x++) {
            for (int z = -renderDistance; z <= renderDistance; z++) {
                
                Vector2 currentPos = new Vector2(x, z);
                if (Vector2.Distance(Vector2.zero, currentPos) > renderDistance) {
                    continue;
                }

                // Generates from -chunkBounds to chunkBounds - 1 (-4 to 3)
                for (int y = -chunkBounds; y < chunkBounds; y++) {
                    Vector3Int chunkCoord = new Vector3Int(x, y, z);
                    CreateChunkData(chunkCoord);
                }
            }
        }

        foreach (var chunk in chunks.Values) {
            chunk.GenerateMesh();
        }
    }

    void CreateChunkData(Vector3Int coord) {
        Vector3 position = new Vector3(
            coord.x * VoxelData.ChunkWidth, 
            coord.y * VoxelData.ChunkHeight, 
            coord.z * VoxelData.ChunkWidth
        );

        GameObject newChunk = new GameObject($"Chunk_{coord.x}_{coord.y}_{coord.z}");
        newChunk.transform.position = position;
        newChunk.transform.parent = this.transform;
        
        // Using the newly renamed ChunkData script
        ChunkData chunkScript = newChunk.AddComponent<ChunkData>();
        newChunk.GetComponent<MeshRenderer>().material = chunkMaterial;
        
        chunkScript.InitData(this, coord);
        chunks.Add(coord, chunkScript);
    }

    public BlockType GetBlockFromGlobal(Vector3Int globalPos) {
        int chunkX = Mathf.FloorToInt((float)globalPos.x / VoxelData.ChunkWidth);
        int chunkY = Mathf.FloorToInt((float)globalPos.y / VoxelData.ChunkHeight);
        int chunkZ = Mathf.FloorToInt((float)globalPos.z / VoxelData.ChunkWidth);

        Vector3Int chunkCoord = new Vector3Int(chunkX, chunkY, chunkZ);

        if (chunks.TryGetValue(chunkCoord, out ChunkData chunk)) {
            int localX = globalPos.x - (chunkX * VoxelData.ChunkWidth);
            int localY = globalPos.y - (chunkY * VoxelData.ChunkHeight);
            int localZ = globalPos.z - (chunkZ * VoxelData.ChunkWidth);
            
            return chunk.GetBlockType(localX, localY, localZ);
        }

        return BlockType.Air; 
    }
}