using UnityEditor;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	[CustomEditor( typeof(Sphere) )]
	[CanEditMultipleObjects]
	public class SphereEditor : ShapeRendererEditor {

		SerializedProperty propRadius = null;
		SerializedProperty propRadiusSpace = null;

		public override void OnInspectorGUI() {
			base.BeginProperties();
			ShapesUI.FloatInSpaceField( propRadius, propRadiusSpace );
			base.EndProperties();
		}

	}

}