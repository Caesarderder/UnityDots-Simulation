using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct GMap : IComponentData
{
	public NativeArray<float> Map;

	public GMap(int2 size)
	{
		Map=new NativeArray<float>(size.x * size.y, Allocator.Persistent);
	}
}
