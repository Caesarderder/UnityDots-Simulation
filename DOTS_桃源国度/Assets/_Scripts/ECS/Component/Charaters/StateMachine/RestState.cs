using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct RestState : IComponentData,IEnableableComponent
{
	public float CurRestTime;

}
