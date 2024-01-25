using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sun : MonoBehaviour
{
	public Vector3 raise;
	public Vector3 down;
	public float duration;
	// Start is called before the first frame update
	void Start()
    {
		raise = new Vector3(76.9361038f, 269.468384f, 269.427826f);
		down = new Vector3(62.7631493f, 90.2625275f, 90.1790619f);
		transform.DORotate(down, duration).SetEase(Ease.Linear).OnComplete(TweenRotation1);

	}
	void TweenRotation1()
	{
		transform.DORotate(raise, duration).SetEase(Ease.Linear).OnComplete(TweenRotation2);
	}

	void TweenRotation2()
	{
		transform.DORotate(down, duration).SetEase(Ease.Linear).OnComplete(TweenRotation1);
	}
}
