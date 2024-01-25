using System;
using System.Collections.Generic;
using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	/// <summary>An immediate-mode gradient fill scope helper that will push in the constructor, and pop on dispose</summary>
	public readonly struct GradientFillStack : IDisposable {

		static readonly Stack<(bool, GradientFill)> gradients = new Stack<(bool, GradientFill)>();
		internal static void Push( bool prevOn, GradientFill prevState ) => gradients.Push( ( prevOn, prevState ) );

		internal static void Pop() {
			try {
				( Draw.UseGradientFill, Draw.GradientFill ) = gradients.Pop();
			} catch( Exception e ) {
				Debug.LogError( $"You are popping more {nameof(GradientFill)} stacks than you are pushing. error: " + e.Message );
			}
		}

		internal GradientFillStack( bool on, GradientFill gradient ) => gradients.Push( ( on, gradient ) );

		/// <summary>Pops/loads the saved gradient state into the current gradient state</summary>
		public void Dispose() => Pop();

	}

}