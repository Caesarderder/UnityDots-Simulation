using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct WorkState : IComponentData,IEnableableComponent
{
	public float CurWorkTime;
}
