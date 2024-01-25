using Unity.Entities;
using UnityEngine;
using ProjectDawn.Navigation.Hybrid;

namespace ProjectDawn.Navigation.Sample.Zerg
{
    [RequireComponent(typeof(UnitAuthoring))]
    public class UnitSmartStopAuthoring : MonoBehaviour
    {
        public float Radius = 1.5f;

        Entity m_Entity;

        void Awake()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            m_Entity = GetComponent<AgentAuthoring>().GetOrCreateEntity();
            world.EntityManager.AddComponentData(m_Entity, new UnitSmartStop
            {
                Radius = Radius,
            });
        }

        void OnDestroy()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world != null)
            {
                world.EntityManager.RemoveComponent<UnitSmartStop>(m_Entity);
            }
        }
    }
}
