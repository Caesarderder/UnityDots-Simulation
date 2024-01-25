using System;
using System.Collections.Generic;
using System.IO;
using Shapes;
using UnityEditor;
using UnityEngine;

#if SHAPES_URP
using System.Linq;
#if UNITY_2021_2_OR_NEWER
using URP_RND_DATA = UnityEngine.Rendering.Universal.ScriptableRendererData;

#else
using URP_RND_DATA = UnityEngine.Rendering.Universal.ForwardRendererData;
#endif
#endif

public class ShapesConfigWindow : EditorWindow {

	// General
	SerializedProperty useHdrColorPickers;
	SerializedProperty autoConfigureRenderPipeline;
	SerializedProperty useImmediateModeInstancing;
	SerializedProperty pushPopStateInDrawCommands;
	SerializedProperty polylineDefaultPointsPerTurn;
	SerializedProperty polylineBezierAngularSumAccuracy;

	// Primitive detail
	SerializedProperty sphereDetail;
	SerializedProperty torusDivsMinorMajor;
	SerializedProperty coneDivs;
	SerializedProperty cylinderDivs;
	SerializedProperty capsuleDivs;

	// Primitive Bounds
	SerializedProperty boundsSizeQuad;
	SerializedProperty boundsSizeTriangle;
	SerializedProperty boundsSizeSphere;
	SerializedProperty boundsSizeTorus;
	SerializedProperty boundsSizeCuboid;
	SerializedProperty boundsSizeCone;
	SerializedProperty boundsSizeCylinder;
	SerializedProperty boundsSizeCapsule;

	SerializedProperty FRAG_OUTPUT_V4;
	SerializedProperty LOCAL_ANTI_ALIASING_QUALITY;
	SerializedProperty QUAD_INTERPOLATION_QUALITY;
	SerializedProperty NOOTS_ACROSS_SCREEN;

	SerializedObject so; // Config SO

	bool showDetailLevels = false;
	bool showBounds = false;

	// render features holders (URP only)
	#if SHAPES_URP
	URP_RND_DATA[] urpRenderers;
	#endif


	void OnEnable() {
		this.minSize = new Vector2( 350, 250 );
		so = new SerializedObject( ShapesConfig.Instance );
		useHdrColorPickers = so.FindProperty( "useHdrColorPickers" );
		autoConfigureRenderPipeline = so.FindProperty( "autoConfigureRenderPipeline" );
		useImmediateModeInstancing = so.FindProperty( "useImmediateModeInstancing" );
		pushPopStateInDrawCommands = so.FindProperty( "pushPopStateInDrawCommands" );
		polylineDefaultPointsPerTurn = so.FindProperty( "polylineDefaultPointsPerTurn" );
		polylineBezierAngularSumAccuracy = so.FindProperty( "polylineBezierAngularSumAccuracy" );
		sphereDetail = so.FindProperty( "sphereDetail" );
		torusDivsMinorMajor = so.FindProperty( "torusDivsMinorMajor" );
		coneDivs = so.FindProperty( "coneDivs" );
		cylinderDivs = so.FindProperty( "cylinderDivs" );
		capsuleDivs = so.FindProperty( "capsuleDivs" );
		boundsSizeQuad = so.FindProperty( "boundsSizeQuad" );
		boundsSizeTriangle = so.FindProperty( "boundsSizeTriangle" );
		boundsSizeSphere = so.FindProperty( "boundsSizeSphere" );
		boundsSizeTorus = so.FindProperty( "boundsSizeTorus" );
		boundsSizeCuboid = so.FindProperty( "boundsSizeCuboid" );
		boundsSizeCone = so.FindProperty( "boundsSizeCone" );
		boundsSizeCylinder = so.FindProperty( "boundsSizeCylinder" );
		boundsSizeCapsule = so.FindProperty( "boundsSizeCapsule" );
		FRAG_OUTPUT_V4 = so.FindProperty( "FRAG_OUTPUT_V4" );
		LOCAL_ANTI_ALIASING_QUALITY = so.FindProperty( "LOCAL_ANTI_ALIASING_QUALITY" );
		QUAD_INTERPOLATION_QUALITY = so.FindProperty( "QUAD_INTERPOLATION_QUALITY" );
		NOOTS_ACROSS_SCREEN = so.FindProperty( "NOOTS_ACROSS_SCREEN" );
		#if SHAPES_URP
		urpRenderers = UnityInfo.LoadAllURPRenderData();
		#endif
	}

	static string TOOLTIP_DETAIL_SPHERE = "The tessellation of the sphere is based on the icosphere. " +
										  "The lowest value of 1 is an icosahedron (a 20-sided polyhedron), " +
										  "while higher numbers divide each face up into multiple triangles, making it more and more sphere-like";

	static string TOOLTIP_DETAIL_TORUS = "The tessellation of the torus is based on the number of divisions around the major axis and its minor axis";
	static string TOOLTIP_DETAIL_CONE = "The tessellation of the cone is based on the number of divisions per full circle. The triangle count shown is the number of triangles of the capped cone";
	static string TOOLTIP_DETAIL_CYLINDER = "The tessellation of the cylinder is based on the number of divisions per full circle";

	static string TOOLTIP_DETAIL_CAPSULE = "The tessellation of the capsule is based on the octasphere, where you specify detail level per quadrant. " +
										   "A value of 1 makes a square prism with pyramids on each end, while higher values tessellate it further. " +
										   "Unlike the cone and cylinder, the capsule's number of sides around the core circle is per quarter turn, instead of for a full turn";

	Vector2 scrollPos;

	void OnGUI() {
		so.Update();

		using( var scroll = new GUILayout.ScrollViewScope( scrollPos, false, false ) ) {
			scrollPos = scroll.scrollPosition;
			using( ShapesUI.Group )
				GUILayout.Label( "Mouseover any setting label for more info", EditorStyles.centeredGreyMiniLabel );

			void Space() => GUILayout.Space( 10 );

			CsharpConfigSettings();
			Space();
			RenderPipeSettings();
			Space();
			PrimitiveSettings();
			Space();
			ShaderSettings();
			Space();
			AdvancedSettings();
		}

		so.ApplyModifiedProperties();

		if( Event.current.type == EventType.MouseDown && Event.current.button == 0 ) {
			GUI.FocusControl( "" );
			Repaint();
		}
	}

	void AdvancedSettings() {
		using( ShapesUI.Group ) {
			GUILayout.Label( "Advanced", EditorStyles.boldLabel );
			if( ShapesUI.CenteredButton( new GUIContent( "Regenerate Shaders & Materials", "Generates all shaders and materials in Shapes" ) ) )
				CodegenShaders.GenerateShadersAndMaterials( UnityInfo.GetCurrentRenderPipelineInUse() );
			if( ShapesUI.CenteredButton( new GUIContent( "Regenerate Draw Overloads", "Regenerates all Draw.X overload functions to DrawOverloads.cs" ) ) )
				CodegenDrawOverloads.GenerateDrawOverloadsScript();
			if( ShapesUI.CenteredButton( new GUIContent( "Regenerate Component Interfaces", "Regenerates all Shape component interfaces" ) ) )
				CodegenInterfaces.Generate();
			if( ShapesUI.CenteredButton( new GUIContent( "Regenerate IM meta MPBs", "Regenerates all meta-material property blocks for each shape, based on their shader parameters in the core.cginc files" ) ) )
				CodegenMpbs.Generate();
		}
	}

	void CsharpConfigSettings() {
		using( new ShapesUI.AssetEditScope( ShapesConfig.Instance ) ) {
			using( ShapesUI.Group ) {
				GUILayout.Label( "Interface", EditorStyles.boldLabel );
				EditorGUILayout.PropertyField( useHdrColorPickers, new GUIContent( "HDR color pickers" ) );
			}

			using( ShapesUI.Group ) {
				GUILayout.Label( "Immediate Mode", EditorStyles.boldLabel );
				EditorGUILayout.PropertyField( useImmediateModeInstancing, new GUIContent( "GPU Instancing" ) );
				using( ShapesUI.TempLabelWidth( 220 ) )
					EditorGUILayout.PropertyField( pushPopStateInDrawCommands, new GUIContent( "Always push/pop in Draw.Command" ) );
				EditorGUILayout.PropertyField( polylineDefaultPointsPerTurn, new GUIContent( "Default polyline density" ) );
				EditorGUILayout.PropertyField( polylineBezierAngularSumAccuracy, new GUIContent( "Bezier accuracy" ) );
			}
		}
	}

	void RenderPipeSettings() {
		using( ShapesUI.Group ) {
			GUILayout.Label( "Render Pipeline", EditorStyles.boldLabel );

			//EditorGUILayout.HelpBox( "Unity's render pipeline landscape is a little messy, so you can use this section to sanity check that everything has been compiled correctly, as well as force-compile to a specific render pipeline", MessageType.Info );

			// detect all states
			RenderPipeline rp = UnityInfo.GetCurrentRenderPipelineInUse();
			RenderPipeline rpShaders = ShapesImportState.Instance.currentShaderRP;
			RenderPipeline? rpKeywords = ShapesImportState.TryGetPreprocessorRP( out RenderPipeline rpkw ) ? rpkw : (RenderPipeline?)null;


			void RpSection( string desc, RenderPipeline pipe ) {
				using( ShapesUI.Horizontal ) {
					GUILayout.Label( desc, GUILayout.Width( 240 ) );
					GUILayout.Label( "[" + pipe.ShortName() + "]", GUILayout.ExpandWidth( false ) );
				}
			}


			EditorGUILayout.BeginVertical( EditorStyles.helpBox );
			// detected RP in Unity
			RpSection( "Shapes detected you seem to be using:", rp );

			// shaders
			RpSection( "Shapes shaders were last compiled for:", rpShaders );
			if( rpShaders != rp ) {
				EditorGUILayout.HelpBox( "Shaders seem to be compiled to a different render pipeline. You might want to force-set to your render pipeline below", MessageType.Warning );
			}

			// keywords
			if( rpKeywords.HasValue ) {
				RpSection( "Preprocessor keywords defined for:", rpKeywords.Value );

				if( rpKeywords != rp ) {
					EditorGUILayout.HelpBox( "Preprocessor keywords seem to be defined for a different render pipeline. You might want to force-set to your render pipeline below", MessageType.Warning );
				}
			} else {
				EditorGUILayout.HelpBox( "Keywords are currently in a mixed state. Please force-set to your render pipeline below", MessageType.Error );
			}

			// Render Feature check
			#if SHAPES_URP
			EditorGUILayout.Space(8);
			GUILayout.Label( "URP renderers detected:", EditorStyles.boldLabel );
			foreach( URP_RND_DATA data in urpRenderers ) {
				EditorGUILayout.ObjectField( GUIContent.none, data, typeof(URP_RND_DATA), false/*, GUILayout.Width( 120 )*/ );
				bool hasShapesRenderer = data.rendererFeatures.Any( x => x.GetType() == typeof(ShapesRenderFeature) );
				if( hasShapesRenderer ) {
					EditorGUILayout.HelpBox( "✔ Immediate mode ready", MessageType.None );
				} else {
					EditorGUILayout.HelpBox( $"{data.name} is missing a ShapesRenderFeature.\nImmediate mode drawing will not be supported unless you select this object and add the ShapesRenderFeature to it", MessageType.Error );
				}
				EditorGUILayout.Space(8);
			}
			#endif


			EditorGUILayout.EndVertical();


			GUILayout.Space( 10 );

			EditorGUILayout.BeginVertical( EditorStyles.helpBox );
			EditorGUILayout.PropertyField( autoConfigureRenderPipeline, new GUIContent( "Auto-configure RP" ) );
			using( new EditorGUI.DisabledScope( autoConfigureRenderPipeline.boolValue ) ) {
				GUILayout.Label( "Force-set Shapes to:", EditorStyles.boldLabel );
				using( ShapesUI.Horizontal ) {
					for( int i = 0; i < 3; i++ ) {
						RenderPipeline rpIter = (RenderPipeline)i;
						if( GUILayout.Button( rpIter.ShortName() ) )
							ShapesImportState.ForceSetRP( rpIter );
					}
				}
			}

			EditorGUILayout.EndVertical();
		}
	}

	void PrimitiveSettings() {
		using( ShapesUI.Group ) {
			GUILayout.Label( "Primitives", EditorStyles.boldLabel );
			using( new ShapesUI.AssetEditScope( ShapesAssets.Instance ) ) {
				if( showDetailLevels = EditorGUILayout.Foldout( showDetailLevels, new GUIContent( "Detail Levels", "Configure the mesh density of each detail level preset, for every type of Shape that can make use of it" ) ) ) {
					using( new EditorGUI.IndentLevelScope( 1 ) ) {
						DrawDetailLevelProperties( sphereDetail, "Sphere", TOOLTIP_DETAIL_SPHERE, min: 1, p => PrimitiveGenerator.TriangleCountIcosphere( p.intValue ) );
						DrawDetailLevelProperties( torusDivsMinorMajor, "Torus", TOOLTIP_DETAIL_TORUS, min: 3, p => PrimitiveGenerator.TriangleCountTorus( p.vector2IntValue ) );
						DrawDetailLevelProperties( coneDivs, "Cone", TOOLTIP_DETAIL_CONE, min: 3, p => PrimitiveGenerator.TriangleCountCone( p.intValue ) );
						DrawDetailLevelProperties( cylinderDivs, "Cylinder", TOOLTIP_DETAIL_CYLINDER, min: 3, p => PrimitiveGenerator.TriangleCountCylinder( p.intValue ) );
						DrawDetailLevelProperties( capsuleDivs, "Capsule", TOOLTIP_DETAIL_CAPSULE, min: 1, p => PrimitiveGenerator.TriangleCountCapsule( p.intValue ) );
					}
				}

				if( showBounds = EditorGUILayout.Foldout( showBounds, new GUIContent( "Bounds Sizes", ShapesConfig.TOOLTIP_BOUNDS ) ) ) {
					using( new EditorGUI.IndentLevelScope( 1 ) ) {
						EditorGUILayout.PropertyField( boundsSizeSphere, new GUIContent( "Sphere" ) );
						EditorGUILayout.PropertyField( boundsSizeTorus, new GUIContent( "Torus" ) );
						EditorGUILayout.PropertyField( boundsSizeCone, new GUIContent( "Cone" ) );
						EditorGUILayout.PropertyField( boundsSizeCylinder, new GUIContent( "Cylinder" ) );
						EditorGUILayout.PropertyField( boundsSizeCapsule, new GUIContent( "Capsule" ) );
						EditorGUILayout.PropertyField( boundsSizeQuad, new GUIContent( "Quad" ) );
						EditorGUILayout.PropertyField( boundsSizeTriangle, new GUIContent( "Triangle" ) );
						EditorGUILayout.PropertyField( boundsSizeCuboid, new GUIContent( "Cuboid" ) );
					}
				}

				GUILayout.Space( 3 );
				if( ShapesUI.CenteredButton( new GUIContent( "Apply & regenerate primitives", "Regenerate all primitive mesh assets using the configuration above" ) ) )
					PrimitiveGenerator.Generate3DPrimitiveAssets();
				GUILayout.Space( 3 );
			}
		}
	}

	void DrawDetailLevelProperties( SerializedProperty array, string label, string tooltip, int min, Func<SerializedProperty, int> triCount ) {
		// EditorGUILayout.PropertyField( array, new GUIContent( label ), true );

		if( array.isExpanded = EditorGUILayout.Foldout( array.isExpanded, new GUIContent( label, tooltip ) ) ) {
			using( new EditorGUI.IndentLevelScope( 1 ) ) {
				for( int i = 0; i < 5; i++ ) {
					SerializedProperty prop = array.GetArrayElementAtIndex( i );
					DrawDetailLevelEntry( prop, (DetailLevel)i, min, triCount );
				}
			}
		}
	}

	void DrawDetailLevelEntry( SerializedProperty prop, DetailLevel detail, int min, Func<SerializedProperty, int> triCount ) {
		bool isInt2 = prop.propertyType == SerializedPropertyType.Vector2Int;

		using( ShapesUI.Horizontal ) {
			using( ShapesUI.TempLabelWidth( 100 ) ) {
				using( var chChk = new EditorGUI.ChangeCheckScope() ) {
					if( isInt2 ) {
						SerializedProperty x = prop.FindPropertyRelative( "x" );
						SerializedProperty y = prop.FindPropertyRelative( "y" );
						EditorGUILayout.PrefixLabel( new GUIContent( detail.ToString(), "Minor & major axis divisions" ) );
						using( new EditorGUI.IndentLevelScope( -2 ) ) {
							EditorGUILayout.PropertyField( x, GUIContent.none, GUILayout.Width( 32 ) );
							EditorGUILayout.PropertyField( y, GUIContent.none, GUILayout.Width( 32 ) );
						}
					} else {
						EditorGUILayout.PropertyField( prop, new GUIContent( detail.ToString() ), GUILayout.Width( 150 ) );
					}


					if( chChk.changed ) {
						// clamp
						if( isInt2 )
							prop.vector2IntValue = new Vector2Int( Mathf.Max( min, prop.vector2IntValue.x ), Mathf.Max( min, prop.vector2IntValue.y ) );
						else
							prop.intValue = Mathf.Max( min, prop.intValue );
					}
				}
			}

			GUILayout.Label( $"{triCount( prop )} tris" );
		}
	}

	static UnityEngine.Object configShaderAssetInst;
	static UnityEngine.Object ConfigShaderAssetInst {
		get {
			if( configShaderAssetInst == null )
				configShaderAssetInst = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>( ShapesIO.ConfigShadersPath );
			return configShaderAssetInst;
		}
	}

	void ShaderSettings() {
		using( ShapesUI.Group ) {
			GUILayout.Label( "Shader Settings", EditorStyles.boldLabel );
			using( new ShapesUI.AssetEditScope( ConfigShaderAssetInst ) ) {
				using( ShapesUI.TempLabelWidth( 160 ) ) {
					EditorGUILayout.PropertyField( FRAG_OUTPUT_V4, new GUIContent( "Output precision" ) );
					EditorGUILayout.PropertyField( LOCAL_ANTI_ALIASING_QUALITY, new GUIContent( "Local AA quality" ) );
					EditorGUILayout.PropertyField( QUAD_INTERPOLATION_QUALITY, new GUIContent( "Quad interpolation quality" ) );
					EditorGUILayout.PropertyField( NOOTS_ACROSS_SCREEN, new GUIContent( "Noots across screen" ) );
				}

				GUILayout.Space( 3 );
				if( ShapesUI.CenteredButton( new GUIContent( "Apply shader settings", "Apply shader settings" ) ) )
					ApplyShaderSettings( ShapesIO.ConfigShadersPath );
				GUILayout.Space( 3 );
			}
		}
	}

	void ApplyShaderSettings( string path ) {
		List<string> lines = new List<string>();
		void Define( SerializedProperty prop, object value ) => lines.Add( $"#define {prop.name} {value}" );

		lines.Add( ShapesInfo.FILE_HEADER_COMMENT_A );
		lines.Add( ShapesInfo.FILE_HEADER_COMMENT_B );
		lines.Add( "" );
		lines.Add( "// Do not edit this manually, this file is automatically generated" );
		lines.Add( "// Please use the Shapes settings window instead" );
		lines.Add( "" );
		Define( FRAG_OUTPUT_V4, (ShapesConfig.FragOutputPrecision)FRAG_OUTPUT_V4.enumValueIndex );
		Define( LOCAL_ANTI_ALIASING_QUALITY, LOCAL_ANTI_ALIASING_QUALITY.enumValueIndex );
		Define( QUAD_INTERPOLATION_QUALITY, QUAD_INTERPOLATION_QUALITY.enumValueIndex );
		Define( NOOTS_ACROSS_SCREEN, NOOTS_ACROSS_SCREEN.intValue );

		File.WriteAllLines( path, lines );
		AssetDatabase.ImportAsset( path );
		Debug.Log( "Applied Shader settings" );
	}


}