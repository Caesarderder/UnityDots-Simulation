using NUnit.Framework;
using Unity.Collections;

/////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka.Tests
{ 
public class MathUtilsTest
{
    [Test]
    unsafe public void ShuffleListTest()
    {
		var sz = 100;
		var srcArr = new NativeList<int>(128, Allocator.Temp);
		var refArr = new NativeList<int>(128, Allocator.Temp);
		
		var rr = new Unity.Mathematics.Random(123);
		for (int i = 0; i < sz; ++i)
		{
			var v = rr.NextInt() % sz * 2;
			srcArr.Add(v);
			refArr.Add(v);
		}

		var indexList = new NativeList<int>(srcArr.Length, Allocator.Temp);
		var shuffleList = new NativeList<int>(srcArr.Length, Allocator.Temp);
		for (int i = 0; i < sz; ++i) indexList.Add(i);

		while (!indexList.IsEmpty)
		{
			var index = (int)(rr.NextUInt() % indexList.Length);
			shuffleList.Add(indexList[index]);
			indexList.RemoveAt(index);
		}

		MathUtils.ShuffleArray(srcArr.AsArray().AsSpan(), shuffleList.AsArray());

		for (int i = 0; i < sz; ++i)
		{
			Assert.IsTrue(srcArr[i] == refArr[shuffleList[i]]);
		}
    }
}
}