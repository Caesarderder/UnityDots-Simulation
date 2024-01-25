
using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

/////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka
{
public struct AnimationTransformFlags
{
	UnsafeBitArray transformFlags;

/////////////////////////////////////////////////////////////////////////////////

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsTranslationSet(int index) => transformFlags.IsSet(index * 4 + 0);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsRotationSet(int index) => transformFlags.IsSet(index * 4 + 1);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsScaleSet(int index) => transformFlags.IsSet(index * 4 + 2);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsAbsoluteTransform(int index) => transformFlags.IsSet(index * 4 + 3);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetTranslationFlag(int index) => transformFlags.SetBitThreadSafe(index * 4 + 0);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetRotationFlag(int index) => transformFlags.SetBitThreadSafe(index * 4 + 1);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetScaleFlag(int index) => transformFlags.SetBitThreadSafe(index * 4 + 2);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetAbsoluteTransformFlag(int index) => transformFlags.SetBitThreadSafe(index * 4 + 3);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void ResetAllFlags() => transformFlags.Clear();

/////////////////////////////////////////////////////////////////////////////////

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	unsafe static UnsafeBitArray GetRigTransformFlagsInternal(void* ptr, int bufLenInBytes, int ulongOffset, int boneCount)
	{
		//	Each bone contains 4 bit flags - TRS and is bone already in absolute transform
		var sizeInUlongs = (boneCount * 4 >> 6) + 1;
		var sizeInBytes = sizeInUlongs * 8;
		var startPtr = (ulong*)ptr + ulongOffset;

	#if ENABLE_UNITY_COLLECTIONS_CHECKS
		if ((byte*)startPtr + sizeInBytes > (byte*)ptr + bufLenInBytes)
		{
			throw new InvalidOperationException($"Buffer range error! Offset and/or count exceed buffer space!");
		}
	#endif

		var rv = new UnsafeBitArray(startPtr, sizeInUlongs * 8, Allocator.None);
		return rv;
	}

///////////////////////////////////////////////////////////////////////////////////////////

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static unsafe AnimationTransformFlags CreateFromBufferRW(in NativeList<ulong> buf, int bufElementOffset, int boneCount)
	{
		var rwPtr = buf.GetUnsafePtr();
		var rv = new AnimationTransformFlags() { transformFlags = GetRigTransformFlagsInternal(rwPtr, buf.Length * sizeof(ulong), bufElementOffset, boneCount) };
		return rv;
	}

///////////////////////////////////////////////////////////////////////////////////////////

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static unsafe AnimationTransformFlags CreateFromBufferRO(in NativeList<ulong> buf, int bufElementOffset, int boneCount)
	{
		var roPtr = buf.GetUnsafeReadOnlyPtr();
		var rv = new AnimationTransformFlags() { transformFlags = GetRigTransformFlagsInternal(roPtr, buf.Length * sizeof(ulong), bufElementOffset, boneCount) };
		return rv;
	}
}
}
