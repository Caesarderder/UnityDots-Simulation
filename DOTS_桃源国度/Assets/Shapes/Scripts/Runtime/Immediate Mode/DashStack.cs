using System;
using System.Collections.Generic;
using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	/// <summary>An immediate-mode dash scope helper that will push in the constructor, and pop on dispose</summary>
	public readonly struct DashStack : IDisposable {

		static readonly Stack<(bool, DashStyle)> dashes = new Stack<(bool, DashStyle)>();
		internal static void Push( bool prevOn, DashStyle prevState ) => dashes.Push( ( prevOn, prevState ) );

		internal static void Pop() {
			try {
				( Draw.UseDashes, Draw.DashStyle ) = dashes.Pop();
			} catch( Exception e ) {
				Debug.LogError( $"You are popping more {nameof(DashStyle)} stacks than you are pushing. error: " + e.Message );
			}
		}

		internal DashStack( bool on, DashStyle dash ) => dashes.Push( ( on, dash ) );

		/// <summary>Pops/loads the saved dash state into the current dash state</summary>
		public void Dispose() => Pop();

	}

}