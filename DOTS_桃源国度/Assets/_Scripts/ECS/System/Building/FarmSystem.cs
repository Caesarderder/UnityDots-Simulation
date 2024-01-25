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

[UpdateAfter(typeof(BuildingSystem))]
public partial struct FarmSystem : ISystem
{
	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		var WheatData = SystemAPI.GetSingleton<Gbuilding>().Wheat;

		new FarmJob
		{
			deltaTime = SystemAPI.Time.DeltaTime,
			maxGrowUpTime = WheatData.GrowTime1,
		}.ScheduleParallel();


	}
}

[BurstCompile]
public partial struct FarmJob : IJobEntity
{
	public float deltaTime;
	public float maxGrowUpTime;

	[BurstCompile]
	public void Execute(SFarmAspect aspect)
	{
		aspect.GrowUp(deltaTime, maxGrowUpTime);

	}
}
