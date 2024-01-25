#if GRIFFIN
using System.Collections.Generic;
using UnityEngine;
using Type = System.Type;

namespace Pinwheel.Griffin.PaintTool
{
    public class GObjectScaler : IGObjectPainter
    {
        public string Instruction
        {
            get
            {
                return string.Format(
                    "Scale game object instances.\n" +
                    "   - Hold Left Mouse to scale up.\n" +
                    "   - Hold Ctrl & Left Mouse to scale down.");
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

        public void Paint(Pinwheel.Griffin.GStylizedTerrain terrain, GObjectPainterArgs args)
        {
            if (args.MouseEventType == GPainterMouseEventType.Up)
            {
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

            int prototypeIndex = -1;
            Vector2 terrainUv = Vector2.zero;
            Vector3 worldPos = Vector3.zero;
            Vector3 bary0 = Vector3.zero;
            Vector3 bary1 = Vector3.zero;
            Vector2 maskUv = Vector2.zero;
            Color maskColor = Color.white;
            Vector3 scale = Vector3.zero;

            for (int i = 0; i < args.PrototypeIndices.Count; ++i)
            {
                prototypeIndex = args.PrototypeIndices[i];
                if (prototypeIndex < 0 || prototypeIndex >= args.Prototypes.Count)
                    continue;
                GameObject g = args.Prototypes[prototypeIndex];
                if (g == null)
                    continue;
                GameObject root = GSpawner.GetRoot(terrain, g);
                Transform[] instances = GUtilities.GetChildrenTransforms(root.transform);
                int instanceCount = instances.Length;
                for (int j = 0; j < instanceCount; ++j)
                {
                    worldPos = instances[j].position;
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
                    if (args.EnableTerrainMask)
                    {
                        terrainUv = terrain.WorldPointToUV(worldPos);
                        maskColor = terrainMask.GetPixelBilinear(terrainUv.x, terrainUv.y);
                        if (Random.value < maskColor.r)
                            continue;
                    }

                    scale.Set(
                        Mathf.Max(0, instances[j].transform.localScale.x + multiplier * maskColor.grayscale * args.ScaleStrength * GUtilities.DELTA_TIME),
                        Mathf.Max(0, instances[j].transform.localScale.y + multiplier * maskColor.grayscale * args.ScaleStrength * GUtilities.DELTA_TIME),
                        Mathf.Max(0, instances[j].transform.localScale.z + multiplier * maskColor.grayscale * args.ScaleStrength * GUtilities.DELTA_TIME));

                    GSpawnFilterArgs filterArgs = GSpawnFilterArgs.Create();
                    filterArgs.Terrain = terrain;
                    filterArgs.Position = worldPos;
                    filterArgs.Rotation = instances[j].transform.rotation;
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

                    instances[j].transform.localScale = filterArgs.Scale;
                }
            }
        }
    }
}
#endif
