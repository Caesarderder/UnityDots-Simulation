using Unity.Entities;

namespace ProjectDawn.Navigation
{
    public struct AgentReciprocalAvoid : IComponentData
    {
        public float Radius;

        public NavigationLayers Layers;

        public static AgentReciprocalAvoid Default => new()
        {
            Radius = 2,
            Layers = NavigationLayers.Everything,
        };
    }
}
