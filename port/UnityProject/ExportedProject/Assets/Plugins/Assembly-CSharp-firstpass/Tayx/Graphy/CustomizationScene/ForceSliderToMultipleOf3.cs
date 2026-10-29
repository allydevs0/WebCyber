using UnityEngine;
using UnityEngine.UI;

namespace Tayx.Graphy.CustomizationScene
{
	public class ForceSliderToMultipleOf3 : MonoBehaviour
	{
		[SerializeField]
		private Slider m_slider;

		private void Start()
		{
			m_slider.onValueChanged.AddListener(UpdateValue);
		}

		private void UpdateValue(float value)
		{
			int num = (int)value;
			if (num % 3 != 0 && num < 300)
			{
				num += 3 - num % 3;
			}
			m_slider.value = num;
		}
	}
}
