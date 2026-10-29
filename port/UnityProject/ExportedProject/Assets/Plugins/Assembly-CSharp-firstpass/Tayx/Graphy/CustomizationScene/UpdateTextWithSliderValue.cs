using UnityEngine;
using UnityEngine.UI;

namespace Tayx.Graphy.CustomizationScene
{
	[RequireComponent(typeof(Text))]
	public class UpdateTextWithSliderValue : MonoBehaviour
	{
		[SerializeField]
		private Slider m_slider;

		private Text m_text;

		private void Start()
		{
			m_text = GetComponent<Text>();
			m_slider.onValueChanged.AddListener(UpdateText);
		}

		private void UpdateText(float value)
		{
			m_text.text = value.ToString();
		}
	}
}
