using NUnit.Framework;
using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.PerformanceTesting;
using UnityEditor;
using UnityEngine.TestTools.Utils;

/////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka.Tests
{ 

[BurstCompile]
public class SampleAnimationCurveTest
{
	struct TestAnimationCurveBlob
	{
		public BlobArray<KeyFrame> animationCurve;
	}

/////////////////////////////////////////////////////////////////////////////////

	[BurstCompile]
	static unsafe void SampleAnimationCurveLinearSearch(ref NativeArray<float> accumulator, in NativeArray<float> sampleTimes, in BlobAssetReference<TestAnimationCurveBlob> ba)
	{
		for (int i = 0; i < accumulator.Length; ++i)
		{
			var sum = 0.0f;
			for (int j = 0; j < sampleTimes.Length; ++j)
			{
				var arr = new ReadOnlySpan<KeyFrame>(ba.Value.animationCurve.GetUnsafePtr(), ba.Value.animationCurve.Length);
				var f = BlobCurve.SampleAnimationCurveLinearSearch(arr, sampleTimes[j]);
				sum += f;
			}
			accumulator[i] = sum;
		}
	}

/////////////////////////////////////////////////////////////////////////////////

	[BurstCompile]
	static unsafe void SampleAnimationCurveBinarySearch(ref NativeArray<float> accumulator, in NativeArray<float> sampleTimes, in BlobAssetReference<TestAnimationCurveBlob> ba)
	{
		for (int i = 0; i < accumulator.Length; ++i)
		{
			var sum = 0.0f;
			for (int j = 0; j < sampleTimes.Length; ++j)
			{
				var arr = new ReadOnlySpan<KeyFrame>(ba.Value.animationCurve.GetUnsafePtr(), ba.Value.animationCurve.Length);
				var f = BlobCurve.SampleAnimationCurveBinarySearch(arr, sampleTimes[j]);
				sum += f;
			}
			accumulator[i] = sum;
		}
	}

/////////////////////////////////////////////////////////////////////////////////

	BlobAssetReference<TestAnimationCurveBlob> CreateTestBlob(int sz)
	{
		var bb = new BlobBuilder(Allocator.Temp);
		ref var cb = ref bb.ConstructRoot<TestAnimationCurveBlob>();
		var keyFramesArr = bb.Allocate(ref cb.animationCurve, sz);

		var dt = 1.0f / keyFramesArr.Length;
		for (int i = 0; i < keyFramesArr.Length; ++i)
		{
			var kf = new KeyFrame()
			{
				time = dt * i,
				v = dt * i,
				inTan = 0.1f,
				outTan = 0.1f
			};
			keyFramesArr[i] = kf;
		}

		var rv = bb.CreateBlobAssetReference<TestAnimationCurveBlob>(Allocator.Temp);
		return rv; 
	}

/////////////////////////////////////////////////////////////////////////////////

    [Test, Performance]
    public void SampleAnimationCurvePerformanceTests()
	{
		var acc = new NativeArray<float>(1000, Allocator.Temp);
		var sampleTimes = new NativeArray<float>(100, Allocator.Temp);
		var rr = new Unity.Mathematics.Random(123);

		for (int i = 0; i < sampleTimes.Length; ++i)
		{
			sampleTimes[i] = rr.NextFloat();
		}

		var testBlobAsset = CreateTestBlob(100);

		Measure.Method(() =>
		{
			SampleAnimationCurveLinearSearch(ref acc, sampleTimes, testBlobAsset);
		})
		.MeasurementCount(10)
		.SampleGroup($"Linear key search")
		.Run();

		Measure.Method(() =>
		{
			SampleAnimationCurveBinarySearch(ref acc, sampleTimes, testBlobAsset);
		})
		.MeasurementCount(10)
		.SampleGroup($"Binary key search")
		.Run();
	}

/////////////////////////////////////////////////////////////////////////////////

	unsafe void ValidationTestBinarySearch(ref BlobArray<KeyFrame> ac)
	{
		var comparer = new FloatEqualityComparer(10e-6f);
		
		var clip = new ReadOnlySpan<KeyFrame>(ac.GetUnsafePtr(), ac.Length);

		var f = BlobCurve.SampleAnimationCurveBinarySearch(clip, 0);
		var refV = ac[0].v;
		Assert.That(f, Is.EqualTo(refV).Using(comparer));

		f = BlobCurve.SampleAnimationCurveBinarySearch(clip, 1);
		refV = ac[ac.Length - 1].v;
		Assert.That(f, Is.EqualTo(refV).Using(comparer));

		f = BlobCurve.SampleAnimationCurveBinarySearch(clip, -100);
		refV = ac[0].v;
		Assert.That(f, Is.EqualTo(refV).Using(comparer));

		f = BlobCurve.SampleAnimationCurveBinarySearch(clip, 100);
		refV = ac[ac.Length - 1].v;
		Assert.That(f, Is.EqualTo(refV).Using(comparer));

		var rr = new Unity.Mathematics.Random((uint)ac.Length);

		var delta = 1.0f / ac.Length;

		for (int i = 0; i < 100; ++i)
		{
			var time = rr.NextFloat();
			f = BlobCurve.SampleAnimationCurveBinarySearch(clip, time);
			Assert.IsTrue(math.abs(f - time) < delta);
		}
	}

/////////////////////////////////////////////////////////////////////////////////

	[Test]
    public void SampleAnimationCurveValidationTests()
	{
		var blob10 = CreateTestBlob(10);
		var blob2 = CreateTestBlob(2);
		var blob3 = CreateTestBlob(3);
		var blob7 = CreateTestBlob(7);
		var blob123 = CreateTestBlob(123);
		var blob321 = CreateTestBlob(321);

		ValidationTestBinarySearch(ref blob10.Value.animationCurve);
		ValidationTestBinarySearch(ref blob2.Value.animationCurve);
		ValidationTestBinarySearch(ref blob3.Value.animationCurve);
		ValidationTestBinarySearch(ref blob7.Value.animationCurve);
		ValidationTestBinarySearch(ref blob123.Value.animationCurve);
		ValidationTestBinarySearch(ref blob321.Value.animationCurve);
	}
}
}