using UnityEngine;

[CreateAssetMenu(fileName = "NewWorldConfig", menuName = "VoxelEngine/World Configuration")]
public class WorldConfiguration : ScriptableObject {
    [Header("Generation Limits")]
    public int renderDistance = 16;
    public int chunkBounds = 4;
    
    [Header("Terrain Generation Settings")]
    public int solidGroundHeight = -4; 
    public int terrainHeightMultiplier = 48; 
    public float terrainNoiseScale = 0.005f; 
    public int seaLevel = 8; 
    public float noiseOffset = 10000f; 
    public float heightCurve = 1.2f; 
    public int deepslateTransitionLevel = -32;
}
