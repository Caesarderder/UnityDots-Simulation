using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class Test44 : MonoBehaviour
{
	public Transform Transform;

	public float3 result;

	private void Update()
	{
		var pos = transform.position;
		var offset = new float3(pos.x, 0f, pos.z);
		var center = new float3(0.5f,0f,0.5f) + offset;
		var target = new float3(Transform.position.x, 0f, Transform.position.z);
		
		result =math.cross(target-center, offset-target);


/*		if(Input.GetKeyDown(KeyCode.I))
		{
			float2 a = new float2(0.5f, 1.0f);
			float2 b = new float2(0.50f, 1f);
			var aa = a == b;
			print(aa);
		}*/
	}
}
