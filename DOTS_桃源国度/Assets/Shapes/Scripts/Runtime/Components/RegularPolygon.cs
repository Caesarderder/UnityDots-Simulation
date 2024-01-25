using System;
using UnityEngine;
using UnityEngine.Serialization;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	/// <summary>A Regular Polygon shape component</summary>
	[ExecuteAlways]
	[AddComponentMenu( "Shapes/RegularPolygon" )]
	public partial class RegularPolygon : ShapeRenderer, IDashable, IFillable {

		[FormerlySerializedAs( "hollow" )] [SerializeField] bool border = false;
		/// <summary>Whether or not this should be a Regular Polygon border instead of filled</summary>
		public bool Border {
			get => border;
			set => SetIntNow( ShapesMaterialUtils.propBorder, ( border = value ).AsInt() );
		}
		[Obsolete( "Please use RegularPolygon.Border instead", true )]
		public bool Hollow {
			get => Border;
			set => Border = value;
		}
		[SerializeField] int sides = 3;
		/// <summary>Number of sides</summary>
		public int Sides {
			get => sides;
			set => SetIntNow( ShapesMaterialUtils.propSides, sides = Mathf.Max( 3, value ) );
		}
		[SerializeField] [Range( 0, 1 )] float roundness = 0;
		/// <summary>Roundness. 0 = not round at all. 1 = the roundest shape there is</summary>
		public float Roundness {
			get => roundness;
			set => SetFloatNow( ShapesMaterialUtils.propRoundness, roundness = Mathf.Clamp01( value ) );
		}
		[SerializeField] float angle = ShapesMath.TAU / 4;
		/// <summary>Angular offset</summary>
		public float Angle {
			get => angle;
			set => SetFloatNow( ShapesMaterialUtils.propAng, angle = value );
		}
		[SerializeField] float radius = 1;
		/// <summary>Radius (center to vertex) in the given radius space</summary>
		public float Radius {
			get => radius;
			set => SetFloatNow( ShapesMaterialUtils.propRadius, radius = Mathf.Max( 0f, value ) );
		}

		// in-editor serialized field, suppressing "assigned but unused field" warning
		#pragma warning disable CS0414
		[SerializeField] AngularUnit angUnitInput = AngularUnit.Degrees;
		#pragma warning restore CS0414

		[SerializeField] RegularPolygonGeometry geometry = RegularPolygonGeometry.Flat2D;
		/// <summary>What type of geometry to use, if it should be flat or billboarded</summary>
		public RegularPolygonGeometry Geometry {
			get => geometry;
			set => SetIntNow( ShapesMaterialUtils.propAlignment, (int)( geometry = value ) );
		}

		[SerializeField] ThicknessSpace radiusSpace = Shapes.ThicknessSpace.Meters;
		/// <summary>The space in which radius is defined</summary>
		public ThicknessSpace RadiusSpace {
			get => radiusSpace;
			set => SetIntNow( ShapesMaterialUtils.propRadiusSpace, (int)( radiusSpace = value ) );
		}
		[SerializeField] float thickness = 0.5f;
		/// <summary>The thickness of the regular polygon border (if it is a border)</summary>
		public float Thickness {
			get => thickness;
			set => SetFloatNow( ShapesMaterialUtils.propThickness, thickness = Mathf.Max( 0f, value ) );
		}
		[SerializeField] ThicknessSpace thicknessSpace = Shapes.ThicknessSpace.Meters;
		/// <summary>The space in which thickness is defined</summary>
		public ThicknessSpace ThicknessSpace {
			get => thicknessSpace;
			set => SetIntNow( ShapesMaterialUtils.propThicknessSpace, (int)( thicknessSpace = value ) );
		}

		private protected override void SetAllMaterialProperties() {
			SetFillProperties();
			SetIntNow( ShapesMaterialUtils.propBorder, border.AsInt() );
			SetInt( ShapesMaterialUtils.propAlignment, (int)geometry );
			SetFloat( ShapesMaterialUtils.propRadius, radius );
			SetInt( ShapesMaterialUtils.propRadiusSpace, (int)radiusSpace );
			SetFloat( ShapesMaterialUtils.propThickness, thickness );
			SetInt( ShapesMaterialUtils.propThicknessSpace, (int)thicknessSpace );
			SetFloat( ShapesMaterialUtils.propAng, angle );
			SetFloat( ShapesMaterialUtils.propSides, sides );
			SetFloat( ShapesMaterialUtils.propRoundness, roundness );
			SetAllDashValues( now: false );
		}

		#if UNITY_EDITOR
		private protected override void ShapeClampRanges() {
			radius = Mathf.Max( 0f, radius ); // disallow negative radius
			thickness = Mathf.Max( 0f, thickness ); // disallow negative inner radius
			sides = Mathf.Max( 3, sides );
			roundness = Mathf.Clamp01( roundness );
		}
		#endif

		internal override bool HasDetailLevels => false;
		private protected override Material[] GetMaterials() => new[] { ShapesMaterialUtils.matRegularPolygon[BlendMode] };

		private protected override Bounds GetBounds_Internal() {
			if( radiusSpace != ThicknessSpace.Meters )
				return new Bounds( Vector3.zero, Vector3.one );
			// presume 0 world space padding when pixels or noots are used
			float padding = thicknessSpace == ThicknessSpace.Meters ? thickness * .5f : 0f;
			float apothem = border ? radius + padding : radius;
			float size = apothem * 2;
			return new Bounds( Vector3.zero, new Vector3( size, size, 0f ) );
		}

	}

}