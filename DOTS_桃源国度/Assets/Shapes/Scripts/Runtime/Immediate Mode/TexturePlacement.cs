using System;
using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	/// <summary>The fill mode to use when drawing textures in a rectangle</summary>
	public enum TextureFillMode {

		/// <summary>Stretches the texture to fit the rectangle, ignoring aspect ratio</summary>
		StretchToFill,

		/// <summary>Scales the texture (preserving aspect ratio) so that it fits within the rectangle</summary>
		ScaleToFit,

		/// <summary>Scales the texture (preserving aspect ratio) so that it covers the rectangle entirely, cropping any overflow</summary>
		ScaleAndCropToFill
	}

	/// <summary>The size mode to use when drawing textures at a position</summary>
	public enum TextureSizeMode {

		/// <summary>Interprets size as the width of the texture draw region</summary>
		Width,

		/// <summary>Interprets size as the height of the texture draw region</summary>
		Height,

		/// <summary>Interprets size as the longest side of the texture draw region</summary>
		LongestSide,

		/// <summary>Interprets size as the shortest side of the texture draw region</summary>
		ShortestSide,

		/// <summary>Interprets size as pixels per meter, scaling the texture accordingly</summary>
		PixelsPerMeter,

		/// <summary>Interprets size as radius to its corner, scaling the texture accordingly</summary>
		Radius
	}

	internal static class TexturePlacement {

		static readonly Rect fitUvs = new Rect( 0, 0, 1, 1 );

		internal static (Rect rect, Rect uvs) Fit( Texture texture, Rect rect, TextureFillMode mode ) {
			switch( mode ) {
				case TextureFillMode.StretchToFill:      return StretchToFill( rect );
				case TextureFillMode.ScaleToFit:         return ScaleToFit( texture, rect );
				case TextureFillMode.ScaleAndCropToFill: return ScaleAndCropToFill( texture, rect );
				default:                                 throw new ArgumentOutOfRangeException( nameof(mode), mode, null );
			}
		}

		internal static (Rect rect, Rect uvs) Size( Texture texture, Vector2 c, float size, TextureSizeMode mode ) {
			float aspect = texture.width / (float)texture.height;

			switch( mode ) {
				case TextureSizeMode.Width:          return FitWidth( c, size, aspect );
				case TextureSizeMode.Height:         return FitHeight( c, size, aspect );
				case TextureSizeMode.LongestSide:    return aspect < 1 ? FitHeight( c, size, aspect ) : FitWidth( c, size, aspect );
				case TextureSizeMode.ShortestSide:   return aspect < 1 ? FitWidth( c, size, aspect ) : FitHeight( c, size, aspect );
				case TextureSizeMode.PixelsPerMeter: return TexelSized( texture, c, size );
				case TextureSizeMode.Radius:         return FitRadius( texture, c, size );
				default:                             throw new ArgumentOutOfRangeException( nameof(mode), mode, null );
			}
		}

		static (Rect rect, Rect uvs) FitWidth( Vector2 c, float w, float aspect ) => SimpleRect( c, w, w / aspect );
		static (Rect rect, Rect uvs) FitHeight( Vector2 c, float h, float aspect ) => SimpleRect( c, h * aspect, h );

		static (Rect rect, Rect uvs) FitRadius( Texture tex, Vector2 c, float r ) {
			Vector2 size = new Vector2( tex.width, tex.height ).normalized * ( r * 2 );
			return SimpleRect( c, size.x, size.y );
		}

		static (Rect rect, Rect uvs) SimpleRect( Vector2 c, float w, float h ) => ( RectCnt( c.x, c.y, w, h ), fitUvs );

		static Rect RectCnt( float cx, float cy, float w, float h ) => new Rect( cx - w / 2, cy - h / 2, w, h );
		static Rect RectCnt( Vector2 c, float w, float h ) => new Rect( c.x - w / 2, c.y - h / 2, w, h );

		static (Rect rect, Rect uvs) StretchToFill( Rect rect ) => ( rect, fitUvs );

		static (Rect rect, Rect uvs) ScaleToFit( Texture texture, Rect rect ) {
			float scaleX = rect.width / texture.width;
			float scaleY = rect.height / texture.height;
			float scale = Mathf.Min( scaleX, scaleY );
			return ( RectCnt( rect.center, texture.width * scale, texture.height * scale ), fitUvs );
		}

		static (Rect rect, Rect uvs) ScaleAndCropToFill( Texture texture, Rect rect ) {
			float scaleX = rect.width / texture.width;
			float scaleY = rect.height / texture.height;
			float scale = Mathf.Max( scaleX, scaleY );
			return ( rect, RectCnt( 0.5f, 0.5f, scaleX / scale, scaleY / scale ) );
		}

		static (Rect rect, Rect uvs) TexelSized( Texture texture, Vector2 center, float pixelsPerMeter ) {
			float scaleX = texture.width / pixelsPerMeter;
			float scaleY = texture.height / pixelsPerMeter;
			return SimpleRect( center, scaleX, scaleY );
		}

	}

}