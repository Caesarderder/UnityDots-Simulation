using Unity.Mathematics;
using UnityEngine;

namespace ProjectDawn.Navigation.Sample.Zerg
{
    public class LifeBar : MonoBehaviour
    {
        public Mesh Mesh;
        public Material Material;

        MaterialPropertyBlock m_Block;

        public void UpdateProperties()
        {
            if (m_Block == null)
                m_Block = new MaterialPropertyBlock();
        }

        public void Draw(float3 position, float progress, int split, float scale, float height)
        {
            m_Block.SetFloat("_Width", math.max(scale * 2f, 1));
            m_Block.SetFloat("_Progress", progress);
            m_Block.SetColor("_Color", Color.Lerp(Color.red, Color.green, progress));
            m_Block.SetInteger("_Split", split);
            Graphics.DrawMesh(Mesh, position + new float3(0, height, 0), quaternion.identity, Material, 0, Camera.main, 0, m_Block);
        }
    }
}
