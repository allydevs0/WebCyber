using UnityEngine;
using UnityEngine.UI;

namespace Reptile
{
	public class GraphicsStatusHandler : MonoBehaviour
	{
		public Text currentQualitySettingText;

		public void SetGraphicsStatus()
		{
			SetCurrentQualityText();
		}

		private void SetCurrentQualityText()
		{
			int qualityLevel = QualitySettings.GetQualityLevel();
			currentQualitySettingText.text = "Current quality level: " + QualitySettings.names[qualityLevel];
		}
	}
}
