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

    private MeshFilter meshFilter;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();

        if (chunkData == null)
        {
            chunkData = new ChunkData();
        }

        chunkData.Populate();
        GenerateMesh();
    }

    public void GenerateMesh()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        for (int z = 0; z < VoxelData.ChunkDepth; z++)
        {
            for (int y = 0; y < VoxelData.ChunkHeight; y++)
            {
                for (int x = 0; x < VoxelData.ChunkWidth; x++)
                {
                    if (chunkData.GetBlock(x, y, z) != 1)
                    {
                        continue;
                    }

                    Vector3Int blockPosition = new Vector3Int(x, y, z);
                    GetCubeCorners(blockPosition,
                        out Vector3 v000, out Vector3 v100, out Vector3 v010, out Vector3 v110,
                        out Vector3 v001, out Vector3 v101, out Vector3 v011, out Vector3 v111);

                    for (int face = 0; face < NeighborOffsets.Length; face++)
                    {
                        Vector3Int neighbor = blockPosition + NeighborOffsets[face];

                        if (chunkData.GetBlock(neighbor.x, neighbor.y, neighbor.z) == 0)
                        {
                            AddFace((FaceDirection)face, v000, v100, v010, v110, v001, v101, v011, v111, vertices, triangles);
                        }
                    }
                }
            }
        }

        ApplyMesh(vertices, triangles);
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
        Vector3 v000,
        Vector3 v100,
        Vector3 v010,
        Vector3 v110,
        Vector3 v001,
        Vector3 v101,
        Vector3 v011,
        Vector3 v111,
        List<Vector3> vertices,
        List<int> triangles)
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

        triangles.Add(startIndex + 0);
        triangles.Add(startIndex + 1);
        triangles.Add(startIndex + 2);
        triangles.Add(startIndex + 0);
        triangles.Add(startIndex + 2);
        triangles.Add(startIndex + 3);
    }

    private void ApplyMesh(List<Vector3> vertices, List<int> triangles)
    {
        Mesh mesh = new Mesh
        {
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
        };

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.sharedMesh = mesh;
    }
}
