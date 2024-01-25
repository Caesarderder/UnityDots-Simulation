#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;

namespace Pinwheel.Griffin.ErosionTool
{
    public class GErosionPreviewDrawer
    {
        private static MaterialPropertyBlock previewPropertyBlock = new MaterialPropertyBlock();

        private static Material geometryPreviewMaterial;
        private static Material GeometryPreviewMaterial
        {
            get
            {
                if (geometryPreviewMaterial == null)
                {
                    geometryPreviewMaterial = new Material(Shader.Find("Hidden/Griffin/ErosionGeometryLivePreview"));
                }
                return geometryPreviewMaterial;
            }
        }

        private static readonly int SIMULATION_DATA = Shader.PropertyToID("_SimulationData");
        private static readonly int FADE_DISTANCE = Shader.PropertyToID("_FadeDistance");
        private static readonly int TRANSPARENCY = Shader.PropertyToID("_Transparency");
        private static readonly int WORLD_TO_SIM = Shader.PropertyToID("_WorldToSim");
        private static readonly int FALLOFF_TEXTURE = Shader.PropertyToID("_FalloffTexture");
        private static readonly int HEIGHT_MAP = Shader.PropertyToID("_HeightMap");
        private static readonly int HEIGHT = Shader.PropertyToID("_Height");
        private static readonly int WORLD_TO_TERRAIN_UV = Shader.PropertyToID("_WorldToTerrainUV");
        private static readonly int EROSION_MAP = Shader.PropertyToID("_ErosionMap");

        private static readonly string KW_SHOW_TEXTURE = "SHOW_TEXTURE";
        private static readonly string KW_SHOW_COLOR = "SHOW_COLOR";
        private static readonly int EROSION_SPLAT = Shader.PropertyToID("_ErosionSplat");
        private static readonly int EROSION_SPLAT_ST = Shader.PropertyToID("_ErosionSplat_ST");
        private static readonly int EROSION_INTENSITY = Shader.PropertyToID("_ErosionIntensity");
        private static readonly int EROSION_ALBEDO = Shader.PropertyToID("_ErosionAlbedo");

        private static readonly int DEPOSITION_SPLAT = Shader.PropertyToID("_DepositionSplat");
        private static readonly int DEPOSITION_SPLAT_ST = Shader.PropertyToID("_DepositionSplat_ST");
        private static readonly int DEPOSITION_INTENSITY = Shader.PropertyToID("_DepositionIntensity");
        private static readonly int DEPOSITION_ALBEDO = Shader.PropertyToID("_DepositionAlbedo");

        public static void DrawGeometryLivePreview(GStylizedTerrain t, Camera cam, GErosionSimulator simulator, bool[] chunkCulling)
        {
            if (t.transform.rotation != Quaternion.identity ||
                t.transform.lossyScale != Vector3.one)
                return;
            if (t.TerrainData == null)
                return;

            Mesh previewMesh = GEditorSettings.Instance.livePreview.GetTriangleMesh(t.TerrainData.Geometry.MeshResolution);

            Vector3 terrainSize = new Vector3(
                t.TerrainData.Geometry.Width,
                t.TerrainData.Geometry.Height,
                t.TerrainData.Geometry.Length);
            previewPropertyBlock.Clear();
            previewPropertyBlock.SetTexture(SIMULATION_DATA, simulator.SimulationData != null ? (Texture)simulator.SimulationData : Texture2D.blackTexture);
            previewPropertyBlock.SetFloat(FADE_DISTANCE, 0f);
            previewPropertyBlock.SetFloat(TRANSPARENCY, 1f);
            previewPropertyBlock.SetMatrix(WORLD_TO_SIM, simulator.transform.worldToLocalMatrix);
            previewPropertyBlock.SetTexture(FALLOFF_TEXTURE, simulator.FalloffTexture != null ? simulator.FalloffTexture : Texture2D.whiteTexture);
            previewPropertyBlock.SetTexture(HEIGHT_MAP, t.TerrainData.Geometry.HeightMap);
            previewPropertyBlock.SetFloat(HEIGHT, t.TerrainData.Geometry.Height);
            previewPropertyBlock.SetMatrix(WORLD_TO_TERRAIN_UV, t.GetWorldToNormalizedMatrix());

            Material mat = GeometryPreviewMaterial;
            mat.renderQueue = 4000;

            mat.DisableKeyword(KW_SHOW_TEXTURE);
            mat.DisableKeyword(KW_SHOW_COLOR);

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
    }
}
#endif
