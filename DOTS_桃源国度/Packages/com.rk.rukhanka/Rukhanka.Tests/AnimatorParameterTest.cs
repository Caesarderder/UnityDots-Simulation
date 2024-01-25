using NUnit.Framework;
using Rukhanka.Hybrid;
using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Random = Unity.Mathematics.Random;

#if RUKHANKA_WITH_TEST_PERFORMANCE
using Unity.PerformanceTesting;
#endif

/////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka.Tests
{ 
[BurstCompile]
public class AnimatorParameterTest
{
#if RUKHANKA_WITH_TEST_PERFORMANCE
	[BurstCompile]
	static void ParameterAccessByLinearSearch(ref NativeSlice<AnimatorControllerParameterComponent> paramArr, int iterationCount, out int dummyResult)
	{
		var l = paramArr.Length;
		dummyResult = 0;
		for (int i = 0; i < iterationCount; ++i)
		{
			var idx = i % l;
			var p = paramArr[idx];

			bool found = false;
			int k = 0;
			for (; k < paramArr.Length && !found; ++k)
			{
				var candidat = paramArr[k];
				found = candidat.hash == p.hash;
			}

			dummyResult += paramArr[k - 1].value.intValue;
		}
	}

/////////////////////////////////////////////////////////////////////////////////

	NativeArray<AnimatorControllerParameterComponent> CreateTestParamArray(int len)
	{
		var paramArr = new NativeArray<AnimatorControllerParameterComponent>(len, Allocator.Temp);
		var rng = new Random((uint)DateTime.Now.Millisecond);

		for (var i = 0; i < paramArr.Length; ++i)
		{
			var randomHash = rng.NextUInt();
			var acpc = new AnimatorControllerParameterComponent()
			{
				hash = randomHash,
				type = ControllerParameterType.Int,
				value = default
			};
			paramArr[i] = acpc;
		}
		return paramArr;
	}

/////////////////////////////////////////////////////////////////////////////////

    [Test, Performance]
    public void LinearSearchParameterAccess()
    {
		//	Create test data
		var paramArr = CreateTestParamArray(200);

		var iterationsCount = 1000000;

		var testGroupSize = new int[] {2, 5, 10, 20, 50, 100};

		foreach (var paramCount in testGroupSize)
		{
			var paramArrForTest = new NativeSlice<AnimatorControllerParameterComponent>(paramArr, 0, paramCount);
			Measure.Method(() =>
			{
				ParameterAccessByLinearSearch(ref paramArrForTest, iterationsCount, out var dummyResult);
			})
			.MeasurementCount(10)
			.SampleGroup($"Linear Parameter Access. Parameter count: {paramArrForTest.Length}, Searches count: {iterationsCount}")
			.Run();
		}
    }

/////////////////////////////////////////////////////////////////////////////////

	[BurstCompile]
	static unsafe void ParameterAccessHashLookup(ref NativeSlice<AnimatorControllerParameterComponent> paramArr, ref BlobAssetReference<ParameterPerfectHashTableBlob> pht, int iterationCount, out int dummyResult)
	{
		dummyResult = 0;
		var l = paramArr.Length;
		for (int i = 0; i < iterationCount; ++i)
		{
			var idx = i % l;
			var p = paramArr[idx];

			var ps = new ReadOnlySpan<AnimatorControllerParameterComponent>(paramArr.GetUnsafePtr(), paramArr.Length);
			var pi = FastAnimatorParameter.GetRuntimeParameterIndex(p.hash, pht, ps);
			dummyResult += paramArr[pi].value.intValue;
		}
	}

/////////////////////////////////////////////////////////////////////////////////

    [Test, Performance]
    public void HashTableParameterAccess()
    {
		//	Create test data
		var paramArr = CreateTestParamArray(200);

		var iterationsCount = 1000000;
		var testGroupSize = new int[] {2, 5, 10, 20, 50, 100};

		foreach (var paramCount in testGroupSize)
		{
			var paramArrSlice = new NativeSlice<AnimatorControllerParameterComponent>(paramArr, 0, paramCount);
			var hashesArr = new NativeArray<uint>(paramCount, Allocator.Temp);
			for (int i = 0; i < hashesArr.Length; ++i)
			{
				hashesArr[i] = paramArr[i].hash;
			}

			var perfectHashTableBlob = AnimatorControllerConversionSystem.CreateBlobAssetsJob.CreateParametersPerfectHashTableBlob(hashesArr);
			Measure.Method(() =>
			{
				ParameterAccessHashLookup(ref paramArrSlice, ref perfectHashTableBlob, iterationsCount, out var dummyResult);
			})
			.MeasurementCount(10)
			.SampleGroup($"Hash Table Parameter Access. Parameter count: {paramArrSlice.Length}, Searches count: {iterationsCount}")
			.Run();

			perfectHashTableBlob.Dispose();
		}
    }
#endif
}
}
