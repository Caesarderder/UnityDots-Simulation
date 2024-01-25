#if GRIFFIN
#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.Rendering;

namespace Pinwheel.Griffin
{
    public static class GLivePreviewDrawer
    {
        private static MaterialPropertyBlock previewPropertyBlock = new MaterialPropertyBlock();

        public static void DrawGeometryLivePreview(GStylizedTerrain t, Camera cam, Texture newHeightMap, Rect dirtyRect)
        {
            if (t.transform.rotation != Quaternion.identity ||
                t.transform.lossyScale != Vector3.one)
                return;

            Mesh previewMesh = GEditorSettings.Instance.livePreview.GetTriangleMesh(t.TerrainData.Geometry.MeshResolution);

            Vector3 terrainSize = new Vector3(
                t.TerrainData.Geometry.Width,
                t.TerrainData.Geometry.Height,
                t.TerrainData.Geometry.Length);
            previewPropertyBlock.Clear();
            previewPropertyBlock.SetTexture("_OldHeightMap", t.TerrainData.Geometry.HeightMap);
            previewPropertyBlock.SetTexture("_NewHeightMap", newHeightMap);
            previewPropertyBlock.SetTexture("_MainTex", newHeightMap);
            previewPropertyBlock.SetFloat("_Height", t.TerrainData.Geometry.Height);
            previewPropertyBlock.SetVector("_BoundMin", t.transform.position);
            previewPropertyBlock.SetVector("_BoundMax", t.transform.TransformPoint(terrainSize));
            Material mat = GInternalMaterials.GeometryLivePreviewMaterial;
            mat.renderQueue = 4000;

            int gridSize = t.TerrainData.Geometry.ChunkGridSize;
            for (int x = 0; x < gridSize; ++x)
            {
                for (int z = 0; z < gridSize; ++z)
                {
                    Rect uvRect = GCommon.GetUvRange(gridSize, x, z);
                    if (!uvRect.Overlaps(dirtyRect))
                        continue;
                    Vector3 localPos = new Vector3(
                        terrainSize.x * uvRect.x,
                        0f,
                        terrainSize.z * uvRect.y);
                    Vector3 worldPos = t.transform.TransformPoint(localPos);
                    Quaternion rotation = Quaternion.identity;
                    Vector3 scale = new Vector3(terrainSize.x * uvRect.width, 1, terrainSize.z * uvRect.height);

                    Graphics.DrawMesh(
                        previewMesh,
                        Matrix4x4.TRS(worldPos, rotation, scale),
                        mat,
                        LayerMask.NameToLayer("Default"),
                        cam,
                        0,
                        previewPropertyBlock);
                }
            }
        }

        public static void DrawGeometryLivePreview(GStylizedTerrain t, Camera cam, Texture newHeightMap, bool[] chunkCulling)
        {
            if (t.transform.rotation != Quaternion.identity ||
                t.transform.lossyScale != Vector3.one)
                return;

            Mesh previewMesh = GEditorSettings.Instance.livePreview.GetTriangleMesh(t.TerrainData.Geometry.MeshResolution);

            Vector3 terrainSize = new Vector3(
                t.TerrainData.Geometry.Width,
                t.TerrainData.Geometry.Height,
                t.TerrainData.Geometry.Length);
            previewPropertyBlock.Clear();
            previewPropertyBlock.SetTexture("_OldHeightMap", t.TerrainData.Geometry.HeightMap);
            previewPropertyBlock.SetTexture("_NewHeightMap", newHeightMap);
            previewPropertyBlock.SetTexture("_MainTex", newHeightMap);
            previewPropertyBlock.SetFloat("_Height", t.TerrainData.Geometry.Height);
            previewPropertyBlock.SetVector("_BoundMin", t.transform.position);
            previewPropertyBlock.SetVector("_BoundMax", t.transform.TransformPoint(terrainSize));
            Material mat = GInternalMaterials.GeometryLivePreviewMaterial;
            mat.renderQueue = 4000;

            Rect[] chunkRects = t.GetChunkRects();
            for (int i = 0; i < chunkRects.Length; ++i)
            {
                if (chunkCulling[i] == false)
                    continue;
                Rect r = chunkRects[i];
                Vector3 position = new Vector3(r.x, t.transform.position.y, r.y);
                Quaternion rotation = Quaternion.identity;
                Vector3 scale = new Vector3(r.width, 1, r.height);
                Matrix4x4 trs = Matrix4x4.TRS(position, rotation, scale);

                Graphics.DrawMesh(
                    previewMesh,
                    trs,
                    mat,
                    LayerMask.NameToLayer("Default"),
                    cam,
                    0,
                    previewPropertyBlock,
                    ShadowCastingMode.Off,
                    false,
                    null,
                    LightProbeUsage.Off,
                    null);
            }
        }

        public static void DrawSubdivLivePreview(GStylizedTerrain t, Camera cam, Texture newHeightMap, Rect dirtyRect, Texture mask, Matrix4x4 worldPointToMaskMatrix)
        {
            if (t.transform.rotation != Quaternion.identity ||
                t.transform.lossyScale != Vector3.one)
                return;

            t.TerrainData.Geometry.Internal_CreateNewSubDivisionMap(newHeightMap);

            int baseResolution = t.TerrainData.Geometry.MeshBaseResolution;
            int resolution = t.TerrainData.Geometry.MeshResolution;
            int maxLevel = baseResolution + Mathf.Min(Mathf.FloorToInt(1f / GCommon.SUB_DIV_STEP), resolution - baseResolution);

            Vector3 terrainSize = new Vector3(
                t.TerrainData.Geometry.Width,
                t.TerrainData.Geometry.Height,
                t.TerrainData.Geometry.Length);
            previewPropertyBlock.Clear();
            previewPropertyBlock.SetColor("_Color", new Color(0, 0, 0, 0.6f));
            previewPropertyBlock.SetTexture("_HeightMap", newHeightMap);
            previewPropertyBlock.SetVector("_Dimension", terrainSize);
            previewPropertyBlock.SetVector("_BoundMin", t.transform.position);
            previewPropertyBlock.SetVector("_BoundMax", t.transform.TransformPoint(terrainSize));
            previewPropertyBlock.SetTexture("_SubdivMap", t.TerrainData.Geometry.Internal_SubDivisionMap);
            previewPropertyBlock.SetTexture("_Mask", mask);
            previewPropertyBlock.SetMatrix("_WorldPointToMask", worldPointToMaskMatrix);

            Material mat = GInternalMaterials.SubdivLivePreviewMaterial;
            mat.renderQueue = 4000;
            Mesh previewMesh = null;

            int gridSize = t.TerrainData.Geometry.ChunkGridSize;
            for (int x = 0; x < gridSize; ++x)
            {
                for (int z = 0; z < gridSize; ++z)
                {
                    Rect uvRect = GCommon.GetUvRange(gridSize, x, z);
                    if (!uvRect.Overlaps(dirtyRect))
                        continue;
                    Vector3 localPos = new Vector3(
                        terrainSize.x * uvRect.x,
                        0f,
                        terrainSize.z * uvRect.y);
                    Vector3 worldPos = t.transform.TransformPoint(localPos);
                    Quaternion rotation = Quaternion.identity;
                    Vector3 scale = new Vector3(terrainSize.x * uvRect.width, 1, terrainSize.z * uvRect.height);

                    for (int i = baseResolution; i <= maxLevel; ++i)
                    {
                        previewMesh = GEditorSettings.Instance.livePreview.GetWireframeMesh(i);
                        previewPropertyBlock.SetVector("_SubdivRange", new Vector4(
                            (i - baseResolution) * GCommon.SUB_DIV_STEP,
                            i != maxLevel ? (i - baseResolution + 1) * GCommon.SUB_DIV_STEP : 1f,
                            0, 0));

                        Graphics.DrawMesh(
                            previewMesh,
                            Matrix4x4.TRS(worldPos, rotation, scale),
                            mat,
                            LayerMask.NameToLayer("Default"),
                            cam,
                            0,
                            previewPropertyBlock);
                    }
                }
            }
        }

        public static void DrawVisibilityLivePreview(GStylizedTerrain t, Camera cam, Texture newHeightMap, Rect dirtyRect, Texture mask, Matrix4x4 worldPointToMaskMatrix)
        {
            if (t.transform.rotation != Quaternion.identity ||
                t.transform.lossyScale != Vector3.one)
                return;

            t.TerrainData.Geometry.Internal_CreateNewSubDivisionMap(newHeightMap);
            int baseResolution = t.TerrainData.Geometry.MeshBaseResolution;
            int resolution = t.TerrainData.Geometry.MeshResolution;
            int maxLevel = baseResolution + Mathf.Min(Mathf.FloorToInt(1f / GCommon.SUB_DIV_STEP), resolution - baseResolution);

            Vector3 terrainSize = new Vector3(
                t.TerrainData.Geometry.Width,
                t.TerrainData.Geometry.Height,
                t.TerrainData.Geometry.Length);
            previewPropertyBlock.Clear();
            previewPropertyBlock.SetColor("_Color", new Color(0, 0, 0, 0.6f));
            previewPropertyBlock.SetTexture("_HeightMap", newHeightMap);
            previewPropertyBlock.SetVector("_Dimension", terrainSize);
            previewPropertyBlock.SetVector("_BoundMin", t.transform.position);
            previewPropertyBlock.SetVector("_BoundMax", t.transform.TransformPoint(terrainSize));
            previewPropertyBlock.SetTexture("_SubdivMap", t.TerrainData.Geometry.Internal_SubDivisionMap);
            if (mask == null)
                mask = Texture2D.whiteTexture;
            previewPropertyBlock.SetTexture("_Mask", mask);
            previewPropertyBlock.SetMatrix("_WorldPointToMask", worldPointToMaskMatrix);

            Material mat = GInternalMaterials.VisibilityLivePreviewMaterial;
            mat.EnableKeyword("USE_MASK");
            mat.renderQueue = 4000;
            Mesh previewMesh = null;

            int gridSize = t.TerrainData.Geometry.ChunkGridSize;
            for (int x = 0; x < gridSize; ++x)
            {
                for (int z = 0; z < gridSize; ++z)
                {
                    Rect uvRect = GCommon.GetUvRange(gridSize, x, z);
                    if (!uvRect.Overlaps(dirtyRect))
                        continue;
                    Vector3 localPos = new Vector3(
                        terrainSize.x * uvRect.x,
                        0f,
                        terrainSize.z * uvRect.y);
                    Vector3 worldPos = t.transform.TransformPoint(localPos);
                    Quaternion rotation = Quaternion.identity;
                    Vector3 scale = new Vector3(terrainSize.x * uvRect.width, 1, terrainSize.z * uvRect.height);

                    for (int i = baseResolution; i <= maxLevel; ++i)
                    {
                        previewMesh = GEditorSettings.Instance.livePreview.GetWireframeMesh(i);
                        previewPropertyBlock.SetVector("_SubdivRange", new Vector4(
                            (i - baseResolution) * GCommon.SUB_DIV_STEP,
                            i != maxLevel ? (i - baseResolution + 1) * GCommon.SUB_DIV_STEP : 1f,
                            0, 0));

                        Graphics.DrawMesh(
                            previewMesh,
                            Matrix4x4.TRS(worldPos, rotation, scale),
                            mat,
                            LayerMask.NameToLayer("Default"),
                            cam,
                            0,
                            previewPropertyBlock);
                    }
                }
            }
        }

        public static void DrawVisibilityLivePreview(GStylizedTerrain t, Camera cam, Texture newHeightMap, bool[] chunkCulling, Texture mask, Matrix4x4 worldPointToMaskMatrix)
        {
            if (t.transform.rotation != Quaternion.identity ||
                t.transform.lossyScale != Vector3.one)
                return;

            t.TerrainData.Geometry.Internal_CreateNewSubDivisionMap(newHeightMap);
            int baseResolution = t.TerrainData.Geometry.MeshBaseResolution;
            int resolution = t.TerrainData.Geometry.MeshResolution;
            int maxLevel = baseResolution + Mathf.Min(Mathf.FloorToInt(1f / GCommon.SUB_DIV_STEP), resolution - baseResolution);

            Vector3 terrainSize = new Vector3(
                t.TerrainData.Geometry.Width,
                t.TerrainData.Geometry.Height,
                t.TerrainData.Geometry.Length);
            previewPropertyBlock.Clear();
            previewPropertyBlock.SetColor("_Color", new Color(0, 0, 0, 0.6f));
            previewPropertyBlock.SetTexture("_HeightMap", newHeightMap);
            previewPropertyBlock.SetVector("_Dimension", terrainSize);
            previewPropertyBlock.SetVector("_BoundMin", t.transform.position);
            previewPropertyBlock.SetVector("_BoundMax", t.transform.TransformPoint(terrainSize));
            previewPropertyBlock.SetTexture("_SubdivMap", t.TerrainData.Geometry.Internal_SubDivisionMap);
            if (mask == null)
                mask = Texture2D.whiteTexture;
            previewPropertyBlock.SetTexture("_Mask", mask);
            previewPropertyBlock.SetMatrix("_WorldPointToMask", worldPointToMaskMatrix);

            Material mat = GInternalMaterials.VisibilityLivePreviewMaterial;
            mat.EnableKeyword("USE_MASK");
            mat.renderQueue = 4000;
            Mesh previewMesh = null;

            Rect[] chunkRects = t.GetChunkRects();
            for (int i = 0; i < chunkRects.Length; ++i)
            {
                if (chunkCulling[i] == false)
                    continue;
                Rect r = chunkRects[i];
                Vector3 position = new Vector3(r.x, t.transform.position.y, r.y);
                Quaternion rotation = Quaternion.identity;
                Vector3 scale = new Vector3(r.width, 1, r.height);
                Matrix4x4 trs = Matrix4x4.TRS(position, rotation, scale);

                for (int res = baseResolution; res <= maxLevel; ++res)
                {
                    previewMesh = GEditorSettings.Instance.livePreview.GetWireframeMesh(res);
                    previewPropertyBlock.SetVector("_SubdivRange", new Vector4(
                        (res - baseResolution) * GCommon.SUB_DIV_STEP,
                        res != maxLevel ? (res - baseResolution + 1) * GCommon.SUB_DIV_STEP : 1f,
                        0, 0));

                    Graphics.DrawMesh(
                        previewMesh,
                        trs,
                        mat,
                        LayerMask.NameToLayer("Default"),
                        cam,
                        0,
                        previewPropertyBlock,
                        ShadowCastingMode.Off,
                        false,
                        null,
                        LightProbeUsage.Off,
                        null);
                }
            }
        }

        public static void DrawVisibilityLivePreview(GStylizedTerrain t, Camera cam, Texture newHeightMap, Rect dirtyRect)
        {
            if (t.transform.rotation != Quaternion.identity ||
                t.transform.lossyScale != Vector3.one)
                return;

            t.TerrainData.Geometry.Internal_CreateNewSubDivisionMap(newHeightMap);
            int baseResolution = t.TerrainData.Geometry.MeshBaseResolution;
            int resolution = t.TerrainData.Geometry.MeshResolution;
            int maxLevel = baseResolution + Mathf.Min(Mathf.FloorToInt(1f / GCommon.SUB_DIV_STEP), resolution - baseResolution);

            Vector3 terrainSize = new Vector3(
                t.TerrainData.Geometry.Width,
                t.TerrainData.Geometry.Height,
                t.TerrainData.Geometry.Length);
            previewPropertyBlock.Clear();
            previewPropertyBlock.SetColor("_Color", new Color(0, 0, 0, 0.6f));
            previewPropertyBlock.SetTexture("_HeightMap", newHeightMap);
            previewPropertyBlock.SetVector("_Dimension", terrainSize);
            previewPropertyBlock.SetVector("_BoundMin", t.transform.position);
            previewPropertyBlock.SetVector("_BoundMax", t.transform.TransformPoint(terrainSize));
            previewPropertyBlock.SetTexture("_SubdivMap", t.TerrainData.Geometry.Internal_SubDivisionMap);

            Material mat = GInternalMaterials.VisibilityLivePreviewMaterial;
            mat.DisableKeyword("USE_MASK");
            mat.renderQueue = 4000;
            Mesh previewMesh = null;

            int gridSize = t.TerrainData.Geometry.ChunkGridSize;
            for (int x = 0; x < gridSize; ++x)
            {
                for (int z = 0; z < gridSize; ++z)
                {
                    Rect uvRect = GCommon.GetUvRange(gridSize, x, z);
                    if (!uvRect.Overlaps(dirtyRect))
                        continue;
                    Vector3 localPos = new Vector3(
                        terrainSize.x * uvRect.x,
                        0f,
                        terrainSize.z * uvRect.y);
                    Vector3 worldPos = t.transform.TransformPoint(localPos);
                    Quaternion rotation = Quaternion.identity;
                    Vector3 scale = new Vector3(terrainSize.x * uvRect.width, 1, terrainSize.z * uvRect.height);

                    for (int i = baseResolution; i <= maxLevel; ++i)
                    {
                        previewMesh = GEditorSettings.Instance.livePreview.GetWireframeMesh(i);
                        previewPropertyBlock.SetVector("_SubdivRange", new Vector4(
                            (i - baseResolution) * GCommon.SUB_DIV_STEP,
                            i != maxLevel ? (i - baseResolution + 1) * GCommon.SUB_DIV_STEP : 1f,
                            0, 0));

                        Graphics.DrawMesh(
                            previewMesh,
                            Matrix4x4.TRS(worldPos, rotation, scale),
                            mat,
                            LayerMask.NameToLayer("Default"),
                            cam,
                            0,
                            previewPropertyBlock);
                    }
                }
            }
        }

        public static void DrawAlbedoLivePreview(GStylizedTerrain t, Camera cam, Texture newAlbedo, Rect dirtyRect)
        {
            Material mat = t.TerrainData.Shading.MaterialToRender;
            if (mat == null)
                return;

            previewPropertyBlock.Clear();
            if (!string.IsNullOrEmpty(t.TerrainData.Shading.AlbedoMapPropertyName))
            {
                previewPropertyBlock.SetTexture(t.TerrainData.Shading.AlbedoMapPropertyName, newAlbedo);
            }

            GTerrainChunk[] chunks = t.GetChunks();
            for (int i = 0; i < chunks.Length; ++i)
            {
                Rect uvRange = chunks[i].GetUvRange();
                if (uvRange.Overlaps(dirtyRect))
                {
                    Mesh chunkMesh = chunks[i].GetMesh(0);
                    if (chunkMesh != null)
                    {
                        Graphics.DrawMesh(
                            chunkMesh,
                            chunks[i].transform.localToWorldMatrix * Matrix4x4.Translate(Vector3.up * 0.01f),
                            mat,
                            chunks[i].gameObject.layer,
                            cam,
                            0,
                            previewPropertyBlock,
                            t.TerrainData.Rendering.CastShadow,
                            t.TerrainData.Rendering.ReceiveShadow);
                    }
                }
            }
        }

        public static void DrawMetallicSmoothnessLivePreview(GStylizedTerrain t, Camera cam, Texture newMetallicMap, Rect dirtyRect)
        {
            Material mat = t.TerrainData.Shading.MaterialToRender;
            if (mat == null)
                return;

            previewPropertyBlock.Clear();
            if (!string.IsNullOrEmpty(t.TerrainData.Shading.MetallicMapPropertyName))
            {
                previewPropertyBlock.SetTexture(t.TerrainData.Shading.MetallicMapPropertyName, newMetallicMap);
            }

            GTerrainChunk[] chunks = t.GetChunks();
            for (int i = 0; i < chunks.Length; ++i)
            {
                Rect uvRange = chunks[i].GetUvRange();
                if (uvRange.Overlaps(dirtyRect))
                {
                    Mesh chunkMesh = chunks[i].GetMesh(0);
                    if (chunkMesh != null)
                    {
                        Graphics.DrawMesh(
                            chunkMesh,
                            chunks[i].transform.localToWorldMatrix * Matrix4x4.Translate(Vector3.up * 0.01f),
                            mat,
                            chunks[i].gameObject.layer,
                            cam,
                            0,
                            previewPropertyBlock,
                            t.TerrainData.Rendering.CastShadow,
                            t.TerrainData.Rendering.ReceiveShadow);
                    }
                }
            }
        }

        public static void DrawAMSLivePreview(GStylizedTerrain t, Camera cam, Texture newAlbedo, Texture newMetallicMap, Rect dirtyRect)
        {
            Material mat = t.TerrainData.Shading.MaterialToRender;
            if (mat == null)
                return;

            previewPropertyBlock.Clear();
            if (!string.IsNullOrEmpty(t.TerrainData.Shading.AlbedoMapPropertyName))
            {
                previewPropertyBlock.SetTexture(t.TerrainData.Shading.AlbedoMapPropertyName, newAlbedo);
            }
            if (!string.IsNullOrEmpty(t.TerrainData.Shading.MetallicMapPropertyName))
            {
                previewPropertyBlock.SetTexture(t.TerrainData.Shading.MetallicMapPropertyName, newMetallicMap);
            }


            GTerrainChunk[] chunks = t.GetChunks();
            for (int i = 0; i < chunks.Length; ++i)
            {
                Rect uvRange = chunks[i].GetUvRange();
                if (uvRange.Overlaps(dirtyRect))
                {

                    Mesh chunkMesh = chunks[i].GetMesh(0);
                    if (chunkMesh != null)
                    {
                        Graphics.DrawMesh(
                            chunkMesh,
                            chunks[i].transform.localToWorldMatrix * Matrix4x4.Translate(Vector3.up * 0.01f),
                            mat,
                            chunks[i].gameObject.layer,
                            cam,
                            0,
                            previewPropertyBlock,
                            t.TerrainData.Rendering.CastShadow,
                            t.TerrainData.Rendering.ReceiveShadow);
                    }
                }
            }
        }

        public static void DrawAMSLivePreview(GStylizedTerrain t, Camera cam, Texture newAlbedo, Texture newMetallicMap, bool[] chunkCulling)
        {
            Material mat = t.TerrainData.Shading.MaterialToRender;
            if (mat == null)
                return;

            previewPropertyBlock.Clear();
            if (!string.IsNullOrEmpty(t.TerrainData.Shading.AlbedoMapPropertyName))
            {
                previewPropertyBlock.SetTexture(t.TerrainData.Shading.AlbedoMapPropertyName, newAlbedo);
            }
            if (!string.IsNullOrEmpty(t.TerrainData.Shading.MetallicMapPropertyName))
            {
                previewPropertyBlock.SetTexture(t.TerrainData.Shading.MetallicMapPropertyName, newMetallicMap);
            }

            int gridSize = t.TerrainData.Geometry.ChunkGridSize;
            GTerrainChunk[] chunks = t.GetChunks();
            for (int i = 0; i < chunks.Length; ++i)
            {
                int chunkIndex = GUtilities.To1DIndex((int)chunks[i].Index.x, (int)chunks[i].Index.y, gridSize);
                if (chunkCulling[chunkIndex] == false)
                    continue;

                Graphics.DrawMesh(
                    chunks[i].GetMesh(0),
                    chunks[i].transform.localToWorldMatrix * Matrix4x4.Translate(Vector3.up * 0.01f),
                    mat,
                    LayerMask.NameToLayer("Default"),
                    cam,
                    0,
                    previewPropertyBlock,
                    ShadowCastingMode.Off,
                    false,
                    null,
                    LightProbeUsage.Off,
                    null);
            }
        }

        public static void DrawSplatLivePreview(GStylizedTerrain t, Camera cam, Texture[] newControlMaps, Rect dirtyRect)
        {
            Material mat = t.TerrainData.Shading.MaterialToRender;
            if (mat == null)
                return;
            int controlMapResolution = t.TerrainData.Shading.SplatControlResolution;
            int controlMapCount = t.TerrainData.Shading.SplatControlMapCount;
            if (controlMapCount == 0)
                return;

            previewPropertyBlock.Clear();
            for (int i = 0; i < controlMapCount; ++i)
            {
                if (!string.IsNullOrEmpty(t.TerrainData.Shading.SplatControlMapPropertyName))
                {
                    previewPropertyBlock.SetTexture(t.TerrainData.Shading.SplatControlMapPropertyName + i, newControlMaps[i] ?? Texture2D.blackTexture);
                }
            }

            GTerrainChunk[] chunks = t.GetChunks();
            for (int i = 0; i < chunks.Length; ++i)
            {
                Rect uvRange = chunks[i].GetUvRange();
                if (uvRange.Overlaps(dirtyRect))
                {
                    Mesh chunkMesh = chunks[i].GetMesh(0);
                    if (chunkMesh != null)
                    {
                        Graphics.DrawMesh(
                            chunkMesh,
                            chunks[i].transform.localToWorldMatrix * Matrix4x4.Translate(Vector3.up * 0.01f),
                            mat,
                            chunks[i].gameObject.layer,
                            cam,
                            0,
                            previewPropertyBlock,
                            t.TerrainData.Rendering.CastShadow,
                            t.TerrainData.Rendering.ReceiveShadow);
                    }
                }
            }
        }


        public static void DrawSplatLivePreview(GStylizedTerrain t, Camera cam, Texture[] newControlMaps, bool[] chunkCulling)
        {
            Material mat = t.TerrainData.Shading.MaterialToRender;
            if (mat == null)
                return;
            int controlMapCount = t.TerrainData.Shading.SplatControlMapCount;
            if (controlMapCount == 0)
                return;

            previewPropertyBlock.Clear();
            for (int i = 0; i < controlMapCount; ++i)
            {
                if (!string.IsNullOrEmpty(t.TerrainData.Shading.SplatControlMapPropertyName))
                {
                    previewPropertyBlock.SetTexture(t.TerrainData.Shading.SplatControlMapPropertyName + i, newControlMaps[i] ?? Texture2D.blackTexture);
                }
            }

            int gridSize = t.TerrainData.Geometry.ChunkGridSize;
            GTerrainChunk[] chunks = t.GetChunks();
            for (int i = 0; i < chunks.Length; ++i)
            {
                int chunkIndex = GUtilities.To1DIndex((int)chunks[i].Index.x, (int)chunks[i].Index.y, gridSize);
                if (chunkCulling[chunkIndex] == false)
                    continue;

                Graphics.DrawMesh(
                    chunks[i].GetMesh(0),
                    chunks[i].transform.localToWorldMatrix * Matrix4x4.Translate(Vector3.up * 0.01f),
                    mat,
                    LayerMask.NameToLayer("Default"),
                    cam,
                    0,
                    previewPropertyBlock,
                    ShadowCastingMode.Off,
                    false,
                    null,
                    LightProbeUsage.Off,
                    null);
            }
        }

        public static void DrawMasksLivePreview(GStylizedTerrain t, Camera cam, Texture[] masks, Color[] colors, Rect dirtyRect, int channel = 0)
        {
            Material mat = GInternalMaterials.MaskVisualizerMaterial;
            if (mat == null)
                return;

            previewPropertyBlock.Clear();
            previewPropertyBlock.SetVector("_Dimension", new Vector4(t.TerrainData.Geometry.Width, t.TerrainData.Geometry.Height, t.TerrainData.Geometry.Length));

            GTerrainChunk[] chunks = t.GetChunks();
            for (int i = 0; i < chunks.Length; ++i)
            {
                Rect uvRange = chunks[i].GetUvRange();
                if (uvRange.Overlaps(dirtyRect))
                {
                    Mesh chunkMesh = chunks[i].GetMesh(0);
                    if (chunkMesh != null)
                    {
                        for (int j = 0; j < masks.Length; ++j)
                        {
                            previewPropertyBlock.SetColor("_Color", colors[j]);
                            previewPropertyBlock.SetTexture("_MainTex", masks[j]);
                            previewPropertyBlock.SetInt("_Channel", channel);
                            Graphics.DrawMesh(
                                chunkMesh,
                                chunks[i].transform.localToWorldMatrix * Matrix4x4.Translate(Vector3.up * 0.05f * j),
                                mat,
                                chunks[i].gameObject.layer,
                                cam,
                                0,
                                previewPropertyBlock,
                                t.TerrainData.Rendering.CastShadow,
                                t.TerrainData.Rendering.ReceiveShadow);
                        }
                    }
                }
            }
        }


        public static void DrawMasksLivePreview(GStylizedTerrain t, Camera cam, Texture[] masks, Color[] colors, bool[] chunkCulling, int channel = 0)
        {
            Material mat = GInternalMaterials.MaskVisualizerMaterial;
            if (mat == null)
                return;

            previewPropertyBlock.Clear();
            previewPropertyBlock.SetVector("_Dimension", new Vector4(t.TerrainData.Geometry.Width, t.TerrainData.Geometry.Height, t.TerrainData.Geometry.Length));

            int gridSize = t.TerrainData.Geometry.ChunkGridSize;
            GTerrainChunk[] chunks = t.GetChunks();
            for (int i = 0; i < chunks.Length; ++i)
            {
                int chunkIndex = GUtilities.To1DIndex((int)chunks[i].Index.x, (int)chunks[i].Index.y, gridSize);
                if (chunkCulling[chunkIndex] == false)
                    continue;
                Mesh chunkMesh = chunks[i].GetMesh(0);
                for (int j = 0; j < masks.Length; ++j)
                {
                    previewPropertyBlock.SetColor("_Color", colors[j]);
                    previewPropertyBlock.SetTexture("_MainTex", masks[j]);
                    previewPropertyBlock.SetInt("_Channel", channel);
                    Graphics.DrawMesh(
                        chunkMesh,
                        chunks[i].transform.localToWorldMatrix * Matrix4x4.Translate(Vector3.up * 0.05f * j),
                        mat,
                        chunks[i].gameObject.layer,
                        cam,
                        0,
                        previewPropertyBlock,
                        t.TerrainData.Rendering.CastShadow,
                        t.TerrainData.Rendering.ReceiveShadow);

                }
            }
        }

        public static void DrawTerrainMask(GStylizedTerrain t, Camera cam)
        {
            if (t.TerrainData == null)
                return;
            GLivePreviewDrawer.DrawMasksLivePreview(
                t, cam,
                new Texture[] { t.TerrainData.Mask.MaskMapOrDefault },
                new Color[] { Color.red },
                GCommon.UnitRect);
        }

        public static void DrawMask4ChannelsLivePreview(GStylizedTerrain t, Camera cam, Texture masks, Rect dirtyRect)
        {
            Material mat = GInternalMaterials.Mask4ChannelsMaterial;
            if (mat == null)
                return;

            previewPropertyBlock.Clear();
            GTerrainChunk[] chunks = t.GetChunks();
            for (int i = 0; i < chunks.Length; ++i)
            {
                Rect uvRange = chunks[i].GetUvRange();
                if (uvRange.Overlaps(dirtyRect))
                {
                    Mesh chunkMesh = chunks[i].GetMesh(0);
                    if (chunkMesh != null)
                    {
                        previewPropertyBlock.SetTexture("_MainTex", masks);
                        Graphics.DrawMesh(
                            chunkMesh,
                            chunks[i].transform.localToWorldMatrix * Matrix4x4.Translate(Vector3.up * 0.05f),
                            mat,
                            chunks[i].gameObject.layer,
                            cam,
                            0,
                            previewPropertyBlock,
                            t.TerrainData.Rendering.CastShadow,
                            t.TerrainData.Rendering.ReceiveShadow);
                    }
                }
            }
        }
    }
}
#endif
#endif
