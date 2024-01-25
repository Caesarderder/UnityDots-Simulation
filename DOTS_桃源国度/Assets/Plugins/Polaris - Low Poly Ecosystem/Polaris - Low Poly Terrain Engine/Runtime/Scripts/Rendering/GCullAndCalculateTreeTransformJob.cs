#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

namespace Pinwheel.Griffin.Rendering
{
#if GRIFFIN_BURST
    [BurstCompile(CompileSynchronously = true)]
#endif
    public struct GCullAndCalculateTreeTransformJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<GTreeInstance> instances;
        [ReadOnly]
        public NativeArray<float> prototypePivotOffset;
        [ReadOnly]
        public NativeArray<Quaternion> prototypeBaseRotation;
        [ReadOnly]
        public NativeArray<Vector3> prototypeBaseScale;
        [ReadOnly]
        public NativeArray<BoundingSphere> prototypeBounds;
        [ReadOnly]
        public NativeArray<bool> prototypeWillDoFrustumTest;
        [ReadOnly]
        public NativeArray<Plane> frustum;

        public NativeArray<int> prototypeIndices;
        [WriteOnly]
        public NativeArray<Matrix4x4> transforms;
        [WriteOnly]
        public NativeArray<byte> cullResult;

        public Vector3 cullBoxMin;
        public Vector3 cullBoxMax;
        public byte flagCulled;
        public byte flagVisible;
        public byte flagBillboard;

        public Vector3 terrainPos;
        public Vector3 terrainSize;

        public Vector3 cameraPos;
        public float treeDistance;
        public float billboardStart;
        public float cullVolumeBias;

        public void Execute(int index)
        {
            GTreeInstance tree = instances[index];
            if (tree.prototypeIndex < 0 || tree.prototypeIndex >= prototypePivotOffset.Length)
            {
                cullResult[index] = flagCulled;
                return;
            }

            if (tree.position.x < cullBoxMin.x || tree.position.x > cullBoxMax.x ||
                tree.position.y < cullBoxMin.y || tree.position.y > cullBoxMax.y ||
                tree.position.z < cullBoxMin.z || tree.position.z > cullBoxMax.z)
            {
                cullResult[index] = flagCulled;
                return;
            }

            float pivotOffset = prototypePivotOffset[tree.prototypeIndex];
            Vector3 worldPos = new Vector3(
                tree.position.x * terrainSize.x + terrainPos.x,
                tree.position.y * terrainSize.y + terrainPos.y + pivotOffset,
                tree.position.z * terrainSize.z + terrainPos.z);

            float sqrDistance = Vector3.SqrMagnitude(worldPos - cameraPos);
            float sqrTreeDistance = treeDistance * treeDistance;
            if (sqrDistance > sqrTreeDistance)
            {
                cullResult[index] = flagCulled;
                return;
            }

            Vector3 baseScale = prototypeBaseScale[tree.prototypeIndex];
            Vector3 worldScale = new Vector3(
                tree.scale.x * baseScale.x,
                tree.scale.y * baseScale.y,
                tree.scale.z * baseScale.z);

            bool testFrustum = prototypeWillDoFrustumTest[tree.prototypeIndex];
            if (testFrustum)
            {
                BoundingSphere b = prototypeBounds[tree.prototypeIndex];
                b.position = worldPos;
                b.radius *= Mathf.Max(worldScale.x, Mathf.Max(worldScale.y, worldScale.z));
                b.radius += cullVolumeBias;
                if (!DoFrustumTest(frustum, b))
                {
                    cullResult[index] = flagCulled;
                    return;
                }
            }

            float sqrBillboardStart = billboardStart * billboardStart;
            if (sqrDistance >= sqrBillboardStart)
            {
                cullResult[index] = flagBillboard;
            }
            else
            {
                cullResult[index] = flagVisible;
            }

            if (prototypeIndices[index] < 0)
            {
                Quaternion baseRotation = prototypeBaseRotation[tree.prototypeIndex];
                Quaternion worldRotation = tree.rotation * baseRotation;

                Matrix4x4 matrix = Matrix4x4.TRS(worldPos, worldRotation, worldScale);
                transforms[index] = matrix;
                prototypeIndices[index] = tree.prototypeIndex;
            }
        }

        private bool DoFrustumTest(NativeArray<Plane> frustum, BoundingSphere bounds)
        {
            float d = 0;
            for (int i = 0; i < 6; ++i)
            {
                d = frustum[i].GetDistanceToPoint(bounds.position);
                if (d < -bounds.radius)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
#endif
