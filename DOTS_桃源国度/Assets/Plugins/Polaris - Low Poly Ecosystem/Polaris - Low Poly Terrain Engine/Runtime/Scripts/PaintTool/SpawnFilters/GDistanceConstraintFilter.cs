#if GRIFFIN
//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//using Pinwheel.Griffin.PaintTool;

//namespace Pinwheel.Griffin.PaintTool
//{
//    [GDisplayName("Distance Constraint")]
//    public class GDistanceConstraintFilter : GSpawnFilter
//    {
//        [SerializeField]
//        private bool checkAllPrototype;
//        public bool CheckAllPrototype
//        {
//            get
//            {
//                return checkAllPrototype;
//            }
//            set
//            {
//                checkAllPrototype = value;
//            }
//        }

//        [SerializeField]
//        private float minimumDistance;
//        public float MinimumDistance
//        {
//            get
//            {
//                return minimumDistance;
//            }
//            set
//            {
//                minimumDistance = Mathf.Max(0, value);
//            }
//        }

//        public override void Apply(ref GSpawnFilterArgs args)
//        {
//            float sqrDistance = 0;
//            float sqrMinimumDistance = MinimumDistance * MinimumDistance;
//            Vector3 localPos = Vector3.zero;
//            Vector3 worldPos = Vector3.zero;
//            Vector3 terrainSize = new Vector3(
//                args.Terrain.TerrainData.Geometry.Width,
//                args.Terrain.TerrainData.Geometry.Height,
//                args.Terrain.TerrainData.Geometry.Length);
//            List<GTreeInstance> instances = args.Terrain.TerrainData.Foliage.TreeInstances;
//            for (int i = instances.Count - 1; i >= 0; --i)
//            {
//                if (!CheckAllPrototype &&
//                    instances[i].PrototypeIndex != args.PrototypeIndex)
//                    continue;
//                localPos.Set(
//                    terrainSize.x * instances[i].Position.x,
//                    terrainSize.y * instances[i].Position.y,
//                    terrainSize.z * instances[i].Position.z);
//                worldPos = args.Terrain.transform.TransformPoint(localPos);

//                sqrDistance = (args.Position - worldPos).sqrMagnitude;
//                if (sqrDistance < sqrMinimumDistance)
//                {
//                    args.ShouldExclude = true;
//                    return;
//                }
//            }

//            args.ShouldExclude = false;
//        }

//        private void Reset()
//        {
//            CheckAllPrototype = false;
//            MinimumDistance = 1;
//        }
//    }
//}
#endif
