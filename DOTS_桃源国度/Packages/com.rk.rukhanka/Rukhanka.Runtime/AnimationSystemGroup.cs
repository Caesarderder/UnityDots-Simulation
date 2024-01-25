
using Unity.Entities;
using Unity.Transforms;

#if RUKHANKA_WITH_NETCODE
using Unity.NetCode;
#endif

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka
{
[UpdateBefore(typeof(TransformSystemGroup))]
#if RUKHANKA_WITH_NETCODE
[UpdateAfter(typeof(PredictedSimulationSystemGroup))]
#else
[WorldSystemFilter(WorldSystemFilterFlags.LocalSimulation | WorldSystemFilterFlags.ClientSimulation)]
#endif
public partial class RukhankaAnimationSystemGroup: ComponentSystemGroup
{
	protected override void OnCreate()
	{
		base.OnCreate();
		EnableSystemSorting = false;
	}
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#if RUKHANKA_WITH_NETCODE
[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial class RukhankaPredictedAnimationSystemGroup: ComponentSystemGroup { }
#endif
}
