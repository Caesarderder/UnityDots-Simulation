using Unity.Entities;
using FixedStringName = Unity.Collections.FixedString512Bytes;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka
{
public readonly partial struct AnimatorStateQueryAspect: IAspect
{
	readonly DynamicBuffer<AnimatorControllerLayerComponent> layersArr;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public struct RuntimeStateInfo
	{
	#if RUKHANKA_DEBUG_INFO
		public FixedStringName name;
	#endif
		public uint hash;
		public float normalizedTime;
	}

	public struct RuntimeTransitionInfo
	{
	#if RUKHANKA_DEBUG_INFO
		public FixedStringName name;
	#endif
		public uint hash;
		public float normalizedTime;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public RuntimeStateInfo GetLayerCurrentStateInfo(int layerIndex)
	{
		if (layersArr.Length <= layerIndex)
			return default;

		var layerRuntimeData = layersArr[layerIndex];
		ref var layerBlob = ref layerRuntimeData.controller.Value.layers[layerIndex];
		var curStateID = layerRuntimeData.rtd.srcState.id;

		if (curStateID < 0 || curStateID >= layerBlob.states.Length)
			return default;

		var rv = new RuntimeStateInfo()
		{
		#if RUKHANKA_DEBUG_INFO
			name = layerBlob.states[curStateID].name.ToFixedString(),	
		#endif
			hash = layerBlob.states[curStateID].hash,
			normalizedTime = layerRuntimeData.rtd.srcState.normalizedDuration,
		};

		return rv;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public RuntimeTransitionInfo GetLayerCurrentTransitionInfo(int layerIndex)
	{
		if (layersArr.Length <= layerIndex)
			return default;

		var layerRuntimeData = layersArr[layerIndex];
		ref var layerBlob = ref layerRuntimeData.controller.Value.layers[layerIndex];
		var curTransitionID = layerRuntimeData.rtd.activeTransition.id;
		var curStateID = layerRuntimeData.rtd.srcState.id;

		if (curTransitionID < 0 || curStateID < 0 || curStateID >= layerBlob.states.Length)
			return default;

		var rv = new RuntimeTransitionInfo()
		{
		#if RUKHANKA_DEBUG_INFO
			name = layerBlob.states[curStateID].transitions[curTransitionID].name.ToFixedString(),	
		#endif
			hash = layerBlob.states[curStateID].transitions[curTransitionID].hash,
			normalizedTime = layerRuntimeData.rtd.activeTransition.normalizedDuration
		};

		return rv;
	}
}
}
