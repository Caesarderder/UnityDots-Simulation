#if GRIFFIN
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

namespace Pinwheel.Griffin.StampTool
{
    [System.Serializable]
    [ExecuteInEditMode]
    public class GObjectStamper : MonoBehaviour
    {
        [SerializeField]
        private bool enableTerrainMask;
        public bool EnableTerrainMask
        {
            get
            {
                return enableTerrainMask;
            }
            set
            {
                enableTerrainMask = value;
            }
        }

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
        private Vector3 position;
        public Vector3 Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
                transform.position = value;
            }
        }

        [SerializeField]
        private Quaternion rotation;
        public Quaternion Rotation
        {
            get
            {
                return rotation;
            }
            set
            {
                rotation = value;
                transform.rotation = value;
            }
        }

        [SerializeField]
        private Vector3 scale;
        public Vector3 Scale
        {
            get
            {
                return scale;
            }
            set
            {
                scale = value;
                transform.localScale = value;
            }
        }

        [SerializeField]
        private Texture2D mask;
        public Texture2D Mask
        {
            get
            {
                return mask;
            }
            set
            {
                mask = value;
            }
        }

        [SerializeField]
        private AnimationCurve falloff;
        public AnimationCurve Falloff
        {
            get
            {
                return falloff;
            }
            set
            {
                falloff = value;
            }
        }

        [SerializeField]
        private List<GObjectStampLayer> layers;
        public List<GObjectStampLayer> Layers
        {
            get
            {
                if (layers == null)
                {
                    layers = new List<GObjectStampLayer>();
                }
                return layers;
            }
            set
            {
                layers = value;
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

        public Rect Rect
        {
            get
            {
                Vector3[] quad = new Vector3[4];
                GetQuad(quad);
                Rect r = GUtilities.GetRectContainsPoints(quad);
                return r;
            }
        }

        private Texture2D falloffTexture;

        private Vector3[] worldPoints = new Vector3[4];
        private Vector2[] uvPoints = new Vector2[4];

        private Dictionary<string, RenderTexture> tempRt;
        private Dictionary<string, RenderTexture> TempRt
        {
            get
            {
                if (tempRt == null)
                {
                    tempRt = new Dictionary<string, RenderTexture>();
                }
                return tempRt;
            }
        }

        private void Reset()
        {
            position = Vector3.zero;
            rotation = Quaternion.identity;
            scale = Vector3.one * 100;
            mask = null;
            falloff = AnimationCurve.EaseInOut(0, 1, 1, 0);
            maskResolution = 1024;
        }

        private void OnDisable()
        {
            ReleaseResources();
        }

        private void OnDestroy()
        {
            ReleaseResources();
        }

        private void ReleaseResources()
        {
            foreach (RenderTexture rt in TempRt.Values)
            {
                if (rt != null)
                {
                    rt.Release();
                    GUtilities.DestroyObject(rt);
                }
            }
        }

        public void Apply()
        {
            if (falloffTexture != null)
                Object.DestroyImmediate(falloffTexture);
            Internal_UpdateFalloffTexture();
            Internal_UpdateLayerTransitionTextures();

            List<GStylizedTerrain> terrains = GUtilities.ExtractTerrainsFromOverlapTest(GCommon.OverlapTest(GroupId, GetQuad()));

            try
            {
                foreach (GStylizedTerrain t in terrains)
                {
                    Apply(t);
                }
            }
            catch (GProgressCancelledException)
            {
                Debug.Log("Stamp process canceled, result may be incorrect. Use History to clean up!");
#if UNITY_EDITOR
                GCommonGUI.ClearProgressBar();
#endif
            }
        }

        private void Apply(GStylizedTerrain t)
        {
            if (t.TerrainData == null)
                return;
            if (Layers.Count == 0)
                return;

            GetQuad(worldPoints);
            GetUvPoints(t, worldPoints, uvPoints);
            Rect dirtyRect = GUtilities.GetRectContainsPoints(uvPoints);
            if (!dirtyRect.Overlaps(new Rect(0, 0, 1, 1)))
                return;

            RenderTexture[] brushes = new RenderTexture[Layers.Count];
            for (int i = 0; i < Layers.Count; ++i)
            {
                brushes[i] = GetRenderTexture("brush" + i.ToString());
            }
            Internal_RenderBrushes(brushes, t, uvPoints);

            for (int i = 0; i < Layers.Count; ++i)
            {
                StampLayer(t, brushes[i], i);
            }
        }

        private void StampLayer(GStylizedTerrain t, RenderTexture brush, int layerIndex)
        {
            GObjectStampLayer layer = Layers[layerIndex];
            if (layer.Ignore)
                return;
            if (layer.InstanceCount == 0)
                return;
            if (layer.Prototypes.Count == 0 ||
                layer.PrototypeIndices.Count == 0)
                return;

            Texture2D layerMask = GCommon.CreateTexture(MaskResolution, Color.clear);
            GCommon.CopyFromRT(layerMask, brush);
            SpawnObjectOnTerrain(t, layerMask, layerIndex);
            GUtilities.DestroyObject(layerMask);
        }

        private void SpawnObjectOnTerrain(GStylizedTerrain t, Texture2D layerMask, int layerIndex)
        {
            GObjectStampLayer layer = Layers[layerIndex];

#if UNITY_EDITOR
            string title = "Stamping on " + t.name;
            string text = string.Format("Layer: {0}", !string.IsNullOrEmpty(layer.Name) ? layer.Name : layerIndex.ToString());
            GCommonGUI.CancelableProgressBar(title, text, 1);
#endif

            int sampleCount = layer.InstanceCount;
            NativeArray<bool> cullResult = new NativeArray<bool>(sampleCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            NativeArray<GPrototypeInstanceInfo> instanceInfo = new NativeArray<GPrototypeInstanceInfo>(sampleCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            NativeArray<int> selectedPrototypeIndices = new NativeArray<int>(layer.PrototypeIndices.ToArray(), Allocator.TempJob);
            GTextureNativeDataDescriptor<Color32> maskHandle = new GTextureNativeDataDescriptor<Color32>(layerMask);

            {
                GSampleInstanceJob job = new GSampleInstanceJob()
                {
                    cullResult = cullResult,
                    instanceInfo = instanceInfo,
                    mask = maskHandle,
                    selectedPrototypeIndices = selectedPrototypeIndices,

                    minRotation = layer.MinRotation,
                    maxRotation = layer.MaxRotation,

                    minScale = layer.MinScale,
                    maxScale = layer.MaxScale,

                    seed = Random.Range(int.MinValue, int.MaxValue)
                };

                JobHandle jHandle = job.Schedule(sampleCount, 100);
                jHandle.Complete();
            }

            Vector3 terrainSize = t.TerrainData.Geometry.Size;
            NativeArray<RaycastCommand> raycastCommands = new NativeArray<RaycastCommand>(instanceInfo.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            NativeArray<RaycastHit> hits = new NativeArray<RaycastHit>(instanceInfo.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            for (int i = 0; i < raycastCommands.Length; ++i)
            {
                GPrototypeInstanceInfo info = instanceInfo[i];
                Vector3 from = new Vector3(t.transform.position.x + info.position.x * terrainSize.x, 10000, t.transform.position.z + info.position.z * terrainSize.z);
#if UNITY_2022_2_OR_NEWER
            QueryParameters q = new QueryParameters(layer.WorldRaycastMask, false, QueryTriggerInteraction.Ignore, false);
            RaycastCommand cmd = new RaycastCommand(from, Vector3.down, q, float.MaxValue);
#else
                RaycastCommand cmd = new RaycastCommand(from, Vector3.down, float.MaxValue, layer.WorldRaycastMask, 1);
#endif
                raycastCommands[i] = cmd;
            }
            {
                JobHandle jHandle = RaycastCommand.ScheduleBatch(raycastCommands, hits, 10);
                jHandle.Complete();
            }

            for (int i = 0; i < instanceInfo.Length; ++i)
            {
                GPrototypeInstanceInfo info = instanceInfo[i];
                RaycastHit h = hits[i];
                info.position.Set(info.position.x * terrainSize.x, h.collider != null ? h.point.y : 0, info.position.z * terrainSize.z);
                instanceInfo[i] = info;
            }

            int count = instanceInfo.Length;
            for (int i = 0; i < count; ++i)
            {
                GPrototypeInstanceInfo info = instanceInfo[i];
                if (cullResult[i] == false)
                    continue;
                if (info.prototypeIndex < 0 || info.prototypeIndex >= layer.Prototypes.Count)
                    continue;

                GameObject prototype = layer.Prototypes[info.prototypeIndex];
                GameObject go = GSpawner.Spawn(t, prototype, Vector3.zero);
                go.transform.position = t.transform.position + info.position;
                go.transform.rotation = info.rotation;
                go.transform.localScale = info.scale;

                if (layer.AlignToSurface)
                {
                    RaycastHit hit = hits[i];
                    if (hit.collider != null)
                    {
                        Quaternion q = Quaternion.FromToRotation(go.transform.up, hit.normal);
                        go.transform.rotation *= q;
                    }
                }
            }

            raycastCommands.Dispose();
            hits.Dispose();

            cullResult.Dispose();
            instanceInfo.Dispose();
            selectedPrototypeIndices.Dispose();

#if UNITY_EDITOR
            GCommonGUI.ClearProgressBar();
#endif
        }

        public void Internal_RenderBrushes(RenderTexture[] brushes, GStylizedTerrain t, Vector2[] uvPoints)
        {
            for (int i = 0; i < brushes.Length; ++i)
            {
                GStampLayerMaskRenderer.Render(
                    brushes[i],
                    Layers[i],
                    t,
                    Matrix4x4.TRS(Position, Rotation, Scale),
                    Mask,
                    falloffTexture,
                    uvPoints,
                    EnableTerrainMask);
            }
        }

        public void Internal_UpdateFalloffTexture()
        {
            if (falloffTexture != null)
                GUtilities.DestroyObject(falloffTexture);
            falloffTexture = GCommon.CreateTextureFromCurve(Falloff, 256, 1);
        }

        public void Internal_UpdateLayerTransitionTextures()
        {
            for (int i = 0; i < Layers.Count; ++i)
            {
                Layers[i].UpdateCurveTextures();
            }
        }

        public Vector3[] GetQuad()
        {
            Matrix4x4 matrix = Matrix4x4.TRS(Position, Rotation, Scale);
            Vector3[] quad = new Vector3[4]
            {
                matrix.MultiplyPoint(new Vector3(-0.5f, 0, -0.5f)),
                matrix.MultiplyPoint(new Vector3(-0.5f, 0, 0.5f)),
                matrix.MultiplyPoint(new Vector3(0.5f, 0, 0.5f)),
                matrix.MultiplyPoint(new Vector3(0.5f, 0, -0.5f))
            };

            return quad;
        }

        public void GetQuad(Vector3[] quad)
        {
            Matrix4x4 matrix = Matrix4x4.TRS(Position, Rotation, Scale);
            quad[0] = matrix.MultiplyPoint(new Vector3(-0.5f, 0, -0.5f));
            quad[1] = matrix.MultiplyPoint(new Vector3(-0.5f, 0, 0.5f));
            quad[2] = matrix.MultiplyPoint(new Vector3(0.5f, 0, 0.5f));
            quad[3] = matrix.MultiplyPoint(new Vector3(0.5f, 0, -0.5f));
        }

        private void GetUvPoints(GStylizedTerrain t, Vector3[] worldPoint, Vector2[] uvPoint)
        {
            for (int i = 0; i < uvPoints.Length; ++i)
            {
                uvPoints[i] = t.WorldPointToUV(worldPoints[i]);
            }
        }

        public void GetBox(Vector3[] box)
        {
            Matrix4x4 matrix = Matrix4x4.TRS(Position, Rotation, Scale);
            box[0] = matrix.MultiplyPoint(new Vector3(-0.5f, 0, -0.5f));
            box[1] = matrix.MultiplyPoint(new Vector3(-0.5f, 0, 0.5f));
            box[2] = matrix.MultiplyPoint(new Vector3(0.5f, 0, 0.5f));
            box[3] = matrix.MultiplyPoint(new Vector3(0.5f, 0, -0.5f));
            box[4] = matrix.MultiplyPoint(new Vector3(-0.5f, 1, -0.5f));
            box[5] = matrix.MultiplyPoint(new Vector3(-0.5f, 1, 0.5f));
            box[6] = matrix.MultiplyPoint(new Vector3(0.5f, 1, 0.5f));
            box[7] = matrix.MultiplyPoint(new Vector3(0.5f, 1, -0.5f));
        }

        private RenderTexture GetRenderTexture(string key)
        {
            int resolution = MaskResolution;
            if (!TempRt.ContainsKey(key) ||
                TempRt[key] == null)
            {
                RenderTexture rt = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                rt.wrapMode = TextureWrapMode.Clamp;
                TempRt[key] = rt;
            }
            else if (TempRt[key].width != resolution || TempRt[key].height != resolution)
            {
                TempRt[key].Release();
                Object.DestroyImmediate(TempRt[key]);
                RenderTexture rt = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                rt.wrapMode = TextureWrapMode.Clamp;
                TempRt[key] = rt;
            }

            return TempRt[key];
        }

        public void ClearObjects()
        {
            List<GStylizedTerrain> terrains = GUtilities.ExtractTerrainsFromOverlapTest(GCommon.OverlapTest(GroupId, GetQuad()));
            foreach (GStylizedTerrain t in terrains)
            {
                ClearObjects(t);
            }
        }

        private void ClearObjects(GStylizedTerrain t)
        {
            if (t.TerrainData == null)
                return;
            Vector3 terrainSize = new Vector3(
                t.TerrainData.Geometry.Width,
                t.TerrainData.Geometry.Height,
                t.TerrainData.Geometry.Length);
            Vector3 scale = new Vector3(
                GUtilities.InverseLerpUnclamped(0, terrainSize.x, Scale.x),
                GUtilities.InverseLerpUnclamped(0, terrainSize.y, Scale.y),
                GUtilities.InverseLerpUnclamped(0, terrainSize.z, Scale.z));
            Matrix4x4 matrix = Matrix4x4.TRS(
                t.WorldPointToNormalized(Position),
                Rotation,
                scale);
            Matrix4x4 normalizeToStamp = matrix.inverse;

            GSpawner.DestroyIf(t, (g) =>
            {
                Vector3 normalizePos = t.WorldPointToNormalized(g.transform.position);
                Vector3 stampSpacePos = normalizeToStamp.MultiplyPoint(normalizePos);
                return
                    stampSpacePos.x >= -0.5f && stampSpacePos.x <= 0.5f &&
                    stampSpacePos.y >= 0f && stampSpacePos.y <= 1f &&
                    stampSpacePos.z >= -0.5f && stampSpacePos.z <= 0.5f;
            });
        }
    }
}
#endif
