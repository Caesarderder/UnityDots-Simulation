using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.Mathematics;

namespace ProjectDawn.Navigation.Sample.Zerg
{
    public class SelectionRectangle : MonoBehaviour
    {
        public RectTransform RectTransform;
        Rect m_Rect;

        public void Show(Rect rect)
        {
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);

            Vector2 size = rect.size;
            Vector2 position = rect.center;

            m_Rect.center = position;
            m_Rect.size = size;

            RectTransform.localPosition = position;
            RectTransform.sizeDelta = size;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
