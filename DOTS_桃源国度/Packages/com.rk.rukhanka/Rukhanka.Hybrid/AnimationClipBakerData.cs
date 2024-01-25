using System;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using FixedStringName = Unity.Collections.FixedString512Bytes;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka.Hybrid
{
//	RTP - Ready to process
namespace RTP
{
public struct AnimationClip: IDisposable
{
	public FixedStringName name;
	public UnsafeList<BoneClip> bones;
	public UnsafeList<BoneClip> curves;
	public bool looped;
	public bool loopPoseBlend;
	public float cycleOffset;
	public float length;
	public float additiveReferencePoseTime;
	public bool hasRootMotionCurves;
	public Hash128 hash;

	public void Dispose()
	{
		foreach (var a in bones) a.Dispose();
		foreach (var a in curves) a.Dispose();

		bones.Dispose();
	}
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public struct BoneClip: IEquatable<Hash128>, IDisposable
{
	public FixedStringName name;
	public Hash128 nameHash;
	public bool isHumanMuscleClip;
	public UnsafeList<AnimationCurve> animationCurves;

	public bool Equals(Hash128 o) => o == nameHash;

	public void SetName(string n)
	{
		name = n;
		nameHash = name.CalculateHash128();
	}

	public void DisposeCurves()
	{
		foreach (var a in animationCurves) a.Dispose();
		animationCurves.Clear();
	}
	
	public void Dispose()
	{
		DisposeCurves();
		animationCurves.Dispose();
	}
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public struct AnimationCurve: IDisposable
{
	public BindingType bindingType;
	public short channelIndex; // 0, 1, 2, 3 -> x, y, z, w
	public UnsafeList<KeyFrame> keyFrames;

	public void Dispose() => keyFrames.Dispose();
}

} // RTP
}


