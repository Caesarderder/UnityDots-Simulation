using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	/// <summary>A Cuboid shape component</summary>
	[ExecuteAlways]
	[AddComponentMenu( "Shapes/Cuboid" )]
	public class Cuboid : ShapeRenderer {

		[SerializeField] Vector3 size = Vector3.one;
		/// <summary>Size of this cube along each axis, in the given SizeSpace</summary>
		public Vector3 Size {
			get => size;
			set => SetVector3Now( ShapesMaterialUtils.propSize, size = value );
		}
		[SerializeField] ThicknessSpace sizeSpace = ThicknessSpace.Meters;
		/// <summary>The space in which size is defined</summary>
		public ThicknessSpace SizeSpace {
			get => sizeSpace;
			set => SetIntNow( ShapesMaterialUtils.propSizeSpace, (int)( sizeSpace = value ) );
		}

		private protected override void SetAllMaterialProperties() {
			SetVector3( ShapesMaterialUtils.propSize, size );
			SetInt( ShapesMaterialUtils.propSizeSpace, (int)sizeSpace );
		}

		internal override bool HasDetailLevels => false;
		internal override bool HasScaleModes => false;
		private protected override void ShapeClampRanges() => size = Vector3.Max( default, size );
		private protected override Material[] GetMaterials() => new[] { ShapesMaterialUtils.matCuboid[BlendMode] };
		private protected override Mesh GetInitialMeshAsset() => ShapesMeshUtils.CuboidMesh[0];

		private protected override Bounds GetBounds_Internal() {
			if( sizeSpace != ThicknessSpace.Meters )
				return new Bounds( default, Vector3.one );
			return new Bounds( default, size );
		}

	}

}