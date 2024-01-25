using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using System.Diagnostics;
using Unity.Collections;
using Unity.Mathematics;
using System.Drawing;
using System.Linq;
using static RootMotion.FinalIK.IKSolver;
using UnityEngine.UIElements;
using Unity.Jobs;
using Unity.Entities.UniversalDelegates;
using UnityEditor.Localization.Plugins.XLIFF.V20;

[BurstCompile]
[UpdateAfter(typeof(HomeCreateCharacterSystem))]
public partial struct StateUpdateSystem : ISystem
{
	public int2 Size;
	public NativeArray<int2> Map;

	//居民查找
	EntityQuery QuryFarmer;
	EntityQuery QuryGoToWorkStateFarmer;
	EntityQuery QuryRestStateFarmer;
	EntityQuery QuryWorkStateFarmer;
	EntityQuery QuryGoToRestStateFarmer;
	EntityQuery QuryDurationCheckStateFarmer;

	EntityQuery QuryScientist;
	EntityQuery QuryGoToWorkStateScientist;
	EntityQuery QuryRestStateScientist;
	EntityQuery QuryWorkStateScientist;
	EntityQuery QuryGoToRestStateScientist;
	EntityQuery QuryDurationCheckStateScientist;

	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		Size = new int2(51, 51);
		Map = new NativeArray<int2>(Size.x * Size.y, Allocator.Persistent);
		for (int i = 0; i < Map.Length; i++)
		{
			Map[i] = new int2(0, 100);
		}
		//初始化居民查找
		QuryFarmer = new EntityQueryBuilder(Allocator.Temp).WithAll<SFarmer>().Build(ref state);
		QuryGoToWorkStateFarmer = new EntityQueryBuilder(Allocator.Temp).WithAll<SFarmer>().WithAspect<GoToWorkStateAspect>().Build(ref state);
		QuryRestStateFarmer = new EntityQueryBuilder(Allocator.Temp).WithAll<SFarmer>().WithAspect<RestStateAspect>().Build(ref state);
		QuryWorkStateFarmer = new EntityQueryBuilder(Allocator.Temp).WithAll<SFarmer>().WithAspect<WorkStateAspect>().Build(ref state);
		QuryGoToRestStateFarmer = new EntityQueryBuilder(Allocator.Temp).WithAll<SFarmer>().WithAspect<GoToRestStateAspect>().Build(ref state);
		QuryDurationCheckStateFarmer = new EntityQueryBuilder(Allocator.Temp).WithAll<SFarmer>().WithAspect<DurationCheckStateAspect>().Build(ref state);

		QuryScientist = new EntityQueryBuilder(Allocator.Temp).WithAll<SScientist>().Build(ref state);
		QuryGoToWorkStateScientist = new EntityQueryBuilder(Allocator.Temp).WithAll<SScientist>().WithAspect<GoToWorkStateAspect>().Build(ref state);
		QuryRestStateScientist = new EntityQueryBuilder(Allocator.Temp).WithAll<SScientist>().WithAspect<RestStateAspect>().Build(ref state);
		QuryWorkStateScientist = new EntityQueryBuilder(Allocator.Temp).WithAll<SScientist>().WithAspect<WorkStateAspect>().Build(ref state);
		QuryGoToRestStateScientist = new EntityQueryBuilder(Allocator.Temp).WithAll<SScientist>().WithAspect<GoToRestStateAspect>().Build(ref state);
		QuryDurationCheckStateScientist = new EntityQueryBuilder(Allocator.Temp).WithAll<SScientist>().WithAspect<DurationCheckStateAspect>().Build(ref state);
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		var buildingEntity=SystemAPI.GetSingletonEntity<Gbuilding>();
		var sourceEntity= SystemAPI.GetSingletonEntity<GSource>();

		var buffer= SystemAPI.GetBuffer<BufferPathInfo>(buildingEntity);

		for (int i = 0; i < buffer.Length; i++)
		{
			Map[GetIndexByPos(buffer[i].Postion)] = new int2(buffer[i].ID, buffer[i].Cost);

		}
		buffer.Clear();

		var buildingdata = SystemAPI.GetSingleton<Gbuilding>();
		var maxRestTime = buildingdata.Home.RestTime;
		var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
		var deltaTime = SystemAPI.Time.DeltaTime;

/*		var mapTempJob = new NativeArray<int2>(Map.Length, Allocator.TempJob);
		mapTempJob.CopyFrom(Map);*/

		#region 农民
		

		if (!QuryFarmer.IsEmpty)
		{
			//工作地点位置
			var farmList = new NativeList<SPointToWork>(Allocator.Temp);

			foreach (var (farmAspect, farmEntity) in SystemAPI.Query<SFarmAspect>().WithEntityAccess())
			{
				var points = farmAspect.GetToWorkPoint();
				foreach (var point in points)
				{

					farmList.Add(new SPointToWork { pos = point, entity = farmEntity });

				}

			}

			//CopyToNativeArray
			var farmArray = new NativeArray<SPointToWork>(farmList.Length, Allocator.TempJob);
			for (int i = 0; i < farmList.Length; i++)
			{
				farmArray[i] = farmList[i];
			}

			farmList.Dispose();



			if (!QuryGoToWorkStateFarmer.IsEmpty)
				new GoToWorkStateJob
				{
					deltaTime = deltaTime,
					targetArrayRead = farmArray,
					ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
				}.ScheduleParallel(QuryGoToWorkStateFarmer);



			if (!QuryRestStateFarmer.IsEmpty)
			{
				UnityEngine.Debug.Log(QuryRestStateFarmer.CalculateEntityCount());
				new RestStateJob
				{
					deltaTime = deltaTime,
					maxRestTime = maxRestTime,
					FoodCost = buildingdata.Home.FoodCost,
					sourceEntity = sourceEntity,
					ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
				}.ScheduleParallel(QuryRestStateFarmer);
			}




			if (!QuryWorkStateFarmer.IsEmpty)
			{
				var changes = new NativeArray<BufferGSourceChange>(2, Allocator.TempJob);
				changes[0] = new BufferGSourceChange { index = 2, value = buildingdata.Wheat.SourceGain };
				changes[1] = new BufferGSourceChange { index = 4, value = buildingdata.Wheat.SourceGain };
				new WorkStateJob
				{
					sourceEntity = sourceEntity,

					deltaTime = deltaTime,
					workTime = buildingdata.Wheat.WorkingTime0,
					durationCost = buildingdata.Wheat.StaminaCost0,
					ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
					SourceGetArrayRead= changes,
				}.ScheduleParallel(QuryWorkStateFarmer);

			}


			if (!QuryGoToRestStateFarmer.IsEmpty)
				new GoToRestStateJob
				{
					deltaTime = deltaTime,
					ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
				}.ScheduleParallel(QuryGoToRestStateFarmer);


			if (!QuryDurationCheckStateFarmer.IsEmpty)
				new DurationCheckStateJob
				{
					DurationCost = buildingdata.Wheat.StaminaCost1,
					mapRead = Map,
					targetArrayRead = farmArray,
					Size = Size,
					ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
				}.ScheduleParallel(QuryDurationCheckStateFarmer);

		}

		#endregion
		state.Dependency.Complete();
		#region 科学家
		if(!QuryScientist.IsEmpty)
		{
			var instituteList = new NativeList<SPointToWork>(Allocator.Temp);
			var institueInfo = buildingdata.Institute;

			foreach (var (instituteAspect, farmEntity) in SystemAPI.Query<SInstituteAspect>().WithEntityAccess())
			{
				var points = instituteAspect.GetToWorkPoint(institueInfo.Capacity);
				foreach (var point in points)
				{

					instituteList.Add(new SPointToWork { pos = point, entity = farmEntity });
				}

			}
			UnityEngine.Debug.Log(instituteList.Length);
			//CopyToNativeArray
			var instituteArray = new NativeArray<SPointToWork>(instituteList.Length, Allocator.TempJob);
			for (int i = 0; i < instituteList.Length; i++)
			{
				instituteArray[i] = instituteList[i];
			}

			instituteList.Dispose();
	
			if (!QuryGoToWorkStateScientist.IsEmpty)
				new GoToWorkStateJob
				{
					deltaTime = deltaTime,
					targetArrayRead = instituteArray,
					ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
				}.ScheduleParallel(QuryGoToWorkStateScientist);



			if (!QuryRestStateScientist.IsEmpty)
			{
				
				new RestStateJob
				{
					deltaTime = deltaTime,
					maxRestTime = maxRestTime,
					FoodCost = buildingdata.Home.FoodCost,
					sourceEntity = sourceEntity,
					ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
				}.ScheduleParallel(QuryRestStateScientist);
			}




			if (!QuryWorkStateScientist.IsEmpty)
			{
				var changes = new NativeArray<BufferGSourceChange>(1, Allocator.TempJob);
				changes[0] = new BufferGSourceChange { index = 6, value = buildingdata.Institute.SourceGain};

				new WorkStateJob
				{
					sourceEntity = sourceEntity,

					deltaTime = deltaTime,
					workTime = buildingdata.Institute.WorkingTime,
					durationCost = buildingdata.Institute.DuationCost,
					ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),

					SourceGetArrayRead = changes,
				}.ScheduleParallel(QuryWorkStateScientist);
			}


			if (!QuryGoToRestStateScientist.IsEmpty)
				new GoToRestStateJob
				{
					deltaTime = deltaTime,
					ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
				}.ScheduleParallel(QuryGoToRestStateScientist);


			if (!QuryDurationCheckStateScientist.IsEmpty)
				new DurationCheckStateJob
				{
					DurationCost = buildingdata.Wheat.StaminaCost1,
					mapRead = Map,
					targetArrayRead = instituteArray,
					Size = Size,
					ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
				}.ScheduleParallel(QuryDurationCheckStateScientist);

		}


		#endregion
	}

	public int GetIndexByPos(float3 pos)
	{
		int x = (int)pos.x + (Size.x - 1) / 2;
		int y = (int)pos.z + (Size.y - 1) / 2;
		return y * Size.x + x;
	}


	public float2 GetGirdPosition(float2 pos)
	{
		var x1 = pos.x >= 0f ? 0f : -1f;
		var y1 = pos.y >= 0f ? 0f : -1f;

		return new float2((int)pos.x + x1, (int)pos.y + y1);
	}
}

[BurstCompile]
public partial struct RestStateJob : IJobEntity
{
	public Entity sourceEntity;
	public  float deltaTime;
	public float maxRestTime;
	public float FoodCost;
	public EntityCommandBuffer.ParallelWriter ECB;

	[BurstCompile]
	public void Execute(RestStateAspect aspect, [EntityIndexInQuery] int sortKey)
	{
		if(aspect.Rest(deltaTime, maxRestTime))
		{
			ECB.AppendToBuffer<BufferGSourceChange>(sortKey, sourceEntity, new BufferGSourceChange { index=2,value=- FoodCost });
			//休息状态->体力检测状态
			aspect.StateTransition(ECB,sortKey);
		}
	}
}

[BurstCompile]
public partial struct DurationCheckStateJob : IJobEntity
{
	public int2 Size;
	public float DurationCost;
	public float MaxRestTime;
	[ReadOnly]public NativeArray<int2> mapRead;
	public EntityCommandBuffer.ParallelWriter ECB;
	//特殊
	[ReadOnly] public NativeArray<SPointToWork> targetArrayRead;

	[BurstCompile]
	public void Execute(DurationCheckStateAspect aspect, [EntityIndexInQuery] int sortKey)
	{
		var map = new NativeArray<int2>(mapRead.Length, Allocator.Temp);
		map.CopyFrom(mapRead);

		var targetArray = new NativeArray<SPointToWork>(targetArrayRead.Length, Allocator.Temp);
		targetArray.CopyFrom(targetArrayRead);


		var flag = aspect.StateTrasitionCheck(DurationCost);
		//Go to Rest
		if (flag == 0)
		{
			//Get Path To Home
			//UnityEngine.Debug.Log("休息from:" + aspect.CurGridPosition +" To:"+ aspect.HomeGridPosition);

			//var path = FindAStarPath(aspect.CurGridPosition, , map);
			var targetPos = aspect.HomeGridPosition;
			aspect.SetTargetPos(targetPos);

			//var targetPos =new float2(0.25f,0.25f);
			var path = FindAStarPath(aspect.CurGridPosition, targetPos, map);

			//没有道路
			if (path.Length == 0)
			{
				return;
			}


			for (int i = path.Length - 1; i >= 0; i--)
			{
				ECB.AppendToBuffer<BufferPath>(sortKey, aspect.Entity, new BufferPath { Position = path[i] });
			}

			//Transition To GoToRestState
			aspect.StateTransitionToGoToRest(ECB,sortKey);
		}
		//Go to Work
		else if (flag == 1)
		{

		
			//Get Path ToTarget

			//当前没有可工作点
		
			if (targetArray.Length==0)
			{
				UnityEngine.Debug.Log("当前没有可工作点");
				//aspect.StateTransitionToGoToRest(ECB,sortKey);
				var targetPos = aspect.HomeGridPosition;
				aspect.SetTargetPos(targetPos);

				//var targetPos =new float2(0.25f,0.25f);
				var path1 = new NativeList<float2>(Allocator.Temp);
				 path1 = FindAStarPath(aspect.CurGridPosition, targetPos, map);

				//没有道路
				if (path1.Length == 0)
				{
					return;
				}


				for (int i = path1.Length - 1; i >= 0; i--)
				{
					ECB.AppendToBuffer<BufferPath>(sortKey, aspect.Entity, new BufferPath { Position = path1[i] });
				}

				//Transition To GoToRestState
				aspect.StateTransitionToGoToRest(ECB, sortKey);
				path1.Dispose();
				return;
			}

			var TargetPos = targetArray[0].pos;
			var curPos = aspect.CurGridPosition;
			var curEntity = targetArray[0].entity;
			var path = new NativeList<float2>(Allocator.Temp);
		

			var minDis = math.distancesq(curPos, TargetPos);
			var index = 0;
			for (int i = 1; i < targetArray.Length; i++)
			{
				var dis = math.distancesq(curPos, targetArray[i].pos);
				if (minDis > dis)
				{
					TargetPos = targetArray[i].pos;
					minDis = dis;
					curEntity = targetArray[i].entity;
					index = i;
				}
			}

			aspect.SetTargetPos(TargetPos);
			UnityEngine.Debug.Log("目的地:" + aspect.GetGirdPosition(TargetPos));

			path = FindAStarPath(curPos, aspect.GetGirdPosition(TargetPos), map);

			//没有找到路， 就回家
			if(path.Length==0)
			{
				UnityEngine.Debug.Log("没有前去工作的找到路");
				//Get Path To Home
				//UnityEngine.Debug.Log("休息from:" + aspect.CurGridPosition +" To:"+ aspect.HomeGridPosition);

				//var path = FindAStarPath(aspect.CurGridPosition, , map);
				/*				var targetPos = aspect.HomeGridPosition;
								aspect.SetTargetPos(targetPos);

								//var targetPos =new float2(0.25f,0.25f);
								path = FindAStarPath(aspect.CurGridPosition, targetPos, map);

								//没有道路
								if (path.Length == 0)
								{
									return;
								}


								for (int i = path.Length - 1; i >= 0; i--)
								{
									ECB.AppendToBuffer<BufferPath>(sortKey, aspect.Entity, new BufferPath { Position = path[i] });
								}

								//Transition To GoToRestState
								aspect.StateTransitionToGoToRest(ECB, sortKey);
								aspect.StateTransitionToGoToRest(ECB, sortKey);*/
				return;
			}


			for (int i = path.Length-1; i >= 0; i--)
			{
				ECB.AppendToBuffer<BufferPath>(sortKey, aspect.Entity, new BufferPath { Position = path[i], });
				//aspect.Clear();
			}
			ECB.AppendToBuffer<BufferPath>(sortKey, aspect.Entity, new BufferPath { Position = TargetPos, });
			//Transition To GoToWorkState
			aspect.StateTransitionToGoToWork(ECB, sortKey, curEntity);
			UnityEngine.Debug.Log("前往工作");
		}
	
	}

	public NativeList<float2> FindAStarPath(float2 startPos, float2 endPos,NativeArray<int2> map)
	{
		
		NativeList<PathPoint> OpenList = new NativeList<PathPoint>(Allocator.Temp);
		NativeList<PathPoint> CloseList = new NativeList<PathPoint>(Allocator.Temp);
		NativeList<float2> Path = new NativeList<float2>(Allocator.Temp);

		var startPoint = new PathPoint(startPos, 0f, endPos, true, -1,0, float2.zero, 0f);

		var curPos = startPos;
		var curPoint = startPoint;
		OpenList.Add(curPoint);

		CloseList.Add(curPoint);

		while (curPos.x != endPos.x || curPos.y != endPos.y)
		{

			//没有找到路
			if (OpenList.Length == 0)
			{
				UnityEngine.Debug.Log("Can not Find Path To Position:" + endPos);

				return Path;
			}

			//F最小的点作为当前点
			var minCost = OpenList[0].F;
			var minIndex = 0;
			for (int i = 0; i < OpenList.Length; i++)
			{
				if (OpenList[i].F <= minCost)
				{
					minCost = OpenList[i].F;
					minIndex = i;
				}
			}

			curPoint = OpenList[minIndex];
			OpenList.RemoveAt(minIndex);

			


			//UnityEngine.Debug.Log("Check And Remove	" + curPoint.Pos + "  F:" + curPoint.F);

			//L_HasChecked.Add(curPoint.Pos);
			curPos = curPoint.Pos;

			//Find Path
			if (curPoint.Pos.x == endPos.x && curPoint.Pos.y == endPos.y)
			{
				UnityEngine.Debug.Log("Find Path！");

				//当前点就是目标点
				if (curPoint.ParentIndexInCloseList == -1)
				{

					Path.Add(curPoint.Pos);
					CloseList.Dispose();
					OpenList.Dispose();
					return Path;
				}

				//根据Parent返回,GetPath
				var pathPoint = curPoint;

				while (pathPoint.ParentIndexInCloseList != -1)
				{
					Path.Add(pathPoint.Pos);
					if (pathPoint.ParentIndexInCloseList == -1)
						break;
					pathPoint = CloseList[pathPoint.ParentIndexInCloseList];

				}

				CloseList.Dispose();
				OpenList.Dispose();
				return Path;
			}

			//将当前点的相邻点加入ToCheck
			NativeArray<float2> pointToAdd = new NativeArray<float2>(4,Allocator.Temp);
			pointToAdd[0] = curPos + new float2(-0.5f, 0f);
			pointToAdd[1] = curPos + new float2(0.5f, 0f);
			pointToAdd[2] = curPos + new float2(0f, 0.5f);
			pointToAdd[3] = curPos + new float2(0f, -0.5f);

			for (int i = 0;i<4; i++)
			{
				var checkPos = pointToAdd[i];
				var flag = false;
				foreach (var point in CloseList)
				{
					if (point.Pos.x == checkPos.x && point.Pos.y == checkPos.y)
					{
						flag = true;
						break;
					}

				}
				if (flag)
					continue;

				var index = GetIndexByPos(checkPos);
				var cost = map[index].y;

				if(checkPos.x == endPos.x && checkPos.y == endPos.y)
				{
					cost = 10;
				}
				//删除不可走的路

			/*	if (map[index].y > 9999f && !(checkPos.x == endPos.x && checkPos.y == endPos.y))
					continue;*/


				//UnityEngine.Debug.Log(map[1]);
				/*				var newPoint = new PathPoint(checkPos, cost, endPos,
									IsInSameBlock(curPos, checkPos), curPoint.CurIndexInClsoeList,
									CloseList.Length, curPos, curPoint.G);*/
				var newPoint = new PathPoint(checkPos, cost, endPos,
					IsInSameBlock(curPos, checkPos), curPoint.CurIndexInClsoeList,
					CloseList.Length, curPos, curPoint.G);


				//UnityEngine.Debug.Log("ParentPos:" + curPos + "  ClosePos:" + "CurIndex: "+ (CloseList.Length - 1)+" Pos:" + CloseList[curPoint.CurIndexInClsoeList].Pos);

				CloseList.Add(newPoint);
				//Test
				//UnityEngine.Debug.Log("Add ToChck1:" + checkPos+" F:"+ newPoint.F+ " G:" + newPoint.G+ " H:" + newPoint.H*100);



				OpenList.Add(newPoint);
			}

			if (OpenList.Length == 0)
			{
				UnityEngine.Debug.Log("Can not Find Path To Position1:" + endPos);
				return Path;
			}


		}

		UnityEngine.Debug.Log("Find Path！");

		//当前点就是目标点
		if (curPoint.ParentIndexInCloseList == -1)
		{

			Path.Add(curPoint.Pos);
			CloseList.Dispose();
			OpenList.Dispose();
			return Path;
		}

		//根据Parent返回,GetPath
		var pathPoint1 = curPoint;

		while (pathPoint1.ParentIndexInCloseList != -1)
		{
			Path.Add(pathPoint1.Pos);
			if (pathPoint1.ParentIndexInCloseList == -1)
				break;
			pathPoint1 = CloseList[pathPoint1.ParentIndexInCloseList];

		}

		CloseList.Dispose();
		OpenList.Dispose();
		return Path;
	}

	public bool IsInSameBlock(float2 pos1, float2 pos2)
	{
		return true;
		if (Value(pos1.x) == Value(pos2.x) && Value(pos1.y) == Value(pos2.y))
		{
			return true;
		}
		return false;

		int Value(float value)
		{
			return value <= 0 ? (int)value - 1 : (int)value;
		}
	}

	public int GetPathCost(float2 pathPos,NativeArray<int2> map)
	{
		var center = new float2((int)pathPos.x >= 0f ? (int)pathPos.x : (int)pathPos.x - 1, (int)pathPos.y >= 0f ? (int)pathPos.y : (int)pathPos.y - 1);
		return map[GetIndexByPos(center)].y;
	}

	public int GetIndexByPos(float2 pos)
	{
		var x1 = pos.x >= 0f ? 0 : -1;
		var y1 = pos.y >= 0f ? 0 : -1;
		int x = (int)pos.x+x1 + (Size.x - 1) / 2;
		int y = (int)pos.y+y1 + (Size.y - 1) / 2;
		
		return y * Size.x + x;
	}

	public float2 GetPosByIndex(int index)
	{
		var y = index / Size.x;
		var x = index-y*Size.x;
		return new float2(x,y);
	}

	public float2 GetGirdPosition(float2 pos)
	{
		var x1 = pos.x >= 0f ? 0f : -1f;
		var y1 = pos.y >= 0f ? 0f : -1f;

		return new float2((int)pos.x + x1, (int)pos.y + y1);
	}
}

public struct PathPoint
{
	public float2 Pos;
	public float Cost;
	public int ParentIndexInCloseList;
	public int CurIndexInClsoeList;

	public float G;
	public float H;
	public float F;

	public PathPoint(float2 pos, float BlockCost, float2 target, bool isInSameBlock, int parentIndexInCloseList,int curIndexInCloseList,float2 parentPos,float parentG)
	{
		Pos = pos;
		//起始点
		if (curIndexInCloseList == 0)
		{
			ParentIndexInCloseList = -1;
			CurIndexInClsoeList = 0;
			var oDis1 = target - pos;
			Cost = 1f;

			H = math.abs(oDis1.y) + math.abs(oDis1.x);
			G = 1f;
			F = H+G;
		}
		else
		{
			CurIndexInClsoeList=curIndexInCloseList;
			ParentIndexInCloseList = parentIndexInCloseList;

			Cost = BlockCost / 2;

			var center = new float3((int)pos.x >= 0f ? (int)pos.x : (int)pos.x - 1, 0f, (int)pos.y >= 0f ? (int)pos.y : (int)pos.y - 1);
			center += new float3(0.5f, 0, 0.5f);
			var parent1 = new float3(parentPos.x, 0, parentPos.y);
			var target1 = new float3(pos.x, 0, pos.y);
			var isRight=math.cross(center - parent1, target1 - parent1).y > 0f;

			//逆行移动代价*4
			var multi = isRight ? 1f : 4f;


			if (isInSameBlock)
			{
				
				G = parentG + Cost * multi;
				//UnityEngine.Debug.Log("Yes" + " Pos:" + pos + " G:" + G);

			}
			else
			{
				G = parentG + Cost / 2 * multi + parentG / 2 * multi;
				//UnityEngine.Debug.Log("Noe" + " Pos:" + pos + " G:" + G);
			}
				



			var oDis = target - pos;
			H = math.abs(oDis.y) + math.abs(oDis.x);

			F = G + H*100;
		}
	}


}


[BurstCompile]
public partial struct GoToWorkStateJob : IJobEntity
{

	public  float deltaTime;
	public EntityCommandBuffer.ParallelWriter ECB;
	[ReadOnly] public NativeArray<SPointToWork> targetArrayRead;

	[BurstCompile]
	public void Execute(GoToWorkStateAspect aspect, [EntityIndexInQuery] int sortKey)
	{
		var targetArray = new NativeArray<SPointToWork>(targetArrayRead.Length, Allocator.Temp);
		targetArray.CopyFrom(targetArrayRead);
		if (aspect.Move(deltaTime))
		{
		
				if (aspect.CanWork(ECB,sortKey, targetArray))
				{ 
					//休息状态->体力检测状态
					aspect.StateTransitionToWork(ECB, sortKey);

					UnityEngine.Debug.Log("工作");
					return;
				}
			
				aspect.StateTransitionToDurationCheck(ECB, sortKey);
				UnityEngine.Debug.Log("工作地点被占用");
			

		}
	}
}

[BurstCompile]
public partial struct WorkStateJob : IJobEntity
{
	public float deltaTime;
	public Entity sourceEntity;
	public EntityCommandBuffer.ParallelWriter ECB;

	//特殊
	public float workTime;
	public float durationCost;
	[ReadOnly] public NativeArray<BufferGSourceChange> SourceGetArrayRead;

	[BurstCompile]
	public void Execute(WorkStateAspect aspect, [EntityIndexInQuery] int sortKey)
	{
		var SourceGetArray = new NativeArray<BufferGSourceChange>(SourceGetArrayRead.Length, Allocator.Temp);
		SourceGetArray.CopyFrom(SourceGetArrayRead);
		if (aspect.Work(deltaTime, workTime))
		{
			foreach (var sourceGet in SourceGetArray)
			{
				ECB.AppendToBuffer<BufferGSourceChange>(sortKey, sourceEntity, sourceGet);
			}

			//工作状态->体力检测状态
			aspect.StateTransition(ECB, sortKey, durationCost);
		}
	}
}

[BurstCompile]
public partial struct GoToRestStateJob : IJobEntity
{
	public float deltaTime;
	public EntityCommandBuffer.ParallelWriter ECB;

	[BurstCompile]
	public void Execute(GoToRestStateAspect aspect, [EntityIndexInQuery] int sortKey)
	{
		if (aspect.Move(deltaTime))
		{
			//休息状态->体力检测状态
			aspect.StateTransition(ECB, sortKey);
			UnityEngine.Debug.Log("开始休息");
		}
	}
}

