using Unity.Entities;
using FixedStringName = Unity.Collections.FixedString512Bytes;
using UnityEngine;
using System;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka
{

public struct FastAnimatorParameter
{
	public FixedStringName paramName;
	public uint hash;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public FastAnimatorParameter(FixedStringName name)
	{
		hash = name.CalculateHash32();
		paramName = name;
	}
	
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public FastAnimatorParameter(uint hash)
	{
		this.hash = hash;
		paramName = default;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	bool GetRuntimeParameterDataInternal(int paramIdx, DynamicBuffer<AnimatorControllerParameterComponent> runtimeParameters, out ParameterValue outData)
	{
		bool isValid = paramIdx >= 0;

		if (isValid)
		{
			outData = runtimeParameters[paramIdx].value;
		}
		else
		{
			outData = default;
		#if RUKHANKA_DEBUG_INFO
			Debug.LogError($"Could find animator parameter with name {paramName} in hash table! Returning default value!");
		#endif
		}
		return isValid;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public bool GetRuntimeParameterData(BlobAssetReference<ParameterPerfectHashTableBlob> cb, DynamicBuffer<AnimatorControllerParameterComponent> runtimeParameters, out ParameterValue outData)
	{
		var paramIdx = GetRuntimeParameterIndex(cb, runtimeParameters);
		return GetRuntimeParameterDataInternal(paramIdx, runtimeParameters, out outData);
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public bool GetRuntimeParameterData(DynamicBuffer<AnimatorControllerParameterComponent> runtimeParameters, out ParameterValue outData)
	{
		var paramIdx = GetRuntimeParameterIndex(runtimeParameters);
		return GetRuntimeParameterDataInternal(paramIdx, runtimeParameters, out outData);
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	bool SetRuntimeParameterDataInternal(int paramIdx, DynamicBuffer<AnimatorControllerParameterComponent> runtimeParameters, in ParameterValue paramData)
	{
		bool isValid = paramIdx >= 0;

		if (isValid)
		{
			var p = runtimeParameters[paramIdx];
			p.value = paramData;
			runtimeParameters[paramIdx] = p;
		}
	#if RUKHANKA_DEBUG_INFO
		else
		{
			Debug.LogError($"Could find animator parameter with name {paramName} in hash table! Setting value is failed!");
		}
	#endif
		return isValid;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public bool SetRuntimeParameterData(BlobAssetReference<ParameterPerfectHashTableBlob> cb, DynamicBuffer<AnimatorControllerParameterComponent> runtimeParameters, in ParameterValue paramData)
	{
		var paramIdx = GetRuntimeParameterIndex(cb, runtimeParameters);
		return SetRuntimeParameterDataInternal(paramIdx, runtimeParameters, paramData);
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public bool SetRuntimeParameterData(DynamicBuffer<AnimatorControllerParameterComponent> runtimeParameters, in ParameterValue paramData)
	{
		var paramIdx = GetRuntimeParameterIndex(runtimeParameters);
		return SetRuntimeParameterDataInternal(paramIdx, runtimeParameters, paramData);
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	//	Linear search variant
	public static int GetRuntimeParameterIndex(uint hash, in ReadOnlySpan<AnimatorControllerParameterComponent> parameters)
	{
		for (int i = 0; i < parameters.Length; ++i)
		{
			var p = parameters[i];
			if (p.hash == hash)
				return i;
		}
		return -1;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	//	Perfect hash table variant
	public static int GetRuntimeParameterIndex(uint hash, in BlobAssetReference<ParameterPerfectHashTableBlob> cb, in ReadOnlySpan<AnimatorControllerParameterComponent> parameters)
	{
		ref var seedTable = ref cb.Value.seedTable;
		var paramIdxShuffled = PerfectHash<UIntPerfectHashed>.QueryPerfectHashTable(ref seedTable, hash);

		if (paramIdxShuffled >= parameters.Length)
			return -1;

		var paramIdx = cb.Value.indirectionTable[paramIdxShuffled];

		var p = parameters[paramIdx];
		if (p.hash != hash)
			return -1;

		return paramIdx;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public unsafe int GetRuntimeParameterIndex(in BlobAssetReference<ParameterPerfectHashTableBlob> cb, in DynamicBuffer<AnimatorControllerParameterComponent> acpc)
	{
		var span = new ReadOnlySpan<AnimatorControllerParameterComponent>(acpc.GetUnsafePtr(), acpc.Length);
		return GetRuntimeParameterIndex(hash, cb, span);
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public unsafe int GetRuntimeParameterIndex(in DynamicBuffer<AnimatorControllerParameterComponent> acpc)
	{
		var span = new ReadOnlySpan<AnimatorControllerParameterComponent>(acpc.GetUnsafePtr(), acpc.Length);
		return GetRuntimeParameterIndex(hash, span);
	}
}
}
