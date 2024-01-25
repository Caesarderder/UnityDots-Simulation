using System;
using Unity.Collections;
using Unity.Entities;

/////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka
{ 

public struct RuntimeAnimatorData
{
	public struct StateRuntimeData
	{
		public int id;
		public float normalizedDuration;

		public static StateRuntimeData MakeDefault()
		{
			var rv = new StateRuntimeData();
			rv.id = -1;
			rv.normalizedDuration = 0;
			return rv;
		}
	}

	public StateRuntimeData srcState;
	public StateRuntimeData dstState;
	public StateRuntimeData activeTransition;

	public static RuntimeAnimatorData MakeDefault()
	{
		var rv = new RuntimeAnimatorData();
		rv.srcState = StateRuntimeData.MakeDefault();
		rv.dstState = StateRuntimeData.MakeDefault();
		rv.activeTransition = StateRuntimeData.MakeDefault();
		return rv;
	}
}

/////////////////////////////////////////////////////////////////////////////////////////////////////

public struct AnimatorControllerLayerComponent: IBufferElementData, IEnableableComponent
{
	public BlobAssetReference<ControllerBlob> controller;
	public int layerIndex;
	public float weight;
	public RuntimeAnimatorData rtd;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////

public struct AnimatorControllerParameterComponent: IBufferElementData
{
#if RUKHANKA_DEBUG_INFO
	public FixedString64Bytes name;
#endif
	public uint hash;
	public ControllerParameterType type;
	public ParameterValue value;

	public float FloatValue
	{
		get => value.floatValue;
		set => this.value.floatValue = value;
	}

	public int IntValue
	{
		get => value.intValue;
		set => this.value.intValue= value;
	}

	public bool BoolValue
	{
		get => value.boolValue;
		set => this.value.boolValue = value;
	}

	public void SetTrigger()
	{
		value.boolValue = true;
	}
}

/////////////////////////////////////////////////////////////////////////////////////////////////////

public struct AnimatorControllerParameterIndexTableComponent: IComponentData
{
	public BlobAssetReference<ParameterPerfectHashTableBlob> seedTable;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////

public struct MotionIndexAndWeight: IComparable<MotionIndexAndWeight>
{
	public int motionIndex;
	public float weight;

	public int CompareTo(MotionIndexAndWeight a)
	{
		if (weight < a.weight)
			return 1;
		else if (weight > a.weight)
			return -1;

		return 0;
	}
};
}
