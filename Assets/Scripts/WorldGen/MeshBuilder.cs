using System.Collections.Generic;
using UnityEngine;

public static class MeshBuilder {
    sealed class MeshBuffers {
        public int VertexIndex;
        public readonly List<Vector3> Vertices = new List<Vector3>(8000);
        public readonly List<int> Triangles = new List<int>(12000);
        public readonly List<Vector2> UVs = new List<Vector2>(8000);
        public readonly List<Color> Colors = new List<Color>(8000);
        public readonly List<int> WaterTriangles = new List<int>(4000);
        public readonly List<int> CutoutTriangles = new List<int>(4000);
    }

    public static void BuildMesh(ChunkData chunk, Mesh mesh) {
        MeshBuffers buffers = new MeshBuffers();
        CreateMeshData(chunk, buffers);

        mesh.Clear();

        if (buffers.Vertices.Count == 0) {
            return;
        }

        mesh.SetVertices(buffers.Vertices);
        mesh.SetUVs(0, buffers.UVs);
        mesh.SetColors(buffers.Colors);
        mesh.subMeshCount = 3;
        mesh.SetTriangles(buffers.Triangles, 0);
        mesh.SetTriangles(buffers.WaterTriangles, 1);
        mesh.SetTriangles(buffers.CutoutTriangles, 2);
        mesh.RecalculateNormals();
    }

    static void CreateMeshData(ChunkData chunk, MeshBuffers buffers) {
        for (int x = 0; x < VoxelData.ChunkWidth; x++) {
            for (int y = 0; y < VoxelData.ChunkHeight; y++) {
                for (int z = 0; z < VoxelData.ChunkWidth; z++) {
                    if (chunk.GetBlockType(x, y, z) != BlockType.Air) {
                        UpdateFaces(chunk, new Vector3Int(x, y, z), buffers);
                    }
                }
            }
        }
    }

    static void UpdateFaces(ChunkData chunk, Vector3Int localPos, MeshBuffers buffers) {
        BlockType currentBlock = chunk.GetBlockType(localPos.x, localPos.y, localPos.z);

        Vector3Int globalPos = new Vector3Int(
            localPos.x + (chunk.ChunkCoord.x * VoxelData.ChunkWidth),
            localPos.y + (chunk.ChunkCoord.y * VoxelData.ChunkHeight),
            localPos.z + (chunk.ChunkCoord.z * VoxelData.ChunkWidth)
        );

        Color blockColor = Color.white;
        int rotation = 0;

        if (currentBlock == BlockType.Grass || currentBlock == BlockType.Leaves) {
            float ambientNoise = Mathf.PerlinNoise(globalPos.x * VoxelConstants.GrassNoiseScale, globalPos.z * VoxelConstants.GrassNoiseScale);
            float colorVariation = Mathf.Lerp(VoxelConstants.GrassTintMinimum, VoxelConstants.GrassTintMaximum, ambientNoise);

            if (currentBlock == BlockType.Grass) {
                blockColor = new Color(0.35f * colorVariation, 0.7f * colorVariation, 0.3f * colorVariation, 1f);
            }
            else {
                blockColor = new Color(0.2f * colorVariation, 0.55f * colorVariation, 0.2f * colorVariation, 1f);
            }
        }
        else if (currentBlock == BlockType.Water) {
            blockColor = new Color(0.2f, 0.5f, 0.9f, VoxelConstants.WaterAlpha);
        }
        else if (currentBlock == BlockType.Dirt || currentBlock == BlockType.CoarseDirt) {
            int rotHash = (globalPos.x * 73856093) ^ (globalPos.y * 19349663) ^ (globalPos.z * 83492791);
            rotHash = (rotHash ^ (rotHash >> 16)) * 73244475;
            rotation = Mathf.Abs(rotHash) % 4;
        }

        for (int p = 0; p < 6; p++) {
            Vector3Int neighborLocalPos = localPos + VoxelData.faceChecks[p];
            BlockType neighborBlock = chunk.GetVoxelType(neighborLocalPos);

            if (currentBlock == BlockType.Water) {
                if (neighborBlock != BlockType.Air) {
                    continue;
                }
            }
            else if (currentBlock == BlockType.Leaves) {
                if (neighborBlock != BlockType.Air && neighborBlock != BlockType.Water) {
                    continue;
                }
            }
            else {
                if (neighborBlock != BlockType.Air && neighborBlock != BlockType.Water && neighborBlock != BlockType.Leaves) {
                    continue;
                }
            }

            buffers.Vertices.Add(localPos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 0]]);
            buffers.Vertices.Add(localPos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 1]]);
            buffers.Vertices.Add(localPos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 2]]);
            buffers.Vertices.Add(localPos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 3]]);

            AddTexture(GetTextureID(currentBlock, globalPos, p), rotation, buffers);

            Color faceColor = currentBlock == BlockType.Grass && p != VoxelConstants.FaceTop ? Color.white : blockColor;

            buffers.Colors.Add(faceColor);
            buffers.Colors.Add(faceColor);
            buffers.Colors.Add(faceColor);
            buffers.Colors.Add(faceColor);

            if (currentBlock == BlockType.Water) {
                buffers.WaterTriangles.Add(buffers.VertexIndex);
                buffers.WaterTriangles.Add(buffers.VertexIndex + 1);
                buffers.WaterTriangles.Add(buffers.VertexIndex + 2);
                buffers.WaterTriangles.Add(buffers.VertexIndex + 2);
                buffers.WaterTriangles.Add(buffers.VertexIndex + 1);
                buffers.WaterTriangles.Add(buffers.VertexIndex + 3);
            }
            else if (currentBlock == BlockType.Leaves) {
                buffers.CutoutTriangles.Add(buffers.VertexIndex);
                buffers.CutoutTriangles.Add(buffers.VertexIndex + 1);
                buffers.CutoutTriangles.Add(buffers.VertexIndex + 2);
                buffers.CutoutTriangles.Add(buffers.VertexIndex + 2);
                buffers.CutoutTriangles.Add(buffers.VertexIndex + 1);
                buffers.CutoutTriangles.Add(buffers.VertexIndex + 3);
            }
            else {
                buffers.Triangles.Add(buffers.VertexIndex);
                buffers.Triangles.Add(buffers.VertexIndex + 1);
                buffers.Triangles.Add(buffers.VertexIndex + 2);
                buffers.Triangles.Add(buffers.VertexIndex + 2);
                buffers.Triangles.Add(buffers.VertexIndex + 1);
                buffers.Triangles.Add(buffers.VertexIndex + 3);
            }

            buffers.VertexIndex += 4;
        }
    }

    static void AddTexture(int textureID, int rotation, MeshBuffers buffers) {
        float y = textureID / VoxelData.TextureAtlasSizeInBlocks;
        float x = textureID - (y * VoxelData.TextureAtlasSizeInBlocks);

        y = (VoxelData.TextureAtlasSizeInBlocks - 1) - y;

        x *= VoxelData.NormalizedBlockTextureSize;
        y *= VoxelData.NormalizedBlockTextureSize;

        float inset = 0.1f / 256f;
        float uvSize = VoxelData.NormalizedBlockTextureSize - inset;

        Vector2 uvBL = new Vector2(x + inset, y + inset);
        Vector2 uvTL = new Vector2(x + inset, y + uvSize);
        Vector2 uvBR = new Vector2(x + uvSize, y + inset);
        Vector2 uvTR = new Vector2(x + uvSize, y + uvSize);

        switch (rotation) {
            case 1:
                buffers.UVs.Add(uvBR); buffers.UVs.Add(uvBL); buffers.UVs.Add(uvTR); buffers.UVs.Add(uvTL);
                break;
            case 2:
                buffers.UVs.Add(uvTR); buffers.UVs.Add(uvBR); buffers.UVs.Add(uvTL); buffers.UVs.Add(uvBL);
                break;
            case 3:
                buffers.UVs.Add(uvTL); buffers.UVs.Add(uvTR); buffers.UVs.Add(uvBL); buffers.UVs.Add(uvBR);
                break;
            default:
                buffers.UVs.Add(uvBL); buffers.UVs.Add(uvTL); buffers.UVs.Add(uvBR); buffers.UVs.Add(uvTR);
                break;
        }
    }

    static int GetTextureID(BlockType type, Vector3Int globalPos, int faceIndex) {
        float variantNoise = Mathf.PerlinNoise(
            (globalPos.x + globalPos.y * 0.5f) * 0.2f,
            globalPos.z * 0.2f
        );

        int variant = variantNoise > 0.5f ? 1 : 0;

        switch (type) {
            case BlockType.Grass:
                if (faceIndex == VoxelConstants.FaceTop) return VoxelConstants.GrassTopTextureId + variant;
                if (faceIndex == VoxelConstants.FaceBottom) return VoxelConstants.GrassBottomTextureId + variant;
                return VoxelConstants.GrassSideTextureId + variant;
            case BlockType.Dirt:
                return VoxelConstants.DirtTextureId + variant;
            case BlockType.CoarseDirt:
                return VoxelConstants.CoarseDirtTextureId + variant;
            case BlockType.Gravel:
                return VoxelConstants.GravelTextureId + variant;
            case BlockType.Sand:
                return VoxelConstants.SandTextureId + variant;
            case BlockType.Stone:
                return VoxelConstants.StoneTextureId + variant;
            case BlockType.Deepslate:
                return VoxelConstants.DeepslateTextureId + variant;
            case BlockType.Water:
                return VoxelConstants.WaterTextureId;
            case BlockType.Leaves:
                return VoxelConstants.LeavesTextureId + variant;
            default:
                return 0;
        }
    }
}