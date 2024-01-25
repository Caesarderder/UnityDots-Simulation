using System;
using TMPro;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	/// <summary>Helper type to draw persistent and more performant text. Note: make sure you dispose when done with it. Also, this does not support changing render state such as ZTest or blending modes</summary>
	public class TextElement : IDisposable {
		static int idCounter = 0;
		public static int GetNextId() => idCounter++;
		public readonly int id;
		public TextMeshPro Tmp => ShapesTextPool.Instance.GetElement( id );
		public TextElement() => this.id = GetNextId();
		public void Dispose() => ShapesTextPool.Instance.ReleaseElement( id );
	}

}