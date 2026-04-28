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
        public readonly List<int> CollisionTriangles = new List<int>(12000); 
    }

    public static void BuildMesh(ChunkData chunk, Mesh mesh) {
        MeshBuffers buffers = new MeshBuffers();
        CreateMeshData(chunk, buffers);

        mesh.Clear();

        if (buffers.Vertices.Count == 0) return;

        mesh.SetVertices(buffers.Vertices);
        mesh.SetUVs(0, buffers.UVs);
        mesh.SetColors(buffers.Colors);
        mesh.subMeshCount = 3;
        mesh.SetTriangles(buffers.Triangles, 0);
        mesh.SetTriangles(buffers.WaterTriangles, 1);
        mesh.SetTriangles(buffers.CutoutTriangles, 2);
        mesh.RecalculateNormals();

        if (buffers.CollisionTriangles.Count > 0) {
            Mesh collisionMesh = new Mesh();
            collisionMesh.SetVertices(buffers.Vertices);
            collisionMesh.SetTriangles(buffers.CollisionTriangles, 0);
            chunk.UpdateCollider(collisionMesh);
        } else {
            chunk.UpdateCollider(null);
        }
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

        if (currentBlock == BlockType.ShortGrass || currentBlock == BlockType.ShortBush || currentBlock == BlockType.ShortDryGrass) {
            BuildCrossMesh(currentBlock, localPos, buffers, globalPos);
            return;
        }

        Color blockColor = Color.white;
        int rotation = 0;

        if (currentBlock == BlockType.Grass || currentBlock == BlockType.Leaves) {
            float ambientNoise = Mathf.PerlinNoise(globalPos.x * VoxelConstants.GrassNoiseScale, globalPos.z * VoxelConstants.GrassNoiseScale);
            float colorVariation = Mathf.Lerp(VoxelConstants.GrassTintMinimum, VoxelConstants.GrassTintMaximum, ambientNoise);

            if (currentBlock == BlockType.Grass) {
                blockColor = new Color(0.35f * colorVariation, 0.7f * colorVariation, 0.3f * colorVariation, 1f);
            } else {
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

        bool isWaterAbove = chunk.GetVoxelType(localPos + new Vector3Int(0, 1, 0)) == BlockType.Water;

        for (int p = 0; p < 6; p++) {
            Vector3Int neighborLocalPos = localPos + VoxelData.faceChecks[p];
            BlockType neighborBlock = chunk.GetVoxelType(neighborLocalPos);

            if (currentBlock == BlockType.Water) {
                if (neighborBlock != BlockType.Air) continue;
            }
            else if (currentBlock == BlockType.Leaves) {
                if (neighborBlock != BlockType.Air && neighborBlock != BlockType.Water) continue;
            }
            else {
                if (neighborBlock != BlockType.Air && 
                    neighborBlock != BlockType.Water && 
                    neighborBlock != BlockType.Leaves && 
                    neighborBlock != BlockType.ShortGrass && 
                    neighborBlock != BlockType.ShortBush &&
                    neighborBlock != BlockType.ShortDryGrass) {
                    continue;
                }
            }

            // Draw the 4 vertices for the current face
            for (int i = 0; i < 4; i++) {
                Vector3 vertex = VoxelData.voxelVerts[VoxelData.voxelTris[p, i]];
                
                // Lower the top vertices of water blocks if there is no water above them
                if (currentBlock == BlockType.Water && !isWaterAbove && vertex.y > 0.5f) {
                    vertex.y -= 0.2f;
                }
                
                buffers.Vertices.Add(localPos + vertex);
            }

            AddTexture(GetTextureID(currentBlock, globalPos, p), rotation, buffers);

            Color faceColor = currentBlock == BlockType.Grass && p != VoxelConstants.FaceTop ? Color.white : blockColor;

            buffers.Colors.Add(faceColor); buffers.Colors.Add(faceColor);
            buffers.Colors.Add(faceColor); buffers.Colors.Add(faceColor);

            if (currentBlock == BlockType.Water) {
                buffers.WaterTriangles.AddRange(new int[] { buffers.VertexIndex, buffers.VertexIndex + 1, buffers.VertexIndex + 2, buffers.VertexIndex + 2, buffers.VertexIndex + 1, buffers.VertexIndex + 3 });
            }
            else if (currentBlock == BlockType.Leaves) {
                buffers.CutoutTriangles.AddRange(new int[] { buffers.VertexIndex, buffers.VertexIndex + 1, buffers.VertexIndex + 2, buffers.VertexIndex + 2, buffers.VertexIndex + 1, buffers.VertexIndex + 3 });
            }
            else {
                buffers.Triangles.AddRange(new int[] { buffers.VertexIndex, buffers.VertexIndex + 1, buffers.VertexIndex + 2, buffers.VertexIndex + 2, buffers.VertexIndex + 1, buffers.VertexIndex + 3 });
            }

            if (currentBlock != BlockType.Water && currentBlock != BlockType.Leaves) {
                buffers.CollisionTriangles.AddRange(new int[] { buffers.VertexIndex, buffers.VertexIndex + 1, buffers.VertexIndex + 2, buffers.VertexIndex + 2, buffers.VertexIndex + 1, buffers.VertexIndex + 3 });
            }

            buffers.VertexIndex += 4;
        }
    }

    static void BuildCrossMesh(BlockType type, Vector3Int localPos, MeshBuffers buffers, Vector3Int globalPos) {
        int plantHash = (globalPos.x * 37476139) ^ (globalPos.y * 66826521) ^ (globalPos.z * 25497383);
        plantHash = (plantHash ^ (plantHash >> 13)) * 12741261;

        int variant = (Mathf.Abs(plantHash) % 100) > 50 ? 1 : 0;
        
        int textureID;
        if (type == BlockType.ShortGrass) textureID = VoxelConstants.ShortGrassTextureId + variant;
        else if (type == BlockType.ShortBush) textureID = VoxelConstants.ShortBushTextureId + variant;
        else textureID = VoxelConstants.ShortDryGrassTextureId + variant;

        Color blockColor = Color.white;
        
        if (type == BlockType.ShortGrass || type == BlockType.ShortBush) {
            float ambientNoise = Mathf.PerlinNoise(globalPos.x * VoxelConstants.GrassNoiseScale, globalPos.z * VoxelConstants.GrassNoiseScale);
            float colorVariation = Mathf.Lerp(VoxelConstants.GrassTintMinimum, VoxelConstants.GrassTintMaximum, ambientNoise);
            blockColor = new Color(0.35f * colorVariation, 0.7f * colorVariation, 0.3f * colorVariation, 1f);
        }

        float offsetX = ((Mathf.Abs(plantHash) % 100) / 100f) * 0.6f - 0.3f;
        float offsetZ = (((Mathf.Abs(plantHash) / 100) % 100) / 100f) * 0.6f - 0.3f;
        float scaleY = 0.7f + (((Mathf.Abs(plantHash) / 10000) % 100) / 100f) * 0.5f;

        Vector3 jitter = new Vector3(offsetX, 0, offsetZ);

        for (int i = 0; i < 2; i++) {
            int vFront = buffers.VertexIndex;
            if (i == 0) {
                buffers.Vertices.Add(localPos + jitter + new Vector3(0, 0, 0));
                buffers.Vertices.Add(localPos + jitter + new Vector3(0, scaleY, 0));
                buffers.Vertices.Add(localPos + jitter + new Vector3(1, 0, 1));
                buffers.Vertices.Add(localPos + jitter + new Vector3(1, scaleY, 1));
            } else {
                buffers.Vertices.Add(localPos + jitter + new Vector3(0, 0, 1));
                buffers.Vertices.Add(localPos + jitter + new Vector3(0, scaleY, 1));
                buffers.Vertices.Add(localPos + jitter + new Vector3(1, 0, 0));
                buffers.Vertices.Add(localPos + jitter + new Vector3(1, scaleY, 0));
            }

            AddTexture(textureID, 0, buffers);
            buffers.Colors.Add(blockColor); buffers.Colors.Add(blockColor);
            buffers.Colors.Add(blockColor); buffers.Colors.Add(blockColor);

            buffers.CutoutTriangles.AddRange(new int[] { vFront, vFront + 1, vFront + 2, vFront + 2, vFront + 1, vFront + 3 });
            buffers.VertexIndex += 4;

            int vBack = buffers.VertexIndex;
            if (i == 0) {
                buffers.Vertices.Add(localPos + jitter + new Vector3(0, 0, 0));
                buffers.Vertices.Add(localPos + jitter + new Vector3(0, scaleY, 0));
                buffers.Vertices.Add(localPos + jitter + new Vector3(1, 0, 1));
                buffers.Vertices.Add(localPos + jitter + new Vector3(1, scaleY, 1));
            } else {
                buffers.Vertices.Add(localPos + jitter + new Vector3(0, 0, 1));
                buffers.Vertices.Add(localPos + jitter + new Vector3(0, scaleY, 1));
                buffers.Vertices.Add(localPos + jitter + new Vector3(1, 0, 0));
                buffers.Vertices.Add(localPos + jitter + new Vector3(1, scaleY, 0));
            }

            AddTexture(textureID, 0, buffers);
            buffers.Colors.Add(blockColor); buffers.Colors.Add(blockColor);
            buffers.Colors.Add(blockColor); buffers.Colors.Add(blockColor);

            buffers.CutoutTriangles.AddRange(new int[] { vBack + 2, vBack + 1, vBack, vBack + 3, vBack + 1, vBack + 2 });
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
            case 1: buffers.UVs.Add(uvBR); buffers.UVs.Add(uvBL); buffers.UVs.Add(uvTR); buffers.UVs.Add(uvTL); break;
            case 2: buffers.UVs.Add(uvTR); buffers.UVs.Add(uvBR); buffers.UVs.Add(uvTL); buffers.UVs.Add(uvBL); break;
            case 3: buffers.UVs.Add(uvTL); buffers.UVs.Add(uvTR); buffers.UVs.Add(uvBL); buffers.UVs.Add(uvBR); break;
            default: buffers.UVs.Add(uvBL); buffers.UVs.Add(uvTL); buffers.UVs.Add(uvBR); buffers.UVs.Add(uvTR); break;
        }
    }

    static int GetTextureID(BlockType type, Vector3Int globalPos, int faceIndex) {
        float variantNoise = Mathf.PerlinNoise((globalPos.x + globalPos.y * 0.5f) * 0.2f, globalPos.z * 0.2f);
        int variant = variantNoise > 0.5f ? 1 : 0;

        switch (type) {
            case BlockType.Grass:
                if (faceIndex == VoxelConstants.FaceTop) return VoxelConstants.GrassTopTextureId + variant;
                if (faceIndex == VoxelConstants.FaceBottom) return VoxelConstants.GrassBottomTextureId + variant;
                return VoxelConstants.GrassSideTextureId + variant;
            case BlockType.Dirt: return VoxelConstants.DirtTextureId + variant;
            case BlockType.CoarseDirt: return VoxelConstants.CoarseDirtTextureId + variant;
            case BlockType.Gravel: return VoxelConstants.GravelTextureId + variant;
            case BlockType.Sand: return VoxelConstants.SandTextureId + variant;
            case BlockType.Stone: return VoxelConstants.StoneTextureId + variant;
            case BlockType.Deepslate: return VoxelConstants.DeepslateTextureId + variant;
            case BlockType.Water: return VoxelConstants.WaterTextureId;
            case BlockType.Leaves: return VoxelConstants.LeavesTextureId + variant;
            case BlockType.OakPlanks: return VoxelConstants.OakPlanksTextureId + variant;
            case BlockType.Bedrock: return VoxelConstants.BedrockTextureId + variant;
            default: return 0;
        }
    }
}
