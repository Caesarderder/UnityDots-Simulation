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

public partial class SourceSystem :  SystemBase
{
	public UnityAction<GSource> CannalSource;
	public List<SMission> missions=new();

	public struct SMission
	{
		public	int index;
		public float value;
	}
	protected override void OnCreate()
	{
		missions.Add(new SMission { index = 100, value = 0f });

	}


	protected override void OnUpdate()
	{
	

		foreach (var source  in
				   SystemAPI.Query<RefRW<GSource>
					   >())
		{
			
			if (missions.Count != 0)
			{
				//UnityEngine.Debug.Log("Cost!!" + missions[0].value);
				foreach (SMission mission in missions)
				{
					switch (mission.index)
					{
						case 0:
							source.ValueRW.PeopleCur = math.clamp(mission.value+ source.ValueRO.PeopleCur,0f, source.ValueRO.PeopleMax) ;
							break;
						case 1:
							source.ValueRW.PeopleMax += mission.value;
							break;
						case 2:
							source.ValueRW.FoodCur = math.clamp(mission.value + source.ValueRO.FoodCur, 0f, source.ValueRO.FoodMax);
							break;
						case 3:
							source.ValueRW.FoodMax += mission.value;
							break;
						case 4:
						
							source.ValueRW.WoodCur = math.clamp(mission.value + source.ValueRO.WoodCur, 0f, source.ValueRO.WoodMax);
							break;
						case 5:
							source.ValueRW.WoodMax += mission.value;
							break;
						case 6:
							source.ValueRW.KnowledgeCur = math.clamp(mission.value + source.ValueRO.KnowledgeCur, 0f, 100000f);
							break;
						case 7:
							source.ValueRW.ScientistCur = math.clamp(mission.value + source.ValueRO.ScientistCur, 0f, 100000f);
							break;
						case 8:
							source.ValueRW.ScientistMax = math.clamp(mission.value + source.ValueRO.ScientistMax, 0f, 100000f);
							break;
						default:
							break;
					}
				}
				missions.Clear();
				CannalSource?.Invoke(source.ValueRO);
			}
			foreach (var buffer in SystemAPI.Query< DynamicBuffer<BufferGSourceChange>
					   >())
			{
				
				if (buffer.Length != 0)
				{
					foreach (BufferGSourceChange mission in buffer)
					{
						
						switch (mission.index)
						{
							case 0:
								source.ValueRW.PeopleCur = math.clamp(mission.value + source.ValueRO.PeopleCur, 0f, source.ValueRO.PeopleMax);
							
								break;
							case 1:
								source.ValueRW.PeopleMax += mission.value;
								break;
							case 2:
								source.ValueRW.FoodCur = math.clamp(mission.value + source.ValueRO.FoodCur, 0f, source.ValueRO.FoodMax);
								break;
							case 3:
								source.ValueRW.FoodMax += mission.value;
								break;
							case 4:
								source.ValueRW.WoodCur = math.clamp(mission.value + source.ValueRO.WoodCur, 0f, source.ValueRO.WoodMax);
								break;
							case 5:
								source.ValueRW.WoodMax += mission.value;
								break;
							case 6:
								source.ValueRW.KnowledgeCur = math.clamp(mission.value + source.ValueRO.KnowledgeCur, 0f, 100000f);
								break;
							case 7:
								source.ValueRW.ScientistCur = math.clamp(mission.value + source.ValueRO.ScientistCur, 0f, 100000f);
	
								break;
							case 8:
								source.ValueRW.ScientistMax = math.clamp(mission.value + source.ValueRO.ScientistMax, 0f, 100000f);
								break;
							default:
								break;
								
						}
						
					}
					buffer.Clear();
					CannalSource?.Invoke(source.ValueRO);
				}
			}
			

			//DateSystem
			var Timer = source.ValueRO.DateTimer+SystemAPI.Time.DeltaTime;
			//一周过去了
			if(Timer>=4f)
			{
				Timer = 0f;
				var week = source.ValueRO.Week+1;
				if(week>4)
				{
					week = 0;
					var month= source.ValueRW.Month + 1;
					if (month > 12)
					{
						month = 0;
						source.ValueRW.Year+= + 1;

					}
					source.ValueRW.Month = month;
				}
				source.ValueRW.Week = week;
				CannalSource?.Invoke(source.ValueRO);
			}
			source.ValueRW.DateTimer = Timer;

		}


		
	}


}
