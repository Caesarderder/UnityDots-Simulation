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
using UnityEngine.Events;
using UnityEngine.TextCore.Text;

public partial class ImprovementSystem :  SystemBase
{
	public UnityAction<GSource> CannalImprovementChange;
	public List<SMission> missions=new();

	public struct SMission
	{
		public	EImporoveMent Index;
		public float Value;
	}
	protected override void OnCreate()
	{
		

	}


	protected override void OnUpdate()
	{
			
		if(missions.Count > 0)
		{
			foreach (var mission in missions)
			{
				UnityEngine.Debug.Log("ECS:"+mission.Index+" "+mission.Value);
				switch (mission.Index)
				{
					case EImporoveMent.None:
						break;
					case EImporoveMent.FarmGrowSpeed:
						foreach (var item in SystemAPI.Query<RefRW<Gbuilding>>())
						{
							item.ValueRW.Wheat.GrowTime0 += mission.Value;
						}
				
						break;
					case EImporoveMent.FarmGainValue:
						foreach (var item in SystemAPI.Query<RefRW<Gbuilding>>())
						{
							item.ValueRW.Wheat.SourceGain += mission.Value;
						}
						break;
					case EImporoveMent.FarmerWorkTime:
						foreach (var item in SystemAPI.Query<RefRW<Gbuilding>>())
						{
							item.ValueRW.Wheat.WorkingTime0 += mission.Value;
						}
						break;
					case EImporoveMent.CharacterMoveSpeed:
						foreach (var item in SystemAPI.Query<RefRW<GCharacterData>>())
						{
							item.ValueRW.MoveSpeed += mission.Value;
						}
						break;
					case EImporoveMent.ScientistGainValue:
						foreach (var item in SystemAPI.Query<RefRW<Gbuilding>>())
						{
							item.ValueRW.Institute.SourceGain += mission.Value;
						}
						break;
					case EImporoveMent.ScientistWorkTime:
						foreach (var item in SystemAPI.Query<RefRW<Gbuilding>>())
						{
							item.ValueRW.Institute.WorkingTime += mission.Value;
						}
						break;
					default:
						break;
				}
			}
			missions.Clear();
		}

		
	}


}
