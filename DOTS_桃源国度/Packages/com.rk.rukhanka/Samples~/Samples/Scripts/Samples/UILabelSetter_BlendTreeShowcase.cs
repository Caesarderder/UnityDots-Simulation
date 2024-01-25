using TMPro;
using UnityEngine;
using UnityEngine.UI;

/////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka.Samples
{
class UILabelSetter_BlendTreeShowcase: MonoBehaviour
{
	public TextMeshProUGUI floatParam1Label;
	public TextMeshProUGUI floatParam2Label;
	public Slider floatParam1Slider;
	public Slider floatParam2Slider;
	public int mode;

/////////////////////////////////////////////////////////////////////////////////

	void Update()
	{
		// Direct
		if (mode == 0)
		{
			floatParam1Label.text = $"First Animation Weight: {floatParam1Slider.value:F2}f";
			floatParam2Label.text = $"Second Animation Weight: {floatParam2Slider.value:F2}f";
		}
		// 1D
		else if (mode == 1)
		{
			floatParam1Label.text = $"Blend Tree Parameter: {floatParam1Slider.value:F2}f";
		}
		// 2D Simple Directional
		else
		{
			floatParam1Label.text = $"Blend Tree Parameter X: {floatParam1Slider.value:F2}f";
			floatParam2Label.text = $"Blend Tree Parameter Y: {floatParam2Slider.value:F2}f";
		}
	}
}
}