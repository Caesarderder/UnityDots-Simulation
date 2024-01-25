#if GRIFFIN
using UnityEditor;
using UnityEngine;

namespace Pinwheel.Griffin
{
    public class GTwoPaneWindowWindow : EditorWindow
    {
        protected float toolbarHeight = EditorGUIUtility.singleLineHeight;
        protected int toolbarOffsetX = 4;
        protected int toolbarOverflowY = 5;
        protected int rightPaneWidth = 300;
        protected int rightPaneWidthMin = 200;
        protected int rightPaneWidthMax = 400;
        protected int resizeRectWidth = 10;
        protected bool isResizinng = false;

        protected Vector2 rightPaneScrollPos;

        protected Rect LeftPaneRect
        {
            get
            {
                Rect r = new Rect();
                r.size = new Vector2(position.size.x - rightPaneWidth, position.size.y - toolbarHeight - toolbarOverflowY);
                r.position = new Vector2(0, toolbarHeight + toolbarOverflowY);
                return r;
            }
        }

        protected Rect RightPaneRect
        {
            get
            {
                Rect r = new Rect();
                r.size = new Vector2(rightPaneWidth, position.size.y - toolbarHeight - toolbarOverflowY);
                r.position = new Vector2(position.size.x - rightPaneWidth, toolbarHeight + toolbarOverflowY);
                return r;
            }
        }

        protected Rect ResizePaneRect
        {
            get
            {
                Rect r = new Rect();
                r.position = new Vector2(position.size.x - rightPaneWidth - resizeRectWidth * 0.5f, toolbarHeight + toolbarOverflowY);
                r.size = new Vector2(resizeRectWidth, position.size.y - toolbarHeight - toolbarOverflowY);
                return r;
            }
        }

        public void OnGUI()
        {
            DrawToolbar();
            DrawLeftPane();
            DrawRightPane();
            HandleResize();
            HandleRepaint();
        }

        private void DrawToolbar()
        {
            Rect r = EditorGUILayout.GetControlRect();
            RectOffset offset = new RectOffset(toolbarOffsetX, toolbarOffsetX, 0, 0);
            GUI.Box(offset.Add(r), string.Empty, EditorStyles.toolbar);

            OnToolbarGUI(r);
        }

        protected virtual void OnToolbarGUI(Rect r)
        {

        }

        private void DrawLeftPane()
        {
            OnLeftPaneGUI(LeftPaneRect);
        }

        protected virtual void OnLeftPaneGUI(Rect r)
        {

        }

        private void DrawRightPane()
        {
            Color separatorColor = GEditorCommon.boxBorderColor;
            GEditorCommon.DrawLine(
                new Vector2(RightPaneRect.min.x - 2, RightPaneRect.min.y),
                new Vector2(RightPaneRect.min.x - 2, RightPaneRect.max.y),
                separatorColor);

            GUILayout.BeginArea(RightPaneRect);
            rightPaneScrollPos = EditorGUILayout.BeginScrollView(rightPaneScrollPos);

            OnRightPaneScrollViewGUI();

            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        protected virtual void OnRightPaneScrollViewGUI()
        {

        }

        private void HandleResize()
        {
            EditorGUIUtility.AddCursorRect(ResizePaneRect, MouseCursor.ResizeHorizontal);
            if (Event.current == null)
                return;
            if (Event.current.type == EventType.MouseDown &&
                ResizePaneRect.Contains(Event.current.mousePosition))
            {
                isResizinng = true;
            }
            else if (Event.current.type == EventType.MouseUp)
            {
                isResizinng = false;
            }

            if (isResizinng)
            {
                rightPaneWidth = (int)(position.size.x - Event.current.mousePosition.x);
                rightPaneWidth = Mathf.Clamp(rightPaneWidth, rightPaneWidthMin, rightPaneWidthMax);
            }
        }

        private void HandleRepaint()
        {
            if (Event.current == null)
                return;
            Repaint();
        }
    }
}
#endif
