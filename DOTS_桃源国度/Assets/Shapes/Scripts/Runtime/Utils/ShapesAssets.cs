using TMPro;
using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	public class ShapesAssets : ScriptableObject {

		[Header( "Config" )]
		public TMP_FontAsset defaultFont;

		[Header( "Meshes" )] public Mesh[] meshQuad = new Mesh[5];
		public Mesh[] meshTriangle = new Mesh[5];
		public Mesh[] meshCube = new Mesh[5];
		public Mesh[] meshSphere = new Mesh[5];
		public Mesh[] meshTorus = new Mesh[5];
		public Mesh[] meshCapsule = new Mesh[5];
		public Mesh[] meshCylinder = new Mesh[5];
		public Mesh[] meshCone = new Mesh[5];
		public Mesh[] meshConeUncapped = new Mesh[5];

		[Header( "Misc" )]
		public TextAsset packageJson;

		static ShapesAssets inst;
		public static ShapesAssets Instance {
			get {
				if( inst == null )
					inst = Resources.Load<ShapesAssets>( "Shapes Assets" );
				return inst;
			}
		}

	}

}