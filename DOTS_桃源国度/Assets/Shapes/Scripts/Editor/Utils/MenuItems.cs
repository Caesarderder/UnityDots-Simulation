using System;
using UnityEditor;
#if UNITY_2021_2_OR_NEWER
using UnityEditor.SceneManagement;
#else
using UnityEditor.Experimental.SceneManagement;
#endif
using UnityEngine;
using Object = UnityEngine.Object;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	public static class MenuItems {

		const string MENU_ROOT = "Shapes/";
		const string CREATE_ROOT = MENU_ROOT + "Create/";

		const string GO_CREATE_ROOT = "GameObject/" + MENU_ROOT;

		const int GO_MENU_SORT_OFFSET = 2;
		const int MENU_ITEM_SORT_CREATE_LINES = 0;
		const int MENU_ITEM_SORT_CREATE_DISCS = 20;
		const int MENU_ITEM_SORT_CREATE_FLATS = 40;
		const int MENU_ITEM_SORT_CREATE_3D = 60;

		const int MENU_ITEM_SORT_CONFIG = 200;

		const int MENU_ITEM_SORT_DOCS = 500;
		const int MENU_ITEM_SORT_BUGREPORT = 501;
		const int MENU_ITEM_SORT_FEEDBACK = 502;
		const int MENU_ITEM_SORT_CHANGELOG = 503;

		const int MENU_ITEM_SORT_ABOUT = 2000;


		[MenuItem( MENU_ROOT + "⚙ Settings", false, MENU_ITEM_SORT_CONFIG )]
		public static void OpenCsharpSettings() => CenterWindow( EditorWindow.GetWindow<ShapesConfigWindow>( "Shapes Settings" ), 400, ShapesIO.IsUsingVcWithCheckoutEnabled ? 700 : 600 );

		[MenuItem( MENU_ROOT + "⏱ Immediate Mode Monitor", false, MENU_ITEM_SORT_CONFIG + 1 )]
		public static void OpenImmediateModeDebugger() => CenterWindow( EditorWindow.GetWindow<ImmediateModeMonitor>( "IM Monitor" ), 400, 640 );

		[MenuItem( MENU_ROOT + "📄 Documentation", false, MENU_ITEM_SORT_DOCS )]
		public static void OpenDocs() => Application.OpenURL( ShapesInfo.LINK_DOCS );

		[MenuItem( MENU_ROOT + "🐞 Report Bug", false, MENU_ITEM_SORT_BUGREPORT )]
		public static void ReportBug() => Application.OpenURL( ShapesInfo.LINK_FEEDBACK );

		[MenuItem( MENU_ROOT + "💬 Feedback Center", false, MENU_ITEM_SORT_FEEDBACK )]
		public static void Feedback() => Application.OpenURL( ShapesInfo.LINK_FEEDBACK );

		[MenuItem( MENU_ROOT + "✨ Changelog", false, MENU_ITEM_SORT_CHANGELOG )]
		public static void Changelog() => Application.OpenURL( ShapesInfo.LINK_CHANGELOG );

		// for creating shapes
		[MenuItem( GO_CREATE_ROOT + "Line", false, GO_MENU_SORT_OFFSET + MENU_ITEM_SORT_CREATE_LINES )]
		[MenuItem( CREATE_ROOT + "Line", false, MENU_ITEM_SORT_CREATE_LINES )]
		public static void CreateLine2D( MenuCommand menuCommand ) => CreateShape<Line>( menuCommand, "Line", x => x.Geometry = LineGeometry.Billboard );

		[MenuItem( GO_CREATE_ROOT + "Polyline", false, GO_MENU_SORT_OFFSET + MENU_ITEM_SORT_CREATE_LINES + 1 )]
		[MenuItem( CREATE_ROOT + "Polyline", false, MENU_ITEM_SORT_CREATE_LINES + 1 )]
		public static void CreatePolyline( MenuCommand menuCommand ) => CreateShape<Polyline>( menuCommand, "Polyline" );

		[MenuItem( GO_CREATE_ROOT + "Disc", false, GO_MENU_SORT_OFFSET + MENU_ITEM_SORT_CREATE_DISCS )]
		[MenuItem( CREATE_ROOT + "Disc", false, MENU_ITEM_SORT_CREATE_DISCS )]
		public static void CreateDisc( MenuCommand menuCommand ) => CreateShape<Disc>( menuCommand, "Disc", x => x.Type = DiscType.Disc );

		[MenuItem( GO_CREATE_ROOT + "Pie", false, GO_MENU_SORT_OFFSET + MENU_ITEM_SORT_CREATE_DISCS + 1 )]
		[MenuItem( CREATE_ROOT + "Pie", false, MENU_ITEM_SORT_CREATE_DISCS + 1 )]
		public static void CreateCircleSector( MenuCommand menuCommand ) => CreateShape<Disc>( menuCommand, "Circular Sector", x => x.Type = DiscType.Pie );

		[MenuItem( GO_CREATE_ROOT + "Ring", false, GO_MENU_SORT_OFFSET + MENU_ITEM_SORT_CREATE_DISCS + 2 )]
		[MenuItem( CREATE_ROOT + "Ring", false, MENU_ITEM_SORT_CREATE_DISCS + 2 )]
		public static void CreateAnnulus( MenuCommand menuCommand ) => CreateShape<Disc>( menuCommand, "Ring", x => x.Type = DiscType.Ring );

		[MenuItem( GO_CREATE_ROOT + "Arc", false, GO_MENU_SORT_OFFSET + MENU_ITEM_SORT_CREATE_DISCS + 3 )]
		[MenuItem( CREATE_ROOT + "Arc", false, MENU_ITEM_SORT_CREATE_DISCS + 3 )]
		public static void CreateAnnulusSector( MenuCommand menuCommand ) => CreateShape<Disc>( menuCommand, "Arc", x => x.Type = DiscType.Arc );

		[MenuItem( GO_CREATE_ROOT + "Rectangle", false, GO_MENU_SORT_OFFSET + MENU_ITEM_SORT_CREATE_FLATS )]
		[MenuItem( CREATE_ROOT + "Rectangle", false, MENU_ITEM_SORT_CREATE_FLATS )]
		public static void CreateRectangle( MenuCommand menuCommand ) => CreateShape<Rectangle>( menuCommand, "Rectangle", x => x.Type = Rectangle.RectangleType.HardSolid );

		[MenuItem( GO_CREATE_ROOT + "Triangle", false, GO_MENU_SORT_OFFSET + MENU_ITEM_SORT_CREATE_FLATS + 1 )]
		[MenuItem( CREATE_ROOT + "Triangle", false, MENU_ITEM_SORT_CREATE_FLATS + 1 )]
		public static void CreateTriangle( MenuCommand menuCommand ) => CreateShape<Triangle>( menuCommand, "Triangle" );

		[MenuItem( GO_CREATE_ROOT + "Quad", false, GO_MENU_SORT_OFFSET + MENU_ITEM_SORT_CREATE_FLATS + 2 )]
		[MenuItem( CREATE_ROOT + "Quad", false, MENU_ITEM_SORT_CREATE_FLATS + 2 )]
		public static void CreateQuad( MenuCommand menuCommand ) => CreateShape<Quad>( menuCommand, "Quad" );

		[MenuItem( GO_CREATE_ROOT + "Polygon", false, GO_MENU_SORT_OFFSET + MENU_ITEM_SORT_CREATE_FLATS + 3 )]
		[MenuItem( CREATE_ROOT + "Polygon", false, MENU_ITEM_SORT_CREATE_FLATS + 3 )]
		public static void CreatePolygon( MenuCommand menuCommand ) => CreateShape<Polygon>( menuCommand, "Polygon" );

		[MenuItem( GO_CREATE_ROOT + "Regular Polygon", false, GO_MENU_SORT_OFFSET + MENU_ITEM_SORT_CREATE_FLATS + 4 )]
		[MenuItem( CREATE_ROOT + "Regular Polygon", false, MENU_ITEM_SORT_CREATE_FLATS + 4 )]
		public static void CreateRegularPolygon( MenuCommand menuCommand ) => CreateShape<RegularPolygon>( menuCommand, "Regular Polygon" );

		[MenuItem( GO_CREATE_ROOT + "Sphere", false, GO_MENU_SORT_OFFSET + MENU_ITEM_SORT_CREATE_3D )]
		[MenuItem( CREATE_ROOT + "Sphere", false, MENU_ITEM_SORT_CREATE_3D )]
		public static void CreateSphere( MenuCommand menuCommand ) => CreateShape<Sphere>( menuCommand, "Sphere" );

		[MenuItem( GO_CREATE_ROOT + "Cuboid", false, GO_MENU_SORT_OFFSET + MENU_ITEM_SORT_CREATE_3D + 1 )]
		[MenuItem( CREATE_ROOT + "Cuboid", false, MENU_ITEM_SORT_CREATE_3D + 1 )]
		public static void CreateCuboid( MenuCommand menuCommand ) => CreateShape<Cuboid>( menuCommand, "Cuboid" );

		[MenuItem( GO_CREATE_ROOT + "Torus", false, GO_MENU_SORT_OFFSET + MENU_ITEM_SORT_CREATE_3D + 2 )]
		[MenuItem( CREATE_ROOT + "Torus", false, MENU_ITEM_SORT_CREATE_3D + 2 )]
		public static void CreateTorus( MenuCommand menuCommand ) => CreateShape<Torus>( menuCommand, "Torus" );

		[MenuItem( GO_CREATE_ROOT + "Cone", false, GO_MENU_SORT_OFFSET + MENU_ITEM_SORT_CREATE_3D + 3 )]
		[MenuItem( CREATE_ROOT + "Cone", false, MENU_ITEM_SORT_CREATE_3D + 3 )]
		public static void CreateCone( MenuCommand menuCommand ) => CreateShape<Cone>( menuCommand, "Cone" );

		static T CreateShape<T>( MenuCommand menuCommand, string name, Action<T> applyModifications = null ) where T : ShapeRenderer {
			Type type = typeof(T);
			string shapeName = type.Name;
			GameObject go = new GameObject( shapeName );
			Vector3 createPos = default;
			if( SceneView.lastActiveSceneView != null )
				createPos = SceneView.lastActiveSceneView.pivot;
			go.transform.position = createPos;
			T component = go.AddComponent<T>();
			Selection.activeObject = go;
			applyModifications?.Invoke( component );
			Undo.RegisterCreatedObjectUndo( go, $"create {shapeName} shape" );
			Place( go, menuCommand.context );
			return component;
		}

		// these should match functionality of Place in https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/Commands/GOCreationCommands.cs#L42
		// except more readable and without internal methods and whatnot
		private static void Place( GameObject go, Object context ) {
			Transform parentTransform;
			if( context != null && context is GameObject goCtx )
				parentTransform = goCtx.transform;
			else
				parentTransform = PrefabStageUtility.GetCurrentPrefabStage()?.prefabContentsRoot.transform;
			Transform transform = go.transform;
			Undo.SetTransformParent( transform, parentTransform, "Reparenting" );
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.identity;
			transform.localScale = Vector3.one;
			if( parentTransform != null ) {
				go.layer = parentTransform.gameObject.layer;
				if( parentTransform.GetComponent<RectTransform>() )
					ObjectFactory.AddComponent<RectTransform>( go );
			}
		}

		[MenuItem( MENU_ROOT + "About  \u2044  Check for Updates", false, MENU_ITEM_SORT_ABOUT )]
		public static void About() => CenterWindow( EditorWindow.GetWindow<AboutWindow>( false, "Shapes " + ShapesInfo.Version, true ), 960, 540 );

		static void CenterWindow( EditorWindow window, int width, int height ) {
			Vector2 center = new Vector2( Screen.currentResolution.width, Screen.currentResolution.height ) / 2;
			Vector2 size = new Vector2( width, height );
			window.position = new Rect( center - size / 2, size );
		}

	}

}