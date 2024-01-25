
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using System;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka
{
public static class CollectionUtils
{
	public static unsafe NativeArray<T> AsArray<T>(this NativeSlice<T> v) where T: unmanaged
	{
		var ptr = v.GetUnsafePtr();
		var rv = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(ptr, v.Length, Allocator.None);
		return rv;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public static unsafe UnsafeList<T> AsUnsafeList<T>(this NativeSlice<T> v) where T: unmanaged
	{
		var ptr = (T*)v.GetUnsafePtr();
		var rv = new UnsafeList<T>(ptr, v.Length);
		return rv;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	static void ValidateSpanCreationParameters<T>(this NativeList<T> v, int startIndex, int length) where T: unmanaged
	{
		if (startIndex >= v.Length)
		{
			throw new InvalidOperationException($"Requested span start index exceed list size (Start index {startIndex}, list length {v.Length})!");
		}

		if (startIndex + length > v.Length)
		{
			throw new InvalidOperationException($"Requested span exceed end of list (Start index {startIndex}, requested length {length}, list length {v.Length})!");
		}
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public static unsafe Span<T> GetSpan<T>(this NativeList<T> v, int startIndex, int length) where T: unmanaged
	{
#if ENABLE_UNITY_COLLECTIONS_CHECKS
		ValidateSpanCreationParameters(v, startIndex, length);
#endif
		var rv = new Span<T>(v.GetUnsafePtr() + startIndex, length);
		return rv;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public static unsafe ReadOnlySpan<T> GetReadOnlySpan<T>(this NativeList<T> v, int startIndex, int length) where T: unmanaged
	{
#if ENABLE_UNITY_COLLECTIONS_CHECKS
		ValidateSpanCreationParameters(v, startIndex, length);
#endif
		var rv = new ReadOnlySpan<T>(v.GetUnsafeReadOnlyPtr() + startIndex, length);
		return rv;
	}

/////////////////////////////////////////////////////////////////////////////////

    public static unsafe Span<T> AsSpan<T>(this UnsafeList<T> l) where T: unmanaged
    {
	    var rv = new Span<T>(l.Ptr, l.Length);
	    return rv;
    }
}
}
