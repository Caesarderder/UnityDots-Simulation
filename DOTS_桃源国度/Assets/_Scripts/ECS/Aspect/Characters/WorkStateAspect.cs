  using Rukhanka;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEditor.Localization.Plugins.XLIFF.V20;
using UnityEngine;
using UnityEngine.TextCore.Text;
using static UnityEngine.EventSystems.EventTrigger;

public readonly partial struct WorkStateAspect : IAspect
{
	public readonly Entity Entity;

	private readonly RefRW<LocalTransform> _transform;

	private readonly RefRW<SCharacter> _character;

	/*	private readonly DynamicBuffer<AnimatorControllerParameterComponent> _bufferHomeLiver;



		private readonly DynamicBuffer<BufferPath> _path;*/

	private readonly RefRW<WorkState> _workState;

	public bool Work(float deltaTime,float workTime)
	{
	
		_workState.ValueRW.CurWorkTime += deltaTime;
		if (_workState.ValueRW.CurWorkTime> workTime)
		{
			return true;
		}
		return false;
	}

	public void StateTransition(EntityCommandBuffer.ParallelWriter ECB,int sortKey,float durationCost)
	{
		_workState.ValueRW.CurWorkTime = 0f;

		_character.ValueRW.CurDuration -= durationCost;
		Debug.Log("工作完成:"+ _character.ValueRW.CurDuration);
		ECB.SetComponentEnabled<WorkState>(sortKey, Entity, false);
		ECB.SetComponentEnabled<DurationCheckState>(sortKey, Entity, true);
	}
}
