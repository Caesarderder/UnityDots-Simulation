using Role;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class SHomeAuthoring: MonoBehaviour
{
	public EHomeType HomeType;
	private void Start()
	{
		
	}

	private class Baker:Baker<SHomeAuthoring>
	{
		public override void Bake(SHomeAuthoring authoring)
		{
			var Entity = GetEntity(TransformUsageFlags.None);
			SHome Home= new SHome { };

			switch (authoring.HomeType)
			{

				case EHomeType.None:
					break;
				case EHomeType.Farmer:
					AddComponent(Entity, new SFarmerHome { });
					break;
				case EHomeType.Scientist:
					AddComponent(Entity, new SScientistHome { });
					break;
				default:
					break;
			}

			SBuilding building = new SBuilding
			{
				WoodCost = 100,
				Stability = 100,
			};
	
			AddComponent(Entity, building);
			AddComponent(Entity, Home);

			AddBuffer<BufferSHomeLiver>(Entity);

			
		}

		
	}
}

public enum EHomeType
{
	None,
	Farmer,
	Scientist,
}
