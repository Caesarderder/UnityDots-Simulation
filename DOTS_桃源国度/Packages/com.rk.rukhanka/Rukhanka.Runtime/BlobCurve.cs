using System;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka
{
public static class BlobCurve
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static public float EvaluateBezierCurve(KeyFrame f0, KeyFrame f1, float l)
	{
		float dt = f1.time - f0.time;
		float m0 = f0.outTan * dt;
		float m1 = f1.inTan * dt;

		float t2 = l * l;
		float t3 = t2 * l;

		float a = 2 * t3 - 3 * t2 + 1;
		float b = t3 - 2 * t2 + l;
		float c = t3 - t2;
		float d = -2 * t3 + 3 * t2;

		float rv = a * f0.v + b * m0 + c * m1 + d * f1.v;
		return rv;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static unsafe float SampleAnimationCurve(ref BlobArray<KeyFrame> kf, float time)
	{
		var arr = new ReadOnlySpan<KeyFrame>(kf.GetUnsafePtr(), kf.Length);
		return SampleAnimationCurveBinarySearch(arr, time);
	}
	
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static unsafe float SampleAnimationCurve(in UnsafeList<KeyFrame> kf, float time)
	{
		var arr = new ReadOnlySpan<KeyFrame>(kf.Ptr, kf.Length);
		return SampleAnimationCurveBinarySearch(arr, time);
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float SampleAnimationCurveBinarySearch(in ReadOnlySpan<KeyFrame> kf, float time)
	{
		var startIndex = 0;
		var endIndex = kf.Length;
		var less = true;
		var greater = true;
		KeyFrame frame0 = default, frame1 = default;

		if (kf.Length < 3)
			return SampleAnimationCurveLinearSearch(kf, time);

		while(endIndex - startIndex >= 1 && (less || greater) && endIndex > 1)
		{
			var middleIndex = (endIndex + startIndex) / 2;
			frame1 = kf[middleIndex];
			frame0 = kf[middleIndex - 1];
			
			less = time < frame0.time;
			greater = time > frame1.time;

			startIndex = math.select(startIndex, middleIndex + 1, greater);
			endIndex = math.select(endIndex, middleIndex, less);
		}

		if (less)
			return kf[0].v;

		if (greater)
			return kf[^1].v;

		float f = (time - frame0.time) / (frame1.time - frame0.time);
		return EvaluateBezierCurve(frame0, frame1, f);
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float SampleAnimationCurveLinearSearch(in ReadOnlySpan<KeyFrame> kf, float time)
	{
		for (int i = 0; i < kf.Length; ++i)
		{
			var frame1 = kf[i];
			if (frame1.time >= time)
			{
				if (i == 0)
					return kf[i].v;
				var frame0 = kf[i - 1];

				float f = (time - frame0.time) / (frame1.time - frame0.time);
				return EvaluateBezierCurve(frame0, frame1, f);
			}
		}
		return kf[^1].v;
	}
}
}
