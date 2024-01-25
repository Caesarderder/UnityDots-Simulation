#if GRIFFIN
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pinwheel.Griffin
{
    public struct GSelectionGridArgs
    {
        public ICollection collection;
        public int selectedIndex;
        public List<int> selectedIndices;
        public int itemPerRow;
        public Vector2 itemSize;
        public bool simpleMode;

        public delegate void DrawHandler(Rect r, object o);
        public DrawHandler drawPreviewFunction;

        public delegate void ItemHandler(Rect r, int index, ICollection collection);
        public ItemHandler contextClickFunction;
    }

    public struct _GSelectionGridArgs
    {
        public ICollection collection;
        public int selectedIndex;
        public List<int> selectedIndices;
        public Vector2 tileSize;
        public float windowWidth;

        public delegate void DrawHandler(Rect r, object o);
        public DrawHandler drawPreviewFunction;
        public DrawHandler drawLabelFunction;
        public DrawHandler customDrawFunction;

        public delegate object CategorizeHandler(object o);
        public CategorizeHandler categorizeFunction;

        public delegate string TooltipHandler(object o);
        public TooltipHandler tooltipFunction;
    }
}
#endif
