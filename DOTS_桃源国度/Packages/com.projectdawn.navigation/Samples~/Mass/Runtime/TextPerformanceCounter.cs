using UnityEngine;
using UnityEngine.UI;

namespace ProjectDawn.Navigation.Sample.BoardDefense
{
    [RequireComponent(typeof(Text))]
    public class TextPerformanceCounter : MonoBehaviour
    {
        public int AverageFrameCount = 5;
        Text m_Text;
        int m_AccumulatedFrames;
        float m_AccumulatedTime;

        void Awake()
        {
            m_Text = GetComponent<Text>();
        }

        void Update()
        {
            m_AccumulatedFrames++;
            m_AccumulatedTime += Time.deltaTime;

            if (m_AccumulatedFrames == AverageFrameCount)
            {
                float ms = (m_AccumulatedTime * 1000) / m_AccumulatedFrames;
                m_Text.text = $"{ms:0.00}ms";
                m_AccumulatedFrames = 0;
                m_AccumulatedTime = 0;
            }
        }
    }
}
