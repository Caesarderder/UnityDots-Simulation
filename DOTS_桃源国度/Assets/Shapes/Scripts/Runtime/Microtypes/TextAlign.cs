using TMPro;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	public enum TextAlign {
		TopLeft,
		Top,
		TopRight,
		Left,
		Center,
		Right,
		BottomLeft,
		Bottom,
		BottomRight,

		// added in later versions to match TMP:
		TopJustified,
		TopFlush,
		TopGeoAligned,
		Justified,
		Flush,
		CenterGeoAligned,
		BottomJustified,
		BottomFlush,
		BottomGeoAligned,
		BaselineLeft,
		Baseline,
		BaselineRight,
		BaselineJustified,
		BaselineFlush,
		BaselineGeoAligned,
		MidlineLeft,
		Midline,
		MidlineRight,
		MidlineJustified,
		MidlineFlush,
		MidlineGeoAligned,
		CaplineLeft,
		Capline,
		CaplineRight,
		CaplineJustified,
		CaplineFlush,
		CaplineGeoAligned,
		Converted
	}

	public static class TextAlignExtensions {
		public static TextAlignmentOptions GetTMPAlignment( this TextAlign align ) {
			switch( align ) {
				case TextAlign.TopLeft:            return TextAlignmentOptions.TopLeft;
				case TextAlign.Top:                return TextAlignmentOptions.Top;
				case TextAlign.TopRight:           return TextAlignmentOptions.TopRight;
				case TextAlign.TopJustified:       return TextAlignmentOptions.TopJustified;
				case TextAlign.TopFlush:           return TextAlignmentOptions.TopFlush;
				case TextAlign.TopGeoAligned:      return TextAlignmentOptions.TopGeoAligned;
				case TextAlign.Left:               return TextAlignmentOptions.Left;
				case TextAlign.Center:             return TextAlignmentOptions.Center;
				case TextAlign.Right:              return TextAlignmentOptions.Right;
				case TextAlign.Justified:          return TextAlignmentOptions.Justified;
				case TextAlign.Flush:              return TextAlignmentOptions.Flush;
				case TextAlign.CenterGeoAligned:   return TextAlignmentOptions.CenterGeoAligned;
				case TextAlign.BottomLeft:         return TextAlignmentOptions.BottomLeft;
				case TextAlign.Bottom:             return TextAlignmentOptions.Bottom;
				case TextAlign.BottomRight:        return TextAlignmentOptions.BottomRight;
				case TextAlign.BottomJustified:    return TextAlignmentOptions.BottomJustified;
				case TextAlign.BottomFlush:        return TextAlignmentOptions.BottomFlush;
				case TextAlign.BottomGeoAligned:   return TextAlignmentOptions.BottomGeoAligned;
				case TextAlign.BaselineLeft:       return TextAlignmentOptions.BaselineLeft;
				case TextAlign.Baseline:           return TextAlignmentOptions.Baseline;
				case TextAlign.BaselineRight:      return TextAlignmentOptions.BaselineRight;
				case TextAlign.BaselineJustified:  return TextAlignmentOptions.BaselineJustified;
				case TextAlign.BaselineFlush:      return TextAlignmentOptions.BaselineFlush;
				case TextAlign.BaselineGeoAligned: return TextAlignmentOptions.BaselineGeoAligned;
				case TextAlign.MidlineLeft:        return TextAlignmentOptions.MidlineLeft;
				case TextAlign.Midline:            return TextAlignmentOptions.Midline;
				case TextAlign.MidlineRight:       return TextAlignmentOptions.MidlineRight;
				case TextAlign.MidlineJustified:   return TextAlignmentOptions.MidlineJustified;
				case TextAlign.MidlineFlush:       return TextAlignmentOptions.MidlineFlush;
				case TextAlign.MidlineGeoAligned:  return TextAlignmentOptions.MidlineGeoAligned;
				case TextAlign.CaplineLeft:        return TextAlignmentOptions.CaplineLeft;
				case TextAlign.Capline:            return TextAlignmentOptions.Capline;
				case TextAlign.CaplineRight:       return TextAlignmentOptions.CaplineRight;
				case TextAlign.CaplineJustified:   return TextAlignmentOptions.CaplineJustified;
				case TextAlign.CaplineFlush:       return TextAlignmentOptions.CaplineFlush;
				case TextAlign.CaplineGeoAligned:  return TextAlignmentOptions.CaplineGeoAligned;
				case TextAlign.Converted:          return TextAlignmentOptions.Converted;
			}

			return default;
		}
	}

}