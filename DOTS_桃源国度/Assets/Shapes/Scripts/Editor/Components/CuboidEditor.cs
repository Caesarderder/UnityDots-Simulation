using UnityEditor;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	[CustomEditor( typeof(Cuboid) )]
	[CanEditMultipleObjects]
	public class CuboidEditor : ShapeRendererEditor {

		SerializedProperty propSize = null;
		SerializedProperty propSizeSpace = null;

		public override void OnInspectorGUI() {
			base.BeginProperties();
			ShapesUI.FloatInSpaceField( propSize, propSizeSpace );
			base.EndProperties();
		}

	}

}