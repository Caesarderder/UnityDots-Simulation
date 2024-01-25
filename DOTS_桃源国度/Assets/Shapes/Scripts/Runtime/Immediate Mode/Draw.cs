using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using TMPro;
#if SHAPES_URP
using UnityEngine.Rendering.Universal;
#elif SHAPES_HDRP
using UnityEngine.Rendering.HighDefinition;
#else
using UnityEngine.Rendering;

#endif

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	/// <summary>The primary class in Shapes for all functionality to draw in immediate mode</summary>
	public static partial class Draw {

		const MethodImplOptions INLINE = MethodImplOptions.AggressiveInlining;

		/// <summary><para>Creates a DrawCommand for drawing in immediate mode.</para>
		/// <para>Example usage:</para>
		/// <para>using(Draw.Command(Camera.main)){ Draw.Line( a, b, Color.red ); }</para></summary>
		/// <param name="cam">The camera to draw in</param>
		/// <param name="cameraEvent">When during the render this command should execute</param>
		#if SHAPES_URP
		public static DrawCommand Command( Camera cam, RenderPassEvent cameraEvent = RenderPassEvent.BeforeRenderingPostProcessing ) => ObjectPool<DrawCommand>.Alloc().Initialize( cam, cameraEvent );
		#elif SHAPES_HDRP
		public static DrawCommand Command( Camera cam, CustomPassInjectionPoint cameraEvent = CustomPassInjectionPoint.BeforePostProcess ) => ObjectPool<DrawCommand>.Alloc().Initialize( cam, cameraEvent );
		#else
		public static DrawCommand Command( Camera cam, CameraEvent cameraEvent = CameraEvent.BeforeImageEffects ) => ObjectPool<DrawCommand>.Alloc().Initialize( cam, cameraEvent );
		#endif

		static MpbLine2D mpbLine = new MpbLine2D();

		[OvldGenCallTarget] static void Line_Internal( [OvldDefault( nameof(LineEndCaps) )] LineEndCap endCaps,
													   [OvldDefault( nameof(ThicknessSpace) )] ThicknessSpace thicknessSpace,
													   Vector3 start,
													   Vector3 end,
													   [OvldDefault( nameof(Color) )] Color colorStart,
													   [OvldDefault( nameof(Color) )] Color colorEnd,
													   [OvldDefault( nameof(Thickness) )] float thickness ) {
			using( new IMDrawer(
					  metaMpb: mpbLine,
					  sourceMat: ShapesMaterialUtils.GetLineMat( Draw.LineGeometry, endCaps )[Draw.BlendMode],
					  sourceMesh: ShapesMeshUtils.GetLineMesh( Draw.LineGeometry, endCaps, DetailLevel ) ) ) {
				MetaMpb.ApplyDashSettings( mpbLine, thickness );
				mpbLine.color.Add( colorStart.ColorSpaceAdjusted() );
				mpbLine.colorEnd.Add( colorEnd.ColorSpaceAdjusted() );
				mpbLine.pointStart.Add( start );
				mpbLine.pointEnd.Add( end );
				mpbLine.thickness.Add( thickness );
				mpbLine.alignment.Add( (float)Draw.LineGeometry ); // this is redundant for 3D lines, but, that's okay, fixing that makes things messier
				mpbLine.thicknessSpace.Add( (float)thicknessSpace );
				mpbLine.scaleMode.Add( (float)ScaleMode );
			}
		}

		static MpbPolyline2D mpbPolyline = new MpbPolyline2D(); // they can use the same mpb structure
		static MpbPolyline2D mpbPolylineJoins = new MpbPolyline2D();

		[OvldGenCallTarget] static void Polyline_Internal( PolylinePath path,
														   [OvldDefault( "false" )] bool closed,
														   [OvldDefault( nameof(PolylineGeometry) )] PolylineGeometry geometry,
														   [OvldDefault( nameof(PolylineJoins) )] PolylineJoins joins,
														   [OvldDefault( nameof(Thickness) )] float thickness,
														   [OvldDefault( nameof(ThicknessSpace) )] ThicknessSpace thicknessSpace,
														   [OvldDefault( nameof(Color) )] Color color ) {
			if( path.EnsureMeshIsReadyToRender( closed, joins, out Mesh mesh ) == false )
				return; // no points defined in the mesh

			switch( path.Count ) {
				case 0:
					Debug.LogWarning( "Tried to draw polyline with no points" );
					return;
				case 1:
					Debug.LogWarning( "Tried to draw polyline with only one point" );
					return;
			}

			void ApplyToMpb( MpbPolyline2D mpb ) {
				mpb.thickness.Add( thickness );
				mpb.thicknessSpace.Add( (int)thicknessSpace );
				mpb.color.Add( color.ColorSpaceAdjusted() );
				mpb.alignment.Add( (int)geometry );
				mpb.scaleMode.Add( (int)ScaleMode );
			}

			if( DrawCommand.IsAddingDrawCommandsToBuffer ) // mark as used by this command to prevent destroy in dispose
				path.RegisterToCommandBuffer( DrawCommand.CurrentWritingCommandBuffer );

			using( new IMDrawer( mpbPolyline, ShapesMaterialUtils.GetPolylineMat( joins )[Draw.BlendMode], mesh, 0 ) )
				ApplyToMpb( mpbPolyline );

			if( joins.HasJoinMesh() ) {
				using( new IMDrawer( mpbPolylineJoins, ShapesMaterialUtils.GetPolylineJoinsMat( joins )[Draw.BlendMode], mesh, 1 ) )
					ApplyToMpb( mpbPolylineJoins );
			}
		}

		static MpbPolygon mpbPolygon = new MpbPolygon();

		[OvldGenCallTarget] static void Polygon_Internal( PolygonPath path,
														  [OvldDefault( nameof(PolygonTriangulation) )] PolygonTriangulation triangulation,
														  [OvldDefault( nameof(Color) )] Color color ) {
			if( path.EnsureMeshIsReadyToRender( triangulation, out Mesh mesh ) == false )
				return; // no points defined in the mesh

			switch( path.Count ) {
				case 0:
					Debug.LogWarning( "Tried to draw polygon with no points" );
					return;
				case 1:
					Debug.LogWarning( "Tried to draw polygon with only one point" );
					return;
				case 2:
					Debug.LogWarning( "Tried to draw polygon with only two points" );
					return;
			}

			if( DrawCommand.IsAddingDrawCommandsToBuffer ) // mark as used by this command to prevent destroy in dispose
				path.RegisterToCommandBuffer( DrawCommand.CurrentWritingCommandBuffer );

			using( new IMDrawer( mpbPolygon, ShapesMaterialUtils.matPolygon[Draw.BlendMode], mesh ) ) {
				MetaMpb.ApplyColorOrFill( mpbPolygon, color );
			}
		}

		[OvldGenCallTarget] [MethodImpl( INLINE )]
		static void Disc_Internal( [OvldDefault( nameof(Radius) )] float radius,
								   [OvldDefault( nameof(Color) )] DiscColors colors ) {
			DiscCore( false, false, radius, 0f, colors );
		}

		[OvldGenCallTarget] [MethodImpl( INLINE )]
		static void Ring_Internal( [OvldDefault( nameof(Radius) )] float radius,
								   [OvldDefault( nameof(Thickness) )] float thickness,
								   [OvldDefault( nameof(Color) )] DiscColors colors ) {
			DiscCore( true, false, radius, thickness, colors );
		}

		[OvldGenCallTarget] [MethodImpl( INLINE )]
		static void Pie_Internal( [OvldDefault( nameof(Radius) )] float radius,
								  [OvldDefault( nameof(Color) )] DiscColors colors,
								  float angleRadStart,
								  float angleRadEnd ) {
			DiscCore( false, true, radius, 0f, colors, angleRadStart, angleRadEnd );
		}

		[OvldGenCallTarget] [MethodImpl( INLINE )]
		static void Arc_Internal( [OvldDefault( nameof(Radius) )] float radius,
								  [OvldDefault( nameof(Thickness) )] float thickness,
								  [OvldDefault( nameof(Color) )] DiscColors colors,
								  float angleRadStart,
								  float angleRadEnd,
								  [OvldDefault( nameof(ArcEndCap) + "." + nameof(ArcEndCap.None) )] ArcEndCap endCaps ) {
			DiscCore( true, true, radius, thickness, colors, angleRadStart, angleRadEnd, endCaps );
		}

		static readonly MpbDisc mpbDisc = new MpbDisc();

		static void DiscCore( bool hollow, bool sector, float radius, float thickness, DiscColors colors, float angleRadStart = 0f, float angleRadEnd = 0f, ArcEndCap arcEndCaps = ArcEndCap.None ) {
			if( sector && Mathf.Abs( angleRadEnd - angleRadStart ) < 0.0001f )
				return;

			using( new IMDrawer( mpbDisc, ShapesMaterialUtils.GetDiscMaterial( hollow, sector )[Draw.BlendMode], ShapesMeshUtils.QuadMesh[0] ) ) {
				MetaMpb.ApplyDashSettings( mpbDisc, thickness );
				mpbDisc.radius.Add( radius );
				mpbDisc.radiusSpace.Add( (int)Draw.RadiusSpace );
				mpbDisc.alignment.Add( (int)Draw.DiscGeometry );
				mpbDisc.thicknessSpace.Add( (int)Draw.ThicknessSpace );
				mpbDisc.thickness.Add( thickness );
				mpbDisc.scaleMode.Add( (int)ScaleMode );
				mpbDisc.angleStart.Add( angleRadStart );
				mpbDisc.angleEnd.Add( angleRadEnd );
				mpbDisc.roundCaps.Add( (int)arcEndCaps );
				mpbDisc.color.Add( colors.innerStart.ColorSpaceAdjusted() );
				mpbDisc.colorOuterStart.Add( colors.outerStart.ColorSpaceAdjusted() );
				mpbDisc.colorInnerEnd.Add( colors.innerEnd.ColorSpaceAdjusted() );
				mpbDisc.colorOuterEnd.Add( colors.outerEnd.ColorSpaceAdjusted() );
			}
		}

		static readonly MpbRegularPolygon mpbRegularPolygon = new MpbRegularPolygon();

		[OvldGenCallTarget] static void RegularPolygon_Internal( [OvldDefault( nameof(RegularPolygonSideCount) )] int sideCount,
																 [OvldDefault( nameof(Radius) )] float radius,
																 [OvldDefault( nameof(Thickness) )] float thickness,
																 [OvldDefault( nameof(Color) )] Color color,
																 bool hollow,
																 [OvldDefault( "0f" )] float roundness,
																 [OvldDefault( "0f" )] float angle ) {
			using( new IMDrawer( mpbRegularPolygon, ShapesMaterialUtils.matRegularPolygon[Draw.BlendMode], ShapesMeshUtils.QuadMesh[0] ) ) {
				MetaMpb.ApplyColorOrFill( mpbRegularPolygon, color );
				MetaMpb.ApplyDashSettings( mpbRegularPolygon, thickness );
				mpbRegularPolygon.radius.Add( radius );
				mpbRegularPolygon.radiusSpace.Add( (int)Draw.RadiusSpace );
				mpbRegularPolygon.alignment.Add( (int)Draw.RegularPolygonGeometry );
				mpbRegularPolygon.sides.Add( Mathf.Max( 3, sideCount ) );
				mpbRegularPolygon.angle.Add( angle );
				mpbRegularPolygon.roundness.Add( roundness );
				mpbRegularPolygon.hollow.Add( hollow.AsInt() );
				mpbRegularPolygon.thicknessSpace.Add( (int)Draw.ThicknessSpace );
				mpbRegularPolygon.thickness.Add( thickness );
				mpbRegularPolygon.scaleMode.Add( (int)ScaleMode );
			}
		}

		static readonly MpbRect mpbRect = new MpbRect();

		[OvldGenCallTarget] static void Rectangle_Internal( [OvldDefault( nameof(BlendMode) )] ShapesBlendMode blendMode,
															[OvldDefault( "false" )] bool hollow,
															Rect rect,
															[OvldDefault( nameof(Color) )] Color color,
															[OvldDefault( nameof(Thickness) )] float thickness,
															[OvldDefault( "default" )] Vector4 cornerRadii ) {
			bool rounded = ShapesMath.MaxComp( cornerRadii ) >= 0.0001f;

			// positive vibes only
			if( rect.width < 0 ) rect.x -= rect.width *= -1;
			if( rect.height < 0 ) rect.y -= rect.height *= -1;

			using( new IMDrawer( mpbRect, ShapesMaterialUtils.GetRectMaterial( hollow, rounded )[blendMode], ShapesMeshUtils.QuadMesh[0] ) ) {
				MetaMpb.ApplyColorOrFill( mpbRect, color );
				MetaMpb.ApplyDashSettings( mpbRect, thickness );
				mpbRect.rect.Add( rect.ToVector4() );
				mpbRect.cornerRadii.Add( cornerRadii );
				mpbRect.thickness.Add( thickness );
				mpbRect.thicknessSpace.Add( (int)Draw.ThicknessSpace );
				mpbRect.scaleMode.Add( (int)ScaleMode );
			}
		}

		static MpbTriangle mpbTriangle = new MpbTriangle();

		[OvldGenCallTarget] static void Triangle_Internal( Vector3 a,
														   Vector3 b,
														   Vector3 c,
														   bool hollow,
														   [OvldDefault( nameof(Thickness) )] float thickness,
														   [OvldDefault( "0f" )] float roundness,
														   [OvldDefault( nameof(Color) )] Color colorA,
														   [OvldDefault( nameof(Color) )] Color colorB,
														   [OvldDefault( nameof(Color) )] Color colorC ) {
			using( new IMDrawer( mpbTriangle, ShapesMaterialUtils.matTriangle[Draw.BlendMode], ShapesMeshUtils.TriangleMesh[0] ) ) {
				MetaMpb.ApplyDashSettings( mpbTriangle, thickness );
				mpbTriangle.a.Add( a );
				mpbTriangle.b.Add( b );
				mpbTriangle.c.Add( c );
				mpbTriangle.color.Add( colorA.ColorSpaceAdjusted() );
				mpbTriangle.colorB.Add( colorB.ColorSpaceAdjusted() );
				mpbTriangle.colorC.Add( colorC.ColorSpaceAdjusted() );
				mpbTriangle.roundness.Add( roundness );
				mpbTriangle.hollow.Add( hollow.AsInt() );
				mpbTriangle.thicknessSpace.Add( (int)Draw.ThicknessSpace );
				mpbTriangle.thickness.Add( thickness );
				mpbTriangle.scaleMode.Add( (int)ScaleMode );
			}
		}

		static MpbQuad mpbQuad = new MpbQuad();

		[OvldGenCallTarget] static void Quad_Internal( Vector3 a,
													   Vector3 b,
													   Vector3 c,
													   [OvldDefault( "a + ( c - b )" )] Vector3 d,
													   [OvldDefault( nameof(Color) )] Color colorA,
													   [OvldDefault( nameof(Color) )] Color colorB,
													   [OvldDefault( nameof(Color) )] Color colorC,
													   [OvldDefault( nameof(Color) )] Color colorD ) {
			using( new IMDrawer( mpbQuad, ShapesMaterialUtils.matQuad[Draw.BlendMode], ShapesMeshUtils.QuadMesh[0] ) ) {
				mpbQuad.a.Add( a );
				mpbQuad.b.Add( b );
				mpbQuad.c.Add( c );
				mpbQuad.d.Add( d );
				mpbQuad.color.Add( colorA.ColorSpaceAdjusted() );
				mpbQuad.colorB.Add( colorB.ColorSpaceAdjusted() );
				mpbQuad.colorC.Add( colorC.ColorSpaceAdjusted() );
				mpbQuad.colorD.Add( colorD.ColorSpaceAdjusted() );
			}
		}


		static readonly MpbSphere metaMpbSphere = new MpbSphere();

		[OvldGenCallTarget] static void Sphere_Internal( [OvldDefault( nameof(Radius) )] float radius,
														 [OvldDefault( nameof(Color) )] Color color ) {
			using( new IMDrawer( metaMpbSphere, ShapesMaterialUtils.matSphere[Draw.BlendMode], ShapesMeshUtils.SphereMesh[(int)DetailLevel] ) ) {
				metaMpbSphere.color.Add( color.ColorSpaceAdjusted() );
				metaMpbSphere.radius.Add( radius );
				metaMpbSphere.radiusSpace.Add( (float)Draw.RadiusSpace );
			}
		}

		static readonly MpbCone mpbCone = new MpbCone();

		[OvldGenCallTarget] static void Cone_Internal( float radius,
													   float length,
													   [OvldDefault( "true" )] bool fillCap,
													   [OvldDefault( nameof(Color) )] Color color ) {
			Mesh mesh = fillCap ? ShapesMeshUtils.ConeMesh[(int)DetailLevel] : ShapesMeshUtils.ConeMeshUncapped[(int)DetailLevel];
			using( new IMDrawer( mpbCone, ShapesMaterialUtils.matCone[Draw.BlendMode], mesh ) ) {
				mpbCone.color.Add( color.ColorSpaceAdjusted() );
				mpbCone.radius.Add( radius );
				mpbCone.length.Add( length );
				mpbCone.sizeSpace.Add( (float)Draw.SizeSpace );
			}
		}

		static readonly MpbCuboid mpbCuboid = new MpbCuboid();

		[OvldGenCallTarget] static void Cuboid_Internal( Vector3 size,
														 [OvldDefault( nameof(Color) )] Color color ) {
			using( new IMDrawer( mpbCuboid, ShapesMaterialUtils.matCuboid[Draw.BlendMode], ShapesMeshUtils.CuboidMesh[0] ) ) {
				mpbCuboid.color.Add( color.ColorSpaceAdjusted() );
				mpbCuboid.size.Add( size );
				mpbCuboid.sizeSpace.Add( (float)Draw.SizeSpace );
			}
		}

		static MpbTorus mpbTorus = new MpbTorus();

		[OvldGenCallTarget] static void Torus_Internal( float radius,
														float thickness,
														[OvldDefault( "0" )] float angleRadStart,
														[OvldDefault( nameof(ShapesMath) + ".TAU" )] float angleRadEnd,
														[OvldDefault( nameof(Color) )] Color color ) {
			if( thickness < 0.0001f )
				return;
			if( radius < 0.00001f ) {
				ThicknessSpace cached = Draw.RadiusSpace;
				Draw.RadiusSpace = Draw.ThicknessSpace;
				Sphere( thickness / 2, color );
				Draw.RadiusSpace = cached;
				return;
			}

			using( new IMDrawer( mpbTorus, ShapesMaterialUtils.matTorus[Draw.BlendMode], ShapesMeshUtils.TorusMesh[(int)DetailLevel] ) ) {
				mpbTorus.color.Add( color.ColorSpaceAdjusted() );
				mpbTorus.radius.Add( radius );
				mpbTorus.thickness.Add( thickness );
				mpbTorus.radiusSpace.Add( (int)Draw.RadiusSpace );
				mpbTorus.thicknessSpace.Add( (int)Draw.ThicknessSpace );
				mpbTorus.scaleMode.Add( (int)Draw.ScaleMode );
				mpbTorus.angleStart.Add( angleRadStart );
				mpbTorus.angleEnd.Add( angleRadEnd );
			}
		}

		static MpbText mpbText = new MpbText();

		[OvldGenCallTarget] static void TextRect_Internal( string content,
														   [OvldDefault( "null" )] TextElement element,
														   Rect rect,
														   [OvldDefault( nameof(Font) )] TMP_FontAsset font,
														   [OvldDefault( nameof(FontSize) )] float fontSize,
														   [OvldDefault( nameof(TextAlign) )] TextAlign align,
														   [OvldDefault( nameof(Color) )] Color color ) {
			PushMatrix();
			Translate( rect.x, rect.y );
			Text_Internal( true, content, element, pivot: default, rect.size, font, fontSize, align, color );
			PopMatrix();
		}

		[OvldGenCallTarget] static void Text_Internal( bool isRect,
													   string content,
													   [OvldDefault( "null" )] TextElement element,
													   [OvldDefault( "default" )] Vector2 pivot, // ignored for simple text
													   [OvldDefault( "default" )] Vector2 size, // ignored for simple text
													   [OvldDefault( nameof(Font) )] TMP_FontAsset font,
													   [OvldDefault( nameof(FontSize) )] float fontSize,
													   [OvldDefault( nameof(TextAlign) )] TextAlign align,
													   [OvldDefault( nameof(Color) )] Color color ) {
			int id;
			TextMeshPro tmp;
			IMDrawer.DrawType drawType;
			if( element == null ) {
				id = TextElement.GetNextId(); // auto-pooling
				tmp = ShapesTextPool.Instance.AllocateElement( id );
				drawType = IMDrawer.DrawType.TextPooledAuto;
			} else {
				id = element.id;
				tmp = element.Tmp;
				drawType = IMDrawer.DrawType.TextPooledPersistent;
			}

			ApplyTextValuesToInstance( tmp, isRect, content, font, fontSize, align, pivot, size, color );
			Text_Internal( tmp, drawType, id );
		}

		delegate void OnPreRenderTmpDelegate( TextMeshPro tmp );

		static OnPreRenderTmpDelegate onPreRenderTmp;
		static OnPreRenderTmpDelegate OnPreRenderTmp {
			get {
				if( onPreRenderTmp == null ) {
					MethodInfo method = typeof(TextMeshPro).GetMethod( "OnPreRenderObject", BindingFlags.Instance | BindingFlags.NonPublic );
					onPreRenderTmp = (OnPreRenderTmpDelegate)method.CreateDelegate( typeof(OnPreRenderTmpDelegate) );
				}

				return onPreRenderTmp;
			}
		}

		static void ApplyTextValuesToInstance( TextMeshPro tmp, bool isRect, string content, TMP_FontAsset font, float fontSize, TextAlign align, Vector2 pivot, Vector2 size, Color color ) {
			// globals
			tmp.fontStyle = FontStyle;
			tmp.characterSpacing = TextCharacterSpacing;
			tmp.wordSpacing = TextWordSpacing;
			tmp.lineSpacing = TextLineSpacing;
			tmp.paragraphSpacing = TextParagraphSpacing;
			tmp.margin = TextMargins;

			// overrides
			tmp.font = font;
			tmp.color = color;
			tmp.fontSize = fontSize;
			tmp.alignment = align.GetTMPAlignment();
			tmp.text = content;

			// positioning & wrapping
			if( isRect ) {
				tmp.enableWordWrapping = TextWrap;
				tmp.overflowMode = TextOverflow;
				tmp.rectTransform.pivot = pivot;
				tmp.rectTransform.sizeDelta = size;
			} else {
				// when we're drawing text without a rectangle, we just always overflow and ignore pivots/sizing/wrapping
				tmp.enableWordWrapping = false;
				tmp.overflowMode = TextOverflowModes.Overflow;
				// tmp.rectTransform.pivot not set, since pivot is ignored when size = 0 anyway
				tmp.rectTransform.sizeDelta = default;
			}

			tmp.rectTransform.position = Matrix.GetColumn( 3 );
			tmp.rectTransform.rotation = Matrix.rotation;

			// set dirty
			OnPreRenderTmp.Invoke( tmp ); // calls OnPreRenderObject. Ensures the mesh is up to date
			// tmp.ForceMeshUpdate(); // alternatively, call this, but this always updates, even when the text doesn't change
		}


		static void Text_Internal( TextMeshPro tmp, IMDrawer.DrawType drawType, int disposeId = -1 ) {
			// todo: something fucky happens sometimes when fallback fonts are the only things in town
			using( new IMDrawer( mpbText, tmp.fontSharedMaterial, tmp.mesh, drawType: drawType, allowInstancing: false, textAutoDisposeId: disposeId ) ) {
				// will draw on dispose
			}

			// ensure child renderers are disabled
			for( int i = 0; i < tmp.transform.childCount; i++ ) {
				// todo: optimize by caching some refs fam
				TMP_SubMesh sm = tmp.transform.GetChild( i ).GetComponent<TMP_SubMesh>();
				sm.renderer.enabled = false; // :>
			}

			// ;-;
			if( tmp.textInfo.materialCount > 1 ) {
				// we have fallback fonts so GreaT!! let's just draw everything because fuck me
				for( int i = 0; i < tmp.transform.childCount; i++ ) {
					TMP_SubMesh sm = tmp.transform.GetChild( i ).GetComponent<TMP_SubMesh>();
					sm.renderer.enabled = false; // :>
					if( sm.sharedMaterial == null )
						continue; // cursed but ok
					using( new IMDrawer( mpbText, sm.sharedMaterial, sm.mesh, drawType: drawType, allowInstancing: false ) ) {
						// will draw on dispose
					}
				}
			}
		}

		static MpbTexture mpbTexture = new MpbTexture();

		[OvldGenCallTarget] static void Texture_Internal( Texture texture, Rect rect, Rect uvs, [OvldDefault( nameof(Color) )] Color color ) {
			Material mat = ShapesMaterialUtils.matTexture[BlendMode];

			using( new IMDrawer( mpbTexture, mat, ShapesMeshUtils.QuadMesh[0], allowInstancing: false ) ) {
				mpbTexture.textures.Add( texture );
				mpbTexture.color.Add( color.ColorSpaceAdjusted() );
				mpbTexture.rect.Add( rect.ToVector4() );
				mpbTexture.uvs.Add( uvs.ToVector4() );
			}
		}

		[MethodImpl( INLINE )] static void Texture_Placement_Internal(
			Texture texture,
			(Rect rect, Rect uvs) placement,
			Color color ) {
			Texture_Internal( texture, placement.rect, placement.uvs, color );
		}

		[MethodImpl( INLINE )] [OvldGenCallTarget] static void Texture_RectFill_Internal(
			Texture texture,
			Rect rect,
			[OvldDefault( nameof(TextureFillMode) + "." + nameof(TextureFillMode.ScaleToFit) )]
			TextureFillMode fillMode,
			[OvldDefault( nameof(Color) )] Color color ) {
			Texture_Placement_Internal( texture, TexturePlacement.Fit( texture, rect, fillMode ), color );
		}

		[MethodImpl( INLINE )] [OvldGenCallTarget] static void Texture_PosSize_Internal(
			Texture texture,
			Vector2 center,
			float size,
			[OvldDefault( nameof(TextureSizeMode) + "." + nameof(TextureSizeMode.LongestSide) )]
			TextureSizeMode sizeMode,
			[OvldDefault( nameof(Color) )] Color color ) {
			Texture_Placement_Internal( texture, TexturePlacement.Size( texture, center, size, sizeMode ), color );
		}

	}

	// these are used by CodegenDrawOverloads
	[AttributeUsage( AttributeTargets.Method )]
	internal class OvldGenCallTarget : Attribute {}

	[AttributeUsage( AttributeTargets.Parameter )]
	internal class OvldDefault : Attribute {
		public string @default;
		public OvldDefault( string @default ) => this.@default = @default;
	}

}