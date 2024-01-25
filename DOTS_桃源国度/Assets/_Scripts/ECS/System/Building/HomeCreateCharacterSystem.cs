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
using static UnityEngine.Rendering.DebugUI;

[UpdateAfter(typeof(BuildingSystem))]
public partial struct HomeCreateCharacterSystem : ISystem
{
	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{


		var source = SystemAPI.GetSingleton<GSource>();
		var sourceEntity = SystemAPI.GetSingletonEntity<GSource>();
		var HomeData = SystemAPI.GetSingleton<Gbuilding>().Home;
		var ecb = new EntityCommandBuffer(Allocator.Temp);

		//生成农民

		var curPeople = source.PeopleCur;
		var maxPeople = source.PeopleMax;

		if (maxPeople - curPeople > 0)
		{
			var Farmer = SystemAPI.GetSingleton<GCharacterData>().Farmer.Entity;

			foreach (var (home, tag, entity) in SystemAPI.Query<SHomeAspect, SFarmerHome>().WithEntityAccess())
			{
				var peopleToCreate = maxPeople - curPeople;
				if (HomeData.Capacity > home.HomeLiverNum() && maxPeople - curPeople > 0)
				{

					var generateNum = HomeData.Capacity - home.HomeLiverNum();
					if (peopleToCreate > generateNum)
						curPeople += generateNum;
					else
					{
						generateNum = (int)peopleToCreate;
						curPeople = maxPeople;

					}

					for (var i = 0; i < generateNum; i++)
					{
						//GenerateFarmer
						var farmer = ecb.Instantiate(Farmer);

						//SetFarmerData
						var trans = SystemAPI.GetComponent<LocalTransform>(Farmer);
						var farmerData = SystemAPI.GetComponent<SCharacter>(Farmer);
						farmerData.Home = entity;
						farmerData.HomeGirdPos = GetGirdPosition(home.GetBornTansform());
						trans.Position = home.GetBornTansform();
						ecb.SetComponent<LocalTransform>(farmer, trans);
						ecb.SetComponent<SCharacter>(farmer, farmerData);
						ecb.SetComponent<RestState>(farmer, new RestState { CurRestTime = HomeData.RestTime * i / generateNum, });
			
						//SetHomeData
						home.SetNewLiver(farmer);
					}

					ecb.AppendToBuffer<BufferGSourceChange>(sourceEntity, new BufferGSourceChange { index = 0, value = generateNum });
				}


			}


		}



		//生成科学家
		curPeople = source.ScientistCur;
		maxPeople = source.ScientistMax;

		if (maxPeople - curPeople > 0)
		{
			var Farmer = SystemAPI.GetSingleton<GCharacterData>().Scientist.Entity;
	
			foreach (var (home, tag, entity) in SystemAPI.Query<SHomeAspect, SScientistHome>().WithEntityAccess())
			{
				var peopleToCreate = maxPeople - curPeople;
				if (HomeData.Capacity > home.HomeLiverNum() && maxPeople - curPeople > 0)
				{

					var generateNum = HomeData.Capacity - home.HomeLiverNum();
					if (peopleToCreate > generateNum)
						curPeople += generateNum;
					else
					{
						generateNum = (int)peopleToCreate;
						curPeople = maxPeople;

					}
					
					for (var i = 0; i < generateNum; i++)
					{
						//GenerateFarmer
						var farmer = ecb.Instantiate(Farmer);

						//SetFarmerData
						var trans = SystemAPI.GetComponent<LocalTransform>(Farmer);
						var farmerData = SystemAPI.GetComponent<SCharacter>(Farmer);
						farmerData.Home = entity;
						farmerData.HomeGirdPos = GetGirdPosition(home.GetBornTansform());
						trans.Position = home.GetBornTansform();
						ecb.SetComponent<LocalTransform>(farmer, trans);
						ecb.SetComponent<SCharacter>(farmer, farmerData);
						ecb.SetComponent<RestState>(farmer, new RestState { CurRestTime = HomeData.RestTime-HomeData.RestTime * i / generateNum, });
						//SetHomeData
						home.SetNewLiver(farmer);
					}

					ecb.AppendToBuffer<BufferGSourceChange>(sourceEntity, new BufferGSourceChange { index = 7, value = generateNum });
					ecb.AppendToBuffer<BufferGSourceChange>(sourceEntity, new BufferGSourceChange { index = 0, value = generateNum });
				}


			}


		}




		ecb.Playback(state.EntityManager);
		ecb.Dispose();



	}

	public float2 GetGirdPosition(float3 pos)
	{
		pos += new float3(0.01f, 0.01f, 0.01f);
		var x1 = pos.x >= 0f ? 0f : -1f;
		var y1 = pos.z >= 0f ? 0f : -1f;

		var curGrid = new float2((int)pos.x + x1, (int)pos.z + y1);

		var x = pos.x - curGrid.x;
		var y = pos.z - curGrid.y;
		return new float2(x >= 0.5f ? 0.75f : 0.25f, y >= 0.5f ? 0.75f : 0.25f) + curGrid;
	}
}
