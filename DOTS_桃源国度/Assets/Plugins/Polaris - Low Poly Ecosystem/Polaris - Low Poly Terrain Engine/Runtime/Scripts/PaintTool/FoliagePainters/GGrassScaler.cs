#if GRIFFIN
using System.Collections.Generic;
using UnityEngine;
using Type = System.Type;

namespace Pinwheel.Griffin.PaintTool
{
    public class GGrassScaler : IGFoliagePainter
    {
        public string Instruction
        {
            get
            {
                string s = string.Format(
                    "Scale grass instances.\n" +
                    "   - Hold Left Mouse to scale up.\n" +
                    "   - Hold Ctrl & Left Mouse to scale down.");
                return s;
            }
        }

        public string HistoryPrefix
        {
            get
            {
                return "Scale Grass";
            }
        }

        public List<Type> SuitableFilterTypes
        {
            get
            {
                List<Type> types = new List<Type>(new Type[]
                {
                    typeof(GScaleClampFilter)
                });
                GPaintToolUtilities.AddCustomSpawnFilter(types);
                return types;
            }
        }


        public List<GTerrainResourceFlag> GetResourceFlagForHistory(GFoliagePainterArgs args)
        {
            return GCommon.GrassInstancesResourceFlags;
        }

        public void Paint(Pinwheel.Griffin.GStylizedTerrain terrain, GFoliagePainterArgs args)
        {
            if (args.GrassIndices.Count == 0)
                return;
            if (terrain.TerrainData == null)
                return;
            if (terrain.TerrainData.Foliage.Grasses == null)
                return;
            if (args.MouseEventType == GPainterMouseEventType.Up || args.ShouldCommitNow)
            {
                terrain.TerrainData.SetDirty(GTerrainData.DirtyFlags.Foliage);
                GRuntimeSettings.Instance.isEditingFoliage = false;
                return;
            }

            Vector2[] uvCorners = new Vector2[args.WorldPointCorners.Length];
            for (int i = 0; i < uvCorners.Length; ++i)
            {
                uvCorners[i] = terrain.WorldPointToUV(args.WorldPointCorners[i]);
            }

            Rect dirtyRect = GUtilities.GetRectContainsPoints(uvCorners);
            if (!dirtyRect.Overlaps(new Rect(0, 0, 1, 1)))
                return;

            GRuntimeSettings.Instance.isEditingFoliage = true;
            Texture2D clonedMask = null;
            Texture2D terrainMask = null;
            if (args.Mask != null)
            {
                clonedMask = GCommon.CloneAndResizeTexture(args.Mask, 256, 256);
            }
            if (args.EnableTerrainMask)
            {
                terrainMask = terrain.TerrainData.Mask.MaskMapOrDefault;
            }

            int multiplier = args.ActionType == GPainterActionType.Normal ? 1 : -1;
            int grassIndex = -1;
            Vector3 terrainSize = new Vector3(
                terrain.TerrainData.Geometry.Width,
                terrain.TerrainData.Geometry.Height,
                terrain.TerrainData.Geometry.Length);
            Vector3 localPos = Vector3.zero;
            Vector3 worldPos = Vector3.zero;
            Vector3 bary0 = Vector3.zero;
            Vector3 bary1 = Vector3.zero;
            Vector2 maskUv = Vector2.zero;
            Color maskColor = Color.white;
            Vector3 scale = Vector3.zero;

            GGrassPatch[] patches = terrain.TerrainData.Foliage.GrassPatches;
            for (int p = 0; p < patches.Length; ++p)
            {
                if (!patches[p].GetUvRange().Overlaps(dirtyRect))
                    continue;

                List<GGrassInstance> instances = patches[p].Instances;
                int instanceCount = instances.Count;
                for (int i = 0; i < instanceCount; ++i)
                {
                    grassIndex = args.GrassIndices[Random.Range(0, args.GrassIndices.Count)];
                    GGrassInstance grass = instances[i];
                    if (grass.PrototypeIndex != grassIndex)
                        continue;

                    localPos.Set(
                        grass.position.x * terrainSize.x,
                        grass.position.y * terrainSize.y,
                        grass.position.z * terrainSize.z);
                    worldPos = terrain.transform.TransformPoint(localPos);
                    GUtilities.CalculateBarycentricCoord(
                            new Vector2(worldPos.x, worldPos.z),
                            new Vector2(args.WorldPointCorners[0].x, args.WorldPointCorners[0].z),
                            new Vector2(args.WorldPointCorners[1].x, args.WorldPointCorners[1].z),
                            new Vector2(args.WorldPointCorners[2].x, args.WorldPointCorners[2].z),
                            ref bary0);
                    GUtilities.CalculateBarycentricCoord(
                        new Vector2(worldPos.x, worldPos.z),
                        new Vector2(args.WorldPointCorners[0].x, args.WorldPointCorners[0].z),
                        new Vector2(args.WorldPointCorners[2].x, args.WorldPointCorners[2].z),
                        new Vector2(args.WorldPointCorners[3].x, args.WorldPointCorners[3].z),
                        ref bary1);
                    if (bary0.x >= 0 && bary0.y >= 0 && bary0.z >= 0)
                    {
                        maskUv = bary0.x * Vector2.zero + bary0.y * Vector2.up + bary0.z * Vector2.one;
                    }
                    else if (bary1.x >= 0 && bary1.y >= 0 && bary1.z >= 0)
                    {
                        maskUv = bary1.x * Vector2.zero + bary1.y * Vector2.one + bary1.z * Vector2.right;
                    }
                    else
                    {
                        continue;
                    }

                    if (clonedMask != null)
                    {
                        maskColor = clonedMask.GetPixelBilinear(maskUv.x, maskUv.y);
                        if (Random.value > maskColor.grayscale)
                            continue;
                    }
                    //sample terrain mask
                    if (args.EnableTerrainMask)
                    {
                        maskColor = terrainMask.GetPixelBilinear(grass.position.x, grass.position.z);
                        if (Random.value < maskColor.r)
                            continue;
                    }

                    scale.Set(
                        Mathf.Max(0, grass.scale.x + multiplier * maskColor.grayscale * args.ScaleStrength * GUtilities.DELTA_TIME),
                        Mathf.Max(0, grass.scale.y + multiplier * maskColor.grayscale * args.ScaleStrength * GUtilities.DELTA_TIME),
                        Mathf.Max(0, grass.scale.z + multiplier * maskColor.grayscale * args.ScaleStrength * GUtilities.DELTA_TIME));

                    GSpawnFilterArgs filterArgs = GSpawnFilterArgs.Create();
                    filterArgs.Terrain = terrain;
                    filterArgs.Position = worldPos;
                    filterArgs.Rotation = grass.Rotation;
                    filterArgs.Scale = scale;
                    List<Type> suitableFilter = SuitableFilterTypes;
                    if (args.Filters != null)
                    {
                        for (int fIndex = 0; fIndex < args.Filters.Length; ++fIndex)
                        {
                            if (args.Filters[fIndex] != null &&
                                args.Filters[fIndex].Ignore != true)
                            {
                                if (suitableFilter.Contains(args.Filters[fIndex].GetType()))
                                    args.Filters[fIndex].Apply(ref filterArgs);
                            }
                            if (filterArgs.ShouldExclude)
                                break;
                        }
                    }

                    grass.scale = filterArgs.Scale;
                    instances[i] = grass;
                }
                patches[p].Changed();
            }

            terrain.TerrainData.Foliage.SetGrassRegionDirty(dirtyRect);
            GUtilities.MarkCurrentSceneDirty();
        }
    }
}
#endif
