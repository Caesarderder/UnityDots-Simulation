#if GRIFFIN
using System.Collections.Generic;
using UnityEngine;
using Type = System.Type;

namespace Pinwheel.Griffin.PaintTool
{
    public class GTreePainter : IGFoliagePainter
    {
        public string Instruction
        {
            get
            {
                string s = string.Format(
                    "Paint tree onto the terrain.\n" +
                    "   - Hold Left Mouse to paint.\n" +
                    "   - Hold Ctrl & Left Mouse to erase instances of the same type.\n" +
                    "   - Hold Shift & Left Mouse to erase all instances in range.");
                return s;
            }
        }

        public string HistoryPrefix
        {
            get
            {
                return "Paint Tree";
            }
        }

        public List<Type> SuitableFilterTypes
        {
            get
            {
                List<Type> types = new List<Type>(new Type[]
                {
                    typeof(GHeightConstraintFilter),
                    typeof(GSlopeConstraintFilter),
                    typeof(GRotationRandomizeFilter),
                    typeof(GScaleRandomizeFilter),
                    typeof(GScaleClampFilter)
                });

                GPaintToolUtilities.AddCustomSpawnFilter(types);
                return types;
            }
        }

        public List<GTerrainResourceFlag> GetResourceFlagForHistory(GFoliagePainterArgs args)
        {
            return GCommon.TreeInstancesResourceFlags;
        }

        public void Paint(Pinwheel.Griffin.GStylizedTerrain terrain, GFoliagePainterArgs args)
        {
            if (args.TreeIndices.Count == 0)
                return;
            if (terrain.TerrainData == null)
                return;
            if (terrain.TerrainData.Foliage.Trees == null)
                return;
            if (args.MouseEventType == GPainterMouseEventType.Up || args.ShouldCommitNow)
            {
                GCommon.SetDirty(terrain.TerrainData.Foliage);
                terrain.UpdateTreesPosition();
                terrain.TerrainData.Foliage.ClearTreeDirtyRegions();
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
            if (args.ActionType == GPainterActionType.Normal)
            {
                HandleSpawnTree(terrain, args);
            }
            else
            {
                HandleEraseTree(terrain, args);
            }

            terrain.TerrainData.Foliage.SetTreeRegionDirty(dirtyRect);
            terrain.TerrainData.SetDirty(GTerrainData.DirtyFlags.Foliage);
            GUtilities.MarkCurrentSceneDirty();
        }

        private void HandleSpawnTree(GStylizedTerrain terrain, GFoliagePainterArgs args)
        {
            int treeIndex = -1;
            Vector3 randomPos = Vector3.zero;
            Vector3 rayOrigin = Vector3.zero;
            Vector3 rayDirection = Vector3.down;
            float sqrtTwo = Mathf.Sqrt(2);
            Ray ray = new Ray();
            RaycastHit samplePoint;
            Vector3 bary0 = Vector3.zero;
            Vector3 bary1 = Vector3.zero;
            Vector2 maskUv = Vector2.zero;
            Vector2 samplePointTexcoord = Vector2.zero;
            Color maskColor = Color.white;
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

            int prototypeCount = terrain.TerrainData.Foliage.Trees.Prototypes.Count;
            List<GTreeInstance> newInstances = new List<GTreeInstance>();
            for (int i = 0; i < args.Density; ++i)
            {
                treeIndex = args.TreeIndices[Random.Range(0, args.TreeIndices.Count)];
                if (treeIndex < 0 || treeIndex >= prototypeCount)
                    continue;
                randomPos = args.HitPoint + Random.insideUnitSphere * args.Radius * sqrtTwo;
                rayOrigin.Set(
                    randomPos.x,
                    10000,
                    randomPos.z);
                ray.origin = rayOrigin;
                ray.direction = rayDirection;
                if (terrain.Raycast(ray, out samplePoint, float.MaxValue))
                {
                    GUtilities.CalculateBarycentricCoord(
                        new Vector2(samplePoint.point.x, samplePoint.point.z),
                        new Vector2(args.WorldPointCorners[0].x, args.WorldPointCorners[0].z),
                        new Vector2(args.WorldPointCorners[1].x, args.WorldPointCorners[1].z),
                        new Vector2(args.WorldPointCorners[2].x, args.WorldPointCorners[2].z),
                        ref bary0);
                    GUtilities.CalculateBarycentricCoord(
                        new Vector2(samplePoint.point.x, samplePoint.point.z),
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

                    //sample mask
                    if (clonedMask != null)
                    {
                        maskColor = clonedMask.GetPixelBilinear(maskUv.x, maskUv.y);
                        if (Random.value > maskColor.grayscale)
                            continue;
                    }

                    //sample terrain mask
                    if (args.EnableTerrainMask)
                    {
                        samplePointTexcoord = samplePoint.textureCoord;
                        maskColor = terrainMask.GetPixelBilinear(samplePointTexcoord.x, samplePointTexcoord.y);
                        if (Random.value < maskColor.r)
                            continue;
                    }

                    //apply filter
                    GSpawnFilterArgs filterArgs = GSpawnFilterArgs.Create();
                    filterArgs.Terrain = terrain;
                    filterArgs.Position = samplePoint.point;
                    filterArgs.SurfaceNormal = samplePoint.normal;
                    filterArgs.SurfaceTexcoord = samplePoint.textureCoord;

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

                    //spawn
                    if (filterArgs.ShouldExclude)
                        continue;

                    GTreeInstance tree = GTreeInstance.Create(treeIndex);
                    tree.Position = terrain.WorldPointToNormalized(filterArgs.Position);
                    tree.Rotation = filterArgs.Rotation;
                    tree.Scale = filterArgs.Scale;
                    newInstances.Add(tree);
                }
            }
            terrain.TerrainData.Foliage.AddTreeInstances(newInstances);
            newInstances.Clear();

            if (clonedMask != null)
                Object.DestroyImmediate(clonedMask);
        }

        private void HandleEraseTree(GStylizedTerrain terrain, GFoliagePainterArgs args)
        {
            int treeIndex = -1;
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

            terrain.TerrainData.Foliage.RemoveTreeInstances(tree =>
            {
                treeIndex = args.TreeIndices[Random.Range(0, args.TreeIndices.Count)];
                localPos.Set(
                    tree.position.x * terrainSize.x,
                    tree.position.y * terrainSize.y,
                    tree.position.z * terrainSize.z);
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
                    return false;
                }

                //sample mask
                if (clonedMask != null)
                {
                    maskColor = clonedMask.GetPixelBilinear(maskUv.x, maskUv.y);
                    if (Random.value > maskColor.grayscale * args.EraseRatio)
                        return false;
                }

                //sample terrain mask
                if (args.EnableTerrainMask)
                {
                    maskColor = terrainMask.GetPixelBilinear(tree.position.x, tree.position.z);
                    if (Random.value < maskColor.r)
                        return false;
                }

                if (args.ActionType == GPainterActionType.Negative &&
                    tree.PrototypeIndex == treeIndex)
                {
                    return true;
                }
                else if (args.ActionType == GPainterActionType.Alternative)
                {
                    return true;
                }

                return false;
            });

            if (clonedMask != null)
                Object.DestroyImmediate(clonedMask);
        }
    }
}
#endif
