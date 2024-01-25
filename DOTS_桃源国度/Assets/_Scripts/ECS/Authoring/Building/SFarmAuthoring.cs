using Role;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class SFarmAuthoring : MonoBehaviour
{
	private void Start()
	{

	}

	private class Baker : Baker<SFarmAuthoring>
	{
		public override void Bake(SFarmAuthoring authoring)
		{
			var Entity = GetEntity(TransformUsageFlags.None);
			SFarm Farm = new SFarm { };
			SBuilding building = new SBuilding
			{
				WoodCost = 100,
				Stability = 100,
			};

			AddComponent(Entity, building);
			AddComponent(Entity, Farm);

			var buffer= AddBuffer<BufferBuildingWorkPoint>(Entity);
			AddBuffer<BufferBuildingHasWorkedPoint>(Entity);
			var center =new float2(authoring.transform.position.x, authoring.transform.position.z) ;
			for (float x = 0.125f;x<1f;x+=0.25f)
			{

				for (float z = 0.125f; z < 1f; z+=0.25f)
				{
					buffer.Add(new BufferBuildingWorkPoint {pos=new float2(x,z),NeedWorking=true });
				}
			}
	


		}


	}
}
