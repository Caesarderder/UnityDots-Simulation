using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ProjectDawn.Navigation.Sample.Zerg
{
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class ConfirmationSystem : SystemBase
    {
        Confirmation m_Confirmation;
        protected override void OnCreate()
        {
            m_Confirmation = GameObject.FindObjectOfType<Confirmation>(true);

            World.EntityManager.CreateSingleton(new Singleton
            {
            }, "Confirmation");
        }

        public struct Singleton : IComponentData
        {
            public float3 Position;
            public bool Play;
        }

        protected override void OnUpdate()
        {
            if (m_Confirmation == null)
                return;

            var confirmation = SystemAPI.GetSingletonRW<Singleton>();

            Dependency.Complete();

            if (confirmation.ValueRW.Play)
            {
                m_Confirmation.Play(confirmation.ValueRW.Position);
                confirmation.ValueRW.Play = false;
            }
        }
    }
}
