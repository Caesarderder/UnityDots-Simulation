#if GRIFFIN
using System.Collections.Generic;
using UnityEngine;

namespace Pinwheel.Griffin.StampTool
{
    [System.Serializable]
    [ExecuteInEditMode]
    public class GGeometryStamper : MonoBehaviour
    {
        public enum GStampChannel
        {
            Elevation, Visibility
        }

        [SerializeField]
        private bool enableTerrainMask = true;
        public bool EnableTerrainMask
        {
            get
            {
                return enableTerrainMask;
            }
            set
            {
                enableTerrainMask = value;
            }
        }

        [SerializeField]
        private int groupId;
        public int GroupId
        {
            get
            {
                return groupId;
            }
            set
            {
                groupId = value;
            }
        }

        [SerializeField]
        private Vector3 position;
        public Vector3 Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
                transform.position = value;
            }
        }

        [SerializeField]
        private Quaternion rotation;
        public Quaternion Rotation
        {
            get
            {
                return rotation;
            }
            set
            {
                rotation = value;
                transform.rotation = value;
            }
        }

        [SerializeField]
        private Vector3 scale;
        public Vector3 Scale
        {
            get
            {
                return scale;
            }
            set
            {
                scale = value;
                transform.localScale = value;
            }
        }

        [SerializeField]
        private Texture2D stamp;
        public Texture2D Stamp
        {
            get
            {
                return stamp;
            }
            set
            {
                stamp = value;
            }
        }

        [SerializeField]
        private GStampChannel channel;
        public GStampChannel Channel
        {
            get
            {
                return channel;
            }
            set
            {
                channel = value;
            }
        }

        [SerializeField]
        private AnimationCurve falloff;
        public AnimationCurve Falloff
        {
            get
            {
                return falloff;
            }
            set
            {
                falloff = value;
            }
        }

        [SerializeField]
        private bool inverseStamp;
        public bool InverseStamp
        {
            get
            {
                return inverseStamp;
            }
            set
            {
                inverseStamp = value;
            }
        }

        [SerializeField]
        private bool useFalloffAsBlendFactor;
        public bool UseFalloffAsBlendFactor
        {
            get
            {
                return useFalloffAsBlendFactor;
            }
            set
            {
                useFalloffAsBlendFactor = value;
            }
        }

        [SerializeField]
        private GStampOperation operation;
        public GStampOperation Operation
        {
            get
            {
                return operation;
            }
            set
            {
                operation = value;
            }
        }

        [SerializeField]
        private float lerpFactor;
        public float LerpFactor
        {
            get
            {
                return lerpFactor;
            }
            set
            {
                lerpFactor = Mathf.Clamp01(value);
            }
        }

        [SerializeField]
        private int additionalMeshResolution;
        public int AdditionalMeshResolution
        {
            get
            {
                return additionalMeshResolution;
            }
            set
            {
                additionalMeshResolution = Mathf.Clamp(value, 0, 10);
            }
        }

        private Texture2D falloffTexture;

        public Rect Rect
        {
            get
            {
                Vector3[] quad = new Vector3[4];
                GetQuad(quad);
                Rect r = GUtilities.GetRectContainsPoints(quad);
                return r;
            }
        }

        private void Reset()
        {
            position = Vector3.zero;
            rotation = Quaternion.identity;
            scale = Vector3.one * 100;
            stamp = null;
            falloff = AnimationCurve.EaseInOut(0, 1, 1, 0);
            inverseStamp = false;
            useFalloffAsBlendFactor = true;

            operation = GStampOperation.Max;
            lerpFactor = 0.5f;
            additionalMeshResolution = 0;
        }

        public void Apply()
        {
            if (falloffTexture != null)
                Object.DestroyImmediate(falloffTexture);
            Internal_UpdateFalloffTexture();

            List<GStylizedTerrain> terrains = GUtilities.ExtractTerrainsFromOverlapTest(GCommon.OverlapTest(GroupId, GetQuad()));
            foreach (GStylizedTerrain t in terrains)
            {
                DrawOnTexture(t);
            }
            foreach(GStylizedTerrain t in terrains)
            {
                UpdateTerrain(t);
            }
            foreach (GStylizedTerrain t in terrains)
            {
                t.MatchEdges();
            }
        }

        private void DrawOnTexture(GStylizedTerrain t)
        {
            if (t.TerrainData == null)
                return;
            int heightMapResolution = t.TerrainData.Geometry.HeightMapResolution;
            RenderTexture rt = new RenderTexture(heightMapResolution, heightMapResolution, 0, GGeometry.HeightMapRTFormat, RenderTextureReadWrite.Linear);
            Internal_DrawOnTexture(t, rt);

            Color[] oldHeightMapColors = t.TerrainData.Geometry.HeightMap.GetPixels();
            RenderTexture.active = rt;
            t.TerrainData.Geometry.HeightMap.ReadPixels(new Rect(0, 0, heightMapResolution, heightMapResolution), 0, 0);
            t.TerrainData.Geometry.HeightMap.Apply();
            RenderTexture.active = null;
            Color[] newHeightMapColors = t.TerrainData.Geometry.HeightMap.GetPixels();

            rt.Release();
            Object.DestroyImmediate(rt);

            List<Rect> dirtyRects = new List<Rect>(GCommon.CompareTerrainTexture(t.TerrainData.Geometry.ChunkGridSize, oldHeightMapColors, newHeightMapColors));
            for (int i = 0; i < dirtyRects.Count; ++i)
            {
                t.TerrainData.Geometry.SetRegionDirty(dirtyRects[i]);
                t.TerrainData.Foliage.SetTreeRegionDirty(dirtyRects[i]);
                t.TerrainData.Foliage.SetGrassRegionDirty(dirtyRects[i]);
            }            
        }

        private void UpdateTerrain(GStylizedTerrain t)
        {
            t.TerrainData.SetDirty(GTerrainData.DirtyFlags.Geometry);
            t.UpdateTreesPosition();
            t.UpdateGrassPatches();
            t.TerrainData.Foliage.ClearTreeDirtyRegions();
            t.TerrainData.Foliage.ClearGrassDirtyRegions();
            t.TerrainData.SetDirty(GTerrainData.DirtyFlags.Foliage);
        }

        public void Internal_DrawOnTexture(GStylizedTerrain t, RenderTexture rt)
        {
            GCommon.CopyToRT(t.TerrainData.Geometry.HeightMap, rt);

            Vector3[] worldCorner = GetQuad();
            Vector2[] uvCorners = new Vector2[worldCorner.Length];
            for (int i = 0; i < uvCorners.Length; ++i)
            {
                uvCorners[i] = t.WorldPointToUV(worldCorner[i]);
            }

            Rect dirtyRect = GUtilities.GetRectContainsPoints(uvCorners);
            if (!dirtyRect.Overlaps(new Rect(0, 0, 1, 1)))
                return;

            Vector3 normalizedPos = t.WorldPointToNormalized(Position);
            float stampHeight = GUtilities.InverseLerpUnclamped(0, t.TerrainData.Geometry.Height, Scale.y);

            Material mat = GInternalMaterials.StamperMaterial;
            mat.SetTexture("_HeightMap", t.TerrainData.Geometry.HeightMap);
            mat.SetTexture("_Stamp", Stamp);
            mat.SetTexture("_Falloff", falloffTexture);
            mat.SetInt("_Operation", (int)Operation);
            mat.SetFloat("_LerpFactor", LerpFactor);
            mat.SetFloat("_StampHeight", stampHeight);
            mat.SetFloat("_StampPositionY", normalizedPos.y);
            mat.SetFloat("_Inverse", InverseStamp ? 1 : 0);
            mat.SetFloat("_UseFalloffAsBlendFactor", UseFalloffAsBlendFactor ? 1 : 0);
            mat.SetFloat("_AdditionalMeshResolution", GCommon.SUB_DIV_STEP * AdditionalMeshResolution);
            if (EnableTerrainMask)
            {
                mat.SetTexture("_TerrainMask", t.TerrainData.Mask.MaskMapOrDefault);
            }
            else
            {
                mat.SetTexture("_TerrainMask", Texture2D.blackTexture);
            }

            int pass = (int)Channel;
            GCommon.DrawQuad(rt, uvCorners, mat, pass);
        }

        public void Internal_UpdateFalloffTexture()
        {
            falloffTexture = GCommon.CreateTextureFromCurve(Falloff, 256, 1);
        }

        public Vector3[] GetQuad()
        {
            Matrix4x4 matrix = Matrix4x4.TRS(Position, Rotation, Scale);
            Vector3[] quad = new Vector3[4]
            {
                matrix.MultiplyPoint(new Vector3(-0.5f, 0, -0.5f)),
                matrix.MultiplyPoint(new Vector3(-0.5f, 0, 0.5f)),
                matrix.MultiplyPoint(new Vector3(0.5f, 0, 0.5f)),
                matrix.MultiplyPoint(new Vector3(0.5f, 0, -0.5f))
            };

            return quad;
        }

        public void GetQuad(Vector3[] quad)
        {
            Matrix4x4 matrix = Matrix4x4.TRS(Position, Rotation, Scale);
            quad[0] = matrix.MultiplyPoint(new Vector3(-0.5f, 0, -0.5f));
            quad[1] = matrix.MultiplyPoint(new Vector3(-0.5f, 0, 0.5f));
            quad[2] = matrix.MultiplyPoint(new Vector3(0.5f, 0, 0.5f));
            quad[3] = matrix.MultiplyPoint(new Vector3(0.5f, 0, -0.5f));
        }

        public void GetBox(Vector3[] box)
        {
            Matrix4x4 matrix = Matrix4x4.TRS(Position, Rotation, Scale);
            box[0] = matrix.MultiplyPoint(new Vector3(-0.5f, 0, -0.5f));
            box[1] = matrix.MultiplyPoint(new Vector3(-0.5f, 0, 0.5f));
            box[2] = matrix.MultiplyPoint(new Vector3(0.5f, 0, 0.5f));
            box[3] = matrix.MultiplyPoint(new Vector3(0.5f, 0, -0.5f));
            box[4] = matrix.MultiplyPoint(new Vector3(-0.5f, 1, -0.5f));
            box[5] = matrix.MultiplyPoint(new Vector3(-0.5f, 1, 0.5f));
            box[6] = matrix.MultiplyPoint(new Vector3(0.5f, 1, 0.5f));
            box[7] = matrix.MultiplyPoint(new Vector3(0.5f, 1, -0.5f));
        }
    }
}
#endif
