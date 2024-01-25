#if GRIFFIN
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Pinwheel.Griffin.SplineTool
{
    [GDisplayName("Object Remover")]
    public class GObjectRemover : GSplineModifier
    {
        [SerializeField]
        private AnimationCurve falloff;
        public AnimationCurve Falloff
        {
            get
            {
                if (falloff == null)
                {
                    falloff = AnimationCurve.EaseInOut(0, 0, 1, 1);
                }
                return falloff;
            }
            set
            {
                falloff = value;
            }
        }

        [SerializeField]
        private List<GameObject> prototypes;
        public List<GameObject> Prototypes
        {
            get
            {
                if (prototypes == null)
                {
                    prototypes = new List<GameObject>();
                }
                return prototypes;
            }
            set
            {
                prototypes = value;
            }
        }

        [SerializeField]
        private List<int> prototypeIndices;
        public List<int> PrototypeIndices
        {
            get
            {
                if (prototypeIndices == null)
                {
                    prototypeIndices = new List<int>();
                }
                return prototypeIndices;
            }
            set
            {
                prototypeIndices = value;
            }
        }

        [SerializeField]
        private Texture2D falloffNoise;
        public Texture2D FalloffNoise
        {
            get
            {
                return falloffNoise;
            }
            set
            {
                falloffNoise = value;
            }
        }

        [SerializeField]
        private Vector2 falloffNoiseSize;
        public Vector2 FalloffNoiseSize
        {
            get
            {
                return falloffNoiseSize;
            }
            set
            {
                falloffNoiseSize = value;
            }
        }

        [SerializeField]
        private int maskResolution;
        public int MaskResolution
        {
            get
            {
                return maskResolution;
            }
            set
            {
                maskResolution = Mathf.Clamp(Mathf.ClosestPowerOfTwo(value), GCommon.TEXTURE_SIZE_MIN, GCommon.TEXTURE_SIZE_MAX);
            }
        }

        private Texture2D falloffTexture;

        private Material maskMaterial;
        private Material MaskMaterial
        {
            get
            {
                if (maskMaterial == null)
                {
                    maskMaterial = new Material(GRuntimeSettings.Instance.internalShaders.splineMaskShader);
                }
                return maskMaterial;
            }
        }

        private static readonly int FALL_OFF = Shader.PropertyToID("_Falloff");
        private static readonly int FALL_OFF_NOISE = Shader.PropertyToID("_FalloffNoise");
        private static readonly int TERRAIN_MASK = Shader.PropertyToID("_TerrainMask");

        public override void Apply()
        {
            if (SplineCreator == null)
                return;
            if (falloffTexture != null)
                Object.DestroyImmediate(falloffTexture);
            Internal_UpdateFalloffTexture();
            List<GStylizedTerrain> terrains = GUtilities.ExtractTerrainsFromOverlapTest(SplineCreator.SweepTest());
            foreach (GStylizedTerrain t in terrains)
            {
                Apply(t);
            }
        }

        private void Apply(GStylizedTerrain t)
        {
            if (t.TerrainData == null)
                return;
            if (PrototypeIndices.Count == 0)
                return;
            if (Prototypes.Count == 0)
                return;
            RenderTexture rt = new RenderTexture(MaskResolution, MaskResolution, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            Internal_Apply(t, rt);
            Texture2D mask = GCommon.CreateTexture(MaskResolution, Color.clear);
            GCommon.CopyFromRT(mask, rt);
            mask.wrapMode = TextureWrapMode.Clamp;

            Color[] maskColors = mask.GetPixels();
            RemoveObjectFromTerrain(t, maskColors);

            rt.Release();
            GUtilities.DestroyObject(rt);
            GUtilities.DestroyObject(mask);
        }

        private void RemoveObjectFromTerrain(GStylizedTerrain t, Color[] maskData)
        {
            for (int i = 0; i < PrototypeIndices.Count; ++i)
            {
                int prototypeIndex = PrototypeIndices[i];
                if (prototypeIndex < 0 || prototypeIndex >= Prototypes.Count)
                    continue;
                GameObject g = Prototypes[prototypeIndex];
                if (g == null)
                    continue;

                GSpawner.DestroyIf(t, g, (instance) =>
                {
                    Vector2 uv = t.WorldPointToUV(instance.transform.position);
                    float alpha = GUtilities.GetColorBilinear(maskData, MaskResolution, MaskResolution, uv).r;
                    return Random.value <= alpha;
                });
            }
        }

        private Material PrepareMaterial(GStylizedTerrain t, RenderTexture rt)
        {
            GCommon.ClearRT(rt);
            Material mat = GInternalMaterials.FoliageRemoverMaterial;
            mat.SetTexture(FALL_OFF, falloffTexture);
            mat.SetTexture(FALL_OFF_NOISE, FalloffNoise);
            mat.SetTextureScale(FALL_OFF_NOISE, new Vector2(
                FalloffNoiseSize.x != 0 ? 1f / FalloffNoiseSize.x : 0,
                FalloffNoiseSize.y != 0 ? 1f / FalloffNoiseSize.y : 0));
            mat.SetTextureOffset(FALL_OFF_NOISE, Vector2.zero);
            if (SplineCreator.EnableTerrainMask)
            {
                mat.SetTexture(TERRAIN_MASK, t.TerrainData.Mask.MaskMapOrDefault);
            }
            else
            {
                mat.SetTexture(TERRAIN_MASK, Texture2D.blackTexture);
            }
            return mat;
        }

        public void Internal_Apply(GStylizedTerrain t, RenderTexture rt)
        {
            Material mat = PrepareMaterial(t, rt);
            SplineCreator.DrawOnTexture(rt, t.Bounds, mat);
        }

        public void Internal_Apply(GStylizedTerrain t, RenderTexture rt, ScriptableRenderContext context)
        {
            Material mat = PrepareMaterial(t, rt);
            SplineCreator.DrawOnTexture(rt, t.Bounds, mat, context);
        }

        public void Reset()
        {
            SplineCreator = GetComponent<GSplineCreator>();
            Falloff = AnimationCurve.EaseInOut(0, 0, 1, 1);
            Prototypes = null;
            PrototypeIndices = null;
            MaskResolution = 1024;
        }

        public void Internal_UpdateFalloffTexture()
        {
            falloffTexture = GCommon.CreateTextureFromCurve(Falloff, 256, 1);
        }
    }
}
#endif
