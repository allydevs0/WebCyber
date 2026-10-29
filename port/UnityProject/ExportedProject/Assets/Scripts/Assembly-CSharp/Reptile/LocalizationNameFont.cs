using TMPro;
using UnityEngine;

namespace Reptile
{
	public class LocalizationNameFont : MonoBehaviour
	{
		[SerializeField]
		private TextMeshProUGUI localizationNameText;

		[SerializeField]
		private GameFontType gameFontType;

		private IGameTextLocalizer localizer;

		private void Awake()
		{
			localizer = Core.Instance.Localizer;
		}

		public void UpdateFont(string languageName)
		{
			SystemLanguage language = localizer.ConvertToLanguage(languageName);
			TextMeshProFont font = gameFontType.GetFont(language);
			UpdateTextMesh(font);
		}

		private void UpdateTextMesh(TextMeshProFont textMeshProFont)
		{
			localizationNameText.font = textMeshProFont.FontAsset;
			localizationNameText.fontSharedMaterial = textMeshProFont.FontMaterial;
			localizationNameText.ForceMeshUpdate();
		}
	}
}
