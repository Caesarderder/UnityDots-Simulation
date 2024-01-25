using System.Linq;
using UnityEditor;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	[CustomEditor( typeof(Cone) )]
	[CanEditMultipleObjects]
	public class ConeEditor : ShapeRendererEditor {

		SerializedProperty propRadius = null;
		SerializedProperty propLength = null;
		SerializedProperty propSizeSpace = null;
		SerializedProperty propFillCap = null;

		public override void OnInspectorGUI() {
			base.BeginProperties();
			ShapesUI.FloatInSpaceField( propRadius, propSizeSpace );
			ShapesUI.FloatInSpaceField( propLength, propSizeSpace, spaceEnabled: false );
			EditorGUILayout.PropertyField( propFillCap );
			if( base.EndProperties() )
				foreach( Cone cone in targets.Cast<Cone>() )
					cone.UpdateMesh();
		}

	}

}