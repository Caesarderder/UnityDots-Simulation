using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct GCharacterData:IComponentData
{
	public float MaxDuration;
	public float RestTime;
	public float MoveSpeed;

	public SCharacterEntity Farmer;
	public SCharacterEntity Logger;
	public SCharacterEntity Architect;
	public SCharacterEntity Scientist;
}

public struct SCharacterEntity
{
	public int ID;
	public Entity Entity;
}
