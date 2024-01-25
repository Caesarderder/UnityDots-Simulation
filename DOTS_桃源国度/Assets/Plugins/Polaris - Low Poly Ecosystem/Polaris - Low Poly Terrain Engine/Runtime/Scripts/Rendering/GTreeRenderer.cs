#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Rendering;

namespace Pinwheel.Griffin.Rendering
{
    public class GTreeRenderer
    {
        public struct PrototypeCache
        {
            public bool validation;
            public bool canDrawInstanced;
            public bool canDrawBillboardInstanced;
            public int subMeshCount;
            public Vector4[] billboardImageTexcoords;
            public Mesh billboardMesh; //global cached, disposed in GRuntimeSettings
        }

        public static Dictionary<GStylizedTerrain, GRenderingVisualization> vis;

        private GStylizedTerrain terrain;
        private GFoliage foliage;

        private Camera camera;
        private Plane[] frustum;
        private Vector3[] nearFrustumCorners;
        private Vector3[] farFrustumCorners;
        private Vector3 cullBoxMin;
        private Vector3 cullBoxMax;

        private const byte CULLED = 0;
        private const byte VISIBLE = 1;
        private const byte BILLBOARD = 2;

        private Matrix4x4 normalizedToLocalMatrix;
        private Matrix4x4 localToWorldMatrix;
        private Matrix4x4 normalizedToWorldMatrix;

        private float treeDistance;
        private float billboardStart;
        private float cullVolumeBias;
        private Vector3 terrainPosition;
        private Vector3 terrainSize;

        private List<GTreePrototype> prototypes;
        private PrototypeCache[] prototypeCache;

        private bool enableInstancing;
        private Matrix4x4[] batchContainer;
        private int batchInstanceCount;
        private Matrix4x4[] billboardBatchContainer;
        private int billboardBatchInstanceCount;

        private int[] instancePrototypeIndices;
        private Matrix4x4[] instanceTransforms;
        private byte[] instanceCullResults;

        private const int BATCH_MAX_INSTANCE_COUNT = 500;
        private bool isWarningLogged;

        private const string BILLBOARD_IMAGE_TEXCOORDS = "_ImageTexcoords";
        private const string BILLBOARD_IMAGE_COUNT = "_ImageCount";

        private GTreeNativeData nativeData;
        private NativeArray<Plane> frustumPlanes;
        private NativeArray<float> prototypePivotOffset;
        private NativeArray<Quaternion> prototypeBaseRotation;
        private NativeArray<Vector3> prototypeBaseScale;
        private NativeArray<BoundingSphere> prototypeBounds;
        private NativeArray<bool> prototypeWillDoFrustumTest;

        public GTreeRenderer(GStylizedTerrain terrain)
        {
            this.terrain = terrain;
        }

        public void Render(Camera cam)
        {
            try
            {
                if (GRuntimeSettings.Instance.isEditingGeometry)
                    return;

                InitFrame(cam);
                if (CullTerrain())
                    return;
                CalculateQuickInstancesCullBox();
                CreateCommonJobData();
                CullAndCalculateTreeTransform();
                CopyInstanceNativeData();
                if (enableInstancing)
                {
                    DrawInstanced();
                }
                else
                {
                    Draw();
                }
            }
            catch (GSkipFrameException) { }

            CleanUpFrame();
        }

        private void InitFrame(Camera cam)
        {
            foliage = terrain.TerrainData.Foliage;

            terrainPosition = terrain.transform.position;
            terrainSize = terrain.TerrainData.Geometry.Size;
            treeDistance = terrain.TerrainData.Rendering.TreeDistance;
            billboardStart = terrain.TerrainData.Rendering.BillboardStart;
            cullVolumeBias = GRuntimeSettings.Instance.renderingDefault.treeCullBias;

            if (terrain.TerrainData.Foliage.Trees != null)
            {
                prototypes = terrain.TerrainData.Foliage.Trees.Prototypes;
            }
            else
            {
                prototypes = new List<GTreePrototype>();
            }

            if (prototypeCache == null || prototypeCache.Length!=prototypes.Count)
            {
                prototypeCache = new PrototypeCache[prototypes.Count];
            }

            for (int i = 0; i < prototypes.Count; ++i)
            {
                GTreePrototype p = prototypes[i];
                PrototypeCache cache = prototypeCache[i];

                bool valid = prototypes[i].IsValid;
                cache.validation = valid;
                if (valid)
                {
                    cache.subMeshCount = p.sharedMesh.subMeshCount;
                    cache.canDrawInstanced = IsInstancingEnabledForAllMaterials(p);
                    cache.canDrawBillboardInstanced =
                        p.billboard != null &&
                        p.billboard.material != null &&
                        p.billboard.material.enableInstancing;
                }
                if (p.billboard != null)
                {
                    cache.billboardMesh = GBillboardUtilities.GetMesh(p.billboard);
                }
                if (p.billboard != null && p.billboard.material != null)
                {
                    if (cache.billboardImageTexcoords == null ||
                        cache.billboardImageTexcoords.Length != p.billboard.imageCount)
                    {
                        cache.billboardImageTexcoords = p.billboard.GetImageTexCoords();
                    }
                    Material mat = p.billboard.material;
                    mat.SetVectorArray(BILLBOARD_IMAGE_TEXCOORDS, cache.billboardImageTexcoords);
                    mat.SetInt(BILLBOARD_IMAGE_COUNT, p.billboard.imageCount);
                }

                prototypeCache[i] = cache;
            }

            enableInstancing = terrain.TerrainData.Rendering.EnableInstancing && SystemInfo.supportsInstancing;

            normalizedToLocalMatrix = Matrix4x4.Scale(terrainSize);
            localToWorldMatrix = terrain.transform.localToWorldMatrix;
            normalizedToWorldMatrix = localToWorldMatrix * normalizedToLocalMatrix;

            camera = cam;
            if (frustum == null)
            {
                frustum = new Plane[6];
            }
            GFrustumUtilities.Calculate(camera, frustum, treeDistance);
            if (nearFrustumCorners == null)
            {
                nearFrustumCorners = new Vector3[4];
            }
            if (farFrustumCorners == null)
            {
                farFrustumCorners = new Vector3[4];
            }

            if (batchContainer == null)
            {
                batchContainer = new Matrix4x4[BATCH_MAX_INSTANCE_COUNT];
            }
            if (billboardBatchContainer == null)
            {
                billboardBatchContainer = new Matrix4x4[BATCH_MAX_INSTANCE_COUNT];
            }

            if (!isWarningLogged)
            {
                for (int i = 0; i < prototypes.Count; ++i)
                {
                    if (!prototypes[i].IsValid)
                    {
                        string msg = string.Format(
                            "Tree prototye {0}: " +
                            "The prototype is not valid, make sure you've assigned a prefab with correct mesh and materials setup.",
                            i);
                        Debug.LogWarning(msg);
                    }
                    if (enableInstancing && prototypes[i].IsValid)
                    {
                        if (!IsInstancingEnabledForAllMaterials(prototypes[i]))
                        {
                            string msg = string.Format(
                                "Tree prototype {0} ({1}): " +
                                "Instancing need to be enabled for all materials for the renderer to work at its best. " +
                                "Otherwise it will fallback to non-instanced for this prototype.",
                                i, prototypes[i].Prefab.name);
                            Debug.LogWarning(msg);
                        }
                        if (prototypes[i].billboard != null &&
                            prototypes[i].billboard.material != null &&
                            prototypes[i].billboard.material.enableInstancing == false)
                        {
                            string msg = string.Format(
                                "Tree prototype {0} ({1}): " +
                                "Instancing need to be enabled for billboard material for the renderer to work at its best. " +
                                "Otherwise it will fallback to non-instanced for this prototype when render billboards.",
                                i, prototypes[i].Prefab.name);
                            Debug.LogWarning(msg);
                        }
                    }
                }

                isWarningLogged = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>True if the terrain is culled</returns>
        private bool CullTerrain()
        {
            bool prototypeCountTest = prototypes.Count > 0;
            if (!prototypeCountTest)
                return true;

            bool nonZeroDistanceTest = terrain.TerrainData.Rendering.TreeDistance > 0;
            if (!nonZeroDistanceTest)
                return true;

            bool instanceCountTest = terrain.TerrainData.Foliage.TreeInstances.Count > 0;
            if (!instanceCountTest)
                return true;

            bool frustumTest = GeometryUtility.TestPlanesAABB(frustum, terrain.Bounds);
            if (!frustumTest)
                return true;

            return false;
        }

        private void CalculateQuickInstancesCullBox()
        {
            camera.CalculateFrustumCorners(GCommon.UnitRect, camera.nearClipPlane, Camera.MonoOrStereoscopicEye.Mono, nearFrustumCorners);
            camera.CalculateFrustumCorners(GCommon.UnitRect, treeDistance, Camera.MonoOrStereoscopicEye.Mono, farFrustumCorners);

            for (int i = 0; i < 4; ++i)
            {
                nearFrustumCorners[i] = camera.transform.TransformPoint(nearFrustumCorners[i]);
                farFrustumCorners[i] = camera.transform.TransformPoint(farFrustumCorners[i]);
            }

            cullBoxMin = Vector3.zero;
            cullBoxMax = Vector3.zero;

            cullBoxMin.x = Mathf.Min(
                nearFrustumCorners[0].x, nearFrustumCorners[1].x, nearFrustumCorners[2].x, nearFrustumCorners[3].x,
                farFrustumCorners[0].x, farFrustumCorners[1].x, farFrustumCorners[2].x, farFrustumCorners[3].x);
            cullBoxMin.y = Mathf.Min(
               nearFrustumCorners[0].y, nearFrustumCorners[1].y, nearFrustumCorners[2].y, nearFrustumCorners[3].y,
               farFrustumCorners[0].y, farFrustumCorners[1].y, farFrustumCorners[2].y, farFrustumCorners[3].y);
            cullBoxMin.z = Mathf.Min(
               nearFrustumCorners[0].z, nearFrustumCorners[1].z, nearFrustumCorners[2].z, nearFrustumCorners[3].z,
               farFrustumCorners[0].z, farFrustumCorners[1].z, farFrustumCorners[2].z, farFrustumCorners[3].z);

            cullBoxMax.x = Mathf.Max(
                nearFrustumCorners[0].x, nearFrustumCorners[1].x, nearFrustumCorners[2].x, nearFrustumCorners[3].x,
                farFrustumCorners[0].x, farFrustumCorners[1].x, farFrustumCorners[2].x, farFrustumCorners[3].x);
            cullBoxMax.y = Mathf.Max(
                nearFrustumCorners[0].y, nearFrustumCorners[1].y, nearFrustumCorners[2].y, nearFrustumCorners[3].y,
                farFrustumCorners[0].y, farFrustumCorners[1].y, farFrustumCorners[2].y, farFrustumCorners[3].y);
            cullBoxMax.z = Mathf.Max(
               nearFrustumCorners[0].z, nearFrustumCorners[1].z, nearFrustumCorners[2].z, nearFrustumCorners[3].z,
               farFrustumCorners[0].z, farFrustumCorners[1].z, farFrustumCorners[2].z, farFrustumCorners[3].z);

            cullBoxMin -= Vector3.one * cullVolumeBias;
            cullBoxMax += Vector3.one * cullVolumeBias;

            cullBoxMin = terrain.WorldPointToNormalized(cullBoxMin);
            cullBoxMax = terrain.WorldPointToNormalized(cullBoxMax);
        }

        private void CreateCommonJobData()
        {
            frustumPlanes = new NativeArray<Plane>(frustum, Allocator.TempJob);
            prototypePivotOffset = new NativeArray<float>(prototypes.Count, Allocator.TempJob);
            prototypeBaseRotation = new NativeArray<Quaternion>(prototypes.Count, Allocator.TempJob);
            prototypeBaseScale = new NativeArray<Vector3>(prototypes.Count, Allocator.TempJob);
            prototypeBounds = new NativeArray<BoundingSphere>(prototypes.Count, Allocator.TempJob);
            prototypeWillDoFrustumTest = new NativeArray<bool>(prototypes.Count, Allocator.TempJob);
        }

        private void CullAndCalculateTreeTransform()
        {
            if (nativeData == null)
            {
                nativeData = new GTreeNativeData(terrain.TerrainData.Foliage.TreeInstances);
            }

            bool willSkipFrame = false;


            try
            {
                for (int i = 0; i < prototypes.Count; ++i)
                {
                    prototypePivotOffset[i] = prototypes[i].PivotOffset;
                    prototypeBaseRotation[i] = prototypes[i].BaseRotation;
                    prototypeBaseScale[i] = prototypes[i].BaseScale;
                    prototypeBounds[i] = prototypes[i].GetBoundingSphere();
                    prototypeWillDoFrustumTest[i] = IsInstancingEnabledForAllMaterials(prototypes[i]);
                }

                GCullAndCalculateTreeTransformJob job = new GCullAndCalculateTreeTransformJob()
                {
                    instances = nativeData.instances,
                    prototypeIndices = nativeData.prototypeIndices,
                    transforms = nativeData.trs,
                    prototypePivotOffset = prototypePivotOffset,
                    prototypeBaseRotation = prototypeBaseRotation,
                    prototypeBaseScale = prototypeBaseScale,
                    cullResult = nativeData.cullResults,
                    cullBoxMin = cullBoxMin,
                    cullBoxMax = cullBoxMax,
                    flagCulled = CULLED,
                    flagVisible = VISIBLE,
                    flagBillboard = BILLBOARD,
                    terrainPos = terrainPosition,
                    terrainSize = terrainSize,
                    cameraPos = camera.transform.position,
                    treeDistance = treeDistance,
                    billboardStart = billboardStart,
                    cullVolumeBias = cullVolumeBias,
                    prototypeBounds = prototypeBounds,
                    prototypeWillDoFrustumTest = prototypeWillDoFrustumTest,
                    frustum = frustumPlanes
                };

                JobHandle handle = job.Schedule(nativeData.instances.Length, 100);
                handle.Complete();
            }
            catch (System.InvalidOperationException)
            {
                foliage.TreeAllChanged();
                willSkipFrame = true;
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }

            prototypePivotOffset.Dispose();
            prototypeBaseRotation.Dispose();
            prototypeBaseScale.Dispose();
            prototypeBounds.Dispose();
            prototypeWillDoFrustumTest.Dispose();
            frustumPlanes.Dispose();

            if (willSkipFrame)
            {
                throw new GSkipFrameException();
            }
        }

        private bool IsInstancingEnabledForAllMaterials(GTreePrototype p)
        {
            if (!enableInstancing)
                return false;
            if (p.SharedMaterials == null || p.SharedMaterials.Length == 0)
                return false;
            for (int i = 0; i < p.SharedMaterials.Length; ++i)
            {
                if (!p.SharedMaterials[i].enableInstancing)
                {
                    return false;
                }
            }

            return true;
        }

        private void CopyInstanceNativeData()
        {
            if (instancePrototypeIndices == null || instancePrototypeIndices.Length != nativeData.prototypeIndices.Length)
            {
                instancePrototypeIndices = new int[nativeData.prototypeIndices.Length];
            }
            if (instanceTransforms == null || instanceTransforms.Length != nativeData.trs.Length)
            {
                instanceTransforms = new Matrix4x4[nativeData.trs.Length];
            }
            if (instanceCullResults == null || instanceCullResults.Length != nativeData.cullResults.Length)
            {
                instanceCullResults = new byte[nativeData.cullResults.Length];
            }

            nativeData.prototypeIndices.CopyTo(instancePrototypeIndices);
            nativeData.trs.CopyTo(instanceTransforms);
            nativeData.cullResults.CopyTo(instanceCullResults);
        }

        private void Draw()
        {
            GTreePrototype proto;
            Mesh mesh;
            Material[] materials;
            Material billboardMaterial;
            int protoIndex = 0;
            int submeshCount = 0;
            int drawCount = 0;
            Matrix4x4 trs;

            int count = instancePrototypeIndices.Length;
            for (int i = 0; i < count; ++i)
            {
                if (instanceCullResults[i] == CULLED)
                    continue;

                protoIndex = instancePrototypeIndices[i];
                if (prototypeCache[protoIndex].validation == false)
                    continue;

                proto = prototypes[protoIndex];
                trs = instanceTransforms[i];

                if (instanceCullResults[i] == VISIBLE)
                {
                    mesh = proto.sharedMesh;
                    materials = proto.sharedMaterials;
                    submeshCount = prototypeCache[protoIndex].subMeshCount;
                    drawCount = Mathf.Min(materials.Length, submeshCount);
                    for (int d = 0; d < drawCount; ++d)
                    {
                        Graphics.DrawMesh(
                            mesh,
                            trs,
                            materials[d],
                            proto.layer,
                            camera,
                            d,
                            null,
                            proto.shadowCastingMode,
                            proto.receiveShadow,
                            null,
                            LightProbeUsage.BlendProbes,
                            null);
                    }
                }
                else
                {
                    if (proto.billboard == null)
                        continue;
                    mesh = prototypeCache[protoIndex].billboardMesh;
                    billboardMaterial = proto.billboard.material;
                    Graphics.DrawMesh(
                        mesh,
                        trs,
                        billboardMaterial,
                        proto.layer,
                        camera,
                        0,
                        null,
                        proto.billboardShadowCastingMode, 
                        proto.BillboardReceiveShadow,
                        null,
                        LightProbeUsage.BlendProbes,
                        null);
                }
            }
        }

        private void DrawInstanced()
        {
            for (int i = 0; i < prototypes.Count; ++i)
            {
                if (prototypeCache[i].validation == false)
                    continue;
                DrawInstanced(i);
            }
        }

        private void DrawInstanced(int prototypeIndex)
        {
            GTreePrototype proto = prototypes[prototypeIndex];
            Mesh mesh = proto.sharedMesh;
            Material[] materials = proto.sharedMaterials;
            int submeshCount = prototypeCache[prototypeIndex].subMeshCount;
            int drawCount = Mathf.Min(submeshCount, materials.Length);

            Mesh billboardMesh = null;
            Material billboardMaterial = null;
            BillboardAsset billboard = proto.billboard;
            if (billboard != null)
            {
                billboardMesh = GBillboardUtilities.GetMesh(billboard);
                billboardMaterial = billboard.material;
            }

            bool canDrawInstanced = prototypeCache[prototypeIndex].canDrawInstanced;
            bool canDrawBillboardInstanced = prototypeCache[prototypeIndex].canDrawBillboardInstanced;

            batchInstanceCount = 0;
            billboardBatchInstanceCount = 0;
            int count = instancePrototypeIndices.Length;
            for (int i = 0; i <= count; ++i)
            {
                if (i == count || batchInstanceCount == BATCH_MAX_INSTANCE_COUNT)
                {
                    if (canDrawInstanced)
                    {
                        for (int d = 0; d < drawCount; ++d)
                        {
                            Graphics.DrawMeshInstanced(
                                mesh, d, materials[d],
                                batchContainer, batchInstanceCount, null,
                                proto.shadowCastingMode, proto.receiveShadow, proto.layer,
                                camera, LightProbeUsage.BlendProbes);
                        }
                        batchInstanceCount = 0;
                    }
                }

                if (i == count || billboardBatchInstanceCount == BATCH_MAX_INSTANCE_COUNT)
                {
                    if (billboard != null && canDrawBillboardInstanced)
                    {
                        Graphics.DrawMeshInstanced(
                            billboardMesh, 0, billboardMaterial,
                            billboardBatchContainer, billboardBatchInstanceCount, null,
                            proto.billboardShadowCastingMode, proto.BillboardReceiveShadow, proto.layer,
                            camera, LightProbeUsage.BlendProbes);
                        billboardBatchInstanceCount = 0;
                    }
                }

                if (i == count)
                    break;

                if (instanceCullResults[i] == CULLED)
                    continue;
                if (instancePrototypeIndices[i] != prototypeIndex)
                    continue;

                if (instanceCullResults[i] == VISIBLE)
                {
                    if (canDrawInstanced)
                    {
                        batchContainer[batchInstanceCount] = instanceTransforms[i];
                        batchInstanceCount += 1;
                    }
                    else
                    {
                        for (int d = 0; d < drawCount; ++d)
                        {
                            Graphics.DrawMesh(
                                mesh, instanceTransforms[i], materials[d],
                                proto.layer, camera, d, null,
                                proto.shadowCastingMode, proto.receiveShadow,
                                null, LightProbeUsage.BlendProbes, null);
                        }
                    }
                }
                else if (instanceCullResults[i] == BILLBOARD && billboard != null)
                {
                    if (canDrawBillboardInstanced)
                    {
                        billboardBatchContainer[billboardBatchInstanceCount] = instanceTransforms[i];
                        billboardBatchInstanceCount += 1;
                    }
                    else
                    {
                        Graphics.DrawMesh(
                            billboardMesh, instanceTransforms[i], billboardMaterial,
                            proto.layer, camera, 0, null,
                            proto.billboardShadowCastingMode, proto.BillboardReceiveShadow,
                            null, LightProbeUsage.BlendProbes, null);
                    }
                }
            }
        }

        private void CleanUpFrame()
        {
            GNativeArrayUtilities.Dispose(frustumPlanes);
            GNativeArrayUtilities.Dispose(prototypePivotOffset);
            GNativeArrayUtilities.Dispose(prototypeBaseRotation);
            GNativeArrayUtilities.Dispose(prototypeBaseScale);
            GNativeArrayUtilities.Dispose(prototypeBounds);
            GNativeArrayUtilities.Dispose(prototypeWillDoFrustumTest);
        }

        internal void ResetTrees()
        {
            if (nativeData != null)
            {
                nativeData.Dispose();
                nativeData = null;
            }
        }

        internal void CleanUp()
        {
            ResetTrees();
        }
    }
}
#endif
