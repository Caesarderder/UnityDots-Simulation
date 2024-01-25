using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using System.Diagnostics;
using UnityEditor.Localization.Plugins.XLIFF.V20;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;
using static UnityEngine.EventSystems.EventTrigger;
using System.Drawing;
using System;

public partial class BuildingSystem :  SystemBase
{

	public List<SBuildingMission> BuildMissions = new();
	public List<SRemoveMission> RemoveMissions = new();


	protected override void OnCreate()
	{



	}

	public struct SBuildingMission
	{
		public List<float3> Postions;
		public int ID;
		public int Index;
		public int Cost;
	}
	public struct SRemoveMission
	{
		public int Index;
		public List<int3> Pos;
	}


	protected override void OnUpdate()
	{
	
		if (SystemAPI.ManagedAPI.TryGetSingleton<BuildingEntityMap>(out var map))
		{
			//安置任务
			if (BuildMissions.Count != 0)
			{
				
				//UnityEngine.Debug.Log("建造!");
				//var ECB = new EntityCommandBuffer(Allocator.Temp);
				foreach (var mission in BuildMissions)
				{
					var entityRefer = map.GetEntityById(mission.ID);
					var localTranform= SystemAPI.GetComponent<LocalTransform>(entityRefer);

					Entity entity = EntityManager.Instantiate(entityRefer);
					SystemAPI.SetComponent(entity, new LocalTransform
					{
						Position = new float3(mission.Postions[0].x, mission.Postions[0].y, mission.Postions[0].z),
						Rotation = localTranform.Rotation,
						Scale = localTranform.Scale,
					});

					map.placedGameObjects.Add(mission.Index,entity);

					var entityMap = SystemAPI.GetSingletonEntity<Gbuilding>();
					var buffer = SystemAPI.GetBuffer<BufferPathInfo>(entityMap);
					
					foreach (var pos in mission.Postions)
					{

						buffer.Add(new BufferPathInfo
						{
							Postion = pos,
							ID = mission.ID,
							Cost = mission.Cost,
						});
					}
	
				}
		
				}
				BuildMissions.Clear();
/*				ECB.Playback(EntityManager);
				ECB.Dispose();*/
			}

			//拆除任务
			if (RemoveMissions.Count != 0)
			{
				UnityEngine.Debug.Log("拆除！");

				foreach (var mission in RemoveMissions)
				{
					
					EntityManager.DestroyEntity(map.placedGameObjects[mission.Index]);

					map.placedGameObjects.Remove(mission.Index);
					var entityMap = SystemAPI.GetSingletonEntity<Gbuilding>();
					var buffer = SystemAPI.GetBuffer<BufferPathInfo>(entityMap);
				foreach (var pos in mission.Pos)
				{
			
					buffer.Add(new BufferPathInfo
					{
						Postion = pos,
						ID = 0,
						Cost = 100,
					});
				}
		
		
						/*map.placedGameObjects[mission] = EntityManager.CreateEntity();*/
					}
				RemoveMissions.Clear();
	
			}

		}

	}



