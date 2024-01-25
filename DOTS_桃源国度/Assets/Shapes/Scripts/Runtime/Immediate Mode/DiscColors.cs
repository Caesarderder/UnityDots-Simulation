using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	/// <summary>Describes the coloring of Discs/Rings/Arcs/Pies</summary>
	public struct DiscColors {

		/// <summary>The color on the inside at the start angle</summary>
		public Color innerStart;

		/// <summary>The color on the outside at the start angle</summary>
		public Color outerStart;

		/// <summary>The color on the inside at the end angle</summary>
		public Color innerEnd;

		/// <summary>The color on the outside at the end angle</summary>
		public Color outerEnd;

		internal DiscColors( Color innerStart, Color outerStart, Color innerEnd, Color outerEnd ) {
			this.innerStart = innerStart;
			this.outerStart = outerStart;
			this.innerEnd = innerEnd;
			this.outerEnd = outerEnd;
		}

		/// <summary>Creates a flat color. Note: Color will implicitly cast to DiscColors.Flat(), so you can probably skip this function</summary>
		/// <param name="color">The color of the Disc/Ring/Arc/Pie</param>
		public static DiscColors Flat( Color color ) => new DiscColors( color, color, color, color );

		/// <summary>Creates a radial gradient for Discs/Rings/Arcs/Pies</summary>
		/// <param name="inner">The color on the inside</param>
		/// <param name="outer">The color on the outside</param>
		public static DiscColors Radial( Color inner, Color outer ) => new DiscColors( inner, outer, inner, outer );

		/// <summary>Creates an angular gradient for Discs/Rings/Arcs/Pies</summary>
		/// <param name="start">The color at the start angle</param>
		/// <param name="end">The color at the end angle</param>
		public static DiscColors Angular( Color start, Color end ) => new DiscColors( start, start, end, end );

		/// <summary>Creates a bilinear gradient for Discs/Rings/Arcs/Pies</summary>
		/// <param name="innerStart">The color on the inside at the start angle</param>
		/// <param name="outerStart">The color on the outside at the start angle</param>
		/// <param name="innerEnd">The color on the inside at the end angle</param>
		/// <param name="outerEnd">The color on the outside at the end angle</param>
		public static DiscColors Bilinear( Color innerStart, Color outerStart, Color innerEnd, Color outerEnd ) => new DiscColors( innerStart, outerStart, innerEnd, outerEnd );

		public static implicit operator DiscColors( Color flatColor ) => Flat( flatColor );

	}

}