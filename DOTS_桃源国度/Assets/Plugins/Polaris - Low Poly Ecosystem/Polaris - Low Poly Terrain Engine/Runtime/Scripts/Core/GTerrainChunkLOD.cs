#if GRIFFIN
using UnityEngine;

namespace Pinwheel.Griffin
{
    [System.Serializable]
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class GTerrainChunkLOD : MonoBehaviour
    {
        [SerializeField]
        private MeshFilter meshFilterComponent;
        public MeshFilter MeshFilterComponent
        {
            get
            {
                if (meshFilterComponent == null)
                {
                    meshFilterComponent = GetComponent<MeshFilter>();
                    if (meshFilterComponent == null)
                    {
                        meshFilterComponent = gameObject.AddComponent<MeshFilter>();
                    }
                }
                return meshFilterComponent;
            }
        }

        [SerializeField]
        private MeshRenderer meshRendererComponent;
        public MeshRenderer MeshRendererComponent
        {
            get
            {
                if (meshRendererComponent == null)
                {
                    meshRendererComponent = GetComponent<MeshRenderer>();
                    if (meshRendererComponent == null)
                    {
                        meshRendererComponent = gameObject.AddComponent<MeshRenderer>();
                    }
                }
                return meshRendererComponent;
            }
        }
    }
}
#endif
