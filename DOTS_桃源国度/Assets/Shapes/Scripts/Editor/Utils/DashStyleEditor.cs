using UnityEngine;
using UnityEditor;

namespace Shapes {

	public class DashStyleEditor {


		SerializedProperty propType;
		SerializedProperty propSpace;
		SerializedProperty propSize;
		SerializedProperty propOffset;
		SerializedProperty propSpacing;
		SerializedProperty propSnap;
		SerializedProperty propShapeModifier;

		SerializedProperty propDashed;
		SerializedProperty propGeometry; // line only
		SerializedProperty dashSizeLinked;

		bool isLine = true;

		public static DashStyleEditor GetLineDashEditor( SerializedProperty propDashSettings, SerializedProperty dashSizeLinked, SerializedProperty propGeometry, SerializedProperty propDashed ) {
			DashStyleEditor editor = new DashStyleEditor {
				isLine = true,
				propDashed = propDashed,
				propGeometry = propGeometry,
				dashSizeLinked = dashSizeLinked
			};
			editor.LoadCommonProperties( propDashSettings );
			return editor;
		}

		public static DashStyleEditor GetDashEditor( SerializedProperty propDashSettings, SerializedProperty dashSizeLinked, SerializedProperty propDashed ) {
			DashStyleEditor editor = new DashStyleEditor {
				isLine = false,
				propDashed = propDashed,
				dashSizeLinked = dashSizeLinked
			};
			editor.LoadCommonProperties( propDashSettings );
			return editor;
		}

		void LoadCommonProperties( SerializedProperty propDashSettings ) {
			propType = propDashSettings.FindPropertyRelative( "type" );
			propSpace = propDashSettings.FindPropertyRelative( "space" );
			propSize = propDashSettings.FindPropertyRelative( "size" );
			propOffset = propDashSettings.FindPropertyRelative( "offset" );
			propSpacing = propDashSettings.FindPropertyRelative( "spacing" );
			propSnap = propDashSettings.FindPropertyRelative( "snap" );
			propShapeModifier = propDashSettings.FindPropertyRelative( "shapeModifier" );
		}

		public void DrawProperties() {
			bool enableDashControls = propDashed.boolValue == false;
			EditorGUILayout.PropertyField( propDashed );

			using( new EditorGUI.DisabledScope( enableDashControls ) ) {
				using( var chChk = new EditorGUI.ChangeCheckScope() ) {
					EditorGUILayout.PropertyField( propSpace, new GUIContent( "Length Space" ) );
					if( chChk.changed && propSpace.enumValueIndex == DashSpace.FixedCount.GetIndex() ) {
						// todo: might want to do per-instance fixup of this, it's a lil wonky
						// but you know what, the world is wonky
						// converts from dash+space to count + space ratio
						float period = propSpacing.floatValue + propSize.floatValue;
						propSpacing.floatValue = propSpacing.floatValue / period;
					}
				}

				EditorGUILayout.PropertyField( propSnap );
			}

			bool displayInCountRatioMode = propSpace.hasMultipleDifferentValues == false && propSpace.enumValueIndex == DashSpace.FixedCount.GetIndex();
			using( new EditorGUI.DisabledScope( enableDashControls ) ) {
				using( ShapesUI.Horizontal ) {
					// size field
					using( var chchk = new EditorGUI.ChangeCheckScope() ) {
						string sizeLabel = displayInCountRatioMode ? "Count" : "Size";
						EditorGUILayout.PropertyField( propSize, new GUIContent( sizeLabel ) );
						//ShapesUI.PropertyFieldWidth( propSize, sizeLabel, 32 );
						if( chchk.changed )
							propSize.floatValue = Mathf.Max( 0.0001f, propSize.floatValue );
					}

					// link button
					bool mixedLinkStates = dashSizeLinked.hasMultipleDifferentValues;
					using( var chchk = new EditorGUI.ChangeCheckScope() ) {
						bool newVal = GUILayout.Toggle( mixedLinkStates ? false : dashSizeLinked.boolValue, mixedLinkStates ? "—" : "=", EditorStyles.miniButton, GUILayout.Width( 22 ) );
						if( chchk.changed )
							dashSizeLinked.boolValue = newVal;
					}
				}


				using( ShapesUI.Horizontal ) {
					DrawSpacingGUI( displayInCountRatioMode );
				}

				using( ShapesUI.Horizontal ) {
					EditorGUILayout.PropertyField( propOffset );
					GUILayout.FlexibleSpace();
				}

				DrawStyleGUI();
			}
		}

		void DrawStyleGUI() {
			bool canSetStyle = !isLine || ( propGeometry.hasMultipleDifferentValues || propGeometry.enumValueIndex != (int)LineGeometry.Volumetric3D );
			if( canSetStyle ) {
				using( new EditorGUILayout.HorizontalScope() ) {
					EditorGUILayout.PrefixLabel( "Style" );
					ShapesUI.DrawTypeSwitchButtons( propType, UIAssets.LineDashButtonContents );
				}

				bool canEditStyle = propShapeModifier.hasMultipleDifferentValues || ( (DashType)propType.enumValueIndex ).HasModifier();
				using( new EditorGUI.DisabledScope( canEditStyle == false ) )
					EditorGUILayout.PropertyField( propShapeModifier );
			} else { // this else is only applicable for lines
				using( new EditorGUI.DisabledScope( true ) ) {
					using( new EditorGUILayout.HorizontalScope() ) {
						EditorGUILayout.PrefixLabel( new GUIContent( "Style", "3D lines support basic dashes only" ) );
						GUILayout.Toggle( true, UIAssets.LineDashButtonContents[0], ShapesUI.GetMiniButtonStyle( 0, 3 ), GUILayout.MinHeight( 20 ) );
						GUILayout.Toggle( false, UIAssets.LineDashButtonContents[1], ShapesUI.GetMiniButtonStyle( 1, 3 ), GUILayout.MinHeight( 20 ) );
						GUILayout.Toggle( false, UIAssets.LineDashButtonContents[2], ShapesUI.GetMiniButtonStyle( 2, 3 ), GUILayout.MinHeight( 20 ) );
					}
				}
			}
		}

		void DrawSpacingGUI( bool displayInCountRatioMode ) {
			bool mixedLinkStates = dashSizeLinked.hasMultipleDifferentValues;
			using( new EditorGUI.DisabledScope( mixedLinkStates || dashSizeLinked.boolValue == true ) ) {
				if( displayInCountRatioMode ) {
					using( var chChk = new EditorGUI.ChangeCheckScope() ) {
						if( mixedLinkStates == false && dashSizeLinked.boolValue == false ) {
							EditorGUI.showMixedValue = propSpacing.hasMultipleDifferentValues;
							float newValue = EditorGUILayout.Slider( "Spacing", propSpacing.floatValue, 0, 1 );
							EditorGUI.showMixedValue = false;
							if( chChk.changed )
								propSpacing.floatValue = Mathf.Clamp01( newValue );
						} else
							EditorGUILayout.Slider( "Spacing", 0.5f, 0, 1 ); // dash == space
					}
				} else {
					if( mixedLinkStates == false && dashSizeLinked.boolValue == true ) {
						EditorGUILayout.PropertyField( propSize, new GUIContent( "Spacing" ) ); // intentional
					} else {
						EditorGUILayout.PropertyField( propSpacing, new GUIContent( "Spacing" ) );
					}
				}
			}
		}


	}


}