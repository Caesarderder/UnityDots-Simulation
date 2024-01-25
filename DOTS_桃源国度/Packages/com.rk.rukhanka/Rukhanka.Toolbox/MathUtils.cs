
using System;
using Unity.Collections;
using Unity.Mathematics;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka
{
public static class MathUtils
{
	public static quaternion ShortestRotation(quaternion from, quaternion to)
	{
		uint4 sign = math.asuint(math.dot(from, to)) & 0x80000000;
		var rv = math.asfloat(sign ^ math.asuint(to.value));
		return rv;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public static quaternion FromToRotationForNormalizedVectors(float3 from, float3 to)
	{
		var w = math.cross(from, to);
		var q = new quaternion(w.x, w.y, w.z, math.dot(from, to) + 1);
		q = math.normalize(q);
		return q;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	//	Shuffle array according to given indices
	public static void ShuffleArray<T>(Span<T> arr, in NativeArray<int> shuffleIndices) where T: unmanaged
	{
		if (arr.Length < 2) return;
		if (arr.Length != shuffleIndices.Length) return;
	
		Span<T> scatterArr = stackalloc T[arr.Length];
		for (int i = 0; i < arr.Length; ++i)
		{
			var shuffleIndex = shuffleIndices[i];
			var v = arr[shuffleIndex];
			scatterArr[i] = v;
		}

		for (int i = 0; i < arr.Length; ++i)
		{
			arr[i] = scatterArr[i];
		}
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public static void Swap<T>(ref T a, ref T b)
	{
		var tmp = a;
		a = b;
		b = tmp;
	}
}
}
