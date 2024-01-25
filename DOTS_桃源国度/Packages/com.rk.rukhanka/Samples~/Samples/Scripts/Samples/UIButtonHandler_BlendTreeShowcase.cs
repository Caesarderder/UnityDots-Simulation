using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka.Samples
{
class UIButtonHandler_BlendTreeShowcase: MonoBehaviour
{
	[Serializable]
	public struct ShowcaseData
	{
		public Button UIBtn;
		public GameObject UIRoot;
		public Transform vCam;
	}
	public ShowcaseData[] showcases;

	Camera cam;
	Transform targetCamera;

/////////////////////////////////////////////////////////////////////////////////

	void Start()
	{
		for (int i = 0; i < showcases.Length; ++i)
		{
			var sc = showcases[i];
			var li = i;
			sc.UIBtn.onClick.AddListener(() => OnBtnClick(li));
		}

		OnBtnClick(0);
		showcases[0].UIBtn.Select();

		cam = Camera.main;
		targetCamera = showcases[0].vCam;
	}

/////////////////////////////////////////////////////////////////////////////////

	IEnumerator DelayedUISwitch(ShowcaseData sc, bool b)
	{
		yield return new WaitForSeconds(0.5f);
		sc.UIRoot.SetActive(b);
	}

/////////////////////////////////////////////////////////////////////////////////

	void Update()
	{
		var dp = targetCamera.position - cam.transform.position;
		if (dp.magnitude < 0.1f)
			return;

		cam.transform.position += dp.normalized * Mathf.Max(dp.magnitude, 2.0f) * Time.deltaTime * 1.5f;
	}

/////////////////////////////////////////////////////////////////////////////////

	void OnBtnClick(int idx)
	{
		for (int i = 0; i < showcases.Length; ++i)
		{
			var sc = showcases[i];
			var b = i == idx;
			if (b)
				targetCamera = sc.vCam;
			StartCoroutine(DelayedUISwitch(sc, b));
		}
	}
}
}
