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
public partial struct InstituteSystem : ISystem
{
	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		var Institute = SystemAPI.GetSingleton<Gbuilding>().Institute;

		new InstituteJob
		{
			deltaTime = SystemAPI.Time.DeltaTime,
			maxGrowUpTime = Institute.MaxGrowUpTime,
			maxCapcity = Institute.Capacity,
		}.ScheduleParallel();
	}
}

[BurstCompile]
public partial struct InstituteJob : IJobEntity
{
	public float deltaTime;
	public float maxGrowUpTime;
	public int maxCapcity;

	[BurstCompile]
	public void Execute(SInstituteAspect aspect)
	{
		aspect.ChangeCurPeople();
		aspect.ReadyForWork(maxCapcity, maxGrowUpTime, deltaTime);

	}
}
