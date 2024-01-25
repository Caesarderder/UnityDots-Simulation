using System;
using System.Collections.Generic;
using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	public readonly struct StyleStack : IDisposable {
		
		static readonly Stack<DrawStyle> styles = new Stack<DrawStyle>();
		internal static void Push( DrawStyle prevState ) => styles.Push( prevState );
		internal static void Pop() {
			try {
				Draw.style = styles.Pop();
			} catch( Exception e ) {
				Debug.LogError( $"You are popping more {nameof(DrawStyle)} stacks than you are pushing. error: " + e.Message );
			}
		}

		internal StyleStack( DrawStyle style ) => styles.Push( style );
		public void Dispose() => Pop();

	}

}