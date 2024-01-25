using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	// find matching shader+keywords+render state
	// if it exists

	internal struct RenderState : IEquatable<RenderState> {

		public Shader shader;
		public string[] keywords; // this is gross

		public bool isTextMaterial;

		/// <summary>Current depth buffer compare function. Default is CompareFunction.LessEqual</summary>
		public CompareFunction zTest;

		/// <summary>This ZOffsetFactor scales the maximum Z slope, with respect to X or Y of the polygon,
		/// while the other ZOffsetUnits, scales the minimum resolvable depth buffer value.
		/// This allows you to force one polygon to be drawn on top of another although they are actually in the same position.
		/// For example, if ZOffsetFactor = 0 &amp; ZOffsetUnits = -1, it pulls the polygon closer to the camera,
		/// ignoring the polygon’s slope, whereas if ZOffsetFactor = -1 &amp; ZOffsetUnits = -1, it will pull the polygon even closer when looking at a grazing angle.</summary>
		public float zOffsetFactor;

		/// <summary>This ZOffsetUnits value scales the minimum resolvable depth buffer value, while the other ZOffsetFactor scales the maximum Z slope, with respect to X or Y of the polygon.
		/// This allows you to force one polygon to be drawn on top of another although they are actually in the same position.
		/// For example, if ZOffsetFactor = 0 &amp; ZOffsetUnits = -1, it pulls the polygon closer to the camera,
		/// ignoring the polygon’s slope, whereas if ZOffsetFactor = -1 &amp; ZOffsetUnits = -1, it will pull the polygon even closer when looking at a grazing angle.</summary>
		public int zOffsetUnits;

		/// <summary>This value will set what channels to render to. By default, it renders to all RGBA channels.
		/// This can be useful when you need something to not write to the alpha channel,
		/// or when you want to render invisible shapes to the stencil buffer</summary>
		public ColorWriteMask colorMask;

		/// <summary> The stencil buffer function used to compare the reference value to the current contents of the buffer. Default: always </summary>
		public CompareFunction stencilComp;

		/// <summary>What to do with the contents of the stencil buffer if the stencil test (and the depth test) passes. Default: keep</summary>
		public StencilOp stencilOpPass;

		/// <summary>The stencil buffer id/reference value to be compared against</summary>
		public byte stencilRefID;

		/// <summary>A stencil buffer 8 bit mask as an 0–255 integer, used when comparing the reference value with the contents of the buffer. Default: 255</summary>
		public byte stencilReadMask;

		/// <summary>A stencil buffer 8 bit mask as an 0–255 integer, used when writing to the buffer. Note that, like other write masks, it specifies which bits of stencil buffer will be affected by write (i.e. WriteMask 0 means that no bits are affected and not that 0 will be written). Default: 255</summary>
		public byte stencilWriteMask;

		public Material CreateMaterial() {
			Material mat = new Material( shader ) { shaderKeywords = keywords };
			mat.SetInt_Shapes( isTextMaterial ? ShapesMaterialUtils.propZTestTMP : ShapesMaterialUtils.propZTest, (int)zTest );
			if( isTextMaterial == false ) { // TMP doesn't support these
				mat.SetFloat( ShapesMaterialUtils.propZOffsetFactor, zOffsetFactor );
				mat.SetInt_Shapes( ShapesMaterialUtils.propZOffsetUnits, zOffsetUnits );
			}

			mat.SetInt_Shapes( ShapesMaterialUtils.propColorMask, (int)colorMask );
			mat.SetInt_Shapes( ShapesMaterialUtils.propStencilComp, (int)stencilComp );
			mat.SetInt_Shapes( ShapesMaterialUtils.propStencilOpPass, (int)stencilOpPass );
			mat.SetInt_Shapes( isTextMaterial ? ShapesMaterialUtils.propStencilIDTMP : ShapesMaterialUtils.propStencilID, stencilRefID );
			mat.SetInt_Shapes( ShapesMaterialUtils.propStencilReadMask, stencilReadMask );
			mat.SetInt_Shapes( ShapesMaterialUtils.propStencilWriteMask, stencilWriteMask );
			mat.enableInstancing = true;
			Object.DontDestroyOnLoad( mat );
			return mat;
		}

		static bool StrArrEquals( string[] a, string[] b ) {
			if( a == null || b == null )
				return a == b;
			return a.Length == b.Length && a.SequenceEqual( b );
		}

		public bool Equals( RenderState other ) =>
			Equals( shader, other.shader ) &&
			StrArrEquals( keywords, other.keywords ) &&
			zTest == other.zTest &&
			zOffsetFactor.Equals( other.zOffsetFactor ) &&
			zOffsetUnits == other.zOffsetUnits &&
			colorMask == other.colorMask &&
			stencilComp == other.stencilComp &&
			stencilOpPass == other.stencilOpPass &&
			stencilRefID == other.stencilRefID &&
			stencilReadMask == other.stencilReadMask &&
			stencilWriteMask == other.stencilWriteMask;

		public override bool Equals( object obj ) => obj is RenderState other && Equals( other );

		public override int GetHashCode() {
			unchecked {
				int hashCode = ( shader != null ? shader.GetHashCode() : 0 );
				if( keywords != null ) {
					foreach( string kw in keywords )
						hashCode = ( hashCode * 397 ) ^ ( kw != null ? kw.GetHashCode() : 0 );
				}

				hashCode = ( hashCode * 397 ) ^ (int)zTest;
				hashCode = ( hashCode * 397 ) ^ zOffsetFactor.GetHashCode();
				hashCode = ( hashCode * 397 ) ^ zOffsetUnits;
				hashCode = ( hashCode * 397 ) ^ (int)colorMask;
				hashCode = ( hashCode * 397 ) ^ (int)stencilComp;
				hashCode = ( hashCode * 397 ) ^ (int)stencilOpPass;
				hashCode = ( hashCode * 397 ) ^ stencilRefID.GetHashCode();
				hashCode = ( hashCode * 397 ) ^ stencilReadMask.GetHashCode();
				hashCode = ( hashCode * 397 ) ^ stencilWriteMask.GetHashCode();
				return hashCode;
			}
		}

	}

}