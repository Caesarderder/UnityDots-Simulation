using Unity.Mathematics;
using UnityEngine;

namespace ProjectDawn.Navigation.Sample.Zerg
{
    public class Gestures : MonoBehaviour
    {
        float3 m_SelectionStart;
        float3 m_SelectionEnd;
        bool m_Selection;
        bool m_SelectionExit;

        public bool Stop()
        {
            return Input.GetKeyUp(KeyCode.S);
        }

        public bool MoveCamera(float borderSize, out float2 direction)
        {
            float2 position = new float2(Input.mousePosition.x, Input.mousePosition.y);
            Rect screenRect = new Rect(-1, -1, Screen.width + 2, Screen.height + 2);
            Rect safeRect = new Rect(borderSize, borderSize, Screen.width - borderSize * 2, Screen.height - borderSize * 2);

            // If position out of screen do not move camera, it can happens if mouse if not cofined
            if (!screenRect.Contains(position))
            {
                direction = 0;
                return false;
            }

            // Do not move camera if safe screen part
            if (safeRect.Contains(position))
            {
                direction = 0;
                return false;
            }
            float2 screenCenter = new float2(Screen.width, Screen.height) * 0.5f;
            direction = math.normalizesafe(position - screenCenter);
            return true;
        }

        public bool Confirmation(out float3 position)
        {
            if (Input.GetMouseButtonUp(1))
            {
                position = Input.mousePosition;
                return true;
            }
            position = float3.zero;
            return false;
        }

        public bool Selection(out Rect rect)
        {
            if (!m_Selection)
            {
                rect = new Rect();
                return false;
            }

            float2 min = math.min(m_SelectionStart, Input.mousePosition).xy;
            float2 max = math.max(m_SelectionStart, Input.mousePosition).xy;

            rect = new Rect(min, max - min);
            return true;
        }

        public bool SelectionExit(out Rect rect)
        {
            if (!m_SelectionExit)
            {
                rect = new Rect();
                return false;
            }

            float2 min = math.min(m_SelectionStart, Input.mousePosition).xy;
            float2 max = math.max(m_SelectionStart, Input.mousePosition).xy;

            rect = new Rect(min, max - min);
            return true;
        }

        void Update()
        {
            m_SelectionExit = false;

            if (Input.GetMouseButtonDown(0))
            {
                m_SelectionStart = Input.mousePosition;
                m_Selection = true;
            }

            if (Input.GetMouseButtonUp(0))
            {
                m_SelectionEnd = Input.mousePosition;
                m_Selection = false;
                m_SelectionExit = true;
            }
        }
    }
}
