using System;
using Unity.Collections.LowLevel.Unsafe;
using Hash128 = Unity.Entities.Hash128;
using FixedStringName = Unity.Collections.FixedString512Bytes;
using Unity.Mathematics;
using Unity.Collections;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka.Hybrid
{
//	RTP - Ready to process
namespace RTP
{ 

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public struct State: IEquatable<int>, IDisposable
{
	public int hashCode;
	public FixedStringName name;
	public float speed;
	public FixedStringName speedMultiplierParameter;
	public UnsafeList<Transition> transitions;
	public FixedStringName timeParameter;
	public float cycleOffset;
	public FixedStringName cycleOffsetParameter;
	public Motion motion;

	public bool Equals(int o) => o == hashCode;
	public void Dispose()
	{
		foreach (var a in transitions) a.Dispose();
		transitions.Dispose();
		motion.Dispose();
	}
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public struct ChildMotion: IDisposable
{
	public Motion motion;
	public float threshold;
	public float timeScale;
	public FixedStringName directBlendParameterName;
	public float2 position2D;

	public void Dispose() => motion.Dispose();
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public struct Motion: IDisposable
{
	public FixedStringName name;
	public MotionBlob.Type type;
	public int animationIndex;
	public BlendTree blendTree;

	public void Dispose() => blendTree.Dispose();
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public struct BlendTree: IDisposable
{
	public FixedStringName name;
	public FixedStringName blendParameterName;
	public FixedStringName blendParameterYName;
	public bool normalizeBlendValues;
	public UnsafeList<ChildMotion> motions;

	public void Dispose()
	{
		foreach (var a in motions) a.Dispose();

		motions.Dispose();
	}
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public struct Transition: IDisposable
{
	public FixedStringName name;
	public float duration;
	public float exitTime;
	public float offset;
	public bool hasExitTime;
	public bool hasFixedDuration;
	public bool soloFlag;
	public bool muteFlag;
	public bool canTransitionToSelf;
	public int targetStateHash;
	public UnsafeList<Condition> conditions;

	public void Dispose() => conditions.Dispose();
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public struct Condition
{
	public FixedStringName name;
	public FixedStringName paramName;
	public ParameterValue threshold;
	public AnimatorConditionMode conditionMode;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public struct Layer: IDisposable
{
	public FixedStringName name;
	public int defaultStateIndex;
	public float weight;
	public AnimationBlendingMode blendMode;
	public UnsafeList<Transition> anyStateTransitions;
	public UnsafeList<State> states;
	public AvatarMask avatarMask;

	public void Dispose()
	{
		foreach (var a in anyStateTransitions) a.Dispose();
		foreach (var a in states) a.Dispose();

		anyStateTransitions.Dispose();
		states.Dispose();
		avatarMask.Dispose();
	}
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public struct Parameter: IEquatable<FixedStringName>
{
	public FixedStringName name;
	public ParameterValue defaultValue;
	public ControllerParameterType type;

	public bool Equals(FixedStringName o) => o == name;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public struct Controller: IDisposable
{
	public FixedStringName name;
	public UnsafeList<Layer> layers;
	public UnsafeList<Parameter> parameters;
	public UnsafeList<AnimationClip> animationClips;

	public void Dispose()
	{
		foreach (var a in layers) a.Dispose();
		foreach (var a in animationClips) a.Dispose();

		layers.Dispose();
		parameters.Dispose();
		animationClips.Dispose();
	}
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public struct AvatarMask: IDisposable
{
	public FixedStringName name;
	public Hash128 hash;
	public NativeList<FixedStringName> includedBonePaths;
	public uint humanBodyPartsAvatarMask;

	public void Dispose() => includedBonePaths.Dispose();
}

} // RTP
}

