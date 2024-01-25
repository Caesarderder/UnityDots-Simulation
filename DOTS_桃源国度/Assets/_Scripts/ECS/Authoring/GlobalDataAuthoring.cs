using Unity.Entities;
using UnityEngine;

public class GlobalDataAuthoring : MonoBehaviour
{
	public GBuildingSO buildingSO;
	public ObjectsDatabaseSO buildingPrefabSO;
	public CharacterGlobalDataSO characterPrefabSO;

	private class Baker:Baker<GlobalDataAuthoring>
	{
		public override void Bake(GlobalDataAuthoring authoring)
		{
			var Entity = GetEntity(TransformUsageFlags.None);
			var data1 = authoring.buildingSO;
			var data2 = authoring.buildingPrefabSO;
			var dataCharacter = authoring.characterPrefabSO;

			//建筑信息
			Gbuilding GBuilding = new Gbuilding
			{
				//Home
				/*HomeCapacity = data.Home.Capacity,
				HomeRestTime = data.Home.RestTime,*/
				Home=new GHomeData
				{
					Capacity = data1.Home.Capacity,
					RestTime = data1.Home.RestTime,
					FoodCost=data1.Home.FoodCost,
				},

				//Wheat
				Wheat=new GPlant4Data
				{
					GrowStage = data1.Wheat.GrowStage,
					GrowTime0 = data1.Wheat.GrowTime0,
					GrowTime1 = data1.Wheat.GrowTime1,
					GrowTime2 = data1.Wheat.GrowTime2,
					GrowTime3 = data1.Wheat.GrowTime3,

					WorkingTime0 = data1.Wheat.WorkingTime0,
					WorkingTime1 = data1.Wheat.WorkingTime1,
					WorkingTime2 = data1.Wheat.WorkingTime2,
					WorkingTime3 = data1.Wheat.WorkingTime3,

					StaminaCost0 = data1.Wheat.StaminaCost0,
					StaminaCost1 = data1.Wheat.StaminaCost1,
					StaminaCost2 = data1.Wheat.StaminaCost2,
					StaminaCost3 = data1.Wheat.StaminaCost3,

					SourceGain = data1.Wheat.SourceGain,
				},

				Institute=new GInstitueData
				{
					Capacity=data1.Institute.Capacity,
					SourceGain=data1.Institute.SourceGain,
					WorkingTime=data1.Institute.WorkingTime,
					DuationCost=data1.Institute.DuationCost,
					MaxGrowUpTime= data1.Institute.MaxGrowUpTime,
				}


	};


			//地图信息
			BuildingEntityMap map = new BuildingEntityMap();
			map.Size = data2.Size;
			int index = 0;
			foreach (var data in data2.objectsData)
			{
				if(data.ID!=0)
				{
					if(!map.EntitiesIndex.ContainsKey(data.ID))
					{
						
						var entity = GetEntity(data.Prefab, TransformUsageFlags.Dynamic);
						map.EntitiesIndex.Add(data.ID, index++);
						map.EntitiesBuffer.Add(entity);
					}
						
				}
			}




			//角色Entity
			var characterEntityData = new GCharacterData
			{
				MaxDuration = dataCharacter.MaxDuration,
				RestTime = dataCharacter.RestTime,
				MoveSpeed = dataCharacter.MoveSpeed,

				Farmer =new SCharacterEntity
				{
					ID= dataCharacter.CharacterPrefabs[0].ID,
					Entity =GetEntity( dataCharacter.CharacterPrefabs[0].Prefab,TransformUsageFlags.Dynamic),
				},

				Logger= new SCharacterEntity
				{
					ID = dataCharacter.CharacterPrefabs[1].ID,
					Entity = GetEntity(dataCharacter.CharacterPrefabs[1].Prefab, TransformUsageFlags.Dynamic),
				},
				Architect = new SCharacterEntity
				{
					ID = dataCharacter.CharacterPrefabs[2].ID,
					Entity = GetEntity(dataCharacter.CharacterPrefabs[2].Prefab, TransformUsageFlags.Dynamic),
				},
				Scientist = new SCharacterEntity
				{
					ID = dataCharacter.CharacterPrefabs[3].ID,
					Entity = GetEntity(dataCharacter.CharacterPrefabs[3].Prefab, TransformUsageFlags.Dynamic),
				},
			};
		
			AddComponent(Entity, GBuilding);
			AddComponent(Entity, characterEntityData);

			AddComponentObject(Entity, map);
			AddBuffer<BufferPathInfo>(Entity);

		}

		
	}
}
