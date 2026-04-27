using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour {
    [Header("Materials")]
    public Material matOpaque;
    public Material matTransparent;
    public Material matCutout;

    [Header("Generation Limits")]
    public int renderDistance = 16;
    public int chunkBounds = 4;

    [Header("Terrain Generation Settings")]
    public int solidGroundHeight = 4; 
    public int terrainHeightMultiplier = 32; 
    public float terrainNoiseScale = 0.016f; 
    public int seaLevel = 8; 

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
        
        ChunkData chunkScript = newChunk.AddComponent<ChunkData>();
        
        // Pass all three materials to the MeshRenderer
        newChunk.GetComponent<MeshRenderer>().materials = new Material[] { matOpaque, matTransparent, matCutout };
        
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
