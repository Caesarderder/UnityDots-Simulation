using System;
using UnityEngine;
using UnityEngine.Serialization;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	/// <summary>A Triangle shape component</summary>
	[ExecuteAlways]
	[AddComponentMenu( "Shapes/Triangle" )]
	public partial class Triangle : ShapeRenderer, IDashable {

		/// <summary>Color modes for the triangle shape</summary>
		public enum TriangleColorMode {
			/// <summary>A single color for the whole triangle</summary>
			Single,

			/// <summary>One color per vertex in the triangle</summary>
			PerCorner
		}

		/// <summary>Get or set vertex positions by index)</summary>
		/// <param name="index">A value from 0 to 3</param>
		/// <exception cref="IndexOutOfRangeException"></exception>
		public Vector3 this[ int index ] {
			get {
				switch( index ) {
					case 0: return A;
					case 1: return B;
					case 2: return C;
					default:
						throw new IndexOutOfRangeException( $"Triangle only has four vertices, 0 to 2, you tried to access element {index}" );
				}
			}
			set {
				switch( index ) {
					case 0:
						A = value;
						break;
					case 1:
						B = value;
						break;
					case 2:
						C = value;
						break;
					default:
						throw new IndexOutOfRangeException( $"Triangle only has four vertices, 0 to 2, you tried to set element {index}" );
				}
			}
		}

		/// <summary>Get vertex positions by index</summary>
		/// <param name="index">An index from 0 to 2</param>
		public Vector3 GetTriangleVertex( int index ) => this[index];

		/// <summary>Set vertex positions by index</summary>
		/// <param name="index">An index from 0 to 2</param>
		/// <param name="value">The position to set the vertex to</param>
		public Vector3 SetTriangleVertex( int index, Vector3 value ) => this[index] = value;

		/// <summary>Get vertex color by index</summary>
		/// <param name="index">An index from 0 to 2</param>
		public Color GetTriangleColor( int index ) {
			switch( index ) {
				case 0: return Color;
				case 1: return ColorB;
				case 2: return ColorC;
				default:
					throw new IndexOutOfRangeException( $"Triangle only has four vertices, 0 to 2, you tried to access element {index}" );
			}
		}

		/// <summary>Set vertex color by index</summary>
		/// <param name="index">An index from 0 to 2</param>
		/// <param name="color">The color to set the vertex to</param>
		public void SetTriangleColor( int index, Color color ) {
			switch( index ) {
				case 0:
					Color = color;
					break;
				case 1:
					ColorB = color;
					break;
				case 2:
					ColorC = color;
					break;
				default:
					throw new IndexOutOfRangeException( $"Triangle only has four vertices, 0 to 3, you tried to set element {index}" );
			}
		}

		[SerializeField] TriangleColorMode colorMode = TriangleColorMode.Single;
		/// <summary>The color mode to use on this triangle</summary>
		public TriangleColorMode ColorMode {
			get => colorMode;
			set {
				colorMode = value;
				ApplyProperties();
			}
		}

		[SerializeField] Vector3 a = Vector3.zero;
		/// <summary>Get or set the position of the first vertex</summary>
		public Vector3 A {
			get => a;
			set => SetVector3Now( ShapesMaterialUtils.propA, a = value );
		}
		[SerializeField] Vector3 b = Vector3.up;
		/// <summary>Get or set the position of the second vertex</summary>
		public Vector3 B {
			get => b;
			set => SetVector3Now( ShapesMaterialUtils.propB, b = value );
		}
		[SerializeField] Vector3 c = Vector3.right;
		/// <summary>Get or set the position of the third vertex</summary>
		public Vector3 C {
			get => c;
			set => SetVector3Now( ShapesMaterialUtils.propC, c = value );
		}
		[FormerlySerializedAs( "hollow" )] [SerializeField] bool border = false;
		/// <summary>Whether this is a triangle border instead of a filled triangle</summary>
		public bool Border {
			get => border;
			set => SetIntNow( ShapesMaterialUtils.propBorder, ( border = value ).AsInt() );
		}
		[Obsolete( "Please use Triangle.Border instead", true )]
		public bool Hollow {
			get => Border;
			set => Border = value;
		}
		[SerializeField] float thickness = 0.5f;
		/// <summary>The thickness of the border (if border triangle)</summary>
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
		[SerializeField] [Range( 0, 1 )] float roundness = 0;
		/// <summary>Roundness. 0 = not round at all. 1 = the roundest shape there is</summary>
		public float Roundness {
			get => roundness;
			set => SetFloatNow( ShapesMaterialUtils.propRoundness, roundness = Mathf.Clamp01( value ) );
		}

		/// <summary>The color of this shape. The alpha channel is used for opacity/intensity in all blend modes</summary>
		public override Color Color {
			get => color;
			set {
				SetColor( ShapesMaterialUtils.propColor, color = value );
				SetColor( ShapesMaterialUtils.propColorB, colorB = value );
				SetColorNow( ShapesMaterialUtils.propColorC, colorC = value );
			}
		}
		/// <summary>Get or set the color of the first vertex, when using the per-corner color mode</summary>
		public Color ColorA {
			get => color;
			set => SetColorNow( ShapesMaterialUtils.propColor, color = value );
		}
		[SerializeField] [ShapesColorField( true )] Color colorB = Color.white;
		/// <summary>Get or set the color of the second vertex, when using the per-corner color mode</summary>
		public Color ColorB {
			get => colorB;
			set => SetColorNow( ShapesMaterialUtils.propColorB, colorB = value );
		}
		[SerializeField] [ShapesColorField( true )] Color colorC = Color.white;
		/// <summary>Get or set the color of the third vertex, when using the per-corner color mode</summary>
		public Color ColorC {
			get => colorC;
			set => SetColorNow( ShapesMaterialUtils.propColorC, colorC = value );
		}

		private protected override void SetAllMaterialProperties() {
			SetVector3( ShapesMaterialUtils.propA, a );
			SetVector3( ShapesMaterialUtils.propB, b );
			SetVector3( ShapesMaterialUtils.propC, c );
			if( colorMode == TriangleColorMode.Single ) {
				SetColor( ShapesMaterialUtils.propColorB, Color );
				SetColor( ShapesMaterialUtils.propColorC, Color );
			} else { // per corner
				SetColor( ShapesMaterialUtils.propColorB, colorB );
				SetColor( ShapesMaterialUtils.propColorC, colorC );
			}

			SetFloat( ShapesMaterialUtils.propRoundness, roundness );
			SetFloat( ShapesMaterialUtils.propThickness, thickness );
			SetFloat( ShapesMaterialUtils.propThicknessSpace, (int)thicknessSpace );
			SetFloat( ShapesMaterialUtils.propBorder, border.AsInt() );
			SetAllDashValues( now: false );
		}


		#if UNITY_EDITOR
		private protected override void ShapeClampRanges() {
			thickness = Mathf.Max( 0f, thickness ); // disallow negative inner radius
			roundness = Mathf.Clamp01( roundness );
		}
		#endif

		internal override bool HasDetailLevels => false;
		private protected override Mesh GetInitialMeshAsset() => ShapesMeshUtils.TriangleMesh[0];
		private protected override Material[] GetMaterials() => new[] { ShapesMaterialUtils.matTriangle[BlendMode] };

		private protected override Bounds GetBounds_Internal() {
			Vector3 min = Vector3.Min( Vector3.Min( a, b ), c );
			Vector3 max = Vector3.Max( Vector3.Max( a, b ), c );
			return new Bounds( ( min + max ) / 2, ShapesMath.Abs( max - min ) );
		}

	}

}