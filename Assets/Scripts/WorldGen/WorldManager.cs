using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour {
    #region Serialized References
    
    [Header("References")]
    public Transform playerTransform;
    public WorldConfiguration config;
    
    [Header("Materials")]
    public Material opaqueMaterial;
    public Material transparentMaterial;
    public Material matCutout;
    
    #endregion
    
    #region Chunk Data
    
    public Dictionary<Vector3Int, ChunkData> chunks = new Dictionary<Vector3Int, ChunkData>();
    
    #endregion
    
    #region Lifecycle Methods

    void Start() {
        if (config == null) {
            Debug.LogError("World Configuration is missing!");
            return;
        }
        GenerateWorld();
    }
    
    #endregion
    
    #region World Generation

    void GenerateWorld() {
        for (int x = -config.renderDistance; x <= config.renderDistance; x++) {
            for (int z = -config.renderDistance; z <= config.renderDistance; z++) {
                
                Vector2 currentPos = new Vector2(x, z);
                if (Vector2.Distance(Vector2.zero, currentPos) > config.renderDistance) {
                    continue;
                }

                for (int y = -config.chunkBounds; y < config.chunkBounds; y++) {
                    Vector3Int chunkCoord = new Vector3Int(x, y, z);
                    CreateChunkData(chunkCoord);
                }
            }
        }

        foreach (var chunk in chunks.Values) {
            chunk.GenerateMesh();
        }

        SpawnPlayer();
    }
    
    #endregion
    
    #region Chunk Management

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
        
        newChunk.GetComponent<MeshRenderer>().materials = new Material[] { opaqueMaterial, transparentMaterial, matCutout };
        
        chunkScript.InitData(this, coord);
        chunks.Add(coord, chunkScript);
    }
    
    #endregion
    
    #region Terrain Sampling

    public int CalculateSurfaceHeight(int globalX, int globalZ) {
        float scale = config.terrainNoiseScale;
        float offset = config.noiseOffset;

        float posX = (globalX + offset) * scale;
        float posZ = (globalZ + offset) * scale;

        float noise1 = Mathf.PerlinNoise(posX, posZ);
        float noise2 = Mathf.PerlinNoise(posX * 2f, posZ * 2f) * 0.5f;
        float noise3 = Mathf.PerlinNoise(posX * 4f, posZ * 4f) * 0.25f;

        float totalNoise = (noise1 + noise2 + noise3) / 1.75f;
        totalNoise = Mathf.Pow(totalNoise, config.heightCurve);

        return config.solidGroundHeight + Mathf.FloorToInt(totalNoise * config.terrainHeightMultiplier);
    }
    
    #endregion
    
    #region Player Spawning

    void SpawnPlayer() {
        if (playerTransform != null) {
            CharacterController cc = playerTransform.GetComponent<CharacterController>();
            
            if (cc != null) cc.enabled = false;
            
            int spawnY = CalculateSurfaceHeight(0, 0);
            playerTransform.position = new Vector3(0.5f, spawnY + 2f, 0.5f);
            
            if (cc != null) cc.enabled = true;
        }
    }
    
    #endregion
    
    #region Block Operations

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
    
    public void SetBlock(Vector3Int globalPos, BlockType type) {
        int chunkX = Mathf.FloorToInt((float)globalPos.x / VoxelData.ChunkWidth);
        int chunkY = Mathf.FloorToInt((float)globalPos.y / VoxelData.ChunkHeight);
        int chunkZ = Mathf.FloorToInt((float)globalPos.z / VoxelData.ChunkWidth);

        Vector3Int chunkCoord = new Vector3Int(chunkX, chunkY, chunkZ);

        if (chunks.TryGetValue(chunkCoord, out ChunkData chunk)) {
            int localX = globalPos.x - (chunkX * VoxelData.ChunkWidth);
            int localY = globalPos.y - (chunkY * VoxelData.ChunkHeight);
            int localZ = globalPos.z - (chunkZ * VoxelData.ChunkWidth);
            
            chunk.SetBlockType(localX, localY, localZ, type);
            chunk.GenerateMesh();

            UpdateAdjacentChunks(chunkCoord, localX, localY, localZ);
        }
    }
    
    #endregion
    
    #region Adjacent Chunk Updates
    
    void UpdateAdjacentChunks(Vector3Int chunkCoord, int localX, int localY, int localZ) {
        if (localX == 0) UpdateChunkAt(chunkCoord + new Vector3Int(-1, 0, 0));
        if (localX == VoxelData.ChunkWidth - 1) UpdateChunkAt(chunkCoord + new Vector3Int(1, 0, 0));
        if (localY == 0) UpdateChunkAt(chunkCoord + new Vector3Int(0, -1, 0));
        if (localY == VoxelData.ChunkHeight - 1) UpdateChunkAt(chunkCoord + new Vector3Int(0, 1, 0));
        if (localZ == 0) UpdateChunkAt(chunkCoord + new Vector3Int(0, 0, -1));
        if (localZ == VoxelData.ChunkWidth - 1) UpdateChunkAt(chunkCoord + new Vector3Int(0, 0, 1));
    }
    
    void UpdateChunkAt(Vector3Int coord) {
        if (chunks.TryGetValue(coord, out ChunkData chunk)) {
            chunk.GenerateMesh();
        }
    }
    
    #endregion
}
