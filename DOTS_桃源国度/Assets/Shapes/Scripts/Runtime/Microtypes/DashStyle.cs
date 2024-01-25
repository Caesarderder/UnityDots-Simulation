// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/

namespace Shapes {

	/// <summary>Dash style, space &amp; size settings</summary>
	[System.Serializable]
	public struct DashStyle {

		public static readonly DashStyle defaultDashStyle = new DashStyle {
			type = DashType.Basic,
			space = DashSpace.Relative,
			snap = DashSnapping.Off,
			size = 4f,
			offset = 0f,
			spacing = 4f,
			shapeModifier = 1
		};

		public static readonly DashStyle defaultDashStyleRing = new DashStyle {
			type = DashType.Basic,
			space = DashSpace.FixedCount,
			snap = DashSnapping.Tiling,
			size = 16f,
			offset = 0f,
			spacing = 0.5f,
			shapeModifier = 1
		};

		public static readonly DashStyle defaultDashStyleLine = new DashStyle {
			type = DashType.Basic,
			space = DashSpace.Relative,
			snap = DashSnapping.EndToEnd,
			size = 4f,
			offset = 0f,
			spacing = 4f,
			shapeModifier = 1
		};

		/// <summary>The type of dash to use</summary>
		public DashType type;

		/// <summary>The space in which dashes are defined</summary>
		public DashSpace space;

		/// <summary>What snapping type to use</summary>
		public DashSnapping snap;

		/// <summary>Size of dashes in the specified dash space. When using DashSpace.FixedCount, this is the number of dashes</summary>
		public float size;

		/// <summary>Size of spacing between each dash, in the specified dash space. When using DashSpace.FixedCount, this is the dash:space ratio</summary>
		public float spacing;

		/// <summary>An offset of 1 is the size of a whole dash+space period</summary>
		public float offset;

		/// <summary>-1 to 1 modifier that allows you to tweak or mirror certain dash types</summary>
		[UnityEngine.Range( -1f, 1f )] public float shapeModifier;

		float GetNet( float v, float thickness ) => space == DashSpace.Relative ? thickness * v : v;
		internal float GetNetAbsoluteSize( bool dashed, float thickness ) => dashed ? GetNet( size, thickness ) : 0f;
		internal float GetNetAbsoluteSpacing( bool dashed, float thickness ) => dashed ? GetNet( spacing, thickness ) : 0f;

		/// <summary>Sets both size and spacing to the same value</summary>
		public float UniformSize {
			get => size; // a lil weird but it's okay
			set {
				size = value;
				spacing = space == DashSpace.FixedCount ? 0.5f : size;
			}
		}

		/// <summary>Creates a dash style where dash lengths are relative to the thickness of the object you're drawing. A dash size of 1 means the dash is as long as the shape is thick</summary>
		/// <param name="type">The type of dash to use (basic, rounded or angled)</param>
		/// <param name="size">The size/length of a single dash. A size of 1 is equal to the thickness of the shape you're using these dashes on</param>
		/// <param name="spacing">The size/length of the spacing between each dash. A size of 1 is equal to the thickness of the shape you're using these dashes on</param>
		/// <param name="snap">What snapping mode to use, if any</param>
		/// <param name="offset">Longitudinal offset for the dashes. A value of 1 is the length of a single dash+space period, which means the offset repeats/looks the same at every integer</param>
		/// <param name="shapeModifier">A modifier for the dash styles, which will set the skewness of angled dashes, for instance. Valid range: -1 to 1</param>
		public static DashStyle RelativeDashes( DashType type, float size, float spacing, DashSnapping snap = DashSnapping.Off, float offset = 0f, float shapeModifier = 1f ) =>
			new DashStyle {
				space = DashSpace.Relative,
				type = type,
				size = size,
				spacing = spacing,
				snap = snap,
				offset = offset,
				shapeModifier = shapeModifier
			};

		/// <summary>Creates a dash style where you set the number of dashes across the entire shape</summary>
		/// <param name="type">The type of dash to use (basic, rounded or angled)</param>
		/// <param name="count">The number of dashes</param>
		/// <param name="spacingFraction">The fraction of each repeating period that should be space. A value of 0.5f means spaces will have the same size as dashes. Valid range: greater than 0, less than 1</param>
		/// <param name="snap">What snapping mode to use, if any</param>
		/// <param name="offset">Longitudinal offset for the dashes. A value of 1 is the length of a single dash+space period, which means the offset repeats/looks the same at every integer</param>
		/// <param name="shapeModifier">A modifier for the dash styles, which will set the skewness of angled dashes, for instance. Valid range: -1 to 1</param>
		public static DashStyle FixedDashCount( DashType type, float count, float spacingFraction = 0.5f, DashSnapping snap = DashSnapping.Off, float offset = 0f, float shapeModifier = 1f ) =>
			new DashStyle {
				space = DashSpace.FixedCount,
				type = type,
				size = count,
				spacing = spacingFraction,
				snap = snap,
				offset = offset,
				shapeModifier = shapeModifier
			};

		/// <summary>Creates a dash style where dash lengths are specified in meters</summary>
		/// <param name="type">The type of dash to use (basic, rounded or angled)</param>
		/// <param name="size">The size/length of a single dash in meters</param>
		/// <param name="spacing">The size/length of the spacing between each dash in meters</param>
		/// <param name="snap">What snapping mode to use, if any</param>
		/// <param name="offset">Longitudinal offset for the dashes. A value of 1 is the length of a single dash+space period, which means the offset repeats/looks the same at every integer</param>
		/// <param name="shapeModifier">A modifier for the dash styles, which will set the skewness of angled dashes, for instance. Valid range: -1 to 1</param>
		public static DashStyle MeterDashes( DashType type, float size, float spacing, DashSnapping snap = DashSnapping.Off, float offset = 0f, float shapeModifier = 1f ) =>
			new DashStyle {
				space = DashSpace.Meters,
				type = type,
				size = size,
				spacing = spacing,
				snap = snap,
				offset = offset,
				shapeModifier = shapeModifier
			};

		#region deprecated stuff

		[System.Obsolete( "Deprecated, please use defaultDashStyle instead (lowercase first letter~)" )]
		public static DashStyle DefaultDashStyle {
			get => defaultDashStyle;
			set => _ = value;
		}
		[System.Obsolete( "Deprecated, please use defaultDashStyleRing instead (lowercase first letter~)" )]
		public static DashStyle DefaultDashStyleRing {
			get => defaultDashStyleRing;
			set => _ = value;
		}
		[System.Obsolete( "Deprecated, please use defaultDashStyleLine instead (lowercase first letter~)" )]
		public static DashStyle DefaultDashStyleLine {
			get => defaultDashStyleLine;
			set => _ = value;
		}

		[System.Obsolete( "Deprecated, please use <c>DashStyle.RelativeDashes/FixedCountDashes/MeterDashes</c> instead", true )]
		public DashStyle( float size ) {
			this.type = defaultDashStyle.type;
			this.space = defaultDashStyle.space;
			this.snap = defaultDashStyle.snap;
			this.size = size;
			this.spacing = size;
			this.offset = defaultDashStyle.offset;
			this.shapeModifier = defaultDashStyle.shapeModifier;
		}

		[System.Obsolete( "Deprecated, please use <c>DashStyle.RelativeDashes/FixedCountDashes/MeterDashes</c> instead", true )]
		public DashStyle( float size, DashType type ) {
			this.type = type;
			this.space = defaultDashStyle.space;
			this.snap = defaultDashStyle.snap;
			this.size = size;
			this.spacing = size;
			this.offset = defaultDashStyle.offset;
			this.shapeModifier = defaultDashStyle.shapeModifier;
		}

		[System.Obsolete( "Deprecated, please use <c>DashStyle.RelativeDashes/FixedCountDashes/MeterDashes</c> instead", true )]
		public DashStyle( float size, float spacing, DashType type ) {
			this.type = type;
			this.space = defaultDashStyle.space;
			this.snap = defaultDashStyle.snap;
			this.size = size;
			this.spacing = spacing;
			this.offset = defaultDashStyle.offset;
			this.shapeModifier = defaultDashStyle.shapeModifier;
		}

		[System.Obsolete( "Deprecated, please use <c>DashStyle.RelativeDashes/FixedCountDashes/MeterDashes</c> instead", true )]
		public DashStyle( float size, float spacing ) {
			this.type = defaultDashStyle.type;
			this.space = defaultDashStyle.space;
			this.snap = defaultDashStyle.snap;
			this.size = size;
			this.spacing = spacing;
			this.offset = defaultDashStyle.offset;
			this.shapeModifier = defaultDashStyle.shapeModifier;
		}

		[System.Obsolete( "Deprecated, please use <c>DashStyle.RelativeDashes/FixedCountDashes/MeterDashes</c> instead", true )]
		public DashStyle( float size, float spacing, float offset ) {
			this.type = defaultDashStyle.type;
			this.space = defaultDashStyle.space;
			this.snap = defaultDashStyle.snap;
			this.size = size;
			this.spacing = spacing;
			this.offset = offset;
			this.shapeModifier = defaultDashStyle.shapeModifier;
		}

		#endregion

	}

}