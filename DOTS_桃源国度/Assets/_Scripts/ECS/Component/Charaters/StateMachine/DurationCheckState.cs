using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct DurationCheckState : IComponentData,IEnableableComponent
{
	public float CurDuration;
}
