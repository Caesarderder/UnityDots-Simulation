using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	internal static class ShapesMeshUtils {


		static Mesh quadMesh;
		public static Mesh[] QuadMesh => ShapesAssets.Instance.meshQuad;

		static Mesh triangleMesh;
		public static Mesh[] TriangleMesh => ShapesAssets.Instance.meshTriangle;

		static Mesh sphereMesh;
		public static Mesh[] SphereMesh => ShapesAssets.Instance.meshSphere;

		static Mesh cuboidMesh;
		public static Mesh[] CuboidMesh => ShapesAssets.Instance.meshCube;

		static Mesh torusMesh;
		public static Mesh[] TorusMesh => ShapesAssets.Instance.meshTorus;

		static Mesh coneMesh;
		public static Mesh[] ConeMesh => ShapesAssets.Instance.meshCone;

		static Mesh coneMeshUncapped;
		public static Mesh[] ConeMeshUncapped => ShapesAssets.Instance.meshConeUncapped;

		static Mesh cylinderMesh;
		public static Mesh[] CylinderMesh => ShapesAssets.Instance.meshCylinder;

		static Mesh capsuleMesh;
		public static Mesh[] CapsuleMesh => ShapesAssets.Instance.meshCapsule;

		#if UNITY_EDITOR

		static ShapesMeshUtils() => AssemblyReloadEvents.beforeAssemblyReload += OnPreAssemblyReload;

		static void OnPreAssemblyReload() {
			AssemblyReloadEvents.beforeAssemblyReload -= OnPreAssemblyReload;
			BindingFlags bfs = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
			bool IsDestroyTarget( FieldInfo f ) => f.GetCustomAttributes( typeof(DestroyOnAssemblyReload), false ).Length > 0 && f.GetValue( null ) != null;

			foreach( FieldInfo field in typeof(ShapesMeshUtils).GetFields( bfs ).Where( IsDestroyTarget ) ) {
				Object obj = (Object)field.GetValue( null );
				Object.DestroyImmediate( obj );
			}
		}

		#endif


		static Mesh EnsureValidMeshBounds( Mesh mesh, Bounds bounds ) {
			mesh.hideFlags = HideFlags.HideInInspector;
			mesh.bounds = bounds;
			return mesh;
		}

		public static Mesh GetLineMesh( LineGeometry geometry, LineEndCap endCaps, DetailLevel detail ) {
			switch( geometry ) {
				case LineGeometry.Billboard:
				case LineGeometry.Flat2D:
					return QuadMesh[0];
				case LineGeometry.Volumetric3D:
					return endCaps == LineEndCap.Round ? CapsuleMesh[(int)detail] : CylinderMesh[(int)detail];
			}

			return default;
		}

	}

}