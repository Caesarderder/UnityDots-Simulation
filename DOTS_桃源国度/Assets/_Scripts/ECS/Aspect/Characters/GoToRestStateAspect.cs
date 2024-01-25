using Rukhanka;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEditor.Localization.Plugins.XLIFF.V20;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;
using static UnityEngine.EventSystems.EventTrigger;

public readonly partial struct GoToRestStateAspect : IAspect
{
	public readonly Entity Entity;

	private readonly RefRW<LocalTransform> _transform;

	private readonly RefRO<SCharacter> _character;

	/*	private readonly DynamicBuffer<AnimatorControllerParameterComponent> _bufferHomeLiver;
	*/


	private readonly DynamicBuffer<BufferPath> _path;

	private readonly RefRW<GoToRestState> _restState;


	public void StateTransition(EntityCommandBuffer.ParallelWriter ECB,int sortKey)
	{
		_transform.ValueRW.Scale = 0f;
		ECB.SetComponentEnabled<GoToRestState>(sortKey, Entity, false);
		ECB.SetComponentEnabled<RestState>(sortKey, Entity, true);
	}

	public bool Move(float deltaTime)
	{
		//起点就是目的地
		if (_path.Length == 0)
			return true;

		var curPos = new float2(_transform.ValueRW.Position.x, _transform.ValueRW.Position.z);
		var targetPos = _path[0].Position;
		var direction = new float3(targetPos.x, 0f, targetPos.y) - new float3(curPos.x, 0f, curPos.y);
		var speed = _character.ValueRO.MoveSpeed;
		var multi = 1 / speed;

		if (math.length(direction)* multi <= deltaTime)
		{
			_path.RemoveAt(0);
			if(_path.Length==0)
				return true;
			//转向
			targetPos = _path[0].Position;
			direction = new float3(targetPos.x, 0f, targetPos.y) - new float3(curPos.x, 0f, curPos.y);
			_transform.ValueRW.Rotation= Quaternion.LookRotation(direction, math.up());
		}

		_transform.ValueRW.Position += math.normalize(direction) * speed * deltaTime;
		return false;
	}
}
