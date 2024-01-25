using System;
using System.Collections.Generic;
using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	public readonly struct StateStack : IDisposable {

		internal static void Push( DrawStyle style, Matrix4x4 mtx ) {
			StyleStack.Push( style );
			MatrixStack.Push( mtx );
		}

		internal static void Pop() {
			MatrixStack.Pop();
			StyleStack.Pop();
		}

		internal StateStack( DrawStyle style, Matrix4x4 mtx ) => Push( style, mtx );
		public void Dispose() => Pop();

	}

}