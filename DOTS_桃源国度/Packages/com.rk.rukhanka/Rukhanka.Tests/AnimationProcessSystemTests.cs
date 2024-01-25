using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;
using static Rukhanka.AnimationProcessSystem;

/////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka.Tests
{ 
public class AnimationProcessSystemTests
{
    [Test]
    public void MuscleRangeToRadiansTest()
    {
		var r0 = ComputeBoneAnimationJob.MuscleRangeToRadians(new float3(-60, -30, -100), new float3(40, 60, 120), new float3(-0.5f, 0.0f, 0.5f));
		Assert.IsTrue(math.all(r0 == new float3(-30, 0, 60)));

		var r1 = ComputeBoneAnimationJob.MuscleRangeToRadians(new float3(-60, -30, -100), new float3(40, 60, 120), new float3(0, 0, 0));
		Assert.IsTrue(math.all(r1 == new float3(0, 0, 0)));

		var r2 = ComputeBoneAnimationJob.MuscleRangeToRadians(new float3(-91, -92, -93), new float3(94, 95, 96), new float3(1, 1, 1));
		Assert.IsTrue(math.all(r2 == new float3(94, 95, 96)));

		var r3 = ComputeBoneAnimationJob.MuscleRangeToRadians(new float3(-91, -92, -93), new float3(94, 95, 96), new float3(-1, -1, -1));
		Assert.IsTrue(math.all(r3 == new float3(-91, -92, -93)));

		var r4 = ComputeBoneAnimationJob.MuscleRangeToRadians(new float3(-60, -30, -100), new float3(40, 60, 120), new float3(-0.9f, 0.1f, 0.9f));
		Assert.IsTrue(math.all(r4 == new float3(-54, 6, 108)));

		var r5 = ComputeBoneAnimationJob.MuscleRangeToRadians(new float3(-60, -30, -100), new float3(40, 60, 120), new float3(0.9f, -0.1f, -0.2f));
		Assert.IsTrue(math.all(r5 == new float3(36, -3, -20)));

		var r6 = ComputeBoneAnimationJob.MuscleRangeToRadians(new float3(-12, -13, -14), new float3(12, 13, 14), new float3(-2, 3, -5.5f));
		Assert.IsTrue(math.all(r6 == new float3(-24, 39, -77)));
    }
}
}
