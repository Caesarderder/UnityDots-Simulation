using NUnit.Framework;
using System;
using Unity.Collections;
using UnityEngine;
using Random = Unity.Mathematics.Random;

/////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka.Tests
{ 
public class PerfectHashTest
{
	[Test]
	public void PerfectHashValidationTest()
	{
		var numTests = 100;
		var seed = 123124u;
		var rng = new Random(seed);

		for (var i = 0; i < numTests; ++i)
		{
			InternalHashFuncTest(rng.NextUInt(), rng.NextInt() % 300);
		}
	}
	
/////////////////////////////////////////////////////////////////////////////////

    public void InternalHashFuncTest(uint rngSeed, int numHashedValues)
    {
	    var rng = new Random(rngSeed);

		var hashArr = new NativeList<uint>(Allocator.Temp);
		for (int i = 0; i < numHashedValues; ++i)
		{
			hashArr.Add(rng.NextUInt());
		};
		var hashReinterpretedArr = hashArr.AsArray().Reinterpret<UIntPerfectHashed>();
		PerfectHash<UIntPerfectHashed>.CreateMinimalPerfectHash(hashReinterpretedArr, out var seedArr, out var shuffleArr);

		var shuffleVerifyArr = new NativeArray<int>(shuffleArr.Length, Allocator.Temp);
		for (int i = 0; i < shuffleArr.Length; ++i)
		{
			var sv = shuffleArr[i];
			Assert.IsTrue(shuffleVerifyArr[sv] == 0);
			shuffleVerifyArr[sv] = 1;
		}

		for (int i = 0; i < hashArr.Length; ++i)
		{
			var iHash = hashArr[i];
			var l = PerfectHash<UIntPerfectHashed>.QueryPerfectHashTable(seedArr, iHash);
			var shuffleIndex = shuffleArr[l];
			var hashFromTable = hashArr[shuffleIndex];
			Assert.IsTrue(hashFromTable == iHash);
		}
    }
    


}
}
