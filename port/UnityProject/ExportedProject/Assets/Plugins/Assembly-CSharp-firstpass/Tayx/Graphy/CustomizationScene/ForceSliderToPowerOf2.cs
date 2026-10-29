using UnityEngine;
using UnityEngine.UI;

namespace Tayx.Graphy.CustomizationScene
{
	public class ForceSliderToPowerOf2 : MonoBehaviour
	{
		[SerializeField]
		private Slider m_slider;

		private int[] m_powerOf2Values = new int[7] { 128, 256, 512, 1024, 2048, 4096, 8192 };

		private Text m_text;

		private void Start()
		{
			m_slider.onValueChanged.AddListener(UpdateValue);
		}

		private void UpdateValue(float value)
		{
			int num = 0;
			int num2 = 100000;
			for (int i = 0; i < m_powerOf2Values.Length; i++)
			{
				int num3 = Mathf.Abs((int)value - m_powerOf2Values[i]);
				if (num3 < num2)
				{
					num2 = num3;
					num = i;
				}
			}
			m_slider.value = m_powerOf2Values[num];
		}
	}
}
