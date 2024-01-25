// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/

namespace Shapes {

	/// <summary>Various dash types</summary>
	public enum DashType {
		/// <summary>Standard dashes</summary>	
		Basic,

		/// <summary>Angled dashes, similar to hazard stripes</summary>
		Angled,

		/// <summary>Rounded dashes</summary>
		Rounded
	}

	public static class DashTypeExtensions {
		public static bool HasModifier( this DashType type ) {
			switch( type ) {
				case DashType.Angled: return true;
				default:              return false;
			}
		}
	}

}