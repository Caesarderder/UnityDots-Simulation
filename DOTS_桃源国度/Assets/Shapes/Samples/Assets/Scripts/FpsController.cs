using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Shapes {

	[Serializable]
	public class Decayer {
		public float decaySpeed;
		public float magnitude;
		public AnimationCurve curve;
		[NonSerialized] public float value;
		[NonSerialized] public float valueInv;
		[NonSerialized] public float t;

		public void SetT( float v ) => t = v;

		public void Update() {
			t = Mathf.Max( 0, ( t - decaySpeed * Time.deltaTime ) );
			float tEval = curve.keys.Length > 0 ? curve.Evaluate( 1f - t ) : t;
			value = tEval * magnitude;
			valueInv = ( 1f - tEval ) * magnitude;
		}
	}

	[ExecuteAlways]
	public class FpsController : ImmediateModeShapeDrawer {

		// components
		public Transform head;
		public Camera cam;
		public Crosshair crosshair;
		public ChargeBar chargeBar;
		public AmmoBar ammoBar;
		public Compass compass;
		public Transform crosshairTransform;

		[Header( "Player Movement" )] [Range( 0.8f, 1f )] public float smoof = 0.99f;
		public float moveSpeed = 1f;
		public float lookSensitivity = 1f;
		float yaw;
		float pitch;
		Vector2 moveInput = Vector2.zero;
		Vector3 moveVel = Vector3.zero;

		[Header( "Sidebar Style" )] [Range( 0f, ShapesMath.TAU / 2 )] public float ammoBarAngularSpanRad;
		[Range( 0, 0.05f )] public float ammoBarOutlineThickness = 0.1f;
		[Range( 0, 0.2f )] public float ammoBarThickness;
		[Range( 0, 0.2f )] public float ammoBarRadius;

		[Header( "Animation" )] [Range( 0f, 0.3f )] public float fireSidebarRadiusPunchAmount = 0.1f;
		public AnimationCurve shakeAnimX = AnimationCurve.Constant( 0, 1, 0 );
		public AnimationCurve shakeAnimY = AnimationCurve.Constant( 0, 1, 0 );

		void Awake() {
			if( Application.isPlaying == false )
				return;
			InputFocus = true;
			StartCoroutine( FixedSteps() );
		}

		// called by the ImmediateModeShapeDrawer base type
		public override void DrawShapes( Camera cam ) {
			if( cam != this.cam ) // only draw in the player camera
				return;

			using( Draw.Command( cam ) ) {
				Draw.ZTest = CompareFunction.Always; // to make sure it draws on top of everything like a HUD
				Draw.Matrix = crosshairTransform.localToWorldMatrix; // draw it in the space of crosshairTransform
				Draw.BlendMode = ShapesBlendMode.Transparent;
				Draw.LineGeometry = LineGeometry.Flat2D;
				crosshair.DrawCrosshair();
				float radiusPunched = ammoBarRadius + fireSidebarRadiusPunchAmount * crosshair.fireDecayer.value;
				ammoBar.DrawBar( this, radiusPunched );
				chargeBar.DrawBar( this, radiusPunched );
				compass.DrawCompass( head.transform.forward );
			}
		}

		IEnumerator FixedSteps() {
			while( true ) {
				FixedUpdateManual();
				yield return new WaitForSeconds( 0.01f ); // 100 fps
			}
		}

		public static void DrawRoundedArcOutline( Vector2 origin, float radius, float thickness, float outlineThickness, float angStart, float angEnd ) {
			// inner / outer
			float innerRadius = radius - thickness / 2;
			float outerRadius = radius + thickness / 2;
			const float aaMargin = 0.01f;
			Draw.Arc( origin, innerRadius, outlineThickness, angStart - aaMargin, angEnd + aaMargin );
			Draw.Arc( origin, outerRadius, outlineThickness, angStart - aaMargin, angEnd + aaMargin );

			// rounded caps
			Vector2 originBottom = origin + ShapesMath.AngToDir( angStart ) * radius;
			Vector2 originTop = origin + ShapesMath.AngToDir( angEnd ) * radius;
			Draw.Arc( originBottom, thickness / 2, outlineThickness, angStart, angStart - ShapesMath.TAU / 2 );
			Draw.Arc( originTop, thickness / 2, outlineThickness, angEnd, angEnd + ShapesMath.TAU / 2 );
		}

		public Vector2 GetShake( float speed, float amp ) {
			float shakeVal = ShapesMath.Frac( Time.time * speed );
			float shakeX = shakeAnimX.Evaluate( shakeVal );
			float shakeY = shakeAnimY.Evaluate( shakeVal );
			return new Vector2( shakeX, shakeY ) * amp;
		}


		bool InputFocus {
			get => !Cursor.visible;
			set {
				Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
				Cursor.visible = !value;
			}
		}

		void FixedUpdateManual() {
			if( Application.isPlaying == false )
				return;
			if( InputFocus ) {
				Vector3 right = head.right;
				Vector3 forward = head.forward;
				forward.y = 0;
				moveVel += ( moveInput.y * forward + moveInput.x * right ) * ( Time.fixedDeltaTime * moveSpeed );
			}

			transform.position += moveVel * Time.deltaTime; // move
			moveVel *= smoof; // decelerate
		}


		void Update() {
			if( Application.isPlaying == false )
				return;

			crosshair.UpdateCrosshairDecay();
			chargeBar.UpdateCharge();

			if( InputFocus ) {
				// mouselook
				yaw += Input.GetAxis( "Mouse X" ) * lookSensitivity;
				pitch -= Input.GetAxis( "Mouse Y" ) * lookSensitivity;
				pitch = Mathf.Clamp( pitch, -90, 90 );
				head.localRotation = Quaternion.Euler( pitch, yaw, 0f );

				chargeBar.isCharging = Input.GetMouseButton( 1 ); // rmb

				if( Input.GetKey( KeyCode.R ) )
					ammoBar.Reload();

				// actions
				if( Input.GetMouseButtonDown( 0 ) && ammoBar.HasBulletsLeft ) {
					// Fire
					ammoBar.Fire();
					crosshair.Fire();
					Ray ray = new Ray( head.position, head.forward );
					if( Physics.Raycast( ray, out RaycastHit hit ) && hit.collider.gameObject.name == "Enemy" )
						crosshair.FireHit();
				}

				// move input
				moveInput = Vector2.zero;

				void DoInput( KeyCode key, Vector2 dir ) {
					if( Input.GetKey( key ) )
						moveInput += dir;
				}

				DoInput( KeyCode.W, Vector2.up );
				DoInput( KeyCode.S, Vector2.down );
				DoInput( KeyCode.D, Vector2.right );
				DoInput( KeyCode.A, Vector2.left );

				// leave focus mode stuff
				if( Input.GetKeyDown( KeyCode.Escape ) )
					InputFocus = false;
			} else if( Input.GetMouseButtonDown( 0 ) ) {
				InputFocus = true;
			}
		}
	}

}