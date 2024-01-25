#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pinwheel.Griffin;

namespace Pinwheel.Griffin.Rendering
{
    public class GSimpleGrassMaterialConfigurator : IGGrassMaterialConfigurator
    {
        private static readonly int COLOR = Shader.PropertyToID("_Color");
        private static readonly int MAIN_TEX = Shader.PropertyToID("_MainTex");
        private static readonly int NOISE_TEX = Shader.PropertyToID("_NoiseTex");
        private static readonly int WIND = Shader.PropertyToID("_Wind");
        private static readonly int BEND_FACTOR = Shader.PropertyToID("_BendFactor");
        private static readonly int VECTOR_FIELD = Shader.PropertyToID("_VectorField");
        private static readonly int WORLD_TO_NORMALIZED = Shader.PropertyToID("_WorldToNormalized");
        private static readonly int FADE_MIN_DISTANCE = Shader.PropertyToID("_FadeMinDistance");
        private static readonly int FADE_MAX_DISTANCE = Shader.PropertyToID("_FadeMaxDistance");

        public void Configure(GStylizedTerrain terrain, int prototypeIndex, MaterialPropertyBlock propertyBlock)
        {
            GGrassPrototype proto = terrain.TerrainData.Foliage.Grasses.Prototypes[prototypeIndex];

            propertyBlock.SetTexture(NOISE_TEX, GRuntimeSettings.Instance.foliageRendering.windNoiseTexture);

            IEnumerator<GWindZone> windZone = GWindZone.ActiveWindZones.GetEnumerator();
            if (windZone.MoveNext())
            {
                GWindZone w = windZone.Current;
                propertyBlock.SetVector(WIND, w.GetWindParams());
            }

            propertyBlock.SetColor(COLOR, proto.Color);
            if (proto.Shape != GGrassShape.DetailObject && proto.Texture != null)
                propertyBlock.SetTexture(MAIN_TEX, proto.Texture);
            propertyBlock.SetFloat(BEND_FACTOR, proto.BendFactor);

            if (terrain.TerrainData.Foliage.EnableInteractiveGrass)
            {
                propertyBlock.SetTexture(VECTOR_FIELD, terrain.GetGrassVectorFieldRenderTexture());
                propertyBlock.SetMatrix(WORLD_TO_NORMALIZED, terrain.GetWorldToNormalizedMatrix());
            }

            float fadeMaxDistance = terrain.TerrainData.Rendering.GrassDistance;
            float fadeMinDistance = Mathf.Clamp(terrain.TerrainData.Rendering.GrassFadeStart, 0f, 0.99f) * fadeMaxDistance;
            propertyBlock.SetFloat(FADE_MIN_DISTANCE, fadeMinDistance);
            propertyBlock.SetFloat(FADE_MAX_DISTANCE, fadeMaxDistance);
        }
    }
}
#endif
