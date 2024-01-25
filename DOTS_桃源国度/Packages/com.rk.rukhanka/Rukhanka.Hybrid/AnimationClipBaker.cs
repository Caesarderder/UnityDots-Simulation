#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor;
using UnityEngine;
using System.Linq;
using Rukhanka.Hybrid.RTP;
using Unity.Assertions;
using FixedStringName = Unity.Collections.FixedString512Bytes;
using Unity.Mathematics;
using Unity.Burst;
using AnimationClip = UnityEngine.AnimationClip;
using Hash128 = Unity.Entities.Hash128;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka.Hybrid
{ 
[BurstCompile]
public partial class AnimationClipBaker
{
	enum BoneType
	{
		Generic,
		MotionCurve,
		RootCurve
	}

	struct ParsedCurveBinding
	{
		public BindingType bindingType;
		public short channelIndex;
		public BoneType boneType;
		public FixedStringName boneName;

		public bool IsValid() => boneName.Length > 0;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	static ValueTuple<string, string> SplitPath(string path)
	{
		var arr = path.Split('/');
		Assert.IsTrue(arr.Length > 0);
		var rv = (arr.Last(), arr.Length > 1 ? arr[arr.Length - 2] : "");
		return rv;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	static (BindingType, BoneType) PickGenericBindingTypeByString(string bindingString) => bindingString switch
	{
		"m_LocalPosition" => (BindingType.Translation, BoneType.Generic),
		//"MotionT" => (BindingType.Translation, BoneType.MotionCurve),
		//"RootT" => (BindingType.Translation, BoneType.RootCurve),
		"m_LocalRotation" => (BindingType.Quaternion, BoneType.Generic),
		//"MotionQ" => (BindingType.Quaternion, BoneType.MotionCurve),
		//"RootQ" => (BindingType.Quaternion, BoneType.RootCurve),
		"localEulerAngles" => (BindingType.EulerAngles, BoneType.Generic),
		"localEulerAnglesRaw" => (BindingType.EulerAngles, BoneType.Generic),
		"m_LocalScale" => (BindingType.Scale, BoneType.Generic),
		_ => (BindingType.Unknown, BoneType.Generic)
	};

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	static short ChannelIndexFromString(string c) => c switch
	{
		"x" => 0,
		"y" => 1,
		"z" => 2,
		"w" => 3,
		_ => 999
	};

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	static FixedStringName ConstructBoneClipName(ValueTuple<string, string> nameAndPath, BoneType bt)
	{
		FixedStringName rv;
		//	Empty name string is unnamed root bone
		if (nameAndPath.Item1.Length == 0 && nameAndPath.Item2.Length == 0)
		{
			rv = SpecialBones.unnamedRootBoneName;
		}
		else
		{
			rv = new FixedStringName(nameAndPath.Item1);
		}
		return rv;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	static RTP.AnimationCurve PrepareAnimationCurve(Keyframe[] keysArr, ParsedCurveBinding pb)
	{
		var animCurve = new RTP.AnimationCurve();
		animCurve.channelIndex = pb.channelIndex;
		animCurve.bindingType = pb.bindingType;
		animCurve.keyFrames = new UnsafeList<KeyFrame>(keysArr.Length, Allocator.Persistent);

		foreach (var k in keysArr)
		{
			var kf = new KeyFrame()
			{
				time = k.time,
				inTan = k.inTangent,
				outTan = k.outTangent,
				v = k.value
			};
			animCurve.keyFrames.Add(kf);
		}
		return animCurve;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	static int GetOrCreateBoneClipHolder(ref UnsafeList<RTP.BoneClip> clipsArr, in Hash128 nameHash, BindingType bt)
	{
		var rv = clipsArr.IndexOf(nameHash);
		if (rv < 0)
		{
			rv = clipsArr.Length;
			var bc = new RTP.BoneClip();
			bc.name = "MISSING_BONE_NAME";
			bc.nameHash = nameHash;
			bc.isHumanMuscleClip = bt == BindingType.HumanMuscle;
			bc.animationCurves = new UnsafeList<RTP.AnimationCurve>(32, Allocator.Persistent);
			clipsArr.Add(bc);
		}
		return rv;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	static int GetOrCreateBoneClipHolder(ref UnsafeList<RTP.BoneClip> clipsArr, in FixedStringName name, BindingType bt)
	{
		//	Hash for generic curves must match parameter name hash which is 32 bit instead od 128
		var nameHash = bt == BindingType.Unknown ? new Hash128(name.CalculateHash32(), 0, 0, 0) : name.CalculateHash128();
		var rv = GetOrCreateBoneClipHolder(ref clipsArr, nameHash, bt);
		ref var c = ref clipsArr.ElementAt(rv);
		c.name = name;
		return rv;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	static RTP.BoneClip MakeBoneClipCopy(in RTP.BoneClip bc)
	{
		var rv = bc;
		rv.animationCurves = new UnsafeList<RTP.AnimationCurve>(bc.animationCurves.Length, Allocator.Persistent);
		for (int i = 0; i < bc.animationCurves.Length; ++i)
		{
			var inKf = bc.animationCurves[i].keyFrames;
			var outKf = new UnsafeList<KeyFrame>(inKf.Length, Allocator.Persistent);
			for (int j = 0; j < inKf.Length; ++j)
			{
				outKf.Add(inKf[j]);
			}
			var ac = bc.animationCurves[i];
			ac.keyFrames = outKf;
			rv.animationCurves.Add(ac);
		}
		return rv;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	static void DebugLoggging(RTP.AnimationClip ac, bool hasRootCurves)
	{
#if RUKHANKA_DEBUG_INFO
		var dc = GameObject.FindObjectOfType<RukhankaDebugConfiguration>();
		var logClipBaking = dc != null && dc.logClipBaking;
		if (!logClipBaking) return;

		Debug.Log($"Baking animation clip '{ac.name}'. Tracks: {ac.bones.Length}. User curves: {ac.curves.Length}. Length: {ac.length}s. Looped: {ac.looped}. Has root curves: {hasRootCurves}");
#endif
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	static ParsedCurveBinding ParseGenericCurveBinding(EditorCurveBinding b, AnimationClip ac)
	{
		var rv = new ParsedCurveBinding();

		var t = b.propertyName.Split('.');
		var propName = t[0];
		var channel = t.Length > 1 ? t[1] : "";

		rv.channelIndex = ChannelIndexFromString(channel);
		(rv.bindingType, rv.boneType) = PickGenericBindingTypeByString(propName);

		//	Ignore root curve if motion curve is present in clip
		if (rv.boneType == BoneType.RootCurve && ac.hasMotionCurves)
			return rv;

		if (rv.bindingType != BindingType.Unknown)
		{
			var nameAndPath = SplitPath(b.path);
			rv.boneName = ConstructBoneClipName(nameAndPath, rv.boneType);
		}
		else
		{
			rv.boneName = new FixedStringName(propName);
		}

		return rv;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	static int GetHumanBoneIndexForHumanName(in HumanDescription hd, FixedStringName humanBoneName)
	{
		var humanBoneIndexInAvatar = Array.FindIndex(hd.human, x => x.humanName == humanBoneName);
		return humanBoneIndexInAvatar;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	static ParsedCurveBinding ParseHumanoidCurveBinding(EditorCurveBinding b, Avatar avatar)
	{
		if (!humanoidMappingTable.TryGetValue(b.propertyName, out var rv))
			return rv;

		var hd = avatar.humanDescription;
		var humanBoneIndexInAvatar = GetHumanBoneIndexForHumanName(hd, rv.boneName);
		if (humanBoneIndexInAvatar < 0)
			return rv;

		if (rv.bindingType == BindingType.HumanMuscle)
		{
			var humanBoneDef = hd.human[humanBoneIndexInAvatar];
			rv.boneName = humanBoneDef.boneName;
		}

		return rv;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	static ParsedCurveBinding ParseCurveBinding(AnimationClip ac, EditorCurveBinding b, Avatar avatar)
	{
		var rv = ac.isHumanMotion ?
			ParseHumanoidCurveBinding(b, avatar) :
			ParseGenericCurveBinding(b, ac);

		return  rv;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	static void AddKeyFrameFromFloatValue(ref UnsafeList<KeyFrame> kfArr, float2 key, float v)
	{
		var kf = new KeyFrame()
		{
			time = key.x,
			inTan = key.y,
			outTan = key.y,
			v = v
		};
		kfArr.Add(kf);
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	[BurstCompile]
	static void ComputeTangents(ref RTP.AnimationCurve ac)
	{
		for (int i = 0; i < ac.keyFrames.Length; ++i)
		{
			var p0 = i == 0 ? ac.keyFrames[0] : ac.keyFrames[i - 1];
			var p1 = ac.keyFrames[i];
			var p2 = i == ac.keyFrames.Length - 1 ? ac.keyFrames[i] : ac.keyFrames[i + 1];

			var outV = math.normalizesafe(new float2(p2.time, p2.v) - new float2(p1.time, p1.v));
			var outTan = outV.x > 0.0001f ? outV.y / outV.x : 0;

			var inV = math.normalizesafe(new float2(p1.time, p1.v) - new float2(p0.time, p0.v));
			var inTan = inV.x > 0.0001f ? inV.y / inV.x : 0;

			var dt = math.abs(inTan) + math.abs(outTan);
			var f = dt > 0 ? math.abs(inTan) / dt : 0;

			var avgTan = math.lerp(inTan, outTan, f);

			var k = ac.keyFrames[i];
			k.outTan = avgTan;
			k.inTan = avgTan;
			ac.keyFrames[i] = k;
		}
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	static NativeList<float> CreateKeyframeTimes(float animationLength, float dt)
	{
		var numFrames = (int)math.ceil(animationLength / dt);

		var rv = new NativeList<float>(numFrames, Allocator.Temp);

		float curTime = 0;
		for (;;)
		{
			rv.Add(curTime);
			curTime += dt;
			if (curTime > animationLength)
			{
				rv.Add(animationLength);
				break;
			}
		}
		return rv;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	static void ReadCurvesFromTransform(Transform tr, NativeArray<RTP.AnimationCurve> animCurves, float time)
	{
		quaternion q = tr.localRotation;
		float3 t = tr.localPosition;

		var vArr = new NativeArray<float>(7, Allocator.Temp);
		vArr[0] = t.x;
		vArr[1] = t.y;
		vArr[2] = t.z;
		vArr[3] = q.value.x;
		vArr[4] = q.value.y;
		vArr[5] = q.value.z;
		vArr[6] = q.value.w;

		for (int l = 0; l < vArr.Length; ++l)
		{
			var keysArr = animCurves[l];
			AddKeyFrameFromFloatValue(ref keysArr.keyFrames, time, vArr[l]);
			animCurves[l] = keysArr;
		}
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	static void SetCurvesToAnimation(ref UnsafeList<BoneClip> outBoneClips, in Hash128 boneHash, NativeArray<RTP.AnimationCurve> animCurve)
	{
		var boneId = GetOrCreateBoneClipHolder(ref outBoneClips, boneHash, BindingType.Translation);
		ref var bc = ref outBoneClips.ElementAt(boneId);
		bc.DisposeCurves();

		for (var i = 0; i < animCurve.Length; ++i)
		{
			var hac = animCurve[i];
			ComputeTangents(ref hac);
			bc.animationCurves.Add(hac);
		}
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	static void SampleUnityAnimation(AnimationClip ac, Animator anm, ValueTuple<Transform, Hash128>[] trs, bool applyRootMotion, ref UnsafeList<RTP.BoneClip> boneClips)
	{
		if (trs.Length == 0)
			return;
		
		var sampleAnimationFrameTime = 1 / 60.0f;
		var keysList = CreateKeyframeTimes(ac.length, sampleAnimationFrameTime);

		var channelDesc = new ValueTuple<BindingType, short>[]
		{
			(BindingType.Translation, 0),
			(BindingType.Translation, 1),
			(BindingType.Translation, 2),
			(BindingType.Quaternion, 0),
			(BindingType.Quaternion, 1),
			(BindingType.Quaternion, 2),
			(BindingType.Quaternion, 3),
		};
 
		var rac = anm.runtimeAnimatorController;
		var origPos = anm.transform.position;
		var origRot = anm.transform.rotation;
		var origRootMotion = anm.applyRootMotion;
		var prevAnmCulling = anm.cullingMode;
		
		anm.runtimeAnimatorController = null;
		anm.cullingMode = AnimatorCullingMode.AlwaysAnimate;
		anm.applyRootMotion = true;
		anm.transform.position = Vector3.zero;
		anm.transform.rotation = quaternion.identity;
		
		var animationCurves = new NativeArray<RTP.AnimationCurve>(channelDesc.Length * trs.Length, Allocator.Temp);
		for (int k = 0; k < animationCurves.Length; ++k)
		{
			animationCurves[k] = new RTP.AnimationCurve()
			{
				bindingType = channelDesc[k % channelDesc.Length].Item1,
				channelIndex = channelDesc[k % channelDesc.Length].Item2,
				keyFrames = new UnsafeList<KeyFrame>(keysList.Length, Allocator.Persistent)
			};
		}

		for (int i = 0; i < keysList.Length; ++i)
		{
			var time = keysList[i];
			var dt = i == 0 ? 0.0000001f : time - keysList[i - 1];
			ac.SampleAnimation(anm.gameObject, time);

			for (int l = 0; l < trs.Length; ++l)
			{
				var tr = trs[l].Item1;
				var curvesSpan = animationCurves.GetSubArray(l * channelDesc.Length, channelDesc.Length);
				ReadCurvesFromTransform(tr, curvesSpan, time);
			}
		}

		for (int l = 0; l < trs.Length; ++l)
		{
			var curvesSpan = animationCurves.GetSubArray(l * channelDesc.Length, channelDesc.Length);
			SetCurvesToAnimation(ref boneClips, trs[l].Item2, curvesSpan);
		}

		anm.cullingMode = prevAnmCulling;
		anm.runtimeAnimatorController = rac;
		anm.transform.position = origPos;
		anm.transform.rotation = origRot;
		anm.applyRootMotion = origRootMotion;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	static (Transform, Hash128) GetRootBoneTransform(Animator anm)
	{
		if (anm.avatar.isHuman)
		{
			var hipsTransform = anm.GetBoneTransform(HumanBodyBones.Hips);
			var hd = anm.avatar.humanDescription;
			var humanBoneIndexInDesc = GetHumanBoneIndexForHumanName(hd, "Hips");
			var rigHipsBoneName = new FixedStringName(hd.human[humanBoneIndexInDesc].boneName).CalculateHash128();
			return (hipsTransform, rigHipsBoneName);
		}

		var rootBoneName =  anm.avatar.GetRootMotionNodeName();
		var rootBoneNameHash = new FixedStringName(rootBoneName).CalculateHash128();
		var rootBoneTransform = Toolbox.FindChildRecursively(anm.transform, rootBoneName);
		return (rootBoneTransform, rootBoneNameHash);
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	static void SampleMissingCurves(AnimationClip ac, Animator anm, ref UnsafeList<RTP.BoneClip> boneClips)
	{
		var trs = new List<ValueTuple<Transform, Hash128>>();
		var entityRootTransform = anm.transform;
		var rootBoneTransformData = GetRootBoneTransform(anm);

		if (anm.isHuman)
			trs.Add(rootBoneTransformData);

		//	Sample curves for non-rootmotion animations
		SampleUnityAnimation(ac, anm, trs.ToArray(), false, ref boneClips);
		
		//	Sample root motion curves
		trs.Clear();
		
		var entityRootHash = SpecialBones.unnamedRootBoneName.CalculateHash128();
		AnimationProcessSystem.ComputeBoneAnimationJob.ModifyBoneHashForRootMotion(ref entityRootHash);
		trs.Add((entityRootTransform, entityRootHash));
		
		//	Modify bone hash to separate root motion tracks and ordinary tracks
		AnimationProcessSystem.ComputeBoneAnimationJob.ModifyBoneHashForRootMotion(ref rootBoneTransformData.Item2);
		trs.Add(rootBoneTransformData);
		
		SampleUnityAnimation(ac, anm, trs.ToArray(), true, ref boneClips);
	}
	
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public static RTP.AnimationClip PrepareAnimationComputeData(AnimationClip ac, Animator animator)
	{
		var acSettings = AnimationUtility.GetAnimationClipSettings(ac);

		var rv = new RTP.AnimationClip();
		rv.name = ac.name;
		rv.bones = new UnsafeList<RTP.BoneClip>(100, Allocator.Persistent);
		rv.curves = new UnsafeList<RTP.BoneClip>(100, Allocator.Persistent);
		rv.length = ac.length;
		rv.looped = ac.isLooping;
		rv.hash = new Hash128((uint)ac.GetHashCode(), 0, 0, 0);
		rv.loopPoseBlend = acSettings.loopBlend;
		rv.cycleOffset = acSettings.cycleOffset;
		rv.additiveReferencePoseTime = acSettings.additiveReferencePoseTime;
		rv.hasRootMotionCurves = ac.hasRootCurves || ac.hasMotionCurves;

		var bindings = AnimationUtility.GetCurveBindings(ac);

		foreach (var b in bindings)
		{
			var ec = AnimationUtility.GetEditorCurve(ac, b);
			var pb = ParseCurveBinding(ac, b, animator.avatar);
			
			if (!pb.IsValid()) continue;

			var animCurve = PrepareAnimationCurve(ec.keys, pb);
			var isGenericCurve = pb.bindingType == BindingType.Unknown;

			var curveHolder = isGenericCurve ? rv.curves : rv.bones;

			if (pb.channelIndex < 0 && !isGenericCurve) continue;

			var boneId = GetOrCreateBoneClipHolder(ref curveHolder, pb.boneName, pb.bindingType);
			var boneClip = curveHolder[boneId];
			boneClip.animationCurves.Add(animCurve);
			curveHolder[boneId] = boneClip;

			if (isGenericCurve)
				rv.curves = curveHolder;
			else
				rv.bones = curveHolder;
		}
		
		if (animator.avatar != null)
		{
			//	Sample root and hips curves and from unity animations. Maybe sometime I will figure out all RootT/RootQ and body pose generation formulas and this step could be replaced with generation.
			SampleMissingCurves(ac, animator, ref rv.bones);
			
			//	Because we have modified tracks we need to make animation hash unique
			rv.hash.Value.y = (uint)animator.avatar.GetHashCode();
		}

		DebugLoggging(rv, ac.hasMotionCurves || ac.hasRootCurves);

		return rv;
	}
}
}

#endif