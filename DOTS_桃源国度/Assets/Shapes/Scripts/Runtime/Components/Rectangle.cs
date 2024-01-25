using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	/// <summary>A Rectangle shape component</summary>
	[ExecuteAlways]
	[AddComponentMenu( "Shapes/Rectangle" )]
	public partial class Rectangle : ShapeRenderer, IDashable, IFillable {

		/// <summary>Types of rectangles</summary>
		public enum RectangleType {
			/// <summary>Filled rectangle with hard corners</summary>
			HardSolid,

			/// <summary>Filled rectangle with rounded corners</summary>
			RoundedSolid,

			/// <summary>Border rectangle with hard corners</summary>
			HardBorder,

			/// <summary>Border rectangle with rounded corners</summary>
			RoundedBorder
		}

		/// <summary>Types of corners on rectangles</summary>
		public enum RectangleCornerRadiusMode {
			/// <summary>Use the same radius on all 4 corners</summary>	
			Uniform,

			/// <summary>Use specific radii on a per-corner basis</summary>
			PerCorner
		}

		/// <summary>Whether or not this is a border rectangle</summary>
		public bool IsBorder => type == RectangleType.HardBorder || type == RectangleType.RoundedBorder;

		[System.Obsolete( "Please use IsBorder instead", true )]
		public bool IsHollow => type == RectangleType.HardBorder || type == RectangleType.RoundedBorder;

		/// <summary>Whether or not this rectangle has rounded corners</summary>
		public bool IsRounded => type == RectangleType.RoundedSolid || type == RectangleType.RoundedBorder;

		[SerializeField] RectPivot pivot = RectPivot.Center;
		/// <summary>Get or set where the pivot (0,0) should be located in this rectangle</summary>
		public RectPivot Pivot {
			get => pivot;
			set {
				pivot = value;
				UpdateRectPositioningNow();
			}
		}

		[SerializeField] float width = 1f;
		/// <summary>The width of the rectangle</summary>
		public float Width {
			get => width;
			set {
				width = value;
				UpdateRectPositioningNow();
			}
		}

		[SerializeField] float height = 1f;
		/// <summary>The height of the rectangle</summary>
		public float Height {
			get => height;
			set {
				height = value;
				UpdateRectPositioningNow();
			}
		}

		[SerializeField] RectangleType type = RectangleType.HardSolid;
		/// <summary>Get or set what type of rectangle this is (border vs filled, hard vs rounded corners)</summary>
		public RectangleType Type {
			get => type;
			set {
				type = value;
				UpdateMaterial();
				ApplyProperties();
			}
		}


		[SerializeField] RectangleCornerRadiusMode cornerRadiusMode = RectangleCornerRadiusMode.Uniform;
		/// <summary>Whether or not you want to set radius on a per-corner basis or uniformly. Applies only to rounded rectangles</summary>
		public RectangleCornerRadiusMode CornerRadiusMode {
			get => cornerRadiusMode;
			set => cornerRadiusMode = value;
		}

		/// <summary>Radius is deprecated, please use CornerRadius instead</summary>
		[System.Obsolete( "Radius is deprecated, please use " + nameof(CornerRadius) + " instead", true )]
		public float Radius {
			get => CornerRadius;
			set => CornerRadius = value;
		}

		[SerializeField] Vector4 cornerRadii = new Vector4( 0.25f, 0.25f, 0.25f, 0.25f );
		/// <summary>Gets or sets a radius for all 4 corners when rounded</summary>
		public float CornerRadius {
			get => cornerRadii.x;
			set {
				float r = Mathf.Max( 0f, value );
				SetVector4Now( ShapesMaterialUtils.propCornerRadii, cornerRadii = new Vector4( r, r, r, r ) );
			}
		}
		/// <summary>Gets or sets a specific radius for each corner when rounded. Order is clockwise from bottom left</summary>
		public Vector4 CornerRadii {
			get => cornerRadii;
			set => SetVector4Now( ShapesMaterialUtils.propCornerRadii, cornerRadii = new Vector4( Mathf.Max( 0f, value.x ), Mathf.Max( 0f, value.y ), Mathf.Max( 0f, value.z ), Mathf.Max( 0f, value.w ) ) );
		}

		[System.Obsolete( "Please use CornerRadii instead because I did a typo~", true )]
		public Vector4 CornerRadiii {
			get => CornerRadii;
			set => CornerRadii = value;
		}

		[Tooltip( "The thickness of the rectangle, in the given thickness space" )]
		[SerializeField] float thickness = 0.1f;

		/// <summary>The thickness of the rectangle (if border rectangle)</summary>
		public float Thickness {
			get => thickness;
			set => SetFloatNow( ShapesMaterialUtils.propThickness, thickness = Mathf.Max( 0f, value ) );
		}

		[Tooltip( "The space in which thickness is defined" )]
		[SerializeField] ThicknessSpace thicknessSpace = Shapes.ThicknessSpace.Meters;

		/// <summary>The space in which thickness is defined</summary>
		public ThicknessSpace ThicknessSpace {
			get => thicknessSpace;
			set => SetIntNow( ShapesMaterialUtils.propThicknessSpace, (int)( thicknessSpace = value ) );
		}

		internal override bool HasDetailLevels => false;
		void UpdateRectPositioningNow() => SetVector4Now( ShapesMaterialUtils.propRect, GetPositioningRect() );
		void UpdateRectPositioning() => SetVector4( ShapesMaterialUtils.propRect, GetPositioningRect() );

		Vector4 GetPositioningRect() {
			float xOffset = pivot == RectPivot.Corner ? 0f : -width / 2f;
			float yOffset = pivot == RectPivot.Corner ? 0f : -height / 2f;
			return new Vector4( xOffset, yOffset, width, height );
		}

		private protected override void SetAllMaterialProperties() {
			if( cornerRadiusMode == RectangleCornerRadiusMode.PerCorner )
				SetVector4( ShapesMaterialUtils.propCornerRadii, cornerRadii );
			else if( cornerRadiusMode == RectangleCornerRadiusMode.Uniform )
				SetVector4( ShapesMaterialUtils.propCornerRadii, new Vector4( CornerRadius, CornerRadius, CornerRadius, CornerRadius ) );
			UpdateRectPositioning();
			SetFloat( ShapesMaterialUtils.propThickness, thickness );
			SetIntNow( ShapesMaterialUtils.propThicknessSpace, (int)thicknessSpace );
			SetFillProperties();
			SetAllDashValues( now: false );
		}


		#if UNITY_EDITOR
		private protected override void ShapeClampRanges() {
			cornerRadii = ShapesMath.AtLeast0( cornerRadii );
			width = Mathf.Max( 0f, width );
			height = Mathf.Max( 0f, height );
			thickness = Mathf.Max( 0f, thickness );
		}
		#endif

		private protected override Material[] GetMaterials() => new[] { ShapesMaterialUtils.GetRectMaterial( type )[BlendMode] };

		private protected override Bounds GetBounds_Internal() {
			Vector2 size = new Vector2( width, height );
			Vector2 center = pivot == RectPivot.Center ? default : size / 2f;
			return new Bounds( center, size );
		}

	}

}