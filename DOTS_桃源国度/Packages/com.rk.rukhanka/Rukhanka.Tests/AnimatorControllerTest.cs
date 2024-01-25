using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using static Rukhanka.AnimatorControllerSystemJobs;

/////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka.Tests
{ 
public class AnimatorControllerTest
{
	[Test]
	public void LoopAwareTransitionTimeTest()
	{
		var fn = TestToolbox.GetStaticPrivateMethod<StateMachineProcessJob>("GetLoopAwareTransitionExitTime");

		var testData = new List<float2>();
		testData.Add(new float2(0, 0));
		testData.Add(new float2(1, 0));
		testData.Add(new float2(0, 1));
		testData.Add(new float2(1, 1));
		testData.Add(new float2(0, 2));
		testData.Add(new float2(1, 2));
		testData.Add(new float2(0, 3));
		testData.Add(new float2(1, 3));
		testData.Add(new float2(0, 3.5f));
		testData.Add(new float2(1, 3.5f));
		testData.Add(new float2(0, 6.99f));
		testData.Add(new float2(1, 6.99f));
		testData.Add(new float2(0, 7.01f));
		testData.Add(new float2(1, 7.01f));

		UnityEngine.Random.InitState(DateTime.Now.Millisecond);

		for (var i = 0; i < 100; ++i)
		{
			var exitTime = UnityEngine.Random.value;
			var normalizedStateTime = (UnityEngine.Random.value * 2 - 1) * 20;
			testData.Add(new float2(exitTime, normalizedStateTime));
		}


		//	Test for exit time [0..1]
		foreach (var td in testData)
		{
			var exitTime = td.x;
			var normalizedStateTime = td.y;
			var t = StateMachineProcessJob.GetLoopAwareTransitionExitTime(exitTime, normalizedStateTime, math.sign(normalizedStateTime));
			var dt = t - normalizedStateTime;

			//Debug.Log($"ET: {exitTime}, NT: {normalizedStateTime}, Transition Time: {t}");

			if (math.sign(normalizedStateTime) > 0)
			{
				Assert.IsTrue(dt >= 0);
				Assert.IsTrue(dt <= 1);
			}
			else
			{
				Assert.IsTrue(dt <= 0);
				Assert.IsTrue(dt >= -1);
			}
		}

		//	Test for exit time >= 1
		for (var i = 0; i < 20; ++i)
		{
			var exitTime = UnityEngine.Random.value * 10 + 1;
			var normalizedStateTime = (UnityEngine.Random.value * 2 - 1) * 20;

			var sgn = math.sign(normalizedStateTime);
			var t = StateMachineProcessJob.GetLoopAwareTransitionExitTime(exitTime, normalizedStateTime, sgn);

			Assert.IsTrue(t == exitTime * sgn);
		}
	}
}
}
