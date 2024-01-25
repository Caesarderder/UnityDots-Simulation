using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Reflection;
using Object = UnityEngine.Object;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	//[CustomEditor( typeof( ShapeRenderer ) )]
	[CanEditMultipleObjects]
	public class ShapeRendererEditor : Editor {

		static bool showDepth = false;
		static bool showStencil = false;

		// ShapeRenderer
		protected SerializedProperty propColor;
		SerializedProperty propZTest = null;
		SerializedProperty propZOffsetFactor = null;
		SerializedProperty propZOffsetUnits = null;
		SerializedProperty propColorMask = null;
		SerializedProperty propStencilComp = null;
		SerializedProperty propStencilOpPass = null;
		SerializedProperty propStencilRefID = null;
		SerializedProperty propStencilReadMask = null;
		SerializedProperty propStencilWriteMask = null;
		SerializedProperty propBlendMode = null;
		SerializedProperty propScaleMode = null;
		SerializedProperty propDetailLevel = null;
		SerializedProperty propRenderQueue = null;

		// MeshRenderer
		SerializedObject soRnd;
		SerializedProperty propSortingOrder;
		SerializedProperty propSortingLayer;

		static GUIContent blendModeGuiContent = new GUIContent(
			"Blend Mode",
			"[Opaque] does not support partial transparency, " +
			"but will write to the depth buffer and sort correctly. " +
			"For best results, use MSAA in your project to avoid aliasing " +
			"(note that it may still be aliased in the scene view)\n" +
			"\n" +
			"[Transparent] supports partial transparency, " +
			"but may not sort properly in some cases.\n" +
			"\n" +
			"[Additive] is good for glowing/brightening effects against dark backgrounds\n" +
			"\n" +
			"[Multiplicative] is good for tinting/darkening effects against bright backgrounds"
		);

		static GUIContent scaleModeGuiContent = new GUIContent(
			"Scale Mode",
			"[Uniform] mode means thickness will also scale with the transform, regardless of thickness space settings\n\n" +
			"[Coordinate] mode means thickness values will remain the same even when scaling"
		);

		static GUIContent renderQueueGuiContent = new GUIContent(
			"Render Queue",
			"The render queue of this object. Default is -1, which means it will use the queue from the shader.\n\n" +
			"[Opaque] = 2450 (AlphaTest)\n" +
			"[All Other Blend Modes] = 3000 (Transparent)"
		);

		static GUIContent zTestGuiContent = new GUIContent(
			"Depth Test",
			"How this shape should render depending on the contents of the depth buffer. Note: anything other than [Less Equal] will not use GPU instancing\n\n" +
			"[Less Equal] means it will not render when behind something (default)\n\n" +
			"[Always] will completely ignore the depth buffer, drawing on top of everything else"
		);

		static GUIContent zOffsetFactorGuiContent = new GUIContent(
			"Depth Offset Factor",
			"Depth buffer offset factor, taking the slope into account (default is 0)\n\n" +
			"Practically, this is mostly used to be able to have two things on the same plane, but have one still render on top of the other without Z-fighting/flickering.\n" +
			"Setting this to, say, -1, will make this render on top of things in the same plane, setting this to 1 will make it render behind things on the same plane"
		);

		static GUIContent zOffsetUnitsGuiContent = new GUIContent(
			"Depth Offset Units",
			"Depth buffer offset, not taking the slope into account (default is 0)\n\n" +
			"I've never found much use of this one, seems like a bad version of Z offset factor? It's mostly here for completeness I guess"
		);

		static GUIContent colorMaskGuiContent = new GUIContent(
			"Color Mask",
			"The color channels to render to (default is RGBA)\n\n" +
			"This is useful when you want to specifically exclude writing to alpha, or, when you want it to not render color at all, and only render to stencil"
		);

		static GUIContent stencilCompGuiContent = new GUIContent( "Compare", "Stencil compare function" );
		static GUIContent stencilOpPassGuiContent = new GUIContent( "Pass", "Stencil Op Pass" );
		static GUIContent stencilIDGuiContent = new GUIContent( "Ref", "Stencil reference ID" );
		static GUIContent stencilReadMaskGuiContent = new GUIContent( "Read Mask", "Bitmask for reading stencil values" );
		static GUIContent stencilWriteMaskGuiContent = new GUIContent( "Write Mask", "Bitmask for writing stencil values" );

		public virtual void OnEnable() {
			soRnd = new SerializedObject( targets.Select( t => ( (Component)t ).GetComponent<MeshRenderer>() as Object ).ToArray() );
			propSortingOrder = soRnd.FindProperty( "m_SortingOrder" );
			propSortingLayer = soRnd.FindProperty( "m_SortingLayerID" );

			// will assign all null properties, even in derived types
			FindAllProperties();

			// hide mesh filter/renderer components
			foreach( ShapeRenderer shape in targets.Cast<ShapeRenderer>() )
				shape.HideMeshFilterRenderer();
		}

		void FindAllProperties() {
			IEnumerable<FieldInfo> GetFields( Type type ) {
				return type.GetFields( BindingFlags.Instance | BindingFlags.NonPublic )
					.Where( x => x.FieldType == typeof(SerializedProperty) && x.Name.StartsWith( "m_" ) == false && x.GetValue( this ) == null );
			}

			IEnumerable<FieldInfo> fieldsBase = GetFields( GetType().BaseType );
			IEnumerable<FieldInfo> fieldsInherited = GetFields( GetType() );

			foreach( FieldInfo field in fieldsBase.Concat( fieldsInherited ) ) {
				string fieldName = char.ToLowerInvariant( field.Name[4] ) + field.Name.Substring( 5 );
				field.SetValue( this, serializedObject.FindProperty( fieldName ) );
				if( field.GetValue( this ) == null )
					Debug.LogError( $"Failed to load {target.GetType()} property: {field.Name} !=> {fieldName}" );
			}
		}

		bool updateMeshFromEditorChange = false;

		protected void BeginProperties( bool showColor = true, bool canEditDetailLevel = true ) {
			soRnd.Update();

			using( new ShapesUI.GroupScope() ) {
				updateMeshFromEditorChange = false;

				ShapesUI.SortedEnumPopup<ShapesBlendMode>( blendModeGuiContent, propBlendMode );
				if( ( target as ShapeRenderer ).HasScaleModes )
					EditorGUILayout.PropertyField( propScaleMode, scaleModeGuiContent );

				// sorting/depth stuff
				using( new EditorGUI.IndentLevelScope( 1 ) ) {
					if( showDepth = EditorGUILayout.Foldout( showDepth, "Sorting & Depth" ) ) {
						using( ShapesUI.TempLabelWidth( 140 ) ) {
							EditorGUILayout.PropertyField( propRenderQueue, renderQueueGuiContent );
							ShapesUI.RenderSortingLayerField( propSortingLayer );
							EditorGUILayout.PropertyField( propSortingOrder );
							EditorGUILayout.PropertyField( propZTest, zTestGuiContent );
							EditorGUILayout.PropertyField( propZOffsetFactor, zOffsetFactorGuiContent );
							EditorGUILayout.PropertyField( propZOffsetUnits, zOffsetUnitsGuiContent );
						}
					}
				}

				// stencil
				using( new EditorGUI.IndentLevelScope( 1 ) ) {
					if( showStencil = EditorGUILayout.Foldout( showStencil, "Masking" ) ) {
						DrawColorMaskButtons();
						EditorGUILayout.PropertyField( propStencilComp, stencilCompGuiContent );
						EditorGUILayout.PropertyField( propStencilOpPass, stencilOpPassGuiContent );
						EditorGUILayout.PropertyField( propStencilRefID, stencilIDGuiContent );
						EditorGUILayout.PropertyField( propStencilReadMask, stencilReadMaskGuiContent );
						EditorGUILayout.PropertyField( propStencilWriteMask, stencilWriteMaskGuiContent );
					}
				}

				// warning box about instancing
				int uniqueCount = 0;
				int instancedCount = 0;
				foreach( ShapeRenderer obj in targets.Cast<ShapeRenderer>() ) {
					if( obj.IsUsingUniqueMaterials )
						uniqueCount++;
					else
						instancedCount++;
				}

				if( uniqueCount > 0 ) {
					string infix;
					if( instancedCount == 0 )
						infix = uniqueCount == 1 ? "this object is" : "these objects are";
					else // mixed selection
						infix = "some of these objects are";

					string label = $"Note: {infix} not GPU instanced due to custom depth/mask settings";

					GUIStyle wrapLabel = new GUIStyle( EditorStyles.miniLabel );
					wrapLabel.wordWrap = true;
					using( ShapesUI.Horizontal ) {
						GUIContent icon = EditorGUIUtility.IconContent( "console.warnicon.sml" );
						GUILayout.Label( icon );
						GUILayout.TextArea( label, wrapLabel );
						if( GUILayout.Button( "Reset", EditorStyles.miniButton ) ) {
							propZTest.enumValueIndex = (int)ShapeRenderer.DEFAULT_ZTEST;
							propZOffsetFactor.floatValue = ShapeRenderer.DEFAULT_ZOFS_FACTOR;
							propZOffsetUnits.intValue = ShapeRenderer.DEFAULT_ZOFS_UNITS;
							propColorMask.intValue = (int)ShapeRenderer.DEFAULT_COLOR_MASK;
							propStencilComp.enumValueIndex = (int)ShapeRenderer.DEFAULT_STENCIL_COMP;
							propStencilOpPass.enumValueIndex = (int)ShapeRenderer.DEFAULT_STENCIL_OP;
							propStencilRefID.intValue = ShapeRenderer.DEFAULT_STENCIL_REF_ID;
							propStencilReadMask.intValue = ShapeRenderer.DEFAULT_STENCIL_MASK;
							propStencilWriteMask.intValue = ShapeRenderer.DEFAULT_STENCIL_MASK;
							propRenderQueue.intValue = ShapeRenderer.DEFAULT_RENDER_QUEUE_AUTO;
						}
					}
				}
			}

			if( ( target as ShapeRenderer ).HasDetailLevels ) {
				using( new EditorGUI.DisabledScope( canEditDetailLevel == false ) ) {
					if( canEditDetailLevel ) {
						using( var chChk = new EditorGUI.ChangeCheckScope() ) {
							EditorGUILayout.PropertyField( propDetailLevel );
							if( chChk.changed )
								updateMeshFromEditorChange = true;
						}
					} else {
						EditorGUILayout.TextField( propDetailLevel.displayName, "∞", GUI.skin.label );
					}
				}
			}

			if( showColor )
				PropertyFieldColor();
		}

		protected bool EndProperties() {
			bool propertiesDidChange = soRnd.ApplyModifiedProperties() | serializedObject.ApplyModifiedProperties();
			if( updateMeshFromEditorChange ) {
				foreach( ShapeRenderer shape in targets.Cast<ShapeRenderer>() )
					shape.UpdateMesh();
				updateMeshFromEditorChange = false;
			}

			return propertiesDidChange;
		}

		protected void PropertyFieldColor() => EditorGUILayout.PropertyField( propColor );
		protected void PropertyFieldColor( string s ) => EditorGUILayout.PropertyField( propColor, new GUIContent( s ) );
		protected void PropertyFieldColor( GUIContent content ) => EditorGUILayout.PropertyField( propColor, content );

		const string colorCh = "RGBA";
		const float COLOR_BTN_SAT = 0.5f;

		static Color[] channelColors = new Color[] {
			Color.HSVToRGB( 0, COLOR_BTN_SAT, 1 ),
			Color.HSVToRGB( 1f / 3f, COLOR_BTN_SAT, 1 ),
			Color.HSVToRGB( 2f / 3f, COLOR_BTN_SAT, 1 ),
			Color.HSVToRGB( 0, 0, 1 ),
		};

		static Color channelUnsetColor = new Color( 0.5f, 0.5f, 0.5f, 1f );

		void DrawColorMaskButtons() {
			int value = propColorMask.intValue;

			void DoButton( int i ) {
				int flagValue = 1 << ( 3 - i ); // enum is in ABGR order
				bool prevBit = ( value & flagValue ) > 0;
				GUI.color = prevBit ? channelColors[i] : channelUnsetColor;
				bool newBit = GUILayout.Toggle( prevBit, colorCh[i].ToString(), ShapesUI.GetMiniButtonStyle( i, 4 ), GUILayout.Width( 28 ) );
				GUI.color = Color.white;
				if( prevBit != newBit ) {
					int sign = prevBit == false ? 1 : -1;
					propColorMask.intValue += sign * flagValue;
				}
			}

			GUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel( colorMaskGuiContent );
			for( int i = 0; i < 4; i++ )
				DoButton( i );
			GUILayout.EndHorizontal();
		}

		public bool HasFrameBounds() => true;

		public Bounds OnGetFrameBounds() {
			if( serializedObject.isEditingMultipleObjects ) {
				// this only works for multiselecting shapes of the same type
				// todo: might be able to make a solution using Editor.CreateEditor shenanigans
				Bounds bounds = ( (ShapeRenderer)serializedObject.targetObjects[0] ).GetWorldBounds();
				for( int i = 1; i < serializedObject.targetObjects.Length; i++ )
					bounds.Encapsulate( ( (ShapeRenderer)serializedObject.targetObjects[i] ).GetWorldBounds() );
				return bounds;
			} else {
				return ( (ShapeRenderer)target ).GetWorldBounds();
			}
		}

	}

}