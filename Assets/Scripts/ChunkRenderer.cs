using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ChunkRenderer : MonoBehaviour
{
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

    public void InitializeData(Vector3Int worldPosition, WorldManager worldManager)
    {
        meshFilter = GetComponent<MeshFilter>();
        this.worldManager = worldManager;
        chunkWorldPosition = worldPosition;

        if (chunkData == null)
        {
            chunkData = new ChunkData();
        }

        chunkData.Populate(
            worldPosition,
            worldManager.BaseTerrainHeight,
            worldManager.SlatePercentage,
            worldManager.StonePercentage,
            worldManager.DirtPercentage,
            worldManager.NoiseScale,
            worldManager.HeightMultiplier);
    }

    public void GenerateMesh()
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
                    Vector3Int chunkOriginWorld = new Vector3Int(
                        Mathf.RoundToInt(transform.position.x),
                        Mathf.RoundToInt(transform.position.y),
                        Mathf.RoundToInt(transform.position.z));
                    Vector3Int blockWorldPosition = chunkOriginWorld + blockPosition;

                    GetCubeCorners(blockPosition,
                        out Vector3 v000, out Vector3 v100, out Vector3 v010, out Vector3 v110,
                        out Vector3 v001, out Vector3 v101, out Vector3 v011, out Vector3 v111);

                    for (int face = 0; face < NeighborOffsets.Length; face++)
                    {
                        Vector3Int neighborWorldPosition = blockWorldPosition + NeighborOffsets[face];

                        if (worldManager.GetBlockFromWorld(neighborWorldPosition) == 0)
                        {
                            AddFace((FaceDirection)face, blockId, v000, v100, v010, v110, v001, v101, v011, v111, vertices, triangles, uvs);
                        }
                    }
                }
            }
        }

        ApplyMesh(vertices, triangles, uvs);
    }

    private static void GetCubeCorners(
        Vector3Int blockPosition,
        out Vector3 v000,
        out Vector3 v100,
        out Vector3 v010,
        out Vector3 v110,
        out Vector3 v001,
        out Vector3 v101,
        out Vector3 v011,
        out Vector3 v111)
    {
        float x = blockPosition.x;
        float y = blockPosition.y;
        float z = blockPosition.z;

        v000 = new Vector3(x, y, z);
        v100 = new Vector3(x + 1f, y, z);
        v010 = new Vector3(x, y + 1f, z);
        v110 = new Vector3(x + 1f, y + 1f, z);
        v001 = new Vector3(x, y, z + 1f);
        v101 = new Vector3(x + 1f, y, z + 1f);
        v011 = new Vector3(x, y + 1f, z + 1f);
        v111 = new Vector3(x + 1f, y + 1f, z + 1f);
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
