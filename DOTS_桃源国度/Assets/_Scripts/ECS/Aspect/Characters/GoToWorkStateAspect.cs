using Rukhanka;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEditor.Localization.Plugins.XLIFF.V20;
using UnityEngine;
using UnityEngine.TextCore.Text;
using static UnityEngine.EventSystems.EventTrigger;

public readonly partial struct GoToWorkStateAspect : IAspect
{
	public readonly Entity Entity;

	private readonly RefRW<LocalTransform> _transform;

	private readonly RefRW<SCharacter> _character;

	private readonly DynamicBuffer<AnimatorControllerParameterComponent> _bufferAnimatorController;
	
	private readonly DynamicBuffer<BufferPath> _path;

	private readonly RefRW<GoToWorkState> _restState;

	public float2 TargetPos => _character.ValueRO.TargetPos;



	public void StateTransitionToWork(EntityCommandBuffer.ParallelWriter ECB,int sortKey)
	{
		_bufferAnimatorController.ElementAt(2).SetTrigger();
		_bufferAnimatorController.ElementAt(3).IntValue=0;

		var pos = GetGirdPosition(TargetPos);
		var workedPos = TargetPos-pos;

		ECB.AppendToBuffer<BufferBuildingHasWorkedPoint>(sortKey, _character.ValueRO.MoveTarget, new BufferBuildingHasWorkedPoint { pos = workedPos });

		ECB.SetComponentEnabled<GoToWorkState>(sortKey, Entity, false);
		ECB.SetComponentEnabled<WorkState>(sortKey, Entity, true);
	}

	public float2 GetGirdPosition(float2 pos)
	{
		var x1 = pos.x >= 0f ? 0f : -1f;
		var y1 = pos.y >= 0f ? 0f : -1f;

		return new float2((int)pos.x + x1, (int)pos.y + y1);
	}

	public void StateTransitionToDurationCheck(EntityCommandBuffer.ParallelWriter ECB, int sortKey)
	{

		ECB.SetComponentEnabled<GoToWorkState>(sortKey, Entity, false);
		ECB.SetComponentEnabled<DurationCheckState>(sortKey, Entity, true);
	}

	public bool Move(float deltaTime)
	{
		//起点就是目的地
		if (_path.Length == 0)
		{
			_transform.ValueRW.Position = new float3(_character.ValueRO.TargetPos.x, _transform.ValueRO.Position.y, _character.ValueRO.TargetPos.y);
			return true;
		}
			

		var curPos = new float2(_transform.ValueRW.Position.x, _transform.ValueRW.Position.z);
		var targetPos = _path[0].Position;
		var direction = new float3(targetPos.x, 0f, targetPos.y) - new float3(curPos.x, 0f, curPos.y);
		var speed = _character.ValueRO.MoveSpeed;
		var multi = 1 / speed;

		targetPos = _path[0].Position;
		direction = new float3(targetPos.x, 0f, targetPos.y) - new float3(curPos.x, 0f, curPos.y);
		_transform.ValueRW.Rotation = Quaternion.LookRotation(direction, math.up());

		if (math.length(direction) * multi <= deltaTime)
		{
			_path.RemoveAt(0);
			if(_path.Length==0)
			{
				_transform.ValueRW.Position = new float3(_character.ValueRO.TargetPos.x, _transform.ValueRO.Position.y, _character.ValueRO.TargetPos.y);
				return true;
			}
				
/*			//转向
			targetPos = _path[0].Position;
			direction = new float3(targetPos.x, 0f, targetPos.y) - new float3(curPos.x, 0f, curPos.y);
			_transform.ValueRW.Rotation= Quaternion.LookRotation(direction, math.up());*/
		}

		_transform.ValueRW.Position += math.normalize(direction) * speed * deltaTime;
		return false;
	}

	public bool CanWork(EntityCommandBuffer.ParallelWriter ECB, int sortKey, NativeArray<SPointToWork> workPoints)
	{
		//var buffer = ECB.SetBuffer<BufferSFarmWorkPoint>(sortKey, _character.ValueRO.MoveTarget);
		//Debug.Log("CanWorkPointLne:"+buffer.Length);
		for (int i = 0; i < workPoints.Length; i++)
		{
			if (workPoints[i].pos.x == TargetPos.x&& workPoints[i].pos.y == TargetPos.y)
				return true;
		}

		return false;
/*		var buffer=ECB.SetBuffer<BufferSFarmWorkPoint>(sortKey,_character.ValueRO.MoveTarget);
		var targetPos = _character.ValueRO.TargetPos;
		for (int i=0; i<buffer.Length; i++)
		{
			Debug.Log("检测点："+buffer[i].pos);
			if (buffer[i].pos.y== targetPos.y&& buffer[i].pos.x == targetPos.x)
			{
				if(buffer[i].NeedWorking)
				{
					buffer.Add(new BufferSFarmWorkPoint {pos= buffer[i].pos,NeedWorking=false, });
					buffer.RemoveAt(i);
					
					return true;

		
				}
					
				return false;
			}	
		}
		Debug.Log("Error: Not Cotains Work Point:" + targetPos);
		return false;*/
	}
}
