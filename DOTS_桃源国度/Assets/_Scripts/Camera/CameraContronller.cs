using Cinemachine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraContronller : MonoBehaviour,GameInput.ISIMInput1Actions
{
	public float MoveMulti=1f;
	public float RotateMulti = 1f;
	public float LenMulti=1f;

	public float MaxDis=4.5f;
	public float MinDis=0f;

	public float MaxXRotation = 123f;
	public float MinXRotation = 70f;


	private bool MouseLeftButtonDown;
	private bool MouseRightButtonDown;
	private bool MouseMidButtonDown;

	private Vector2 MouseDelta;
	private float MouseScrollDelta;

	public CinemachineVirtualCamera Cinemachine;
	public Cinemachine3rdPersonFollow CameraController;


	private GameInput _gameInput;

	private void Awake()
	{
		CameraController = Cinemachine.GetCinemachineComponent<Cinemachine3rdPersonFollow>();

	}

	private void OnEnable()
	{

		if (_gameInput == null)
		{
			_gameInput = new GameInput();
			_gameInput.Enable();
			_gameInput.SIMInput1.SetCallbacks(this);
		
		}

	}

	public void OnMouseDelta(InputAction.CallbackContext context)
	{
		if(MouseRightButtonDown)
		{
			MouseDelta = context.ReadValue<Vector2>() *0.02f * MoveMulti;
			var forward = transform.forward;
			forward.y = 0f;

			var right = transform.right;
			right.y = 0f;

			transform.position += right *- MouseDelta.x+ forward *- MouseDelta.y;
		}
		if(MouseMidButtonDown)
		{
			MouseDelta = context.ReadValue<Vector2>() *0.1f * RotateMulti;
			transform.Rotate(new Vector3(0f, MouseDelta.x, 0f),Space.World );
		}
		
	}

	public void OnMouseLeftButton(InputAction.CallbackContext context)
	{
		switch (context.phase)
		{
			case InputActionPhase.Performed:
				MouseLeftButtonDown = true;
				break;
			case InputActionPhase.Canceled:
				MouseLeftButtonDown = false;
				break;
		}
	}
	public void OnMouseMidButton(InputAction.CallbackContext context)
	{
		switch (context.phase)
		{
			case InputActionPhase.Performed:
				MouseMidButtonDown = true;
				break;
			case InputActionPhase.Canceled:
				MouseMidButtonDown = false;
				break;
		}
	}
	public void OnMouseRightButton(InputAction.CallbackContext context)
	{
		switch (context.phase)
		{
			case InputActionPhase.Performed:
				MouseRightButtonDown = true;
				break;
			case InputActionPhase.Canceled:
				MouseRightButtonDown = false;
				break;
		}
	}

	public void OnMouseScroll(InputAction.CallbackContext context)
	{
		MouseScrollDelta=context.ReadValue<Vector2>().y;

		CameraController.CameraDistance = Mathf.Clamp(CameraController.CameraDistance + MouseScrollDelta *- 0.01f * LenMulti,MinDis, MaxDis);

		var delta = 1 - (CameraController.CameraDistance - MinDis) / (MaxDis - MinDis);
		var x=Mathf.Lerp(MinXRotation, MaxXRotation, delta);


		transform.DORotate(new(x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z), Time.deltaTime)  ;


	}


	// Start is called before the first frame update
	void Start()
    {
		
    }

    // Update is called once per frame
    void Update()
    {

	}


}
