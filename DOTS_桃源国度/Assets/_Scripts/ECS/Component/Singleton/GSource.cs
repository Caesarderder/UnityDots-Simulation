using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct GSource : IComponentData
{
	public float PeopleCur;
	public float PeopleMax;

	public float FoodCur;
	public float FoodMax;

	public float WoodCur;
	public float WoodMax;

	public float KnowledgeCur;

	public float ScientistCur;
	public float ScientistMax;

	public int Year;
	public int Month;
	public int Week;
	public float DateTimer;
}

[InternalBufferCapacity(8)]
public struct BufferGSourceChange : IBufferElementData
{
	public int index;
	public float value;
}
