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

public readonly partial struct RestStateAspect : IAspect
{
	public readonly Entity Entity;

	private readonly RefRW<LocalTransform> _transform;

	private readonly RefRW<SCharacter> _character;

	/*	private readonly DynamicBuffer<AnimatorControllerParameterComponent> _bufferHomeLiver;



		private readonly DynamicBuffer<BufferPath> _path;*/

	private readonly RefRW<RestState> _restState;

	public bool Rest(float deltaTime,float maxRestTime)
	{
		_restState.ValueRW.CurRestTime += deltaTime;
		if(_restState.ValueRO.CurRestTime >= maxRestTime)
		{
			Debug.Log("ÐÝÏ¢½áÊø");
			return true;
		
		}
		else

		return false;
	}

	public void ShowCharacter()
	{
		_transform.ValueRW.Scale = 0.128f;
	}

	public void StateTransition(EntityCommandBuffer.ParallelWriter ECB,int sortKey)
	{
		_restState.ValueRW.CurRestTime = _character.ValueRW.Random.NextFloat(0,1f);
		var max = _character.ValueRW.MaxDuration;
		_character.ValueRW.CurDuration = max;
		ShowCharacter();
		ECB.SetComponentEnabled<RestState>(sortKey, Entity, false);
		ECB.SetComponentEnabled<DurationCheckState>(sortKey, Entity, true);
	}
}
