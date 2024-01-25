using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectDawn.Navigation.Sample.Mass
{
    [RequireComponent(typeof(Text))]
    public class TextAgentCounter : MonoBehaviour
    {
        Text m_Text;

        Spawner[] m_Spawners;

        void Awake()
        {
            m_Spawners = FindObjectsOfType<Spawner>();
            m_Text = GetComponent<Text>();
        }

        public void Update()
        {
            int count = 0;
            foreach (var spawner in m_Spawners)
            {
                count += spawner.Count;
            }
            if (m_Text)
                m_Text.text = count.ToString();
        }
    }
}
