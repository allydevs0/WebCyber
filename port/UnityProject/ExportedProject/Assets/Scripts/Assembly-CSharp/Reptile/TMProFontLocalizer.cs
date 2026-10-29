using TMPro;
using UnityEngine;

namespace Reptile
{
	[RequireComponent(typeof(TextMeshProUGUI))]
	public class TMProFontLocalizer : MonoBehaviour
	{
		[SerializeField]
		private TextMeshProUGUI textMesh;

		[SerializeField]
		private GameFontType gameFontType;

		private IGameTextLocalizer localizer;

		private SystemLanguage currentLanguage = SystemLanguage.Unknown;

		private void Awake()
		{
			localizer = Core.Instance.Localizer;
		}

		private void OnEnable()
		{
			localizer.OnLanguageChanged += OnLanguageChanged;
			if (currentLanguage != localizer.Language)
			{
				UpdateTextMeshProFont(localizer.Language);
				currentLanguage = localizer.Language;
			}
		}

		private void OnDisable()
		{
			if (localizer != null)
			{
				localizer.OnLanguageChanged -= OnLanguageChanged;
			}
		}

		private void OnDestroy()
		{
			if (localizer != null)
			{
				localizer.OnLanguageChanged -= OnLanguageChanged;
			}
		}

		public void UpdateTextMeshLanguageFont(SystemLanguage language)
		{
			if (!(gameFontType == null))
			{
				UpdateTextMeshProFont(language);
				currentLanguage = language;
			}
		}

		private void OnLanguageChanged(IGameTextLocalizer localizer)
		{
			UpdateTextMeshLanguageFont(localizer.Language);
		}

		private void UpdateTextMeshProFont(SystemLanguage language)
		{
			TextMeshProFont font = gameFontType.GetFont(language);
			UpdateTextMesh(font);
		}

		private void UpdateTextMesh(TextMeshProFont textMeshProFont)
		{
			textMesh.font = textMeshProFont.FontAsset;
			textMesh.fontSharedMaterial = textMeshProFont.FontMaterial;
			textMesh.ForceMeshUpdate();
		}
	}
}
