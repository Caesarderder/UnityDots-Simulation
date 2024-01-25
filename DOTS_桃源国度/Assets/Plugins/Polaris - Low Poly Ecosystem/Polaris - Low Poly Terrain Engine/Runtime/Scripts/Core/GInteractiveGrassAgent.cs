#if GRIFFIN
using System.Collections.Generic;
using UnityEngine;

namespace Pinwheel.Griffin
{
    [ExecuteInEditMode]
    public class GInteractiveGrassAgent : MonoBehaviour
    {
        [SerializeField]
        private float radius;
        public float Radius
        {
            get
            {
                return radius;
            }
            set
            {
                radius = Mathf.Max(0, value);
            }
        }

        private void Update()
        {
            IEnumerator<GStylizedTerrain> terrains = GStylizedTerrain.ActiveTerrains.GetEnumerator();
            while (terrains.MoveNext())
            {
                GStylizedTerrain t = terrains.Current;
                if (t.TerrainData == null)
                    continue;
                if (t.TerrainData.Foliage.EnableInteractiveGrass == false)
                    continue;
                DrawOnTerrain(t);
            }
        }

        private void DrawOnTerrain(GStylizedTerrain t)
        {
            Vector3[] worldCorners = GCommon.GetBrushQuadCorners(transform.position, Radius, 0);
            Vector2[] uvCorners = new Vector2[worldCorners.Length];
            for (int i = 0; i < uvCorners.Length; ++i)
            {
                uvCorners[i] = t.WorldPointToUV(worldCorners[i]);
            }

            Rect dirtyRect = GUtilities.GetRectContainsPoints(uvCorners);
            if (!dirtyRect.Overlaps(new Rect(0, 0, 1, 1)))
                return;

            RenderTexture rt = t.GetGrassVectorFieldRenderTexture();
            Material mat = GInternalMaterials.InteractiveGrassVectorFieldMaterial;
            mat.SetFloat("_Opacity", t.TerrainData.Foliage.BendSensitive);
            int pass = 0;
            GCommon.DrawQuad(rt, uvCorners, mat, pass);
        }
    }
}
#endif
