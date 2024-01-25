using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace ProjectDawn.Navigation.Sample.Zerg
{
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class DrawSelectionCircleSystem : SystemBase
    {
        SelectionCircle m_SelectionCircle;

        protected override void OnCreate()
        {
            m_SelectionCircle = GameObject.FindObjectOfType<SelectionCircle>(true);
        }

        protected override void OnUpdate()
        {
            if (m_SelectionCircle == null)
                return;

            var selection = SystemAPI.GetSingleton<SelectionSystem.Singleton>();
            if (selection.SelectedEntities.IsEmpty)
                return;

            var shapeLookup = GetComponentLookup<AgentShape>(true);
            var transformLookup = GetComponentLookup<LocalTransform>(true);

            Dependency.Complete();

            foreach (var entity in selection.SelectedEntities)
            {
                if (!shapeLookup.TryGetComponent(entity, out AgentShape shape))
                    continue;
                if (!transformLookup.TryGetComponent(entity, out LocalTransform transform))
                    continue;
                m_SelectionCircle.Draw(transform.Position, shape.Radius * 2.5f);
            }
        }
    }
}
