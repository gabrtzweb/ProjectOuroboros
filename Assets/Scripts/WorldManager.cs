using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    [Header("World Dimensions")]
    public GameObject chunkPrefab;

    [Range(1, 10)]
    public int worldSizeX = 4;

    [Range(1, 10)]
    public int worldSizeY = 3;

    [Range(1, 10)]
    public int worldSizeZ = 4;

    [Header("Terrain Generation")]
    [Range(10, 100)]
    public int baseTerrainHeight = 32;

    [Range(0f, 1f)]
    public float dirtPercentage = 0.10f;

    [Range(0f, 1f)]
    public float stonePercentage = 0.45f;

    [Range(0f, 1f)]
    public float slatePercentage = 0.45f;

    public float noiseScale = 0.05f;
    public float heightMultiplier = 10f;

    public Dictionary<Vector3Int, ChunkData> chunks = new Dictionary<Vector3Int, ChunkData>();

    private readonly Dictionary<Vector3Int, ChunkRenderer> chunkRenderers = new Dictionary<Vector3Int, ChunkRenderer>();
    private float xOffset;
    private float yOffset;
    private float zOffset;

    private void Start()
    {
        GenerateWorld();
    }

    [ContextMenu("Generate World")]
    public void GenerateWorld()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);

            if (Application.isPlaying)
            {
                Destroy(child.gameObject);
            }
            else
            {
                DestroyImmediate(child.gameObject);
            }
        }

        chunks.Clear();
        chunkRenderers.Clear();

        float totalWidth = worldSizeX * VoxelData.ChunkWidth;
        float totalDepth = worldSizeZ * VoxelData.ChunkDepth;
        xOffset = totalWidth * 0.5f;
        zOffset = totalDepth * 0.5f;
        yOffset = baseTerrainHeight;

        for (int x = 0; x < worldSizeX; x++)
        {
            for (int y = 0; y < worldSizeY; y++)
            {
                for (int z = 0; z < worldSizeZ; z++)
                {
                    Vector3Int worldCoordinate = new Vector3Int(x, y, z);
                    Vector3 spawnPosition = new Vector3(
                        (x * VoxelData.ChunkWidth) - xOffset,
                        (y * VoxelData.ChunkHeight) - yOffset,
                        (z * VoxelData.ChunkDepth) - zOffset);

                    GameObject chunkObject = Instantiate(chunkPrefab, spawnPosition, Quaternion.identity);
                    chunkObject.transform.SetParent(transform, true);

                    ChunkRenderer chunkRenderer = chunkObject.GetComponent<ChunkRenderer>();
                    chunkRenderer.InitializeData(worldCoordinate, this);

                    chunks[worldCoordinate] = chunkRenderer.Data;
                    chunkRenderers[worldCoordinate] = chunkRenderer;
                }
            }
        }

        foreach (KeyValuePair<Vector3Int, ChunkData> chunkEntry in chunks)
        {
            chunkRenderers[chunkEntry.Key].GenerateMesh();
        }
    }

    public float NoiseScale => noiseScale;
    public float HeightMultiplier => heightMultiplier;
    public int BaseTerrainHeight => baseTerrainHeight;
    public float SlatePercentage => slatePercentage;
    public float StonePercentage => stonePercentage;
    public float DirtPercentage => dirtPercentage;
    public float YOffset => yOffset;

    public byte GetBlockFromWorld(Vector3Int worldPos)
    {
        Vector3Int adjustedWorldPos = new Vector3Int(
            Mathf.RoundToInt(worldPos.x + xOffset),
            Mathf.RoundToInt(worldPos.y + yOffset),
            Mathf.RoundToInt(worldPos.z + zOffset));

        Vector3Int chunkCoordinate = new Vector3Int(
            Mathf.FloorToInt((float)adjustedWorldPos.x / VoxelData.ChunkWidth),
            Mathf.FloorToInt((float)adjustedWorldPos.y / VoxelData.ChunkHeight),
            Mathf.FloorToInt((float)adjustedWorldPos.z / VoxelData.ChunkDepth));

        if (!chunks.TryGetValue(chunkCoordinate, out ChunkData chunkData))
        {
            return 0;
        }

        Vector3Int localPosition = new Vector3Int(
            adjustedWorldPos.x - (chunkCoordinate.x * VoxelData.ChunkWidth),
            adjustedWorldPos.y - (chunkCoordinate.y * VoxelData.ChunkHeight),
            adjustedWorldPos.z - (chunkCoordinate.z * VoxelData.ChunkDepth));

        return chunkData.GetBlock(localPosition.x, localPosition.y, localPosition.z);
    }
}
