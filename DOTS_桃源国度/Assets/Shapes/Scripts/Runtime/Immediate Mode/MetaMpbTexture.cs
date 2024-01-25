using System.Collections.Generic;
using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	internal class MpbTexture : MetaMpb {

		internal readonly List<Texture> textures = InitList<Texture>();
		internal readonly List<Vector4> rect = InitList<Vector4>();
		internal readonly List<Vector4> uvs = InitList<Vector4>();

		protected override void TransferShapeProperties() {
			Transfer( ShapesMaterialUtils.propRect, rect );
			Transfer( ShapesMaterialUtils.propUvs, uvs );
			Transfer( ShapesMaterialUtils.propMainTex, textures );
		}

	}

}