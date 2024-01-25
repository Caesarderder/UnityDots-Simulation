using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	/// <summary>An interface for dashable shapes</summary>
	public interface IDashable {

		/// <summary>Whether or not dash spacing should be auto-set to equal the dash size</summary>
		bool MatchDashSpacingToSize { get; set; }

		/// <summary>Whether or not this shape should be dashed</summary>
		bool Dashed { get; set; }

		/// <summary>Size of dashes in the specified dash space. When using DashSpace.FixedCount, this is the number of dashes</summary>
		float DashSize { get; set; }

		/// <summary>Size of spacing between each dash, in the specified dash space. When using DashSpace.FixedCount, this is the dash:space ratio</summary>
		float DashSpacing { get; set; }

		/// <summary>Offset for dashes. An offset of 1 is the size of a whole dash+space period</summary>
		float DashOffset { get; set; }

		/// <summary>The space in which dashes are defined</summary>
		DashSpace DashSpace { get; set; }

		/// <summary>What snapping type to use for dashed shapes</summary>
		DashSnapping DashSnap { get; set; }

		/// <summary>What dash type to use when dashed shapes</summary>
		DashType DashType { get; set; }

		/// <summary>A -1 to 1 modifier that allows you to tweak or mirror certain dash types</summary>
		float DashShapeModifier { get; set; }
	}

}