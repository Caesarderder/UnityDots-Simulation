using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	/// <summary>A Cone shape component</summary>
	[ExecuteAlways]
	[AddComponentMenu( "Shapes/Cone" )]
	public class Cone : ShapeRenderer {

		[SerializeField] float radius = 1;
		/// <summary>The radius of the base of the cone</summary>
		public float Radius {
			get => radius;
			set => SetFloatNow( ShapesMaterialUtils.propRadius, radius = Mathf.Max( 0f, value ) );
		}
		[SerializeField] float length = 1.5f;
		/// <summary>The length/height of this cone</summary>
		public float Length {
			get => length;
			set => SetFloatNow( ShapesMaterialUtils.propLength, length = Mathf.Max( 0f, value ) );
		}
		[SerializeField] ThicknessSpace sizeSpace = Shapes.ThicknessSpace.Meters;
		/// <summary>this property is obsolete I'm sorry! this was a typo, please use SizeSpace instead!</summary>
		[System.Obsolete( "this property is obsolete I'm sorry! this was a typo, please use SizeSpace instead!", true )]
		public ThicknessSpace RadiusSpace {
			get => SizeSpace;
			set => SizeSpace = value;
		}
		/// <summary>The space in which radius and length is defined in</summary>
		public ThicknessSpace SizeSpace {
			get => sizeSpace;
			set => SetIntNow( ShapesMaterialUtils.propSizeSpace, (int)( sizeSpace = value ) );
		}
		[SerializeField] bool fillCap = true;
		/// <summary>Whether or not the base cap should be filled</summary>
		public bool FillCap {
			get => fillCap;
			set {
				fillCap = value;
				UpdateMesh( true );
			}
		}

		private protected override void SetAllMaterialProperties() {
			SetFloat( ShapesMaterialUtils.propRadius, radius );
			SetFloat( ShapesMaterialUtils.propLength, length );
			SetInt( ShapesMaterialUtils.propSizeSpace, (int)sizeSpace );
		}

		private protected override void ShapeClampRanges() {
			radius = Mathf.Max( 0f, radius );
			length = Mathf.Max( 0f, length );
		}

		internal override bool HasDetailLevels => true;
		internal override bool HasScaleModes => false;
		private protected override Material[] GetMaterials() => new[] { ShapesMaterialUtils.matCone[BlendMode] };
		private protected override Mesh GetInitialMeshAsset() => fillCap ? ShapesMeshUtils.ConeMesh[(int)detailLevel] : ShapesMeshUtils.ConeMeshUncapped[(int)detailLevel];

		private protected override Bounds GetBounds_Internal() {
			if( sizeSpace != ThicknessSpace.Meters )
				return new Bounds( Vector3.zero, Vector3.one );
			return new Bounds( Vector3.zero, new Vector3( radius * 2, radius * 2, length ) );
		}

	}

}