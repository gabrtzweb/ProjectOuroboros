using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    [Header("World Dimensions")]
    public GameObject chunkPrefab;

    [Range(1, 10)]
    public int worldSizeY = 3;

    [Range(1, 10)]
    public int worldSizeZ = 4;

    public Transform viewer;
    public int viewDistanceInChunks = 4;
    public int ringWidthInChunks = 16;

    public float ringRadius = 1000f;

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
    public float transitionNoiseScale = 0.1f;
    public float transitionAmplitude = 4f;

    public ConcurrentDictionary<Vector3Int, ChunkData> chunks = new ConcurrentDictionary<Vector3Int, ChunkData>();

    private readonly Dictionary<Vector3Int, ChunkRenderer> chunkRenderers = new Dictionary<Vector3Int, ChunkRenderer>();
    private readonly Queue<Vector3Int> chunksToGenerate = new Queue<Vector3Int>();
    private static readonly Vector3Int[] NeighborChunkOffsets =
    {
        new Vector3Int(1, 0, 0),
        new Vector3Int(-1, 0, 0),
        new Vector3Int(0, 1, 0),
        new Vector3Int(0, -1, 0),
        new Vector3Int(0, 0, 1),
        new Vector3Int(0, 0, -1)
    };

    private Vector2Int currentViewerChunkCoord;
    private float xOffset;
    private float yOffset;
    private float zOffset;
    private CancellationTokenSource generationCancellation;
    private bool isShuttingDown;

    private void Awake()
    {
        generationCancellation = new CancellationTokenSource();
        _ = ProcessChunkQueueAsync(generationCancellation.Token);
        GenerateWorld();
    }

    private void OnDisable()
    {
        StopGenerationLoop();
    }

    private void OnDestroy()
    {
        StopGenerationLoop();
    }

    private void OnApplicationQuit()
    {
        isShuttingDown = true;
        StopGenerationLoop();
    }

    private void Update()
    {
        if (isShuttingDown)
        {
            return;
        }

        UpdateVisibleChunks();
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
        lock (chunksToGenerate)
        {
            chunksToGenerate.Clear();
        }

        float totalWidth = ringWidthInChunks * VoxelData.ChunkWidth;
        float totalDepth = worldSizeZ * VoxelData.ChunkDepth;
        xOffset = totalWidth * 0.5f;
        zOffset = totalDepth * 0.5f;
        yOffset = baseTerrainHeight;

        UpdateVisibleChunks();
    }

    private void UpdateVisibleChunks()
    {
        if (isShuttingDown || viewer == null)
        {
            return;
        }

        currentViewerChunkCoord = new Vector2Int(
            Mathf.FloorToInt((viewer.position.x + xOffset) / VoxelData.ChunkWidth),
            Mathf.FloorToInt((viewer.position.z + zOffset) / VoxelData.ChunkDepth));

        HashSet<Vector3Int> chunksInView = new HashSet<Vector3Int>();
        HashSet<Vector3Int> chunksToRemesh = new HashSet<Vector3Int>();
        List<Vector3Int> newChunksToLoad = new List<Vector3Int>();

        for (int x = -viewDistanceInChunks; x <= viewDistanceInChunks; x++)
        {
            int targetChunkX = currentViewerChunkCoord.x + x;
            if (targetChunkX < 0 || targetChunkX >= ringWidthInChunks)
            {
                continue;
            }

            for (int y = 0; y < worldSizeY; y++)
            {
                for (int z = -viewDistanceInChunks; z <= viewDistanceInChunks; z++)
                {
                    Vector3Int worldCoordinate = new Vector3Int(
                        targetChunkX,
                        y,
                        currentViewerChunkCoord.y + z);

                    chunksInView.Add(worldCoordinate);

                    if (chunks.ContainsKey(worldCoordinate))
                    {
                        continue;
                    }

                    if (!chunksToGenerate.Contains(worldCoordinate))
                    {
                        newChunksToLoad.Add(worldCoordinate);
                    }
                }
            }
        }

        newChunksToLoad.Sort((a, b) =>
        {
            float distA = Vector2.Distance(new Vector2(a.x, a.z), currentViewerChunkCoord);
            float distB = Vector2.Distance(new Vector2(b.x, b.z), currentViewerChunkCoord);
            return distA.CompareTo(distB);
        });

        lock (chunksToGenerate)
        {
            foreach (Vector3Int chunk in newChunksToLoad)
            {
                chunksToGenerate.Enqueue(chunk);
            }
        }

        List<Vector3Int> chunksToDestroy = new List<Vector3Int>();

        foreach (Vector3Int chunkCoord in chunks.Keys)
        {
            if (!chunksInView.Contains(chunkCoord))
            {
                chunksToDestroy.Add(chunkCoord);
            }
        }

        foreach (Vector3Int chunkCoord in chunksToDestroy)
        {
            AddChunkAndNeighborsToRemesh(chunkCoord, chunksToRemesh);

            if (chunkRenderers.TryGetValue(chunkCoord, out ChunkRenderer chunkRenderer))
            {
                if (Application.isPlaying)
                {
                    Destroy(chunkRenderer.gameObject);
                }
                else
                {
                    DestroyImmediate(chunkRenderer.gameObject);
                }
            }

            chunkRenderers.Remove(chunkCoord);
            chunks.TryRemove(chunkCoord, out _);
        }

        foreach (Vector3Int chunkCoord in chunksToRemesh)
        {
            if (chunkRenderers.TryGetValue(chunkCoord, out ChunkRenderer chunkRenderer))
            {
                chunkRenderer.GenerateMeshAsync();
            }
        }
    }

    private async Task ProcessChunkQueueAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && !isShuttingDown)
        {
            Vector3Int worldCoordinate = default;
            bool hasChunk = false;

            lock (chunksToGenerate)
            {
                if (chunksToGenerate.Count > 0)
                {
                    worldCoordinate = chunksToGenerate.Dequeue();
                    hasChunk = true;
                }
            }

            if (hasChunk && !chunks.ContainsKey(worldCoordinate))
            {
                if (cancellationToken.IsCancellationRequested || isShuttingDown || this == null)
                {
                    break;
                }

                GameObject chunkObject = Instantiate(chunkPrefab, Vector3.zero, Quaternion.identity);
                chunkObject.transform.SetParent(transform, true);

                ChunkRenderer chunkRenderer = chunkObject.GetComponent<ChunkRenderer>();
                await chunkRenderer.InitializeDataAsync(worldCoordinate, this);

                if (cancellationToken.IsCancellationRequested || isShuttingDown || this == null || chunkRenderer == null)
                {
                    if (chunkObject != null)
                    {
                        Destroy(chunkObject);
                    }

                    break;
                }

                chunks.TryAdd(worldCoordinate, chunkRenderer.Data);
                chunkRenderers[worldCoordinate] = chunkRenderer;

                HashSet<Vector3Int> chunksToRemesh = new HashSet<Vector3Int>();
                AddChunkAndNeighborsToRemesh(worldCoordinate, chunksToRemesh);

                foreach (Vector3Int chunkCoord in chunksToRemesh)
                {
                    if (chunkRenderers.TryGetValue(chunkCoord, out ChunkRenderer meshRenderer))
                    {
                        meshRenderer.GenerateMeshAsync();
                    }
                }
            }

            try
            {
                await Task.Delay(10, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }

    private void StopGenerationLoop()
    {
        if (generationCancellation == null)
        {
            return;
        }

        if (!generationCancellation.IsCancellationRequested)
        {
            generationCancellation.Cancel();
        }

        generationCancellation.Dispose();
        generationCancellation = null;

        lock (chunksToGenerate)
        {
            chunksToGenerate.Clear();
        }
    }

    private static void AddChunkAndNeighborsToRemesh(Vector3Int chunkCoordinate, HashSet<Vector3Int> chunksToRemesh)
    {
        chunksToRemesh.Add(chunkCoordinate);

        for (int i = 0; i < NeighborChunkOffsets.Length; i++)
        {
            chunksToRemesh.Add(chunkCoordinate + NeighborChunkOffsets[i]);
        }
    }

    public float NoiseScale => noiseScale;
    public float HeightMultiplier => heightMultiplier;
    public float TransitionNoiseScale => transitionNoiseScale;
    public float TransitionAmplitude => transitionAmplitude;
    public int BaseTerrainHeight => baseTerrainHeight;
    public float SlatePercentage => slatePercentage;
    public float StonePercentage => stonePercentage;
    public float DirtPercentage => dirtPercentage;
    public float YOffset => yOffset;
    public float XOffset => xOffset;
    public float ZOffset => zOffset;
    public float RingRadius => ringRadius;

    public byte GetBlockFromWorld(Vector3Int worldPos)
    {
        Vector3Int chunkCoordinate = new Vector3Int(
            Mathf.FloorToInt((float)worldPos.x / VoxelData.ChunkWidth),
            Mathf.FloorToInt((float)worldPos.y / VoxelData.ChunkHeight),
            Mathf.FloorToInt((float)worldPos.z / VoxelData.ChunkDepth));

        if (!chunks.TryGetValue(chunkCoordinate, out ChunkData chunkData))
        {
            return 0;
        }

        Vector3Int localPosition = new Vector3Int(
            worldPos.x - (chunkCoordinate.x * VoxelData.ChunkWidth),
            worldPos.y - (chunkCoordinate.y * VoxelData.ChunkHeight),
            worldPos.z - (chunkCoordinate.z * VoxelData.ChunkDepth));

        return chunkData.GetBlock(localPosition.x, localPosition.y, localPosition.z);
    }
}
