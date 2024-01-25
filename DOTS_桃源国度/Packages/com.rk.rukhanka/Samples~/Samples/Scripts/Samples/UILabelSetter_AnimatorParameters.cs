using TMPro;
using UnityEngine;
using UnityEngine.UI;

/////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka.Samples
{
class UILabelSetter_AnimatorParameters: MonoBehaviour
{
	public TextMeshProUGUI moveSpeedLabel;
	public TextMeshProUGUI selectAnimationLabel;
	public Slider moveSpeedSlider;
	public Slider selectAnimationSlider;

/////////////////////////////////////////////////////////////////////////////////

	void Update()
	{
		moveSpeedLabel.text = $"Animation Speed (float): {moveSpeedSlider.value:F2}f";
		selectAnimationLabel.text = $"Select Animation (int): {selectAnimationSlider.value}";
	}
}
}