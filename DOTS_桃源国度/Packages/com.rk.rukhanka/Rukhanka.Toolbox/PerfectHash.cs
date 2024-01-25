using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using Hash128 = Unity.Entities.Hash128;

/////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka
{
	
public interface IPerfectHashedValue
{
	uint HashFunc(int prime, int k);
	uint InitialHash();
}
	
/////////////////////////////////////////////////////////////////////////////////

public struct UIntPerfectHashed: IPerfectHashedValue
{
	public uint value;

	public UIntPerfectHashed(uint v) { value = v; }
	public static implicit operator UIntPerfectHashed(uint v) => new UIntPerfectHashed(v);
	public uint InitialHash() => value;
	public uint HashFunc(int prime, int k)
	{
		var rv = k * value % prime;
		return (uint)rv;
	}
}

/////////////////////////////////////////////////////////////////////////////////

public struct Hash128PerfectHashed: IPerfectHashedValue
{
	public Hash128 value;
	
	public Hash128PerfectHashed(Hash128 v) { value = v; } 
	public static implicit operator Hash128PerfectHashed(Hash128 v) => new Hash128PerfectHashed(v);
	public uint InitialHash() => math.hash(value.Value);
	public uint HashFunc(int prime, int k)
	{
		var rv = math.hash(value.Value);
		rv = (uint)(k * rv % prime);
		return rv;
	}
}

/////////////////////////////////////////////////////////////////////////////////

internal static class PerfectHashPrimes
{
	public static NativeArray<int2> CreatePerfectHashPrimes(int numPrimes = 0xffff)
	{
		var rv = new NativeArray<int2>(numPrimes, Allocator.Temp);
		var rng = new Unity.Mathematics.Random((uint)numPrimes);
		var currentPrime = PerfectHash<UIntPerfectHashed>.InitialPrime;
		for (var i = 0; i < numPrimes; ++i)
		{
			currentPrime = NextPrime(currentPrime + 1);
			var v = new int2(currentPrime, (int)(rng.NextUInt() & 0x7fffffff));
			rv[i] = v;
		}

		return rv;
	}

	static bool IsPrime(int p)
	{
		if (p % 2 == 0) return false;
		for (var d = 3; d * d <= p; d += 2)
		{
			if (p % d == 0) return false;
		}
		return true;
	}

	static int NextPrime(int p)
	{
		if (p <= 2) return 2;
		for (var l = p | 1; ; l += 2)
			if (IsPrime(l))
				return l;
	}
}

/////////////////////////////////////////////////////////////////////////////////

public class PerfectHash<T> where T: unmanaged, IPerfectHashedValue
{
	public static readonly int InitialPrime = 100003;
	public static readonly int InitialRng = 1931387198;
	
	public static bool CreateMinimalPerfectHash(in NativeArray<T> inArr, out NativeList<int2> seedValues, out NativeList<int> shuffleIndices)
	{
		var primes = PerfectHashPrimes.CreatePerfectHashPrimes();
		var sz = inArr.Length;

		var buckets = new NativeArray<int>(sz * sz, Allocator.Temp).AsSpan();
		var bucketsCount = new NativeArray<int2>(sz, Allocator.Temp).AsSpan();
		var hashesArray = new NativeArray<uint>(sz, Allocator.Temp);
		seedValues = new NativeList<int2>(sz, Allocator.Temp);
		shuffleIndices = new NativeList<int>(sz, Allocator.Temp);
		
		for (var l = 0; l < bucketsCount.Length; ++l)
		{
			bucketsCount[l] = new int2(l, 0);
		}

		for (int i = 0; i < sz; ++i)
		{
			var v = inArr[i];
			var h = v.InitialHash();
			var k = (int)(h % sz);
			buckets[k * sz + bucketsCount[k].y++] = i;

			hashesArray[i] = h;
		}
		
		//	Check for uniqueness of value hashes
		if (!CheckForUniqueness(hashesArray))
		{
			return false;
		}

		//	Simple bubble sort
		for (int i = 0; i < bucketsCount.Length - 1; ++i)
		{
			for (int l = 0; l < bucketsCount.Length - i - 1; ++l)
			{
				if (bucketsCount[l].y < bucketsCount[l + 1].y)
				{
					var t = bucketsCount[l];
					bucketsCount[l] = bucketsCount[l + 1];
					bucketsCount[l + 1] = t;
				}
			}
		}

		var freeList = new NativeArray<int>(sz, Allocator.Temp).AsSpan();

		var sv = new int2(-sz, 0);
		for (int i = 0; i < sz; ++i)
		{
			seedValues.Add(sv);
			shuffleIndices.Add(-1);
		}

		int bucketIndex = 0;
		for (; bucketIndex < bucketsCount.Length && bucketsCount[bucketIndex].y > 1; ++bucketIndex)
		{
			var seed = 0;
			var l = 0;
			var bucketInfo = bucketsCount[bucketIndex];

			//	Skip buckets with less than two items
			ResetFreeList(ref freeList);

			var maxNumIterations = 0xffff;

			int2 primeAndRnd = 0;
			while (l < bucketInfo.y && seed < maxNumIterations)
			{
				var itemIndex = buckets[bucketInfo.x * sz + l];
				var item = inArr[itemIndex];
				primeAndRnd = primes[seed];
				var slotIndex = (int)(item.HashFunc(primeAndRnd.x, primeAndRnd.y) % sz);
				if (freeList[slotIndex] >= 0 || shuffleIndices[slotIndex] >= 0)
				{
					ResetFreeList(ref freeList);
					l = 0;
					seed++;
				}
				else
				{
					freeList[slotIndex] = itemIndex;
					l++;
				}
			}
			
			Assert.IsTrue(seed < maxNumIterations);

			seedValues[bucketInfo.x] = primeAndRnd;
			for (int k = 0; k < freeList.Length; ++k)
			{
				var f = freeList[k];
				if (f < 0) continue;

				shuffleIndices[k] = f;
			}
		}

		//	Add buckets with one element
		for (int i = bucketIndex; i < bucketsCount.Length && bucketsCount[i].y > 0; ++i)
		{
			var bucketInfo = bucketsCount[i];
			var l = buckets[bucketInfo.x * sz];

			var freeSlotIndex = shuffleIndices.IndexOf(-1);
			var seedVal = new int2(-freeSlotIndex - 1, 0);
			seedValues[bucketInfo.x] = seedVal;
			Assert.IsTrue(shuffleIndices[freeSlotIndex] == -1);
			shuffleIndices[freeSlotIndex] = l;
		}

		return true;
	}

/////////////////////////////////////////////////////////////////////////////////

	static bool CheckForUniqueness(NativeArray<uint> hashesArr)
	{
		var origCount = hashesArr.Length;
		hashesArr.Sort();
		var uniqueCount = hashesArr.Unique();

		if (origCount > uniqueCount)
		{
			Debug.LogError($"Input values do not produce unique hashes. Check input array for duplicated values. Creation of perfect hash table is failed!");
			return false;
		}

		return true;
	}

/////////////////////////////////////////////////////////////////////////////////

	public static unsafe int QueryPerfectHashTable(in NativeList<int2> t, T h)
	{
		var tablePtr = t.GetUnsafePtr();
		var seedTableAsArr = new Span<int2>(tablePtr, t.Length);
		var paramIdx = QueryPerfectHashTable(seedTableAsArr, h);
		return paramIdx;
	}

/////////////////////////////////////////////////////////////////////////////////

	public static unsafe int QueryPerfectHashTable(ref BlobArray<int2> t, T h)
	{
		var tablePtr = t.GetUnsafePtr();
		var seedTableAsArr = new Span<int2>(tablePtr, t.Length);
		var paramIdx = QueryPerfectHashTable(seedTableAsArr, h);
		return paramIdx;
	}

/////////////////////////////////////////////////////////////////////////////////

	public static int QueryPerfectHashTable(ReadOnlySpan<int2> t, T h)
	{
		var h0 = h.InitialHash();
		var i0 = (int)(h0 % t.Length);
		var d = t[i0];
		if (d.x < 0)
			return -d.x - 1;

		var h1 = h.HashFunc(d.x, d.y);
		var i1 = (int)(h1 % t.Length);
		return i1;
	}

/////////////////////////////////////////////////////////////////////////////////

	static void ResetFreeList(ref Span<int> freeList)
	{
		for (int k = 0; k < freeList.Length; ++k)
			freeList[k] = -1;
	}
}
}
