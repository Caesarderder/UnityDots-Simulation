using System;
using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	[AttributeUsage( AttributeTargets.Field, Inherited = true, AllowMultiple = false )]
	public sealed class ShapesColorFieldAttribute : PropertyAttribute {
		public readonly bool showAlpha = true;
		public ShapesColorFieldAttribute( bool showAlpha ) => this.showAlpha = showAlpha;
	}

}