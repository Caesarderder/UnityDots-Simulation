using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public readonly partial struct SHomeAspect : IAspect
{
	public readonly Entity Entity;

	private readonly RefRO<LocalTransform> _transform;


	private readonly DynamicBuffer<BufferSHomeLiver> _bufferHomeLiver;

	public int HomeLiverNum()
	{
		return _bufferHomeLiver.Length;
	}

	public void SetNewLiver(Entity liver)
	{
		_bufferHomeLiver.Add(new BufferSHomeLiver { entity = liver, });

	}

	public float3 GetBornTansform()
	{
		return new float3(_transform.ValueRO.Position.x+0.25f,0.467f, _transform.ValueRO.Position.z + 0.25f);
	}
}
