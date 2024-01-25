#if GRIFFIN
using UnityEngine;

namespace Pinwheel.Griffin.BillboardTool
{
    [System.Serializable]
    public struct GBillboardCreatorArgs
    {
        public GBillboardRenderMode Mode { get; set; }
        public GameObject Target { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        public int CellSize { get; set; }
        public Vector3 CameraOffset { get; set; }
        public float CameraSize { get; set; }
        public Material AtlasMaterial { get; set; }
        public Material NormalMaterial { get; set; }
        public string SrcColorProps { get; set; }
        public string DesColorProps { get; set; }
        public string SrcTextureProps { get; set; }
        public string DesTextureProps { get; set; }
        public int CellIndex { get; set; }
        public Vector2[] Vertices { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public float Bottom { get; set; }
    }
}
#endif
