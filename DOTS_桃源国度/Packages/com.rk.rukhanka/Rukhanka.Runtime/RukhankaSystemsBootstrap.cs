using Unity.Entities;
using UnityEngine;
#if RUKHANKA_WITH_NETCODE
using Unity.NetCode;
#endif

/////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka
{
[CreateAfter(typeof(RukhankaAnimationSystemGroup))]
#if RUKHANKA_WITH_NETCODE
[CreateAfter(typeof(PredictedSimulationSystemGroup))]
#endif
public partial class RukhankaSystemsBootstrap: SystemBase
{
	protected override void OnCreate()
	{
	#if RUKHANKA_DEBUG_INFO
		Debug.LogWarning("RUKHANKA_DEBUG_INFO is defined. Performance may be reduced. Do not forget remove it in release builds.\nFor debug and logging functionality configuration please see documentation");
	#endif
		
	#if !UNITY_BURST_EXPERIMENTAL_ATOMIC_INTRINSICS
		Debug.LogError($"'UNITY_BURST_EXPERIMENTAL_ATOMIC_INTRINSICS' script symbol is not defined. It is required for proper functioning of Rukhanka. Please add it by hand, or just restart editor and Rukhanka's bootstrap will add it automatically");
		return;
	#endif

	#if RUKHANKA_WITH_NETCODE
		bool isClient = World.IsClient();
		bool isServer = World.IsServer();
	#else
		bool isClient = IsClientOrLocalSimulationWorld(World);
	#endif

		//	Add client animator controller systems
		if (isClient)
		{
			var sysGroup = World.GetOrCreateSystemManaged<RukhankaAnimationSystemGroup>();
			var acs = World.CreateSystem<AnimatorControllerSystem<AnimatorControllerQuery>>();
			var facs = World.CreateSystem<FillAnimationsFromControllerSystem>();
			var aps = World.CreateSystem<AnimationProcessSystem>();
			var aas = World.CreateSystem<AnimationApplicationSystem>();
			var bvs = World.CreateSystem<BoneVisualizationSystem>();
			sysGroup.AddSystemToUpdateList(acs);
			sysGroup.AddSystemToUpdateList(facs);
			sysGroup.AddSystemToUpdateList(aps);
			sysGroup.AddSystemToUpdateList(aas);
			sysGroup.AddSystemToUpdateList(bvs);

		#if RUKHANKA_WITH_NETCODE
			var acsForPrediction = World.CreateSystem<AnimatorControllerSystem<PredictedAnimatorControllerQuery>>();
			var sysGroupPrediction = World.GetOrCreateSystemManaged<RukhankaPredictedAnimationSystemGroup>();
			sysGroupPrediction.AddSystemToUpdateList(acsForPrediction);
		#endif
		}

		//	Server systems only for Netcode enabled version
	#if RUKHANKA_WITH_NETCODE
		if (isServer)
		{
			var sysGroup = World.GetOrCreateSystemManaged<RukhankaAnimationSystemGroup>();
			var acs = World.CreateSystem<AnimatorControllerSystem<AnimatorControllerQuery>>();
			var facs = World.CreateSystem<FillAnimationsFromControllerSystem>();
			var aps = World.CreateSystem<AnimationProcessSystem>();
			var aas = World.CreateSystem<AnimationApplicationSystem>();
			sysGroup.AddSystemToUpdateList(acs);
			sysGroup.AddSystemToUpdateList(facs);
			sysGroup.AddSystemToUpdateList(aps);
			sysGroup.AddSystemToUpdateList(aas);

			var acsForPrediction = World.CreateSystem<AnimatorControllerSystem<PredictedAnimatorControllerQuery>>();
			var sysGroupPrediction = World.GetOrCreateSystemManaged<RukhankaPredictedAnimationSystemGroup>();
			sysGroupPrediction.AddSystemToUpdateList(acsForPrediction);
		}
	#endif

		//	Remove bootstrap system from world
		this.Enabled = false;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////

	public static bool IsClientOrLocalSimulationWorld(World w)
	{
		var rv =
			(w.Flags & WorldFlags.GameClient) == WorldFlags.GameClient ||
			(w.Flags & WorldFlags.Game) == WorldFlags.Game;
		return rv;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////

	protected override void OnUpdate() {}
}
}
