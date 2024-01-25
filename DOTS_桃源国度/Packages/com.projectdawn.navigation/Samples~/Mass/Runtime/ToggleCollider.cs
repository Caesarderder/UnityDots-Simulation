using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectDawn.Navigation.Sample.Mass
{
    public class ToggleCollider : MonoBehaviour
    {
        public void OnValueChange(Toggle value)
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null)
                return;

            var system = world.GetExistingSystem(typeof(AgentColliderSystem));
            if (system == SystemHandle.Null)
                return;

            ref var state = ref world.Unmanaged.ResolveSystemStateRef(system);
            state.Enabled = value.isOn;
        }
    }
}
