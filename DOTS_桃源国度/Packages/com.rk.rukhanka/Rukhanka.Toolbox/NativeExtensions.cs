using System;
using Unity.Burst.Intrinsics;
using Unity.Collections.LowLevel.Unsafe;

/////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka
{
public static class NativeExtensions
{
	public static unsafe void SetBitThreadSafe(this UnsafeBitArray ba, int pos)
    {
#if UNITY_BURST_EXPERIMENTAL_ATOMIC_INTRINSICS
		var idx = pos >> 6;
		var shift = pos & 0x3f;
		var value = 1ul << shift;
		Common.InterlockedOr(ref UnsafeUtility.AsRef<ulong>(ba.Ptr + idx), value);
#endif
    }

}
}
