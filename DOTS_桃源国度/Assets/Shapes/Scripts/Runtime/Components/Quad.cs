using System;
using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	/// <summary>A Quad shape component</summary>
	[ExecuteAlways]
	[AddComponentMenu( "Shapes/Quad" )]
	public class Quad : ShapeRenderer {

		/// <summary>Gradient color modes for the quad shape</summary>
		public enum QuadColorMode {
			/// <summary>A single color for the whole quad</summary>
			Single,

			/// <summary>Two colors running horizontally across the quad</summary>
			Horizontal,

			/// <summary>Two colors running vertically across the quad</summary>
			Vertical,

			/// <summary>One color per corner of the quad</summary>
			PerCorner
		}

		/// <summary>Get or set vertex positions by index (clockwise)</summary>
		/// <param name="index">A value from 0 to 3</param>
		/// <exception cref="IndexOutOfRangeException"></exception>
		public Vector3 this[ int index ] {
			get {
				switch( index ) {
					case 0: return A;
					case 1: return B;
					case 2: return C;
					case 3: return D;
					default:
						throw new IndexOutOfRangeException( $"Quad only has four vertices, 0 to 3, you tried to access element {index}" );
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
					case 3:
						D = value;
						break;
					default:
						throw new IndexOutOfRangeException( $"Quad only has four vertices, 0 to 3, you tried to set element {index}" );
				}
			}
		}

		/// <summary>Get vertex positions by index (clockwise)</summary>
		/// <param name="index">An index from 0 to 3</param>
		public Vector3 GetQuadVertex( int index ) => this[index];

		/// <summary>Set vertex positions by index (clockwise)</summary>
		/// <param name="index">An index from 0 to 3</param>
		/// <param name="value">The position to set the vertex to</param>
		public Vector3 SetQuadVertex( int index, Vector3 value ) => this[index] = value;

		/// <summary>Get vertex color by index (clockwise)</summary>
		/// <param name="index">An index from 0 to 3</param>
		public Color GetQuadColor( int index ) {
			switch( index ) {
				case 0: return Color;
				case 1: return ColorB;
				case 2: return ColorC;
				case 3: return ColorD;
				default:
					throw new IndexOutOfRangeException( $"Quad only has four vertices, 0 to 3, you tried to access element {index}" );
			}
		}

		/// <summary>Set vertex color by index (clockwise)</summary>
		/// <param name="index">An index from 0 to 3</param>
		/// <param name="color">The color to set the vertex to</param>
		public void SetQuadColor( int index, Color color ) {
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
				case 3:
					ColorD = color;
					break;
				default:
					throw new IndexOutOfRangeException( $"Quad only has four vertices, 0 to 3, you tried to set element {index}" );
			}
		}

		[SerializeField] QuadColorMode colorMode = QuadColorMode.Single;
		/// <summary>The color gradient mode to use on this quad</summary>
		public QuadColorMode ColorMode {
			get => colorMode;
			set {
				colorMode = value;
				ApplyProperties();
			}
		}

		[SerializeField] Vector3 a = new Vector2( -0.5f, -0.5f );
		/// <summary>Get or set the position of the first vertex (clockwise)</summary>
		public Vector3 A {
			get => a;
			set {
				SetVector3Now( ShapesMaterialUtils.propA, a = value );
				CheckAutoSetD();
			}
		}
		[SerializeField] Vector3 b = new Vector2( -0.5f, 0.5f );
		/// <summary>Get or set the position of the second vertex (clockwise)</summary>
		public Vector3 B {
			get => b;
			set {
				SetVector3Now( ShapesMaterialUtils.propB, b = value );
				CheckAutoSetD();
			}
		}
		[SerializeField] Vector3 c = new Vector2( 0.5f, 0.5f );
		/// <summary>Get or set the position of the third vertex (clockwise)</summary>
		public Vector3 C {
			get => c;
			set {
				SetVector3Now( ShapesMaterialUtils.propC, c = value );
				CheckAutoSetD();
			}
		}
		[SerializeField] Vector3 d = new Vector2( 0.5f, -0.5f );
		/// <summary>Get or set the position of the fourth vertex (clockwise)</summary>
		public Vector3 D {
			get => d;
			set {
				if( autoSetD )
					Debug.LogWarning( "tried to set D when auto-set is enabled, you might want to turn off auto-set on this object", gameObject );
				else
					SetVector3Now( ShapesMaterialUtils.propD, d = value );
			}
		}

		/// <summary>Whether or not the position of D should be automatically set based on the other points</summary>
		public bool IsUsingAutoD {
			get => autoSetD;
			set {
				autoSetD = value;
				AutoSetD();
			}
		}
		[SerializeField] bool autoSetD = false;

		/// <summary>Get an auto-calculated fourth position based on the first three vertices</summary>
		public Vector3 DAuto => A + ( C - B );

		void AutoSetD() => SetVector3( ShapesMaterialUtils.propD, DAuto );

		void CheckAutoSetD() {
			if( autoSetD )
				AutoSetD();
		}

		/// <summary>The color of this shape. The alpha channel is used for opacity/intensity in all blend modes</summary>
		public override Color Color {
			get => color;
			set {
				SetColor( ShapesMaterialUtils.propColor, color = value );
				SetColor( ShapesMaterialUtils.propColorB, colorB = value );
				SetColor( ShapesMaterialUtils.propColorC, colorC = value );
				SetColorNow( ShapesMaterialUtils.propColorD, colorD = value );
			}
		}
		/// <summary>The color on the left side (A and B) of this shape when using the horizontal gradient color mode</summary>
		public Color ColorLeft {
			get => color;
			set {
				SetColor( ShapesMaterialUtils.propColor, color = value );
				SetColorNow( ShapesMaterialUtils.propColorB, colorB = value );
			}
		}
		/// <summary>The color on the top side (B and C) of this shape when using the vertical gradient color mode</summary>
		public Color ColorTop {
			get => colorB;
			set {
				SetColor( ShapesMaterialUtils.propColorB, colorB = value );
				SetColorNow( ShapesMaterialUtils.propColorC, colorC = value );
			}
		}
		/// <summary>The color on the right side (C and D) of this shape when using the horizontal gradient color mode</summary>
		public Color ColorRight {
			get => colorC;
			set {
				SetColor( ShapesMaterialUtils.propColorC, colorC = value );
				SetColorNow( ShapesMaterialUtils.propColorD, colorD = value );
			}
		}
		/// <summary>The color on the bottom side (D and A) of this shape when using the vertical gradient color mode</summary>
		public Color ColorBottom {
			get => colorD;
			set {
				SetColor( ShapesMaterialUtils.propColorD, colorD = value );
				SetColorNow( ShapesMaterialUtils.propColor, color = value );
			}
		}

		/// <summary>Get or set the color of the first vertex (clockwise), when using the per-corner color mode</summary>
		public Color ColorA {
			get => color;
			set => SetColorNow( ShapesMaterialUtils.propColor, color = value );
		}
		[SerializeField] [ShapesColorField( true )] Color colorB = Color.white;
		/// <summary>Get or set the color of the second vertex (clockwise), when using the per-corner color mode</summary>
		public Color ColorB {
			get => colorB;
			set => SetColorNow( ShapesMaterialUtils.propColorB, colorB = value );
		}
		[SerializeField] [ShapesColorField( true )] Color colorC = Color.white;
		/// <summary>Get or set the color of the third vertex (clockwise), when using the per-corner color mode</summary>
		public Color ColorC {
			get => colorC;
			set => SetColorNow( ShapesMaterialUtils.propColorC, colorC = value );
		}
		[SerializeField] [ShapesColorField( true )] Color colorD = Color.white;
		/// <summary>Get or set the color of the fourth vertex (clockwise), when using the per-corner color mode</summary>
		public Color ColorD {
			get => colorD;
			set => SetColorNow( ShapesMaterialUtils.propColorD, colorD = value );
		}

		private protected override void SetAllMaterialProperties() {
			SetVector3( ShapesMaterialUtils.propA, a );
			SetVector3( ShapesMaterialUtils.propB, b );
			SetVector3( ShapesMaterialUtils.propC, c );
			if( autoSetD )
				AutoSetD();
			else
				SetVector3( ShapesMaterialUtils.propD, d );


			switch( colorMode ) {
				case QuadColorMode.Single:
					SetColor( ShapesMaterialUtils.propColorB, Color );
					SetColor( ShapesMaterialUtils.propColorC, Color );
					SetColor( ShapesMaterialUtils.propColorD, Color );
					break;
				case QuadColorMode.Horizontal:
					SetColor( ShapesMaterialUtils.propColorB, Color );
					SetColor( ShapesMaterialUtils.propColorC, colorC );
					SetColor( ShapesMaterialUtils.propColorD, colorC );
					break;
				case QuadColorMode.Vertical:
					SetColor( ShapesMaterialUtils.propColor, colorD ); // todo: this is a double-assign, it's already set before this, but to the wrong value :c
					SetColor( ShapesMaterialUtils.propColorB, colorB );
					SetColor( ShapesMaterialUtils.propColorC, colorB );
					SetColor( ShapesMaterialUtils.propColorD, colorD );
					break;
				case QuadColorMode.PerCorner:
					SetColor( ShapesMaterialUtils.propColorB, colorB );
					SetColor( ShapesMaterialUtils.propColorC, colorC );
					SetColor( ShapesMaterialUtils.propColorD, colorD );
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		internal override bool HasDetailLevels => false;
		internal override bool HasScaleModes => false;
		private protected override Mesh GetInitialMeshAsset() => ShapesMeshUtils.QuadMesh[0];
		private protected override Material[] GetMaterials() => new[] { ShapesMaterialUtils.matQuad[BlendMode] };

		private protected override Bounds GetBounds_Internal() {
			Vector3 min = Vector3.Min( Vector3.Min( a, b ), c );
			Vector3 max = Vector3.Max( Vector3.Max( a, b ), c );
			return new Bounds( ( min + max ) / 2, ShapesMath.Abs( max - min ) );
		}

	}

}