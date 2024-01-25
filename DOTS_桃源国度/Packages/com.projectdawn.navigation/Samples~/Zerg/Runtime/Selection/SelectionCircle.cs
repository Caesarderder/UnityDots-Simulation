using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace ProjectDawn.Navigation.Sample.Zerg
{
    public class SelectionCircle : MonoBehaviour
    {
        public Mesh Mesh;
        public Material Material;
        public float3 Offset;

        public void Draw(float3 position, float scale)
        {
            Graphics.DrawMesh(Mesh, Matrix4x4.TRS(position + Offset, quaternion.RotateX(math.radians(90)), Vector3.one * scale), Material, 0);
        }
    }
}
