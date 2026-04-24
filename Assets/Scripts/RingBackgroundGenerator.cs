using System.Collections.Generic;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class RingBackgroundGenerator : MonoBehaviour
{
    public WorldManager worldManager;
    public int segments = 60;

    private MeshFilter meshFilter;

    private IEnumerator Start()
    {
        meshFilter = GetComponent<MeshFilter>();

        // Wait one frame so WorldManager.Start can initialize X/Y/Z offsets.
        yield return null;

        GenerateBackgroundMesh();
    }

    private void GenerateBackgroundMesh()
    {
        if (worldManager == null || meshFilter == null)
        {
            return;
        }

        // Task 1: Calculate Dimensions
        float radius = worldManager.RingRadius;
        float width = worldManager.ringWidthInChunks * VoxelData.ChunkWidth;
        float circumference = 2f * Mathf.PI * radius;

        float resolvedXOffset = worldManager.XOffset;
        if (Mathf.Approximately(resolvedXOffset, 0f))
        {
            resolvedXOffset = width * 0.5f;
        }

        float centeredXLeft = 0f - resolvedXOffset;
        float centeredXRight = width - resolvedXOffset;

        EnsureMainCameraFarClip(radius);

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        // Task 2: Build the Vertices and UVs
        for (int i = 0; i <= segments; i++)
        {
            float t = (float)i / segments;
            float uZ = t;

            // Centered coordinates for distortion
            float centeredZ = t * circumference;
            float centeredY = -2f;

            // Left edge vertex
            Vector3 leftDistorted = ApplyRingDistortion(centeredXLeft, centeredY, centeredZ, radius);
            vertices.Add(leftDistorted);
            uvs.Add(new Vector2(0, uZ));

            // Right edge vertex
            Vector3 rightDistorted = ApplyRingDistortion(centeredXRight, centeredY, centeredZ, radius);
            vertices.Add(rightDistorted);
            uvs.Add(new Vector2(1, uZ));
        }

        // Task 3: Build the Triangles (inward facing)
        for (int i = 0; i < segments; i++)
        {
            int left_i = i * 2;
            int right_i = i * 2 + 1;
            int left_next = (i + 1) * 2;
            int right_next = (i + 1) * 2 + 1;

            // Inward-facing triangles (counter-clockwise from inside)
            triangles.Add(left_i);
            triangles.Add(left_next);
            triangles.Add(right_i);

            triangles.Add(right_i);
            triangles.Add(left_next);
            triangles.Add(right_next);
        }

        // Task 4: Apply the Mesh
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.mesh = mesh;
    }

    private static void EnsureMainCameraFarClip(float radius)
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return;
        }

        float minimumFarClip = radius * 2.5f;
        if (mainCamera.farClipPlane < minimumFarClip)
        {
            mainCamera.farClipPlane = minimumFarClip;
        }
    }

    private Vector3 ApplyRingDistortion(float centeredX, float centeredY, float centeredZ, float radius)
    {
        float angle = centeredZ / radius;
        float currentRadius = radius - centeredY;

        float newY = radius - (Mathf.Cos(angle) * currentRadius);
        float newZ = Mathf.Sin(angle) * currentRadius;

        return new Vector3(centeredX, newY, newZ);
    }
}
