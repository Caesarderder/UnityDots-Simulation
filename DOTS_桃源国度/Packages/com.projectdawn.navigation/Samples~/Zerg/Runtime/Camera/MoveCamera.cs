using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace ProjectDawn.Navigation.Sample.Zerg
{
    public class MoveCamera : MonoBehaviour
    {
        public float Speed = 1;
        public float BorderSize = 10;
        public Rect Bounds = new Rect(0, 0, 10, 10);

        Gestures m_Gestures;

        void Start()
        {
            Cursor.lockState = CursorLockMode.Confined;
            m_Gestures = GameObject.FindObjectOfType<Gestures>(true);
        }

        void Update()
        {
            if (m_Gestures == null)
                return;

            if (m_Gestures.MoveCamera(BorderSize, out float2 direction))
            {
                Vector3 position = Camera.main.transform.position + new Vector3(direction.x, 0, direction.y) * Speed * Time.deltaTime;

                // Force position within the bounds
                position.x = Mathf.Clamp(position.x, Bounds.xMin, Bounds.xMax);
                position.z = Mathf.Clamp(position.z, Bounds.yMin, Bounds.yMax);

                Camera.main.transform.position = position;
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(new Vector3(Bounds.center.x, 0, Bounds.center.y), new Vector3(Bounds.size.x, 0, Bounds.size.y));
        }
    }
}
