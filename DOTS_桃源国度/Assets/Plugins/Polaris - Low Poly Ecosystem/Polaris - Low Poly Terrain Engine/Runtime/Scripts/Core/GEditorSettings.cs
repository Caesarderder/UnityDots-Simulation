#if GRIFFIN
#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Pinwheel.Griffin.Rendering;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.Threading.Tasks;

namespace Pinwheel.Griffin
{
    //[CreateAssetMenu(menuName = "Griffin/Editor Settings")]
    public partial class GEditorSettings : ScriptableObject, ISerializationCallbackReceiver
    {
        private static GEditorSettings instance;
        public static GEditorSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<GEditorSettings>("PolarisEditorSettings");
                    if (instance == null)
                    {
                        instance = ScriptableObject.CreateInstance<GEditorSettings>();
                    }
                }
                return instance;
            }
        }

        public GeneralSettings general = new GeneralSettings();
        public LivePreviewSettings livePreview = new LivePreviewSettings();
        public PaintToolsSettings paintTools = new PaintToolsSettings();
        public SplineToolsSettings splineTools = new SplineToolsSettings();
        public BillboardToolsSettings billboardTools = new BillboardToolsSettings();
        public StampToolsSettings stampTools = new StampToolsSettings();
        public WizardToolsSettings wizardTools = new WizardToolsSettings();
        public RenderPipelinesSettings renderPipelines = new RenderPipelinesSettings();
        public TopographicSettings topographic = new TopographicSettings();
        public ErosionToolsSettings erosionTools = new ErosionToolsSettings();
        public LayerSettings layers = new LayerSettings();
        public DemoAssetSettings demoAssets = new DemoAssetSettings();

        #region Serialization Callbacks
        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {

        }
        #endregion

        private void OnEnable()
        {
#if UNITY_EDITOR
            SetupLayersAsync();
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
#endif
        }

#if UNITY_EDITOR
        private void OnPlayModeStateChanged(PlayModeStateChange obj)
        {
            //Some hack to clean up billboard meshes
            GBillboardUtilities.CleanUp();
        }
#endif 

        private async void SetupLayersAsync()
        {
            await Task.Delay(1000);
            GLayerInitializer.SetupRaycastLayer();
            GLayerInitializer.SetupSplineLayer();
        }
    }

    public partial class GEditorSettings : ScriptableObject
    {
        [System.Serializable]
        public class GeneralSettings
        {
            public bool enableAnalytics = true;
            public bool enableAffiliateLinks = true;
            public bool debugMode = false;
            public bool showGeometryChunkInHierarchy = true;
        }

        [System.Serializable]
        public class LivePreviewSettings
        {
            public Mesh[] triangleMeshes;
            public Mesh[] wireframeMeshes;

            public Mesh GetTriangleMesh(int detail)
            {
                if (triangleMeshes == null || triangleMeshes.Length == 0)
                    return null;
                detail = Mathf.Clamp(detail, 0, triangleMeshes.Length - 1);
                return triangleMeshes[detail];
            }

            public Mesh GetWireframeMesh(int detail)
            {
                if (wireframeMeshes == null || wireframeMeshes.Length == 0)
                    return null;
                detail = Mathf.Clamp(detail, 0, wireframeMeshes.Length - 1);
                return wireframeMeshes[detail];
            }
        }

        [System.Serializable]
        public class PaintToolsSettings
        {
            public bool enableHistory;
            public bool enableLivePreview;
            public bool useSimpleCursor;
            public Color normalActionCursorColor;
            public Color negativeActionCursorColor;
            public Color alternativeActionCursorColor;
            public float radiusStep;
            public float rotationStep;
            public float opacityStep;
            public int densityStep;
            public bool useMultiSplatsSelector;
            public bool showTerrainMask;
        }

        [System.Serializable]
        public class SplineToolsSettings
        {
            public Color anchorColor;
            public Color segmentColor;
            public Color meshColor;
            public Color selectedElementColor;
            public Color positiveHighlightColor;
            public Color negativeHighlightColor;
            public bool autoTangent;
            public LayerMask raycastLayers;
            public bool showTransformGizmos;
            public LivePreviewToggle livePreview;

            [System.Serializable]
            public struct LivePreviewToggle
            {
                public bool rampMaker;
                public bool pathPainter;
                public bool foliageSpawner;
                public bool foliageRemover;
                public bool objectSpawner;
                public bool objectRemover;
            }
        }

        [System.Serializable]
        public class BillboardToolsSettings
        {
            public Material atlasMaterial;
            public Material normalMaterial;
        }

        [System.Serializable]
        public class StampToolsSettings
        {
            public Color visualizeColor;
            public float minRotation;
            public float maxRotation;
            public Vector3 minScale;
            public Vector3 maxScale;
            public bool showLivePreview;
            public bool showBounds;
            public bool showTerrainMask;
        }

        [System.Serializable]
        public class WizardToolsSettings
        {
            public GLightingModel lightingModel;
            public GTexturingModel texturingModel;
            public GSplatsModel splatsModel;
            public Vector3 origin;
            public Vector3 tileSize;
            public int tileCountX;
            public int tileCountZ;
            public int groupId;
            public string terrainNamePrefix;
            public string dataDirectory;
            [System.NonSerialized]
            public GStylizedTerrain setShaderTerrain;
            public int setShaderGroupId;
        }

        [System.Serializable]
        public class RenderPipelinesSettings
        {
            public Object universalRenderPipelinePackage;

            public string GetUrpPackagePath()
            {
                if (universalRenderPipelinePackage == null)
                    return null;
                string path = AssetDatabase.GetAssetPath(universalRenderPipelinePackage);
                return path;
            }
        }

        [System.Serializable]
        public class TopographicSettings
        {
            public bool enable;
            public Material topographicMaterial;
        }

        [System.Serializable]
        public class ErosionToolsSettings
        {
            public enum LivePreviewMode
            {
                Geometry, Texture, Off
            }

            public enum DataViewSelection
            {
                SimulationData, SimulationMask, ErosionMap
            }

            public enum DataViewChannel
            {
                R, G, B, A
            }

            public LivePreviewMode livePreviewMode;
            public DataViewSelection dataView;
            public float dataViewScale;
            public DataViewChannel dataViewChannel;
            public bool showTerrainMask;
        }

        [System.Serializable]
        public class LayerSettings
        {
            public int raycastLayerIndex;
            public int splineLayerIndex;

            public bool SetupLayer(int index, string layer)
            {
                bool success = false;
                Object tagManager = AssetDatabase.LoadAssetAtPath<Object>("ProjectSettings/TagManager.asset");
                SerializedObject so = new SerializedObject(tagManager);
                SerializedProperty layers = so.FindProperty("layers");
                for (int i = 8; i < 32; ++i)
                {
                    SerializedProperty li = layers.GetArrayElementAtIndex(i);
                    if (li.stringValue.Equals(layer) && i != index)
                    {
                        li.stringValue = string.Empty;
                    }
                    if (!li.stringValue.Equals(layer) && i == index)
                    {
                        li.stringValue = layer;
                        success = true;
                    }
                }
                so.ApplyModifiedProperties();
                so.Dispose();
                EditorUtility.SetDirty(tagManager);
                return success;
            }
        }

        [System.Serializable]
        public class DemoAssetSettings
        {
            public Material[] demoMaterials;
        }
    }
}
#endif
#endif
