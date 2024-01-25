
using System.Runtime.CompilerServices;
using Unity.Entities;
using Unity.Mathematics;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka
{
	public enum BindingType: short
{
	Translation,
	Quaternion,
	EulerAngles,
	HumanMuscle,
	Scale,
	Unknown
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public struct KeyFrame
{
	public float v;
	public float inTan, outTan;
	public float time;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public struct AnimationCurve
{
	public BindingType bindingType;
	public short channelIndex; // 0, 1, 2, 3 -> x, y, z, w
	public BlobArray<KeyFrame> keyFrames;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public struct BoneClipBlob
{
#if RUKHANKA_DEBUG_INFO
	public BlobString name;
#endif
	public Hash128 hash;
	public bool isHumanMuscleClip;
	public BlobArray<AnimationCurve> animationCurves;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public struct AvatarMaskBlob
{
#if RUKHANKA_DEBUG_INFO
	public BlobString name;
	public BlobArray<BlobString> includedBoneNames;
#endif
	public Hash128 hash;
	public BlobArray<Hash128> includedBoneHashes;
	public uint humanBodyPartsAvatarMask;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public struct AnimationClipBlob
{
#if RUKHANKA_DEBUG_INFO
	public BlobString name;
#endif
	public Hash128 hash;
	public BlobArray<BoneClipBlob> bones;
	public BlobArray<int2> bonesPerfectHashSeedTable;
	public BlobArray<BoneClipBlob> curves;
	uint flags;
	public float cycleOffset;
	public float length;
	public float additiveReferencePoseTime;
	public bool looped { get => GetFlag(1); set => SetFlag(1, value); }
	public bool loopPoseBlend { get => GetFlag(2); set => SetFlag(2, value); }
	public bool hasRootMotionCurves { get => GetFlag(3); set => SetFlag(3, value); }
	
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	void SetFlag(int index, bool value)
	{
		var v = 1u << index;
		var mask = ~v;
		var valueBits = math.select(0, v, value);
		flags = flags & mask | valueBits;
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	bool GetFlag(int index)
	{
		var v = 1u << index;
		return (flags & v) != 0;
	}
}
}
