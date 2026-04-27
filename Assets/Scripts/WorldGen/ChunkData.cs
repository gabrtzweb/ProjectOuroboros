using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class ChunkData : MonoBehaviour {
    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    MeshCollider meshCollider; 
    
    int vertexIndex = 0;
    List<Vector3> vertices = new List<Vector3>(8000);
    List<int> triangles = new List<int>(12000);
    List<Vector2> uvs = new List<Vector2>(8000); 
    List<Color> colors = new List<Color>(8000);
    List<int> waterTriangles = new List<int>(4000);
    List<int> cutoutTriangles = new List<int>(4000);
    
    BlockType[] voxelMap = new BlockType[VoxelData.ChunkVolume];

    WorldManager worldManager;
    Vector3Int chunkCoord;

    public void InitData(WorldManager world, Vector3Int coord) {
        worldManager = world;
        chunkCoord = coord;
        
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>(); 
        
        PopulateVoxelMap();
    }

    // Fill the voxel array using height, noise, and boundary rules.
    void PopulateVoxelMap() {
        for (int x = 0; x < VoxelData.ChunkWidth; x++) {
            for (int z = 0; z < VoxelData.ChunkWidth; z++) {
                
                int globalX = x + (chunkCoord.x * VoxelData.ChunkWidth);
                int globalZ = z + (chunkCoord.z * VoxelData.ChunkWidth);
                
                int surfaceHeight = worldManager.CalculateSurfaceHeight(globalX, globalZ);

                float layerWave = Mathf.PerlinNoise((globalX + worldManager.noiseOffset) * 0.03f, (globalZ + worldManager.noiseOffset) * 0.03f) * 3f;
                int baseDirtBoundary = surfaceHeight - 4 - Mathf.FloorToInt(layerWave * 0.5f);
                int baseCoarseBoundary = surfaceHeight - 9 - Mathf.FloorToInt(layerWave);
                int baseGravelBoundary = surfaceHeight - 11 - Mathf.FloorToInt(layerWave); 

                float deepslateWave = Mathf.PerlinNoise((globalX + worldManager.noiseOffset) * 0.05f, (globalZ + worldManager.noiseOffset) * 0.05f) * 4f;
                int baseDeepslateBoundary = worldManager.deepslateTransitionLevel + Mathf.FloorToInt(deepslateWave);

                for (int y = 0; y < VoxelData.ChunkHeight; y++) {
                    int index = VoxelData.Get1DIndex(x, y, z);
                    int globalY = y + (chunkCoord.y * VoxelData.ChunkHeight);
                    
                    int hash = (globalX * 37476139) ^ (globalY * 66826521) ^ (globalZ * 25497383);
                    hash = (hash ^ (hash >> 13)) * 12741261;
                    
                    int dither = (Mathf.Abs(hash) % 5) - 2; 

                    int dirtBoundary = baseDirtBoundary + dither;
                    int coarseBoundary = baseCoarseBoundary + dither;
                    int gravelBoundary = baseGravelBoundary + dither;
                    int deepslateDitheredBoundary = baseDeepslateBoundary + dither;

                    if (globalY > surfaceHeight) {
                        if (globalY <= worldManager.seaLevel) {
                            voxelMap[index] = BlockType.Water;
                        } else {
                            voxelMap[index] = BlockType.Air;
                        }
                    } 
                    else if (globalY == surfaceHeight) {
                        if (globalY == worldManager.seaLevel + 1) {
                            voxelMap[index] = BlockType.Sand;
                        } 
                        else if (globalY <= worldManager.seaLevel) {
                            float mixNoise = Mathf.PerlinNoise((globalX + worldManager.noiseOffset) * 0.1f, (globalZ + worldManager.noiseOffset) * 0.1f);
                            
                            if (globalY >= worldManager.seaLevel - 2) {
                                if (mixNoise > 0.6f) voxelMap[index] = BlockType.Gravel;
                                else if (mixNoise > 0.3f) voxelMap[index] = BlockType.Sand;
                                else voxelMap[index] = BlockType.Dirt;
                            } else {
                                voxelMap[index] = mixNoise > 0.4f ? BlockType.Gravel : BlockType.Dirt;
                            }
                        } 
                        else {
                            voxelMap[index] = BlockType.Grass; 
                        }
                    } 
                    else if (globalY >= dirtBoundary) {
                        voxelMap[index] = BlockType.Dirt; 
                    } 
                    else if (globalY >= coarseBoundary) {
                        voxelMap[index] = BlockType.CoarseDirt; 
                    } 
                    else if (globalY >= gravelBoundary) {
                        voxelMap[index] = BlockType.Gravel; 
                    } 
                    else if (globalY <= deepslateDitheredBoundary) {
                        voxelMap[index] = BlockType.Deepslate; 
                    } 
                    else {
                        voxelMap[index] = BlockType.Stone; 
                    }
                }
            }
        }
    }

    // Convert visible voxels into the mesh and route each material to its submesh.
    public void GenerateMesh() {
        CreateMeshData();
        CreateMesh();
    }

    void CreateMeshData() {
        for (int x = 0; x < VoxelData.ChunkWidth; x++) {
            for (int y = 0; y < VoxelData.ChunkHeight; y++) {
                for (int z = 0; z < VoxelData.ChunkWidth; z++) {
                    
                    int index = VoxelData.Get1DIndex(x, y, z);
                    
                    if (voxelMap[index] != BlockType.Air) {
                        UpdateFaces(new Vector3Int(x, y, z));
                    }
                }
            }
        }
    }

    void UpdateFaces(Vector3Int localPos) {
        int index = VoxelData.Get1DIndex(localPos.x, localPos.y, localPos.z);
        BlockType currentBlock = voxelMap[index];

        for (int p = 0; p < 6; p++) {
            Vector3Int neighborLocalPos = localPos + VoxelData.faceChecks[p];
            BlockType neighborBlock = GetVoxelType(neighborLocalPos);

            if (currentBlock == BlockType.Water) {
                if (neighborBlock != BlockType.Air) continue; 
            } 
            else if (currentBlock == BlockType.Leaves) {
                if (neighborBlock != BlockType.Air && neighborBlock != BlockType.Water) continue;
            }
            else {
                if (neighborBlock != BlockType.Air && neighborBlock != BlockType.Water && neighborBlock != BlockType.Leaves) continue;
            }

            vertices.Add(localPos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 0]]);
            vertices.Add(localPos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 1]]);
            vertices.Add(localPos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 2]]);
            vertices.Add(localPos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 3]]);

            Vector3Int globalPos = new Vector3Int(
                localPos.x + (chunkCoord.x * VoxelData.ChunkWidth),
                localPos.y + (chunkCoord.y * VoxelData.ChunkHeight),
                localPos.z + (chunkCoord.z * VoxelData.ChunkWidth)
            );

            int rotation = 0;
            
            if (currentBlock == BlockType.Dirt || currentBlock == BlockType.CoarseDirt) {
                int rotHash = (globalPos.x * 73856093) ^ (globalPos.y * 19349663) ^ (globalPos.z * 83492791);
                rotHash = (rotHash ^ (rotHash >> 16)) * 73244475; 
                rotation = Mathf.Abs(rotHash) % 4; 
            }

            AddTexture(GetTextureID(currentBlock, globalPos, p), rotation);

            Color faceColor = Color.white;
            
            if (currentBlock == BlockType.Grass && p == 2) {
                float noiseScale = 0.03f; 
                float ambientNoise = Mathf.PerlinNoise(globalPos.x * noiseScale, globalPos.z * noiseScale);
                float colorVariation = Mathf.Lerp(0.85f, 1.15f, ambientNoise);
                faceColor = new Color(0.35f * colorVariation, 0.7f * colorVariation, 0.3f * colorVariation, 1f); 
            }
            else if (currentBlock == BlockType.Water) {
                faceColor = new Color(0.2f, 0.5f, 0.9f, 0.75f);
            }
            else if (currentBlock == BlockType.Leaves) {
                float noiseScale = 0.03f; 
                float ambientNoise = Mathf.PerlinNoise(globalPos.x * noiseScale, globalPos.z * noiseScale);
                float colorVariation = Mathf.Lerp(0.85f, 1.15f, ambientNoise);
                faceColor = new Color(0.2f * colorVariation, 0.55f * colorVariation, 0.2f * colorVariation, 1f);
            }

            colors.Add(faceColor);
            colors.Add(faceColor);
            colors.Add(faceColor);
            colors.Add(faceColor);

            if (currentBlock == BlockType.Water) {
                waterTriangles.Add(vertexIndex);
                waterTriangles.Add(vertexIndex + 1);
                waterTriangles.Add(vertexIndex + 2);
                waterTriangles.Add(vertexIndex + 2);
                waterTriangles.Add(vertexIndex + 1);
                waterTriangles.Add(vertexIndex + 3);
            } 
            else if (currentBlock == BlockType.Leaves) {
                cutoutTriangles.Add(vertexIndex);
                cutoutTriangles.Add(vertexIndex + 1);
                cutoutTriangles.Add(vertexIndex + 2);
                cutoutTriangles.Add(vertexIndex + 2);
                cutoutTriangles.Add(vertexIndex + 1);
                cutoutTriangles.Add(vertexIndex + 3);
            } 
            else {
                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 3);
            }

            vertexIndex += 4;
        }
    }

    void AddTexture(int textureID, int rotation) {
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
                uvs.Add(uvBR); uvs.Add(uvBL); uvs.Add(uvTR); uvs.Add(uvTL); 
                break;
            case 2: 
                uvs.Add(uvTR); uvs.Add(uvBR); uvs.Add(uvTL); uvs.Add(uvBL); 
                break;
            case 3: 
                uvs.Add(uvTL); uvs.Add(uvTR); uvs.Add(uvBL); uvs.Add(uvBR); 
                break;
            default: 
                uvs.Add(uvBL); uvs.Add(uvTL); uvs.Add(uvBR); uvs.Add(uvTR); 
                break;
        }
    }

    int GetTextureID(BlockType type, Vector3Int globalPos, int faceIndex) {
        float noiseScale = 0.2f;
        float variantNoise = Mathf.PerlinNoise(
            (globalPos.x + globalPos.y * 0.5f) * noiseScale, 
            globalPos.z * noiseScale
        );
        
        int variant = variantNoise > 0.5f ? 1 : 0; 
        
        switch (type) {
            case BlockType.Grass:
                if (faceIndex == 2) return 10 + variant; 
                if (faceIndex == 3) return 4 + variant;  
                return 6 + variant;
            case BlockType.Dirt:
                return 4 + variant;
            case BlockType.CoarseDirt:
                return 0 + variant;
            case BlockType.Gravel:
                return 12 + variant; 
            case BlockType.Sand:
                return 22 + variant;
            case BlockType.Stone:
                return 24 + variant;
            case BlockType.Deepslate:
                return 2 + variant;
            case BlockType.Water:
                return 26; 
            case BlockType.Leaves:
                return 14 + variant;
            default:
                return 0;
        }
    }

    BlockType GetVoxelType(Vector3Int localPos) {
        if (localPos.x < 0 || localPos.x >= VoxelData.ChunkWidth || 
            localPos.y < 0 || localPos.y >= VoxelData.ChunkHeight || 
            localPos.z < 0 || localPos.z >= VoxelData.ChunkWidth) {
            
            Vector3Int globalPos = new Vector3Int(
                localPos.x + (chunkCoord.x * VoxelData.ChunkWidth),
                localPos.y + (chunkCoord.y * VoxelData.ChunkHeight),
                localPos.z + (chunkCoord.z * VoxelData.ChunkWidth)
            );
            
            return worldManager.GetBlockFromGlobal(globalPos);
        }
        
        int index = VoxelData.Get1DIndex(localPos.x, localPos.y, localPos.z);
        return voxelMap[index];
    }

    public BlockType GetBlockType(int x, int y, int z) {
        int index = VoxelData.Get1DIndex(x, y, z);
        return voxelMap[index];
    }

    void CreateMesh() {
        if (vertices.Count == 0) return;

        Mesh mesh = new Mesh {
            vertices = vertices.ToArray(),
            uv = uvs.ToArray(),
            colors = colors.ToArray() 
        };

        mesh.subMeshCount = 3;
        mesh.SetTriangles(triangles.ToArray(), 0);
        mesh.SetTriangles(waterTriangles.ToArray(), 1);
        mesh.SetTriangles(cutoutTriangles.ToArray(), 2);

        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh; 
        
        vertices.Clear();
        triangles.Clear();
        waterTriangles.Clear();
        cutoutTriangles.Clear();
        uvs.Clear(); 
        colors.Clear(); 
        vertexIndex = 0;
    }
}
