using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public readonly partial struct SFarmAspect : IAspect
{
	public readonly Entity Entity;

	private readonly RefRO<LocalTransform> _transform;

	private readonly RefRW<SFarm> _farm;

	private readonly DynamicBuffer<BufferBuildingWorkPoint> _bufferWorkPoint;
	private readonly DynamicBuffer<BufferBuildingHasWorkedPoint> _bufferHasWorkedPoint;

	public NativeList<float2> GetToWorkPoint()
	{
		var points=new NativeList<float2>(Allocator.Temp);
		var center = new float2(_transform.ValueRO.Position.x, _transform.ValueRO.Position.z);
		foreach (var point in _bufferWorkPoint)
		{
			int flag = 0;
			foreach (var point2 in _bufferHasWorkedPoint)
			{

				if (math.distancesq(point.pos, point2.pos)<0.01f)
				{

					flag = 1;
					continue;
				}

			}
			if(flag == 0)
				points.Add(center+point.pos);

		}

		return points;
	}

	public void GrowUp(float deltaTime,float MaxGrowTime)
	{
		if(_bufferHasWorkedPoint.Length>= _bufferWorkPoint.Length)
		{

			_farm.ValueRW.CurGrowTime += deltaTime;
			if (_farm.ValueRW.CurGrowTime >= MaxGrowTime)
			{
				_bufferHasWorkedPoint.Clear();
				_farm.ValueRW.CurGrowTime = 0f;
			}
		}

	}
}
