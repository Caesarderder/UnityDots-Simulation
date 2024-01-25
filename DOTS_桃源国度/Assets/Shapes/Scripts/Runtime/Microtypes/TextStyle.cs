// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/

using TMPro;
using UnityEngine;

namespace Shapes {

	/// <summary>Text style settings</summary>
	[System.Serializable]
	public struct TextStyle {

		public static readonly TextStyle defaultTextStyle = new TextStyle {
			font = ShapesAssets.Instance.defaultFont,
			size = 1f,
			style = FontStyles.Normal,
			alignment = TextAlign.Center,
			characterSpacing = 0,
			wordSpacing = 0,
			lineSpacing = 0,
			paragraphSpacing = 0,
			margins = Vector4.zero,
			wrap = true,
			overflow = TextOverflowModes.Overflow
		};

		/// <summary>The TMP font to use when drawing text</summary>
		public TMP_FontAsset font;

		/// <summary>The font size to use when drawing text</summary>
		public float size;

		/// <summary>The font style (normal, bold, italics, etc) to use when drawing text</summary>
		public FontStyles style;

		/// <summary>The text alignment to use when drawing text</summary>
		public TextAlign alignment;

		/// <summary>The spacing adjustment between characters to use when drawing text</summary>
		public float characterSpacing;

		/// <summary>The spacing adjustment between words to use when drawing text</summary>
		public float wordSpacing;

		/// <summary>The spacing adjustment between lines to use when drawing text</summary>
		public float lineSpacing;

		/// <summary>The spacing adjustment between paragraphs to use when drawing text</summary>
		public float paragraphSpacing;

		/// <summary>The margins to use when drawing text</summary>
		public Vector4 margins;

		/// <summary>Whether or not to wrap (add line breaks) when the text reaches the end of its containing rectangle</summary>
		public bool wrap;

		/// <summary>How to handle text overflowing the containing rectangle when drawing text</summary>
		public TextOverflowModes overflow;

	}

}