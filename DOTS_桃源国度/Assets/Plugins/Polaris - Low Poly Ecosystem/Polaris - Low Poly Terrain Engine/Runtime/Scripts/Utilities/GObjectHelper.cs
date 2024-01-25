#if GRIFFIN
using UnityEngine;


namespace Pinwheel.Griffin
{
    public class GObjectHelper : MonoBehaviour
    {
        [SerializeField]
        private GStylizedTerrain terrain;
        public GStylizedTerrain Terrain
        {
            get
            {
                return terrain;
            }
            set
            {
                terrain = value;
            }
        }

        [SerializeField]
        private GSnapMode snapMode;
        public GSnapMode SnapMode
        {
            get
            {
                return snapMode;
            }
            set
            {
                snapMode = value;
            }
        }

        [SerializeField]
        private LayerMask layerMask;
        public LayerMask LayerMask
        {
            get
            {
                return layerMask;
            }
            set
            {
                layerMask = value;
            }
        }

        [SerializeField]
        private bool alignToSurface;
        public bool AlignToSurface
        {
            get
            {
                return alignToSurface;
            }
            set
            {
                alignToSurface = value;
            }
        }

        public void Snap()
        {
            RaycastHit hit = new RaycastHit();
            Ray r = new Ray();
            r.direction = Vector3.down;
            Transform[] children = GUtilities.GetChildrenTransforms(transform);
            for (int i = 0; i < children.Length; ++i)
            {
                r.origin = new Vector3(children[i].position.x, 10000, children[i].position.z);
                bool isHit = false;

                if (SnapMode == GSnapMode.Terrain)
                {
                    isHit = Terrain.Raycast(r, out hit, float.MaxValue);
                }
                else if (SnapMode == GSnapMode.World)
                {
                    RaycastHit hitTerrain;
                    bool isHitTerrain = Terrain.Raycast(r, out hitTerrain, float.MaxValue);
                    float terrainHitPoint = isHitTerrain ? hitTerrain.point.y : -10000;

                    RaycastHit hitWorld;
                    bool isHitWorld = Physics.Raycast(r, out hitWorld, float.MaxValue, LayerMask);
                    float worldHitPoint = isHitWorld ? hitWorld.point.y : -10000;

                    isHit = isHitTerrain || isHitWorld;
                    hit = terrainHitPoint > worldHitPoint ? hitTerrain : hitWorld;
                }

                if (isHit)
                {
#if UNITY_EDITOR
                    UnityEditor.Undo.RecordObject(children[i], "Snap");
#endif
                    children[i].transform.position = hit.point;
                    children[i].transform.up = Vector3.up;
                    if (AlignToSurface)
                    {
                        children[i].transform.up = hit.normal;
                    }
                }
            }
        }
    }
}
#endif
