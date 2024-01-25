using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectDawn.Navigation.Sample.Mass
{
    public class ToggleNavMesh : MonoBehaviour
    {
        public void OnValueChange(Toggle value)
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null)
                return;

            var system = world.GetExistingSystem(typeof(NavMeshSteeringSystem));
            if (system == SystemHandle.Null)
                return;

            ref var state = ref world.Unmanaged.ResolveSystemStateRef(system);
            state.Enabled = value.isOn;

            var system2 = world.GetExistingSystem(typeof(NavMeshDisplacementSystem));
            if (system == SystemHandle.Null)
                return;

            ref var state2 = ref world.Unmanaged.ResolveSystemStateRef(system2);
            state2.Enabled = value.isOn;
        }
    }
}
