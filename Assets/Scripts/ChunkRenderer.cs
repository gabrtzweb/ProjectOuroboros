using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ChunkRenderer : MonoBehaviour
{
    private struct MeshData
    {
        public List<Vector3> vertices;
        public List<int> triangles;
        public List<Vector2> uvs;
    }

    private enum FaceDirection
    {
        Up,
        Down,
        Left,
        Right,
        Forward,
        Back
    }

    private static readonly Vector3Int[] NeighborOffsets =
    {
        new Vector3Int(0, 1, 0),
        new Vector3Int(0, -1, 0),
        new Vector3Int(-1, 0, 0),
        new Vector3Int(1, 0, 0),
        new Vector3Int(0, 0, 1),
        new Vector3Int(0, 0, -1)
    };

    [SerializeField]
    private ChunkData chunkData;

    private WorldManager worldManager;
    private Vector3Int chunkWorldPosition;
    private MeshFilter meshFilter;

    public ChunkData Data => chunkData;

    public async Task InitializeDataAsync(Vector3Int worldPosition, WorldManager worldManager)
    {
        meshFilter = GetComponent<MeshFilter>();
        this.worldManager = worldManager;
        chunkWorldPosition = worldPosition;

        if (chunkData == null)
        {
            chunkData = new ChunkData();
        }

        if (worldManager == null)
        {
            return;
        }

        int baseTerrainHeight = worldManager.BaseTerrainHeight;
        float slatePercentage = worldManager.SlatePercentage;
        float stonePercentage = worldManager.StonePercentage;
        float dirtPercentage = worldManager.DirtPercentage;
        float noiseScale = worldManager.NoiseScale;
        float heightMultiplier = worldManager.HeightMultiplier;
        float transitionNoiseScale = worldManager.TransitionNoiseScale;
        float transitionAmplitude = worldManager.TransitionAmplitude;
        int ringWidthInChunks = worldManager.ringWidthInChunks;

        await System.Threading.Tasks.Task.Run(() =>
        {
            chunkData.Populate(
                worldPosition,
                baseTerrainHeight,
                slatePercentage,
                stonePercentage,
                dirtPercentage,
                noiseScale,
                heightMultiplier,
                transitionNoiseScale,
                transitionAmplitude,
                ringWidthInChunks);
        });
    }

    public async void GenerateMeshAsync()
    {
        if (this == null || meshFilter == null || worldManager == null)
        {
            return;
        }

        WorldManager wm = worldManager;
        float xOffset = wm.XOffset;
        float yOffset = wm.YOffset;
        float zOffset = wm.ZOffset;
        float ringRadius = wm.RingRadius;

        MeshData meshData = await System.Threading.Tasks.Task.Run(() =>
        {
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector2> uvs = new List<Vector2>();

            for (int z = 0; z < VoxelData.ChunkDepth; z++)
            {
                for (int y = 0; y < VoxelData.ChunkHeight; y++)
                {
                    for (int x = 0; x < VoxelData.ChunkWidth; x++)
                    {
                        byte blockId = chunkData.GetBlock(x, y, z);
                        if (blockId == 0)
                        {
                            continue;
                        }

                        Vector3Int blockPosition = new Vector3Int(x, y, z);
                        Vector3Int blockWorldPosition = new Vector3Int(
                            (chunkWorldPosition.x * VoxelData.ChunkWidth) + x,
                            (chunkWorldPosition.y * VoxelData.ChunkHeight) + y,
                            (chunkWorldPosition.z * VoxelData.ChunkDepth) + z);

                        float centeredX = blockWorldPosition.x - xOffset;
                        float centeredY = blockWorldPosition.y - yOffset;
                        float centeredZ = blockWorldPosition.z - zOffset;

                        Vector3 v000 = Distort(new Vector3(centeredX, centeredY, centeredZ), ringRadius);
                        Vector3 v100 = Distort(new Vector3(centeredX + 1f, centeredY, centeredZ), ringRadius);
                        Vector3 v010 = Distort(new Vector3(centeredX, centeredY + 1f, centeredZ), ringRadius);
                        Vector3 v110 = Distort(new Vector3(centeredX + 1f, centeredY + 1f, centeredZ), ringRadius);
                        Vector3 v001 = Distort(new Vector3(centeredX, centeredY, centeredZ + 1f), ringRadius);
                        Vector3 v101 = Distort(new Vector3(centeredX + 1f, centeredY, centeredZ + 1f), ringRadius);
                        Vector3 v011 = Distort(new Vector3(centeredX, centeredY + 1f, centeredZ + 1f), ringRadius);
                        Vector3 v111 = Distort(new Vector3(centeredX + 1f, centeredY + 1f, centeredZ + 1f), ringRadius);

                        for (int face = 0; face < NeighborOffsets.Length; face++)
                        {
                            Vector3Int neighborWorldPosition = blockWorldPosition + NeighborOffsets[face];
                            byte neighborBlock;

                            try
                            {
                                neighborBlock = wm.GetBlockFromWorld(neighborWorldPosition);
                            }
                            catch (MissingReferenceException)
                            {
                                return new MeshData { vertices = new List<Vector3>(), triangles = new List<int>(), uvs = new List<Vector2>() };
                            }

                            if (neighborBlock == 0)
                            {
                                AddFace((FaceDirection)face, blockId, v000, v100, v010, v110, v001, v101, v011, v111, vertices, triangles, uvs);
                            }
                        }
                    }
                }
            }

            return new MeshData { vertices = vertices, triangles = triangles, uvs = uvs };
        });

        if (this == null || meshFilter == null || worldManager == null)
        {
            return;
        }

        ApplyMesh(meshData.vertices, meshData.triangles, meshData.uvs);
    }

    private static Vector3 Distort(Vector3 globalFlatPosition, float radius)
    {
        float angle = globalFlatPosition.z / radius;
        float currentRadius = radius - globalFlatPosition.y;

        float newY = radius - (Mathf.Cos(angle) * currentRadius);
        float newZ = Mathf.Sin(angle) * currentRadius;

        return new Vector3(globalFlatPosition.x, newY, newZ);
    }

    private void GetCubeCorners(
        Vector3Int blockPosition,
        Vector3Int blockWorldPosition,
        out Vector3 v000,
        out Vector3 v100,
        out Vector3 v010,
        out Vector3 v110,
        out Vector3 v001,
        out Vector3 v101,
        out Vector3 v011,
        out Vector3 v111)
    {
        float centeredX = blockWorldPosition.x - worldManager.XOffset;
        float centeredY = blockWorldPosition.y - worldManager.YOffset;
        float centeredZ = blockWorldPosition.z - worldManager.ZOffset;

        Vector3 flatPos000 = new Vector3(centeredX, centeredY, centeredZ);
        Vector3 flatPos100 = new Vector3(centeredX + 1f, centeredY, centeredZ);
        Vector3 flatPos010 = new Vector3(centeredX, centeredY + 1f, centeredZ);
        Vector3 flatPos110 = new Vector3(centeredX + 1f, centeredY + 1f, centeredZ);
        Vector3 flatPos001 = new Vector3(centeredX, centeredY, centeredZ + 1f);
        Vector3 flatPos101 = new Vector3(centeredX + 1f, centeredY, centeredZ + 1f);
        Vector3 flatPos011 = new Vector3(centeredX, centeredY + 1f, centeredZ + 1f);
        Vector3 flatPos111 = new Vector3(centeredX + 1f, centeredY + 1f, centeredZ + 1f);

        v000 = ApplyRingDistortion(flatPos000);
        v100 = ApplyRingDistortion(flatPos100);
        v010 = ApplyRingDistortion(flatPos010);
        v110 = ApplyRingDistortion(flatPos110);
        v001 = ApplyRingDistortion(flatPos001);
        v101 = ApplyRingDistortion(flatPos101);
        v011 = ApplyRingDistortion(flatPos011);
        v111 = ApplyRingDistortion(flatPos111);
    }

    private Vector3 ApplyRingDistortion(Vector3 globalFlatPosition)
    {
        float radius = worldManager.RingRadius;
        float angle = globalFlatPosition.z / radius;
        float currentRadius = radius - globalFlatPosition.y;

        float newY = radius - (Mathf.Cos(angle) * currentRadius);
        float newZ = Mathf.Sin(angle) * currentRadius;

        return new Vector3(globalFlatPosition.x, newY, newZ);
    }

    private static void AddFace(
        FaceDirection face,
        int blockId,
        Vector3 v000,
        Vector3 v100,
        Vector3 v010,
        Vector3 v110,
        Vector3 v001,
        Vector3 v101,
        Vector3 v011,
        Vector3 v111,
        List<Vector3> vertices,
        List<int> triangles,
        List<Vector2> uvs)
    {
        int startIndex = vertices.Count;

        switch (face)
        {
            case FaceDirection.Up:
                vertices.Add(v011);
                vertices.Add(v111);
                vertices.Add(v110);
                vertices.Add(v010);
                break;

            case FaceDirection.Down:
                vertices.Add(v000);
                vertices.Add(v100);
                vertices.Add(v101);
                vertices.Add(v001);
                break;

            case FaceDirection.Left:
                vertices.Add(v000);
                vertices.Add(v001);
                vertices.Add(v011);
                vertices.Add(v010);
                break;

            case FaceDirection.Right:
                vertices.Add(v101);
                vertices.Add(v100);
                vertices.Add(v110);
                vertices.Add(v111);
                break;

            case FaceDirection.Forward:
                vertices.Add(v001);
                vertices.Add(v101);
                vertices.Add(v111);
                vertices.Add(v011);
                break;

            case FaceDirection.Back:
                vertices.Add(v100);
                vertices.Add(v000);
                vertices.Add(v010);
                vertices.Add(v110);
                break;
        }

        Vector2[] faceUVs = GetUVCoords(blockId, face);
        uvs.Add(faceUVs[0]);
        uvs.Add(faceUVs[1]);
        uvs.Add(faceUVs[2]);
        uvs.Add(faceUVs[3]);

        triangles.Add(startIndex + 0);
        triangles.Add(startIndex + 1);
        triangles.Add(startIndex + 2);
        triangles.Add(startIndex + 0);
        triangles.Add(startIndex + 2);
        triangles.Add(startIndex + 3);
    }

    private static Vector2[] GetUVCoords(int blockID, FaceDirection direction)
    {
        const float tileSize = 0.25f;

        float uMin;
        float uMax;
        float vMin;
        float vMax;

        switch (blockID)
        {
            case 1:
                if (direction == FaceDirection.Up)
                {
                    uMin = 0.00f;
                    uMax = 0.25f;
                    vMin = 0.75f;
                    vMax = 1.00f;
                }
                else if (direction == FaceDirection.Down)
                {
                    uMin = 0.50f;
                    uMax = 0.75f;
                    vMin = 0.75f;
                    vMax = 1.00f;
                }
                else
                {
                    uMin = 0.25f;
                    uMax = 0.50f;
                    vMin = 0.75f;
                    vMax = 1.00f;
                }
                break;

            case 2:
                uMin = 0.50f;
                uMax = 0.75f;
                vMin = 0.75f;
                vMax = 1.00f;
                break;

            case 3:
                uMin = 0.75f;
                uMax = 1.00f;
                vMin = 0.75f;
                vMax = 1.00f;
                break;

            case 4:
                uMin = 0.00f;
                uMax = 0.25f;
                vMin = 0.50f;
                vMax = 0.75f;
                break;

            default:
                uMin = 0.00f;
                uMax = tileSize;
                vMin = 0.00f;
                vMax = tileSize;
                break;
        }

        return new[]
        {
            new Vector2(uMin, vMin),
            new Vector2(uMax, vMin),
            new Vector2(uMax, vMax),
            new Vector2(uMin, vMax)
        };
    }

    private void ApplyMesh(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
    {
        Mesh mesh = new Mesh
        {
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
        };

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetUVs(0, uvs);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.sharedMesh = mesh;
    }
}
