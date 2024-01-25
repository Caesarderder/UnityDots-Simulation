using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class PathMap
{
	/// <summary>
	/// Pos=>Cost
	/// </summary>
	public Dictionary<Vector2,float> D_movablePoint;

	public List<Vector2> curPath=new();

	public PathMap()
	{
		D_movablePoint = new Dictionary<Vector2, float>();
	}

	public void AddMovableBlock(MovableBlock block)
	{
		
		foreach(var point in block.OffsetPoints)
		{
			if(!D_movablePoint.ContainsKey(point+block.Pos))
			{
				D_movablePoint.Add(point + block.Pos, block.Cost);
				//Test
				curPath.Add(point + block.Pos);
			}
			else
				Debug.LogError("D_movablePoint Already Has Key(" + point + ")");
		}
	}

	public void RemoveMovableBlock(MovableBlock block)
	{
		foreach (var point in block.OffsetPoints)
		{
			if (D_movablePoint.ContainsKey(point + block.Pos))
			{
				D_movablePoint.Remove(point + block.Pos);
				//Test
				curPath.Remove(point + block.Pos);
			}
			else
				Debug.LogError("D_movablePoint Has Not Key(" + point + ")");
		}
	}




}
[System.Serializable]
public class MovableBlock
{
	public Vector2 Pos;
	public List<Vector2> OffsetPoints;
	public float Cost;

	public MovableBlock(Vector2 pos, List<Vector2> points, float cost)
	{
		Pos= pos;
		OffsetPoints = points;
		Cost= cost;
	}
}
[System.Serializable]
public class Point : IComparable<Point>
{
	public Vector2 Pos;
	public float Cost;
	public Point Parent;

	public float G;
	public float H;
	public float F;

	public Point(Vector2 pos,float BlockCost,Vector2 target, bool isInSameBlock = true, Point parent=null)
	{
		Pos= pos;
		//起始点
		if(parent==null)
		{
			Parent = null;
			var oDis1 = target - pos;
			Cost = 1f;

			H = Mathf.Abs(oDis1.y) + Mathf.Abs(oDis1.x);
			G = 1f;
			F = H;
		}
		else
		{
			Parent = parent;
			Cost = BlockCost / 2;
			var isRight=IsRight(pos,parent.Pos);
			//逆行移动代价*4
			var multi = isRight?1f:4f;


			if (isInSameBlock)
			{
				
				G = parent.G + Cost*multi;

			}			
			else
				G = parent.G + Cost/2 * multi + parent.Cost/2 * multi;



			var oDis = target - pos;
			H = Mathf.Abs(oDis.y) + Mathf.Abs(oDis.x);

			F = G + H;
		}

	}

	public bool IsRight(Vector2 targetPos,Vector2 parentPos)
	{
		var center = new Vector2((int)targetPos.x>=0f? (int)targetPos.x: (int)targetPos.x-1 , (int)targetPos.y >= 0f ? (int)targetPos.y : (int)targetPos.y - 1);
		center += new Vector2(0.5f, 0.5f);
		return Vector3.Cross(center - parentPos, targetPos - parentPos).z<0f;
	}

	public int CompareTo(Point other)
	{
		var result = F.CompareTo(other.F);
		//result= result==0?1:result;
		return result;
		
	}
}

