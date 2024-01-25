using System;
using System.Collections.Generic;
using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	/// <summary>An immediate-mode matrix scope helper that will push in the constructor, and pop on dispose</summary>
	public readonly struct MatrixStack : IDisposable {

		static readonly Stack<Matrix4x4> matrices = new Stack<Matrix4x4>();
		internal static void Push( Matrix4x4 prevState ) => matrices.Push( prevState );

		internal static void Pop() {
			try {
				Draw.Matrix = matrices.Pop();
			} catch( Exception e ) {
				Debug.LogError( $"You are popping more {nameof(Matrix4x4)} stacks than you are pushing. error: " + e.Message );
			}
		}

		internal MatrixStack( Matrix4x4 mtx ) => matrices.Push( mtx );

		/// <summary>Pops the drawing matrix</summary>
		public void Dispose() => Pop();

	}

}