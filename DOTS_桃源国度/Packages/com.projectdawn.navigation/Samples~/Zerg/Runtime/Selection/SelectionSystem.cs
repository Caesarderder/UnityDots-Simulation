using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;
using UnityEngine;

namespace ProjectDawn.Navigation.Sample.Zerg
{
    [RequireMatchingQueriesForUpdate]
    public partial class SelectionSystem : SystemBase
    {
        NativeList<Entity> m_SelectedEntities;
        SelectionRectangle m_SelectionRectangle;
        Gestures m_Gestures;

        protected override void OnCreate()
        {
            m_SelectedEntities = new NativeList<Entity>(Allocator.Persistent);
            m_SelectionRectangle = GameObject.FindObjectOfType<SelectionRectangle>(true);
            m_Gestures = GameObject.FindObjectOfType<Gestures>(true);

            World.EntityManager.CreateSingleton(new Singleton
            {
                SelectedEntities = m_SelectedEntities,
            }, "Selection");
        }

        public struct Singleton : IComponentData
        {
            public NativeList<Entity> SelectedEntities;
        }

        protected override void OnDestroy()
        {
            m_SelectedEntities.Dispose();
        }

        protected override void OnUpdate()
        {
            if (m_SelectionRectangle == null)
                return;

            if (m_Gestures == null)
                return;

            var selection = SystemAPI.GetSingletonRW<Singleton>();
            var selectectEntities = selection.ValueRW.SelectedEntities;

            if (m_Gestures.Selection(out Rect rect))
            {
                m_SelectionRectangle.Show(rect);
            }

            if (m_Gestures.SelectionExit(out rect))
            {
                selectectEntities.Clear();
                Entities.ForEach((Entity entity, in Unit unit, in LocalTransform transform) =>
                {
                    Vector3 position = Camera.main.WorldToScreenPoint(transform.Position);
                    if (rect.Contains(position) && unit.Owner == PlayerId.Red)
                    {
                        selectectEntities.Add(entity);
                    }
                }).WithoutBurst().Run();
                m_SelectionRectangle.Hide();
            }
        }
    }
}
