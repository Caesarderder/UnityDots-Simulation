using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : Singleton<InputManager>
{
	[SerializeField]
	private Camera sceneCamera;

	private Vector3 lastPosition;

	public LayerMask WhatIsPlacementGrid;

	public event Action OnClicked, OnExit;

	private void Start()
	{
		sceneCamera = Camera.main;
	}

	private void Update()
	{
		if (Input.GetMouseButtonDown(0))
			OnClicked?.Invoke();
		if (Input.GetKeyDown(KeyCode.Escape))
			OnExit?.Invoke();
	}

	public bool IsPointerOverUI()
	   => EventSystem.current.IsPointerOverGameObject();

	public Vector3 GetSelectedMapPosition()
	{
		Vector3 mousePos = Input.mousePosition;
		mousePos.z = sceneCamera.nearClipPlane;
		Ray ray = sceneCamera.ScreenPointToRay(mousePos);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit, 1000, WhatIsPlacementGrid))
		{
			lastPosition = hit.point;
		}
		return lastPosition;
	}
}
