#if GRIFFIN
using System.Collections.Generic;
using UnityEngine;

namespace Pinwheel.Griffin.GroupTool
{
    [System.Serializable]
    [ExecuteInEditMode]
    public class GTerrainGroup : MonoBehaviour
    {
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
        private bool deferredUpdate;
        public bool DeferredUpdate
        {
            get
            {
                return deferredUpdate;
            }
            set
            {
                deferredUpdate = value;
            }
        }

        [SerializeField]
        private GGeometryOverride geometryOverride;
        public GGeometryOverride GeometryOverride
        {
            get
            {
                return geometryOverride;
            }
            set
            {
                geometryOverride = value;
            }
        }

        [SerializeField]
        private GShadingOverride shadingOverride;
        public GShadingOverride ShadingOverride
        {
            get
            {
                return shadingOverride;
            }
            set
            {
                shadingOverride = value;
            }
        }

        [SerializeField]
        private GRenderingOverride renderingOverride;
        public GRenderingOverride RenderingOverride
        {
            get
            {
                return renderingOverride;
            }
            set
            {
                renderingOverride = value;
            }
        }

        [SerializeField]
        private GFoliageOverride foliageOverride;
        public GFoliageOverride FoliageOverride
        {
            get
            {
                return foliageOverride;
            }
            set
            {
                foliageOverride = value;
            }
        }

        [SerializeField]
        private GMaskOverride maskOverride;
        public GMaskOverride MaskOverride
        {
            get
            {
                return maskOverride;
            }
            set
            {
                maskOverride = value;
            }
        }

        private void Reset()
        {
            geometryOverride.Reset();
            shadingOverride.Reset();
            renderingOverride.Reset();
            foliageOverride.Reset();
            maskOverride.Reset();
        }

        public void ResetGeometry()
        {
            geometryOverride.Reset();
        }

        public void ResetShading()
        {
            shadingOverride.Reset();
        }

        public void ResetRendering()
        {
            renderingOverride.Reset();
        }

        public void ResetFoliage()
        {
            foliageOverride.Reset();
        }

        public void ResetMask()
        {
            maskOverride.Reset();
        }

        public void OverrideGeometry()
        {
            IEnumerator<GStylizedTerrain> terrains = GStylizedTerrain.ActiveTerrains.GetEnumerator();
            while (terrains.MoveNext())
            {
                if (terrains.Current.TerrainData != null &&
                    (terrains.Current.GroupId == GroupId || GroupId < 0))
                {
                    GeometryOverride.Override(terrains.Current.TerrainData.Geometry);
                    terrains.Current.TerrainData.Geometry.SetRegionDirty(new Rect(0, 0, 1, 1));
                    terrains.Current.TerrainData.SetDirty(GTerrainData.DirtyFlags.GeometryTimeSliced);
                }
            }
            GStylizedTerrain.MatchEdges(GroupId);
        }

        public void OverrideShading()
        {
            IEnumerator<GStylizedTerrain> terrains = GStylizedTerrain.ActiveTerrains.GetEnumerator();
            while (terrains.MoveNext())
            {
                if (terrains.Current.TerrainData != null &&
                    (terrains.Current.GroupId == GroupId || GroupId < 0))
                {
                    ShadingOverride.Override(terrains.Current.TerrainData.Shading);
                    terrains.Current.TerrainData.SetDirty(GTerrainData.DirtyFlags.Shading);
                }
            }
        }

        public void OverrideRendering()
        {
            IEnumerator<GStylizedTerrain> terrains = GStylizedTerrain.ActiveTerrains.GetEnumerator();
            while (terrains.MoveNext())
            {
                if (terrains.Current.TerrainData != null &&
                    (terrains.Current.GroupId == GroupId || GroupId < 0))
                {
                    RenderingOverride.Override(terrains.Current.TerrainData.Rendering);
                    terrains.Current.TerrainData.SetDirty(GTerrainData.DirtyFlags.Rendering);
                }
            }
        }

        public void OverrideFoliage()
        {
            IEnumerator<GStylizedTerrain> terrains = GStylizedTerrain.ActiveTerrains.GetEnumerator();
            while (terrains.MoveNext())
            {
                if (terrains.Current.TerrainData != null &&
                    (terrains.Current.GroupId == GroupId || GroupId < 0))
                {
                    FoliageOverride.Override(terrains.Current.TerrainData.Foliage);
                    terrains.Current.TerrainData.SetDirty(GTerrainData.DirtyFlags.Foliage);
                }
            }
        }

        public void OverrideMask()
        {
            IEnumerator<GStylizedTerrain> terrains = GStylizedTerrain.ActiveTerrains.GetEnumerator();
            while (terrains.MoveNext())
            {
                if (terrains.Current.TerrainData != null &&
                    (terrains.Current.GroupId == GroupId || GroupId < 0))
                {
                    MaskOverride.Override(terrains.Current.TerrainData.Mask);
                    terrains.Current.TerrainData.SetDirty(GTerrainData.DirtyFlags.Mask);
                }
            }
        }

        public void ReArrange()
        {
            IEnumerator<GStylizedTerrain> terrains = GStylizedTerrain.ActiveTerrains.GetEnumerator();
            while (terrains.MoveNext())
            {
                if (terrains.Current.TerrainData != null &&
                    (terrains.Current.GroupId == GroupId || GroupId < 0))
                {
                    GStylizedTerrain t = terrains.Current;
                    if (t.TopNeighbor != null)
                    {
                        t.transform.rotation = Quaternion.identity;
                        t.transform.localScale = Vector3.one;
                        t.TopNeighbor.transform.position = t.transform.position + Vector3.forward * t.TerrainData.Geometry.Length;
                        t.TopNeighbor.transform.rotation = Quaternion.identity;
                        t.TopNeighbor.transform.localScale = Vector3.one;
                    }
                    if (t.BottomNeighbor != null)
                    {
                        t.transform.rotation = Quaternion.identity;
                        t.transform.localScale = Vector3.one;
                        t.BottomNeighbor.transform.position = t.transform.position + Vector3.back * t.TerrainData.Geometry.Length;
                        t.BottomNeighbor.transform.rotation = Quaternion.identity;
                        t.BottomNeighbor.transform.localScale = Vector3.one;
                    }
                    if (t.LeftNeighbor != null)
                    {
                        t.transform.rotation = Quaternion.identity;
                        t.transform.localScale = Vector3.one;
                        t.LeftNeighbor.transform.position = t.transform.position + Vector3.left * t.TerrainData.Geometry.Width;
                        t.LeftNeighbor.transform.rotation = Quaternion.identity;
                        t.LeftNeighbor.transform.localScale = Vector3.one;
                    }
                    if (t.RightNeighbor != null)
                    {
                        t.transform.rotation = Quaternion.identity;
                        t.transform.localScale = Vector3.one;
                        t.RightNeighbor.transform.position = t.transform.position + Vector3.right * t.TerrainData.Geometry.Width;
                        t.RightNeighbor.transform.rotation = Quaternion.identity;
                        t.RightNeighbor.transform.localScale = Vector3.one;
                    }
                }
            }
        }
    }
}
#endif
