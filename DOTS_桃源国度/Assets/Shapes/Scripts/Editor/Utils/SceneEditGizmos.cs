using UnityEditor;
using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	public class SceneEditGizmos {

		public Editor parentEditor;

		protected virtual bool IsEditing {
			get => false;
			set => _ = value;
		}

		public bool IsHoldingAlt => ( Event.current.modifiers & EventModifiers.Alt ) != 0;
		bool DidCancelAction => Tools.current != Tool.None || ( Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape );

		void RepaintSceneAndInspector() {
			if( SceneView.currentDrawingSceneView != null )
				SceneView.currentDrawingSceneView.Repaint();
			parentEditor.Repaint();
		}

		// called both in the scene view and the inspector/editor to allow being focused on either
		protected void CheckForCancelEditAction() {
			if( IsEditing == false )
				return;
			if( DidCancelAction ) {
				SetEditMode( false );
				RepaintSceneAndInspector();
			}
		}

		protected void SetEditMode( bool on ) {
			if( IsEditing == on ) return;
			IsEditing = on;
			if( on ) {
				#if UNITY_2019_1_OR_NEWER
				// turn on gizmos if they're off (global gizmo enabled state was added in 2019.1)
				if( SceneView.lastActiveSceneView != null )
					SceneView.lastActiveSceneView.drawGizmos = true;
				#endif
				Tools.current = Tool.None;
			}

			SceneView.RepaintAll();
		}

		public void GUIEditButton( string label = "Edit in Scene" ) {
			using( var chChk = new EditorGUI.ChangeCheckScope() ) {
				bool newValue = GUILayout.Toggle( IsEditing, label, GUI.skin.button );
				if( chChk.changed )
					SetEditMode( newValue );
			}

			CheckForCancelEditAction();
		}

	}

}