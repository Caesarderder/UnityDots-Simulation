using System;
using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {


	[Obsolete( OBSOLETE, true )]
	public abstract class ShapeRendererFillable : ShapeRenderer {

		const string OBSOLETE = "Shapes now use the IFillable interface instead of inheriting from ShapeRendererFillable";

		// global color fill gradient shenanigans
		[Obsolete( OBSOLETE, true )] private protected GradientFill fill = new GradientFill();
		[Obsolete( OBSOLETE, true )] private protected bool useFill;
		[Obsolete( OBSOLETE, true )] int FillTypeShaderInt => useFill ? (int)fill.type : GradientFill.FILL_NONE;
		[Obsolete( OBSOLETE, true )] public bool UseFill {
			get => useFill;
			set {
				useFill = value;
				SetIntNow( ShapesMaterialUtils.propFillType, FillTypeShaderInt );
			}
		}
		[Obsolete( OBSOLETE, true )] public FillType FillType {
			get => default;
			set => _ = value;
		}
		[Obsolete( OBSOLETE, true )] public FillSpace FillSpace {
			get => default;
			set => _ = value;
		}

		[Obsolete( OBSOLETE, true )] public Vector3 FillRadialOrigin {
			get => default;
			set => _ = value;
		}
		[Obsolete( OBSOLETE, true )] public float FillRadialRadius {
			get => default;
			set => _ = value;
		}
		[Obsolete( OBSOLETE, true )] public Vector3 FillLinearStart {
			get => default;
			set => _ = value;
		}
		[Obsolete( OBSOLETE, true )] public Vector3 FillLinearEnd {
			get => default;
			set => _ = value;
		}
		[Obsolete( OBSOLETE, true )] public Color FillColorStart {
			get => default;
			set => _ = value;
		}
		[Obsolete( OBSOLETE, true )] public Color FillColorEnd {
			get => default;
			set => _ = value;
		}

		[Obsolete( OBSOLETE, true )] private protected void SetFillProperties() => _ = 0;


	}

}