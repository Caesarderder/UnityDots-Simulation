#if GRIFFIN
using Pinwheel.Griffin.PaintTool;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Pinwheel.Griffin
{
    public class GRuntimeTexturePainter : MonoBehaviour
    {
        [SerializeField]
        private GTerrainTexturePainter painter;
        public GTerrainTexturePainter Painter
        {
            get
            {
                return painter;
            }
            set
            {
                painter = value;
            }
        }

        [SerializeField]
        private GameObject cursorPrefab;
        public GameObject CursorPrefab
        {
            get
            {
                return cursorPrefab;
            }
            set
            {
                cursorPrefab = value;
            }
        }

        [SerializeField]
        private KeyCode alternativeKey;
        public KeyCode AlternativeKey
        {
            get
            {
                return alternativeKey;
            }
            set
            {
                alternativeKey = value;
            }
        }

        [SerializeField]
        private KeyCode negativeKey;
        public KeyCode NegativeKey
        {
            get
            {
                return negativeKey;
            }
            set
            {
                negativeKey = value;
            }
        }

        [SerializeField]
        private Dropdown modeDropdown;
        public Dropdown ModeDropdown
        {
            get
            {
                return modeDropdown;
            }
            set
            {
                modeDropdown = value;
            }
        }

        [SerializeField]
        private Slider redSlider;
        public Slider RedSlider
        {
            get
            {
                return redSlider;
            }
            set
            {
                redSlider = value;
            }
        }

        [SerializeField]
        private Slider greenSlider;
        public Slider GreenSlider
        {
            get
            {
                return greenSlider;
            }
            set
            {
                greenSlider = value;
            }
        }

        [SerializeField]
        private Slider blueSlider;
        public Slider BlueSlider
        {
            get
            {
                return blueSlider;
            }
            set
            {
                blueSlider = value;
            }
        }

        [SerializeField]
        private Slider alphaSlider;
        public Slider AlphaSlider
        {
            get
            {
                return alphaSlider;
            }
            set
            {
                alphaSlider = value;
            }
        }

        [SerializeField]
        private Image colorImage;
        public Image ColorImage
        {
            get
            {
                return colorImage;
            }
            set
            {
                colorImage = value;
            }
        }

        [SerializeField]
        private Slider radiusSlider;
        public Slider RadiusSlider
        {
            get
            {
                return radiusSlider;
            }
            set
            {
                radiusSlider = value;
            }
        }

        [SerializeField]
        private Slider opacitySlider;
        public Slider OpacitySlider
        {
            get
            {
                return opacitySlider;
            }
            set
            {
                opacitySlider = value;
            }
        }

        [SerializeField]
        private Button resetButton;
        public Button ResetButton
        {
            get
            {
                return resetButton;
            }
            set
            {
                resetButton = value;
            }
        }

        [SerializeField]
        private Text logText;
        public Text LogText
        {
            get
            {
                return logText;
            }
            set
            {
                logText = value;
            }
        }

        private GameObject cursorInstance;
        private List<string> logs;

        private void Reset()
        {
            Painter = GetComponent<GTerrainTexturePainter>();
            AlternativeKey = KeyCode.LeftShift;
            NegativeKey = KeyCode.LeftControl;
        }

        private void OnEnable()
        {
            if (ResetButton != null)
            {
                ResetButton.onClick.AddListener(OnResetButtonClicked);
            }

            logs = new List<string>();
        }

        private void OnDisable()
        {
            if (ResetButton != null)
            {
                ResetButton.onClick.RemoveListener(OnResetButtonClicked);
            }
        }

        private void Update()
        {
            try
            {
                UpdateParameters();
                if (Camera.main == null)
                    return;
                if (Painter == null)
                    return;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit = new RaycastHit();
                if (GStylizedTerrain.Raycast(ray, out hit, 1000, Painter.GroupId))
                {
                    DrawCursor(hit, true);
                    Paint(hit);
                }
                else
                {
                    DrawCursor(hit, false);
                }
            }
            catch (System.Exception e)
            {
                logs.Add(e.ToString());
                ShowLogs();
            }
        }

        private void UpdateParameters()
        {
            if (Painter == null)
                return;
            Painter.Mode = ModeDropdown.value == 0 ? GTexturePaintingMode.Elevation : GTexturePaintingMode.Albedo;
            Painter.BrushRadius = RadiusSlider.value;
            Painter.BrushOpacity = OpacitySlider.value;

            Color c = new Color(
                RedSlider.value, GreenSlider.value, BlueSlider.value, AlphaSlider.value);
            ColorImage.color = c;
            Painter.BrushColor = c;
        }

        private void DrawCursor(RaycastHit hit, bool isHit)
        {
            if (CursorPrefab == null)
                return;

            if (cursorInstance == null)
            {
                cursorInstance = GameObject.Instantiate(CursorPrefab);
                cursorInstance.transform.parent = transform;
            }

            if (isHit)
            {
                cursorInstance.gameObject.SetActive(true);
                cursorInstance.transform.position = hit.point;
                cursorInstance.transform.localScale = 2 * Vector3.one * Painter.BrushRadius;
            }
            else
            {
                cursorInstance.gameObject.SetActive(false);
            }
        }

        private void Paint(RaycastHit hit)
        {
            if (!Input.GetMouseButton(0))
                return;

            GTexturePainterArgs args = new GTexturePainterArgs();
            args.HitPoint = hit.point;

            args.MouseEventType =
                Input.GetMouseButtonDown(0) ? GPainterMouseEventType.Down :
                Input.GetMouseButton(0) ? GPainterMouseEventType.Drag :
                GPainterMouseEventType.Up;
            args.ActionType =
                Input.GetKey(AlternativeKey) ? GPainterActionType.Alternative :
                Input.GetKey(NegativeKey) ? GPainterActionType.Negative :
                GPainterActionType.Normal;
            Painter.Paint(args);
        }

        private void OnResetButtonClicked()
        {
            IEnumerator<GStylizedTerrain> terrains = GStylizedTerrain.ActiveTerrains.GetEnumerator();
            while (terrains.MoveNext())
            {
                GStylizedTerrain t = terrains.Current;
                if (t.TerrainData != null)
                {
                    t.TerrainData.Geometry.ResetFull();
                }
            }
        }

        private void ShowLogs()
        {
            string s = GUtilities.ListElementsToString(logs, "\n");
            LogText.text = s;
        }
    }
}
#endif
