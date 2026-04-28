public static class VoxelConstants {
    #region Terrain Layer Boundaries
    
    public const int TerrainDirtBoundaryOffset = 4;
    public const int TerrainCoarseBoundaryOffset = 9;
    public const int TerrainGravelBoundaryOffset = 11;
    public const float TerrainLayerWaveScale = 0.03f;
    public const float TerrainLayerWaveAmplitude = 3f;
    public const float TerrainDeepslateWaveScale = 0.05f;
    public const float TerrainDeepslateWaveAmplitude = 4f;
    public const int TerrainDitherModulus = 5;
    public const int TerrainDitherOffset = 2;
    
    #endregion
    
    #region Water and Surface Blending
    
    public const float TerrainUnderwaterMixNoiseScale = 0.1f;
    public const float TerrainShallowWaterMixThreshold = 0.6f;
    public const float TerrainMidWaterMixThreshold = 0.3f;
    public const float TerrainDeepWaterMixThreshold = 0.4f;
    public const int TerrainShallowWaterDepth = 2;
    public const int TerrainSandBeachOffset = 1;
    
    #endregion
    
    #region Face Indices
    
    public const int FaceTop = 2;
    public const int FaceBottom = 3;
    
    #endregion
    
    #region Grass and Water Tinting
    
    public const float GrassNoiseScale = 0.03f;
    public const float GrassTintMinimum = 0.85f;
    public const float GrassTintMaximum = 1.15f;
    public const float WaterAlpha = 0.75f;
    
    #endregion
    
    #region Texture Indices
    
    public const int GrassTopTextureId = 10;
    public const int GrassBottomTextureId = 4;
    public const int GrassSideTextureId = 6;
    public const int DirtTextureId = 4;
    public const int CoarseDirtTextureId = 0;
    public const int GravelTextureId = 12;
    public const int SandTextureId = 22;
    public const int StoneTextureId = 24;
    public const int DeepslateTextureId = 2;
    public const int WaterTextureId = 26;
    public const int LeavesTextureId = 14;
    public const int OakPlanksTextureId = 20;
    public const int BedrockTextureId = 28;
    
    #endregion
    
    #region World Boundaries
    
    public const int WorldBottomLevel = -64;
    public const int TextureVariantCount = 2;
    
    #endregion
}