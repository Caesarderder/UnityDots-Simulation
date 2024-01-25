using Rukhanka;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEditor.Localization.Plugins.XLIFF.V20;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public readonly partial struct DurationCheckStateAspect : IAspect
{
	public readonly Entity Entity;

	private readonly RefRW<LocalTransform> _transform;

	private readonly DynamicBuffer<AnimatorControllerParameterComponent> _bufferAnimatorController;

	private readonly RefRW<SCharacter> _character;

	private readonly DynamicBuffer<BufferPath> _path;

	private readonly RefRW<DurationCheckState> _DurationCheckState;

	public int StateTrasitionCheck(float durationNeed)
	{
		//Debug.Log("Cur:" + _character.ValueRO.CurDuration + "   " + durationNeed);
		if(_character.ValueRO.CurDuration>=durationNeed)
		{
			return 1;
		}
		else
		{
			return 0;	
		}
	}

	public float3 CurPosition => _transform.ValueRO.Position;

	public void SetTargetPos(float2 targetPos)
	{
		_character.ValueRW.TargetPos = targetPos;
	}

	public float2 CurGridPosition => GetGirdPosition(_transform.ValueRO.Position);
	public float2 HomeGridPosition =>_character.ValueRO.HomeGirdPos;

	//!!!!!不能是int，不然会卡!!!!!
	public float2 GetGirdPosition(float3 pos)
	{
		pos+=new float3(0.01f, 0.01f, 0.01f);
		var x1 = pos.x >= 0f ? 0f : -1f;
		var y1 = pos.z >= 0f ? 0f : -1f;

		var curGrid = new float2((int)pos.x + x1, (int)pos.z + y1);

		var x = pos.x - curGrid.x;
		var y = pos.z - curGrid.y;
		return new float2(x >= 0.5f ? 0.75f : 0.25f, y >= 0.5f ? 0.75f : 0.25f) + curGrid;
	}

	public float2 GetGirdPosition(float2 pos)
	{
		var x1 = pos.x >= 0f ? 0f : -1f;
		var y1 = pos.y >= 0f ? 0f : -1f;

		var curGrid = new float2((int)pos.x + x1, (int)pos.y + y1);

		var x = pos.x - curGrid.x;
		var y = pos.y - curGrid.y;
		return new float2(x >= 0.5f ? 0.75f : 0.25f, y >= 0.5f ? 0.75f : 0.25f) + curGrid;
	}

	public void ShowCharacter()
	{
		_transform.ValueRW.Scale = 0.128f;
	}

	public void Clear()
	{
		_path.Clear();
	}

	public void StateTransitionToGoToWork(EntityCommandBuffer.ParallelWriter ECB,int sortKey,Entity farmNearest)
	{
		ShowCharacter();

		_bufferAnimatorController.ElementAt(1).SetTrigger();

/*		if (_path.Length!=0)
		{
			var curPos = _transform.ValueRO.Position;
			var targetPos = _path[0].Position;
			var direction = new float3(targetPos.x, 0f, targetPos.y) - new float3(curPos.x, 0f, curPos.y);
			_transform.ValueRW.Rotation = Quaternion.LookRotation(direction, math.up());
		}*/

		_character.ValueRW.MoveTarget = farmNearest;
		ECB.SetComponentEnabled<DurationCheckState>(sortKey, Entity, false);
		ECB.SetComponentEnabled<GoToWorkState>(sortKey, Entity, true);
	}

	public void StateTransitionToGoToRest(EntityCommandBuffer.ParallelWriter ECB, int sortKey)
	{
		ShowCharacter();

		_bufferAnimatorController.ElementAt(1).SetTrigger();

		if (_path.Length != 0)
		{
			var curPos = _transform.ValueRO.Position;
			var targetPos = _path[0].Position;
			var direction = new float3(targetPos.x, 0f, targetPos.y) - new float3(curPos.x, 0f, curPos.y);
			_transform.ValueRW.Rotation = Quaternion.LookRotation(direction, math.up());
		}

		_character.ValueRW.MoveTarget = _character.ValueRO.Home;
		ECB.SetComponentEnabled<DurationCheckState>(sortKey, Entity, false);
		ECB.SetComponentEnabled<GoToRestState>(sortKey, Entity, true);
	}

	public void StateTrasitionToRest(EntityCommandBuffer.ParallelWriter ECB, int sortKey,float maxRestTime)
	{
		_bufferAnimatorController.ElementAt(0).SetTrigger();
		ECB.SetComponent<RestState>( sortKey, Entity, new RestState { CurRestTime = maxRestTime- GetRandomFloat (0.1f,1f)});
		ECB.SetComponentEnabled<DurationCheckState>(sortKey, Entity, false);
		ECB.SetComponentEnabled<RestState>(sortKey, Entity, true);
	}

	public float GetRandomFloat(float min,float max)
	{
		return _character.ValueRO.Random.NextFloat(min, max);
	}
}
