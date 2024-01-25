using Unity.Mathematics;
using Random = Unity.Mathematics.Random;
using UnityEngine;
using ProjectDawn.Navigation.Hybrid;

namespace ProjectDawn.Navigation.Sample.Mass
{
    public class Spawner : MonoBehaviour
    {
        public GameObject Prefab;
        public float Interval = 1;
        public float3 Size = new float3(1, 0, 1);
        public int Count;
        public int MaxCount = 1000;
        public Transform Destination;
        public bool DestinationDeferred = true;

        float m_Elapsed;
        Random m_Random = new Random(1);

        void Update()
        {
            if (MaxCount == Count)
                return;

            m_Elapsed += Time.deltaTime;
            if (m_Elapsed >= Interval)
            {
                float3 offset = m_Random.NextFloat3(-Size, Size);
                float3 position = (float3) transform.position + offset;
                GameObject unit = GameObject.Instantiate(Prefab, position, Quaternion.identity);
                if (Destination != null)
                {
                    var agent = unit.GetComponent<AgentAuthoring>();
                    if (DestinationDeferred)
                    {
                        agent.SetDestinationDeferred(Destination.position);
                    }
                    else
                    {
                        agent.SetDestination(Destination.position);
                    }
                }
                m_Elapsed -= Interval;
                Count++;
            }
        }
    }
}
