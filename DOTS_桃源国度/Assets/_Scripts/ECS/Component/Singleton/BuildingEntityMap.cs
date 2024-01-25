using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class BuildingEntityMap : IComponentData
{

	#region BuildingEntityMap
	public int2 Size;
	public Dictionary<int, Entity> placedGameObjects=new();

	#endregion

	#region BuildingEntity
	public Dictionary<int, int> EntitiesIndex = new();
	public List<Entity> EntitiesBuffer = new();



	public Entity GetEntityById(int id)
	{
		if(EntitiesIndex.ContainsKey(id))
		{
			
			return EntitiesBuffer[EntitiesIndex[id]];
		}
				

		Debug.LogWarning("Null Entity ID:" + id);
		return EntitiesBuffer[0];
	}
	#endregion

}
