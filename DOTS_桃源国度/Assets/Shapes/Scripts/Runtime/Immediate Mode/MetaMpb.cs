using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	internal abstract class MetaMpb : IDisposable {

		bool initialized = false;
		public bool HasContent => initialized;
		int instanceCount = 0;
		ShapeDrawState drawState;
		Matrix4x4[] matrices = ArrayPool<Matrix4x4>.Alloc( UnityInfo.INSTANCES_MAX );
		bool HasMultipleInstances => instanceCount > 1;
		bool directMaterialApply = false;

		// all shapes have a color property (except TMP text), the rest are defined in the derived classes
		internal List<Vector4> color = InitList<Vector4>();

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		internal static void ApplyColorOrFill<T>( T fillable, Color baseColor ) where T : MetaMpb, IFillableMpb {
			if( Draw.style.useGradients ) {
				GradientFill fill = Draw.style.gradientFill;
				fillable.color.Add( fill.colorStart.ColorSpaceAdjusted() );
				fillable.fillType.Add( (int)fill.type );
				fillable.fillSpace.Add( (float)fill.space );
				fillable.fillStart.Add( fill.GetShaderStartVector() );
				fillable.fillColorEnd.Add( fill.colorEnd.ColorSpaceAdjusted() );
				fillable.fillEnd.Add( fill.linearEnd );
			} else {
				fillable.color.Add( baseColor.ColorSpaceAdjusted() );
				fillable.fillType.Add( GradientFill.FILL_NONE );
				fillable.fillSpace.Add( default );
				fillable.fillStart.Add( default );
				fillable.fillColorEnd.Add( default );
				fillable.fillEnd.Add( default );
			}
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		internal static void ApplyDashSettings<T>( T dashable, float thickness ) where T : MetaMpb, IDashableMpb {
			if( Draw.UseDashes && Draw.DashStyle.size > 0f ) {
				DashStyle style = Draw.DashStyle;
				dashable.dashSize.Add( style.GetNetAbsoluteSize( true, thickness ) );
				dashable.dashType.Add( (float)style.type );
				dashable.dashShapeModifier.Add( style.shapeModifier );
				dashable.dashSpace.Add( (float)style.space );
				dashable.dashSnap.Add( (int)style.snap );
				dashable.dashOffset.Add( style.offset );
				dashable.dashSpacing.Add( style.GetNetAbsoluteSpacing( true, thickness ) );
			} else {
				dashable.dashSize.Add( 0 );
				dashable.dashType.Add( default );
				dashable.dashShapeModifier.Add( 0 );
				dashable.dashSpace.Add( 0 );
				dashable.dashSnap.Add( 0 );
				dashable.dashOffset.Add( 0 );
				dashable.dashSpacing.Add( 0 );
			}
		}

		internal static List<T> InitList<T>() => new List<T>( UnityInfo.INSTANCES_MAX );

		protected abstract void TransferShapeProperties();

		protected void Transfer( int propertyID, List<Vector4> listVec ) {
			if( directMaterialApply ) {
				drawState.mat.SetVector( propertyID, listVec[0] ); // direct draw
			} else if( HasMultipleInstances )
				sdc.mpb.SetVectorArray( propertyID, listVec ); // instanced draw command (multiple shapes)
			else
				sdc.mpb.SetVector( propertyID, listVec[0] ); // single draw command

			listVec.Clear();
		}

		protected void Transfer( int propertyID, List<float> listFloat ) {
			if( directMaterialApply ) {
				drawState.mat.SetFloat( propertyID, listFloat[0] ); // direct draw
			} else {
				if( HasMultipleInstances )
					sdc.mpb.SetFloatArray( propertyID, listFloat ); // instanced draw command (multiple shapes)
				else
					sdc.mpb.SetFloat( propertyID, listFloat[0] ); // single draw command
			}

			listFloat.Clear();
		}
		
		protected void Transfer( int propertyID, List<Texture> listTex ) {
			if( directMaterialApply ) {
				drawState.mat.SetTexture( propertyID, listTex[0] ); // direct draw
			} else {
				if( HasMultipleInstances )
					Debug.LogError( "no GPU instancing support for textures" );
				else
					sdc.mpb.SetTexture( propertyID, listTex[0] ); // single draw command
			}

			listTex.Clear();
		}

		public bool PreAppendCheck( ShapeDrawState additionDrawState, Matrix4x4 mtx ) {
			bool append = false;

			if( initialized == false ) {
				initialized = true;
				drawState = additionDrawState; // straight up set draw state
				append = true;
			} else if( instanceCount < UnityInfo.INSTANCES_MAX && drawState.CompatibleWith( additionDrawState ) ) { // it already exists, but if it's full, we can't merge. also check compatibility, otherwise no merge pls
				append = true;
			}

			if( append ) // append data
				matrices[instanceCount++] = mtx;

			return append;
		}

		ShapeDrawCall sdc;

		public ShapeDrawCall ExtractDrawCall() {
			if( HasMultipleInstances ) {
				sdc = new ShapeDrawCall( drawState, instanceCount, matrices );
				matrices = ArrayPool<Matrix4x4>.Alloc( UnityInfo.INSTANCES_MAX ); // passed it off to the instanced call
			} else
				sdc = new ShapeDrawCall( drawState, matrices[0] );

			TransferAllProperties();
			Dispose();
			return sdc;
		}

		public void ApplyDirectlyToMaterial() {
			directMaterialApply = true;
			TransferAllProperties();
			directMaterialApply = false;
			Dispose();
		}

		internal void TransferAllProperties() {
			// all shapes have a color property (except TMP text)
			if( this is MpbText == false )
				Transfer( ShapesMaterialUtils.propColor, color );


			if( this is IFillableMpb fillable ) {
				Transfer( ShapesMaterialUtils.propFillType, fillable.fillType );
				Transfer( ShapesMaterialUtils.propFillSpace, fillable.fillSpace );
				Transfer( ShapesMaterialUtils.propFillStart, fillable.fillStart );
				Transfer( ShapesMaterialUtils.propColorEnd, fillable.fillColorEnd );
				Transfer( ShapesMaterialUtils.propFillEnd, fillable.fillEnd );
			}

			if( this is IDashableMpb dashable ) {
				Transfer( ShapesMaterialUtils.propDashSize, dashable.dashSize );
				Transfer( ShapesMaterialUtils.propDashType, dashable.dashType );
				Transfer( ShapesMaterialUtils.propDashShapeModifier, dashable.dashShapeModifier );
				Transfer( ShapesMaterialUtils.propDashSpace, dashable.dashSpace );
				Transfer( ShapesMaterialUtils.propDashSnap, dashable.dashSnap );
				Transfer( ShapesMaterialUtils.propDashOffset, dashable.dashOffset );
				Transfer( ShapesMaterialUtils.propDashSpacing, dashable.dashSpacing );
			}

			// all other properties. also clears the lists
			TransferShapeProperties();
		}

		public void Dispose() {
			initialized = false;
			drawState = default;
			instanceCount = 0;
		}
	}

}