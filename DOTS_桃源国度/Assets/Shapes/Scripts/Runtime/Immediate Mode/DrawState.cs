// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/

using System.Runtime.CompilerServices;

namespace Shapes {

	public static partial class Draw {

		// initialize all default values
		static Draw() => ResetAllDrawStates();

		/// <summary>Resets all static states - both style &amp; matrix</summary>
		public static void ResetAllDrawStates() {
			ResetMatrix();
			ResetStyle();
		}
		
		/// <summary>using( StateStack ){ /*code*/ } lets you modify the draw state within that scope, automatically restoring the previous state once you leave the scope</summary>
		public static StateStack Scope => new StateStack( Draw.style, Draw.matrix );

		/// <summary>Pushes the current draw state onto the stack. Calling <see cref="Draw.Pop()"/> will restore the saved state state</summary>
		[MethodImpl( INLINE )]public static void Push() => StateStack.Push( Draw.style, Draw.matrix );

		/// <summary>Restores the draw state to the previously pushed state from the stack</summary>
		[MethodImpl( INLINE )]public static void Pop() => StateStack.Pop();

	}

}