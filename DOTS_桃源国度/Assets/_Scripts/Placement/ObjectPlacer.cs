using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


public class ObjectPlacer : MonoBehaviour
{
	[SerializeField]
	/*    private List<GameObject> placedGameObjects = new();*/
	public int Index=0;
	private BuildingSystem buildingSystem;
	private SourceSystem sourceUISystem;

	private void Start()
	{
		buildingSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<BuildingSystem>();
		sourceUISystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<SourceSystem>();
	}

	public int PlaceObject(Vector3 pos,Vector2Int size, int ID,float woodCost,int MoveCost)
    {
		var center = new float3(pos.x, pos.y, pos.z);
		List<float3> positions= new List<float3>();
		for (int i = 0;i<size.x;i++)
		{
			for (int j = 0;j<size.y;j++)
			{
				positions.Add(center+new float3(i,0f,j));
			}
		}
		buildingSystem.BuildMissions.Add(new BuildingSystem.SBuildingMission
		{
			Postions = positions,
			ID = ID,
			Index = Index,
			Cost= MoveCost,
		});
		sourceUISystem.missions.Add(new SourceSystem.SMission { index = 4, value =- woodCost });


		return Index++;
    }

    internal void RemoveObjectAt(int gameObjectIndex,Vector3Int pos,Vector2Int size)
    {

        if (Index <= gameObjectIndex 
            )
            return;

		var center = new int3(pos.x, pos.y, pos.z);
		List<int3> positions = new List<int3>();

		for (int i = 0; i < size.x; i++)
		{
			for (int j = 0; j < size.y; j++)
			{
				positions.Add(center + new int3(i, 0, j));
			}
		}

		buildingSystem.RemoveMissions.Add(new BuildingSystem.SRemoveMission { Index= gameObjectIndex, Pos= positions });


/*		Destroy(placedGameObjects[gameObjectIndex]);
        placedGameObjects[gameObjectIndex] = null;*/
    }
}
