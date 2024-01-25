#if GRIFFIN
using UnityEngine;

namespace Pinwheel.Griffin.TextureTool
{
    public class GHeightMapFromMeshGenerator : IGTextureGenerator
    {
        public const float DEFAULT_CAMERA_ORTHO_SIZE = 10;

        private Camera camera;
        private GameObject gameObject;

        public void Generate(RenderTexture targetRt)
        {
            GHeightMapFromMeshGeneratorParams param = GTextureToolParams.Instance.HeightMapFromMesh;
            if (param.SrcMesh == null)
            {
                GCommon.FillTexture(targetRt, Color.black);
            }
            else
            {
                try
                {
                    SetUp(param, targetRt);
                    Render(param, targetRt);
                }
                catch (System.Exception e)
                {
                    Debug.Log(e);
                }
                finally
                {
                    CleanUp();
                }
            }
        }

        private void SetUp(GHeightMapFromMeshGeneratorParams param, RenderTexture targetRt)
        {
            GameObject camGo = new GameObject("~HeightMapFromMeshCamera");
            camGo.transform.position = -Vector3.one * 5000;
            camGo.transform.rotation = Quaternion.Euler(90, 0, 0);
            camGo.transform.localScale = Vector3.one;

            camera = camGo.AddComponent<Camera>();
            camera.orthographic = true;
            camera.enabled = false;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.clear;
            camera.nearClipPlane = 0;
            camera.farClipPlane = param.ProjectionDepth;
            camera.orthographicSize = DEFAULT_CAMERA_ORTHO_SIZE;
            camera.aspect = 1;
            camera.targetTexture = targetRt;

            gameObject = new GameObject("~HeightMapFromMeshGameObjectPivot");
            gameObject.transform.position = camGo.transform.position + param.Offset;
            gameObject.transform.rotation = param.Rotation;
            gameObject.transform.localScale = param.Scale;

            GameObject model = new GameObject("~Model");
            GUtilities.ResetTransform(model.transform, gameObject.transform);
            model.transform.localPosition = -param.SrcMesh.bounds.center;

            MeshFilter mf = model.AddComponent<MeshFilter>();
            mf.sharedMesh = param.SrcMesh;

            MeshRenderer mr = model.AddComponent<MeshRenderer>();
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
            mr.sharedMaterials = new Material[] { GInternalMaterials.HeightMapFromMeshMaterial };

        }

        private void Render(GHeightMapFromMeshGeneratorParams param, RenderTexture targetRt)
        {
            camera.targetTexture = targetRt;
            camera.Render();
        }

        private void CleanUp()
        {
            if (camera != null)
            {
                camera.targetTexture = null;
                GUtilities.DestroyGameobject(camera.gameObject);
            }
            if (gameObject != null)
            {
                GUtilities.DestroyGameobject(gameObject);
            }
        }
    }
}
#endif
