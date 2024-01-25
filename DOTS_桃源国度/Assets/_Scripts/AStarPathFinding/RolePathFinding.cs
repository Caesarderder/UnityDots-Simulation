using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Transforms;
using UnityEngine;

[System.Serializable]
public class RolePathFinding
{
	//Test
	PathTest pathTest;

	public List<Point> Path=new();

	/// <summary>
	/// 起始点都要在PathMap里面
	/// </summary>
	public Vector2 StartPos;
	public Vector2 EndPos;

	public PathMap Map;
	/// <summary>
	/// 检测排序列表(Point=>F排序)
	/// </summary>
	public List<Point> SL_ToCheck=new();
	/// <summary>
	/// 已检测列表
	/// </summary>
	public List<Vector2> L_HasChecked = new();

	//Test
	public bool test;

	public RolePathFinding(PathMap map, PathTest pathTest)
	{
		test = true;
		Map = map;
		this.pathTest = pathTest;
	}

	public List<Point> FindPath(Vector2 startPos, Vector2 endPos)
	{
		if (FindAStarPath(startPos, endPos))
			return Path;
		else
			return null;
	}

	private bool FindAStarPath(Vector2 startPos,Vector2 endPos)
	{
		SL_ToCheck.Clear();
		L_HasChecked.Clear();
		StartPos = startPos;
		if (!Map.D_movablePoint.ContainsKey(StartPos))
		{
			Debug.Log("Can not Find Path To Position:" + endPos);
			return false;
		}

		EndPos = endPos;
		Path.Clear();

		var startPoint = new Point(startPos, 0f, endPos);

		var curPos = StartPos;
		var curPoint = startPoint;

		SL_ToCheck.Add(curPoint);

		L_HasChecked.Add(curPoint.Pos);

		while (curPos != endPos)
		{

			test = false;

			//没有找到路
			if (SL_ToCheck.Count== 0)
			{
				Debug.Log("Can not Find Path To Position:" + endPos);
				SL_ToCheck.Clear();
				L_HasChecked.Clear();
				return false;
			}

			//F最小的点作为当前点
			var sortedPoints = SL_ToCheck.OrderBy(p => p.F);



			curPoint = sortedPoints.First();



			Debug.Log("Check And Remove	" + curPoint.Pos + "  F:" + curPoint.F);
			SL_ToCheck.Remove(curPoint);

			L_HasChecked.Add(curPoint.Pos);



			//L_HasChecked.Add(curPoint.Pos);
			curPos = curPoint.Pos;

			float x, y;
			//将当前点的相邻点加入ToCheck
			for (x = -0.5f; x <= 0.5f; x += 0.5f)
			{
				if (x == -0.5f || x == 0.5f)
				{
					y = 0f;
					var checkPos = curPos + new Vector2(x, y);
					if (L_HasChecked.Contains(checkPos) || !Map.D_movablePoint.ContainsKey(checkPos))
						continue;




					var newPoint = new Point(checkPos, Map.D_movablePoint[startPos], endPos, IsInSameBlock(curPos, checkPos), curPoint);
					L_HasChecked.Add(checkPos);
					//Test
					Debug.Log("Add ToChck:" + checkPos);
					pathTest.CreateBall(checkPos, newPoint.F);
	

					if (checkPos == endPos)
					{
						Debug.Log("Find Path！");
						//根据Parent返回
						GetPath(newPoint);
						SL_ToCheck.Clear();
						L_HasChecked.Clear();
						 return true;
					}
					SL_ToCheck.Add(newPoint);
					
				}
				else
					for (y = -0.5f; y <= 0.5f; y += 1f)
					{
						var checkPos = curPos + new Vector2(x, y);
						if (L_HasChecked.Contains(checkPos) || !Map.D_movablePoint.ContainsKey(checkPos))
							continue;

						var newPoint = new Point(checkPos, Map.D_movablePoint[startPos], endPos, IsInSameBlock(curPos, checkPos), curPoint);
						L_HasChecked.Add(checkPos);
						//Test
						Debug.Log("Add ToChck:" + checkPos);
						pathTest.CreateBall(checkPos, newPoint.F);
						

						if (checkPos == endPos)
						{
							Debug.Log("Find Path！");
							//根据Parent返回
							GetPath(newPoint);
							SL_ToCheck.Clear();
							L_HasChecked.Clear();
							 return true;
						}
						SL_ToCheck.Add(newPoint);
						
					}
			}
			if (SL_ToCheck == null)
			{
				Debug.Log("Can not Find Path To Position:" + endPos);
				break;
			}

		
		}
		SL_ToCheck.Clear();
		L_HasChecked.Clear();
		Debug.Log("Can not Find Path To Position:" + endPos);
		return false;
			
	}

/*	public IEnumerator FindPathTest(Vector2 startPos, Vector2 endPos)
	{
		StartPos = startPos;
		if(!Map.D_movablePoint.ContainsKey(StartPos))
		{
			;
		}

		EndPos = endPos;
		Path.Clear();

		var startPoint = new Point(startPos, 0f, endPos);

		var curPos = StartPos;
		var curPoint = startPoint;

		SL_ToCheck.Add(curPoint);

		L_HasChecked.Add(curPoint.Pos);

		while (curPos != endPos)
		{
	*//*		if (test)
			//if (test||!test)
			{*//*
			Debug.Log("Test!!!");
			test = false;
			
			//F最小的点作为当前点
			curPoint = SL_ToCheck.Min;
			//没有找到路
			if(curPoint==null)
			{
				Debug.Log("Can not Find Path To Position:" + endPos);
				break;
			}

			Debug.Log("Check And Remove	" + curPoint.Pos + "  F:" + curPoint.F);
			var oo = SL_ToCheck.Remove(curPoint);
				
			Debug.Log("Remove:"+oo);
			L_HasChecked.Add(curPoint.Pos);


			//L_HasChecked.Add(curPoint.Pos);
			curPos = curPoint.Pos;

			float x, y;
			//将当前点的相邻点加入ToCheck
			for (x = -0.5f; x <= 0.5f; x += 0.5f)
			{
				if (x == -0.5f || x == 0.5f)
				{
					y = 0f;
					var checkPos = curPos + new Vector2(x, y);
					if (L_HasChecked.Contains(checkPos) || !Map.D_movablePoint.ContainsKey(checkPos))
						continue;




					var newPoint = new Point(checkPos, Map.D_movablePoint[startPos], endPos, IsInSameBlock(curPos, checkPos), curPoint);
					L_HasChecked.Add(checkPos);
					//Test
					Debug.Log("Add ToChck:" + checkPos);
					pathTest.CreateBall(checkPos, newPoint.F);
			

					if (checkPos == endPos)
					{
						Debug.Log("Find Path！");
						//根据Parent返回
						GetPath(newPoint);
						SL_ToCheck.Clear();
						L_HasChecked.Clear();
						yield return null;
					}
					var ad = SL_ToCheck.Add(newPoint);
					Debug.Log("Add:" + ad);
				
			
				}
				else
					for (y = -0.5f; y <= 0.5f; y += 1f)
					{
						var checkPos = curPos + new Vector2(x, y);
						if (L_HasChecked.Contains(checkPos) || !Map.D_movablePoint.ContainsKey(checkPos))
							continue;

						var newPoint = new Point(checkPos, Map.D_movablePoint[startPos], endPos, IsInSameBlock(curPos, checkPos), curPoint);
						L_HasChecked.Add(checkPos);
						//Test
						Debug.Log("Add ToChck:" + checkPos);
						pathTest.CreateBall(checkPos, newPoint.F);
		

						if (checkPos == endPos)
						{
							Debug.Log("Find Path！");
							//根据Parent返回
							GetPath(newPoint);
							SL_ToCheck.Clear();
							L_HasChecked.Clear();
							yield return null;
						}
						SL_ToCheck.Add(newPoint);
				

					}
			}
			if (SL_ToCheck == null)
			{
				Debug.Log("Can not Find Path To Position:" + endPos);
				break;
			}
			test = false;
			//}
			//
			yield return null;
		}
		SL_ToCheck.Clear();
		L_HasChecked.Clear();

	}*/


	private List<Point> GetPath(Point endPoint)
	{
		var curPoint = endPoint;
		Path.Add(curPoint);
		while(curPoint.Pos!=StartPos)
		{
			if(curPoint.Parent==null)
			{
				Debug.LogError("Wrong Path!!!");
				Path.Reverse();
				return Path;
			}
			curPoint = curPoint.Parent;
			Path.Add(curPoint);

		}
		Path.Reverse();
		//Test
		foreach (var point in Path)
		{
			pathTest.CreatePathBall(point.Pos);
		}

		Debug.Log("Right Path");
		return Path;
	}

	public bool IsInSameBlock(Vector2 pos1, Vector2 pos2)
	{
		if (Value((int)pos1.x)  == Value((int)pos2.x) && Value((int)pos1.y ) == Value((int)pos2.y ))
		{
			return true;
		}
		return false;

		int Value(int value)
		{
			return value < 0 ? value - 1 : value;
		}
	}


}
