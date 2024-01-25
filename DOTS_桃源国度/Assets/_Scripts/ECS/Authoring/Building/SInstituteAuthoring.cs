using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class SInstituteAuthoring : MonoBehaviour
{
	private class Baker : Baker<SInstituteAuthoring>
	{
		public override void Bake(SInstituteAuthoring authoring)
		{
			var Entity = GetEntity(TransformUsageFlags.None);
			SInstitute Institute = new SInstitute { };

			SBuilding building = new SBuilding
			{
				WoodCost = 100,
				Stability = 100,
			};
			AddBuffer<BufferBuildingHasWorkedPoint>(Entity);
			AddComponent(Entity, building);
			AddComponent(Entity, Institute);
		}


	}
}
