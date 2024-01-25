using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/

namespace Shapes {

	/// <summary>An interface for fillable shapes</summary>
	public interface IFillable {

		/// <summary>Get or set the fill style. Note: you must also set <c>UseFill</c> to true if you want to enable fills</summary>
		GradientFill Fill { get; set; }

		/// <summary>Whether or not to use the gradient fill overlay</summary>
		bool UseFill { get; set; }

		/// <summary>What gradient fill type to use</summary>
		FillType FillType { get; set; }

		/// <summary>Whether gradient fills should be in local or world space</summary>
		FillSpace FillSpace { get; set; }

		/// <summary>The origin (in the given FillSpace) to use for radial gradients</summary>
		Vector3 FillRadialOrigin { get; set; }

		/// <summary>The radius (in the given FillSpace) to use for radial gradients</summary>
		float FillRadialRadius { get; set; }

		/// <summary>The start point (in the given FillSpace) to use for linear gradients</summary>
		Vector3 FillLinearStart { get; set; }

		/// <summary>The end point (in the given FillSpace) to use for linear gradients</summary>
		Vector3 FillLinearEnd { get; set; }

		/// <summary>The start color of linear gradients. The center color for radial gradients</summary>
		Color FillColorStart { get; set; }

		/// <summary>The end color of linear gradients. The outer color for radial gradients</summary>
		Color FillColorEnd { get; set; }
		
	}

}