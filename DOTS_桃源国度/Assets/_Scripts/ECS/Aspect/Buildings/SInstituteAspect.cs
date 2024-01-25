using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public readonly partial struct SInstituteAspect : IAspect
{
	public readonly Entity Entity;

	private readonly RefRO<LocalTransform> _transform;

	private readonly RefRW<SInstitute> _institute;

	private readonly DynamicBuffer<BufferBuildingHasWorkedPoint> _bufferHasWorkedPoint;

	public NativeList<float2> GetToWorkPoint(int maxCapcity)
	{
		var points=new NativeList<float2>(Allocator.Temp);
		if(_institute.ValueRO.CurPeople<= maxCapcity)
			points.Add(new float2(_transform.ValueRO.Position.x+0.35f, _transform.ValueRO.Position.z+0.35f));
		return points;
	}

	public void ChangeCurPeople()
	{
		if(_bufferHasWorkedPoint.Length!=0)
		{
			_institute.ValueRW.CurPeople += _bufferHasWorkedPoint.Length;
		}
		_bufferHasWorkedPoint.Clear();
	}

	//人满了之后，需要等待一段时间
	public void ReadyForWork(int maxCapcity,float GrowUpTime,float deltaTime)
	{
		if(_institute.ValueRO.CurPeople>= maxCapcity)
		{
			_institute.ValueRW.GrowUpTime += deltaTime;
			if(_institute.ValueRO.GrowUpTime> GrowUpTime)
			{
				_institute.ValueRW.GrowUpTime = 0f;
				_institute.ValueRW.CurPeople = 0;
			}
		}
	}
}
