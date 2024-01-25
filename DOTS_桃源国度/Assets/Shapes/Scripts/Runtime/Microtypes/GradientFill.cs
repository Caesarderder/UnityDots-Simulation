using System;
using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	[Serializable]
	public struct GradientFill {

		internal const int FILL_NONE = -1;

		public static readonly GradientFill defaultFill = new GradientFill() {
			type = FillType.LinearGradient,
			space = FillSpace.Local,
			colorStart = Color.black,
			colorEnd = Color.white,
			linearStart = Vector3.zero,
			linearEnd = Vector3.up,
			radialOrigin = Vector3.zero,
			radialRadius = 1f
		};

		/// <summary>The type of color gradient to use (Linear vs Radial)</summary>
		public FillType type;

		/// <summary>The space to draw the gradient in (Local or World)</summary>
		public FillSpace space;

		/// <summary>The start color of the gradient. For radial gradients, this is the inner color</summary>
		public Color colorStart;

		/// <summary>The end color of the gradient. For radial gradients, this is the outer color</summary>
		public Color colorEnd;

		/// <summary>The starting point of linear gradients, in the given space</summary>
		public Vector3 linearStart;

		/// <summary>The endpoint of linear gradients, in the given space</summary>
		public Vector3 linearEnd;

		/// <summary>The origin of radial gradients, in the given space</summary>
		public Vector3 radialOrigin;

		/// <summary>The radius of radial gradients, in the given space</summary>
		public float radialRadius;

		/// <summary>Creates a linear gradient with a start location and color, and an end location and color</summary>
		/// <param name="start">The start location of the gradient, in the given <c>space</c></param>
		/// <param name="end">The end location of the gradient, in the given <c>space</c></param>
		/// <param name="colorStart">The color at the start of the gradient</param>
		/// <param name="colorEnd">The color at the end of the gradient</param>
		/// <param name="space">Whether or not this gradient should be in local or world space</param>
		public static GradientFill Linear( Vector3 start, Vector3 end, Color colorStart, Color colorEnd, FillSpace space = FillSpace.Local ) =>
			new GradientFill {
				type = FillType.LinearGradient,
				colorStart = colorStart,
				colorEnd = colorEnd,
				space = space,
				linearStart = start,
				linearEnd = end,
				radialOrigin = defaultFill.radialOrigin,
				radialRadius = defaultFill.radialRadius
			};

		/// <summary>Creates a radial gradient with an origin and a radius</summary>
		/// <param name="origin">The origin of the radial gradient, in the given <c>space</c></param>
		/// <param name="radius">The radius of the radial gradient, in the given <c>space</c></param>
		/// <param name="colorInner">The color at the center of the gradient</param>
		/// <param name="colorOuter">The color at the outer part of the gradient, at <c>radius</c> distance away from the <c>origin</c></param>
		/// <param name="space">Whether or not this gradient should be in local or world space</param>
		public static GradientFill Radial( Vector3 origin, float radius, Color colorInner, Color colorOuter, FillSpace space = FillSpace.Local ) =>
			new GradientFill {
				type = FillType.RadialGradient,
				space = space,
				colorStart = colorInner,
				colorEnd = colorOuter,
				linearStart = defaultFill.linearStart,
				linearEnd = defaultFill.linearEnd,
				radialOrigin = origin,
				radialRadius = radius
			};

		internal Vector4 GetShaderStartVector() {
			if( type == FillType.LinearGradient ) return linearStart;
			return new Vector4( radialOrigin.x, radialOrigin.y, radialOrigin.z, radialRadius );
		}

		internal int GetShaderFillTypeInt( bool use ) => use ? (int)type : FILL_NONE;

		// used for editor stuff only
		#if UNITY_EDITOR
		bool WorldSpace => space == FillSpace.World;
		public Vector3 GetRadialOriginWorld( Transform tf ) => GetWorldPos( tf, radialOrigin );
		public void SetRadialOriginWorld( Transform tf, Vector3 worldOrigin ) => SetWorldPos( tf, ref radialOrigin, worldOrigin );
		public Vector3 GetLinearStartWorld( Transform tf ) => GetWorldPos( tf, linearStart );
		public void SetLinearStartWorld( Transform tf, Vector3 worldOrigin ) => SetWorldPos( tf, ref linearStart, worldOrigin );
		public Vector3 GetLinearEndWorld( Transform tf ) => GetWorldPos( tf, linearEnd );
		public void SetLinearEndWorld( Transform tf, Vector3 worldOrigin ) => SetWorldPos( tf, ref linearEnd, worldOrigin );
		Vector3 GetWorldPos( Transform tf, Vector3 coord ) => WorldSpace ? coord : tf.TransformPoint( coord );

		void SetWorldPos( Transform tf, ref Vector3 field, Vector3 worldValue ) {
			field = WorldSpace ? worldValue : tf.InverseTransformPoint( worldValue );
		}

		public float GetRadialWorldRadius( Transform tf ) => WorldSpace ? radialRadius : tf.lossyScale.x * radialRadius;

		public void SetRadialWorldRadius( Transform tf, float newWorldRadius ) {
			radialRadius = WorldSpace ? newWorldRadius : newWorldRadius / tf.lossyScale.x;
		}
		#endif

		#region Deprecated stuff

		[Obsolete( "Use GradientFill.Linear instead", true )]
		public static GradientFill CreateLinear( Vector3 start, Vector3 end, Color colorStart, Color colorEnd, FillSpace space ) => default;

		[Obsolete( "Use GradientFill.Radial instead", true )]
		public static GradientFill CreateRadial( Vector3 origin, float radius, Color colorInner, Color colorOuter, FillSpace space ) => default;

		#endregion

	}

}