// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/

using System;
using UnityEngine;

namespace Shapes {

	public enum AngularUnit {
		Radians,
		Degrees,
		Turns
	}

	public static class AngularUnitExtensions {
		public static string[] angUnitToSuffix = { "rad", "°", "tr" };
		public static string[] angUnitNames = { "Radians", "Degrees", "Turns" };
		public static string[] angUnitNamesShort = { "Rad", "Deg", "Turns" };
		public static string Suffix( this AngularUnit unit ) => angUnitToSuffix[(int)unit];
		public static string Name( this AngularUnit unit ) => angUnitNames[(int)unit];
		public static string NameShort( this AngularUnit unit ) => angUnitNamesShort[(int)unit];

		public static float FromRadians( this AngularUnit unit ) => 1f / unit.ToRadians();

		public static float ToRadians( this AngularUnit unit ) {
			switch( unit ) {
				case AngularUnit.Radians: return 1f;
				case AngularUnit.Degrees: return Mathf.Deg2Rad;
				case AngularUnit.Turns:   return ShapesMath.TAU;
				default:                  throw new ArgumentOutOfRangeException( nameof(unit), unit, null );
			}
		}


	}

}