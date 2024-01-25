
using Unity.Mathematics;
using Unity.Burst;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// For Bezier formulas reference use great online book: https://pomax.github.io/bezierinfo/
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka
{
[BurstCompile]
public static class LineTools
{
	public static float2 Get2DLineEquationFromTwoPoints(float2 p0, float2 p1)
	{
		var k = (p1.y - p0.y) / (p1.x - p0.x);
		var b = p1.y - k * p1.x;
		return new float2(k, b);
	}

/////////////////////////////////////////////////////////////////////////////////

	static float3 CalculateBoundingCircle(float2 p0, float2 p1, float2 p2)
	{
		var pc1 = (p1 + p0) * 0.5f;
		var pc2 = (p1 + p2) * 0.5f;

		var p1p0 = p1 - p0;
		var p1p2 = p1 - p2;

		float2 sincosHalfPi = new float2(1, 0);

		//	Normals
		var n1 = new float2(math.dot(p1p0, sincosHalfPi.yx * new float2(1, -1)), math.dot(p1p0, sincosHalfPi));
		var n2 = new float2(math.dot(p1p2, sincosHalfPi.yx * new float2(1, -1)), math.dot(p1p2, sincosHalfPi));

		var pn1 = pc1 - n1;
		var pn2 = pc2 + n2;

		//	Circle center
		var dx1 = pc1.x - pn1.x;
		var dx2 = pc2.x - pn2.x;

		var center = float2.zero;
		if (math.abs(dx1) < 0.0001f)
		{
			center.x = pc1.x;
			var kb = Get2DLineEquationFromTwoPoints(pc2, pn2);
			center.y = kb.x * center.x + kb.y;
		}
		else if (math.abs(dx2) < 0.0001f)
		{
			center.x = pc2.x;
			var kb = Get2DLineEquationFromTwoPoints(pc1, pn1);
			center.y = kb.x * center.x + kb.y;
		}
		else
		{
			var kb1 = Get2DLineEquationFromTwoPoints(pc1, pn1);
			var kb2 = Get2DLineEquationFromTwoPoints(pc2, pn2);
			var dk = kb1.x - kb2.x;
			center.x = dk != 0 ? (kb2.y - kb1.y) / dk : 0;
			center.y = center.x * kb1.x + kb1.y;
		}

		var circleR = math.length(p0 - center);

		return new float3(center, circleR);
	}

/////////////////////////////////////////////////////////////////////////////////

	static float4 BezierGetAC(float2 p0, float2 p1, float2 p2, float t)
	{
		var t2 = t * t;
		var t3 = t2 * t;
		var oneMinusT = 1 - t;
		var oneMinusT3 = oneMinusT * oneMinusT * oneMinusT;
		var ratio = math.abs((t3 + oneMinusT3 - 1) / (t3 + oneMinusT3));

		var ut = oneMinusT3 / (t3 + oneMinusT3);
		var c = math.lerp(p2, p0, ut);
		var a = p1 + (p1 - c) / ratio;
		return new float4(a, c);
	}

/////////////////////////////////////////////////////////////////////////////////

	public struct CubicBezierCurve
	{
		public float2 s, e, c1, c2, e1, e2, a, c;
	}

/////////////////////////////////////////////////////////////////////////////////

	[BurstCompile]
	public static void ConstructBezierApproximation(in float2 p0, in float2 p1, in float2 p2, ref CubicBezierCurve rv)
	{
		var p2p0 = p2 - p0;
		var p1p0 = p1 - p0;
		var p1p2 = p1 - p2;

		var d1 = math.length(p1p0);
		var d2 = math.length(p1p2);
		var t = d1 / (d1 + d2);

		var angle = math.atan2(p2p0.y, p2p0.x) - math.atan2(p1p0.y, p1p0.x);
		var sgn = angle < 0 || angle > math.PI ? -1 : 1; 
		var bc = math.length(p2p0) * sgn * 0.333f;
		var de1 = t * bc;
		var de2 = (1 - t) * bc;

		var boundingCircle = CalculateBoundingCircle(p0, p1, p2);
		
		var tanL = new float2(p1.x - (p1.y - boundingCircle.y), p1.y + (p1.x - boundingCircle.x));
		var tanR = new float2(p1.x + (p1.y - boundingCircle.y), p1.y - (p1.x - boundingCircle.x));

		var tanLen = math.length(new float4(tanL, tanR));

		var dx = (tanR.x - tanL.x) / tanLen;
		var dy = (tanR.y - tanL.y) / tanLen;
		var dxdy = new float2(dx, dy);

		var e1 = p1 + de1 * dxdy;
		var e2 = p1 - de2 * dxdy;

		var ac = BezierGetAC(p0, p1, p2, t);
		var a = ac.xy;
		var v1 = a + (e1 - a) / (1 - t);
		var v2 = a + (e2 - a) / t;
		var c1 = p0 + (v1 - p0) / t;
		var c2 = p2 + (v2 - p2) / (1 - t);

		rv = new CubicBezierCurve()
		{
			c1 = c1,
			c2 = c2,
			s = p0,
			e = p2,
			e1 = e1,
			e2 = e2,
			c = ac.zw,
			a = ac.xy
		};

	}
}
}
