using System;
using System.Collections.Generic;
using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	/// <summary>An immediate-mode color scope helper that will push in the constructor, and pop on dispose</summary>
	public readonly struct ColorStack : IDisposable {

		static readonly Stack<Color> colors = new Stack<Color>();
		internal static void Push( Color prevState ) => colors.Push( prevState );

		internal static void Pop() {
			try {
				Draw.Color = colors.Pop();
			} catch( Exception e ) {
				Debug.LogError( $"You are popping more {nameof(Color)} stacks than you are pushing. error: " + e.Message );
			}
		}

		internal ColorStack( Color mtx ) => colors.Push( mtx );

		/// <summary>Pops/loads the saved color into the current color</summary>
		public void Dispose() => Pop();

	}

}