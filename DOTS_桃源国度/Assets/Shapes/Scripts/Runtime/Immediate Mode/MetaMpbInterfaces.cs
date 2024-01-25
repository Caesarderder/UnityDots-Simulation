using System.Collections.Generic;
using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	internal interface IFillableMpb {
		List<float> fillType { get; }
		List<float> fillSpace { get; }
		List<Vector4> fillStart { get; }
		List<Vector4> fillEnd { get; }
		List<Vector4> fillColorEnd { get; }
	}

	internal interface IDashableMpb {
		List<float> dashSize { get; }
		List<float> dashType { get; }
		List<float> dashShapeModifier { get; }
		List<float> dashSpace { get; }
		List<float> dashSnap { get; }
		List<float> dashOffset { get; }
		List<float> dashSpacing { get; }
	}

}