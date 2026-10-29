using TMPro;
using UnityEngine;

namespace Reptile
{
	public class TextMeshProMenuButtonFont : MonoBehaviour
	{
		public GameFontType highlightedGameFontType;

		public GameFontType defaultGameFontType;

		public GameFontType highlightedPressedGameFontType;

		private TextMeshProFont highlightedFont;

		private TextMeshProFont defaultFont;

		private TextMeshProFont highlightedPressedFont;

		private GameFontType currentFontType;

		public Color selectedColor;

		public Color deselectedColor;

		public TextMeshProUGUI textMesh;

		private IGameTextLocalizer localizer;

		private SystemLanguage currentLanguage = SystemLanguage.Unknown;

		public void HighlightText()
		{
			textMesh.font = highlightedFont.FontAsset;
			if (highlightedFont.FontMaterial != null && !textMesh.fontSharedMaterial.Equals(highlightedFont.FontMaterial))
			{
				textMesh.fontSharedMaterial = highlightedFont.FontMaterial;
				currentFontType = highlightedGameFontType;
			}
			textMesh.color = new Color(selectedColor.r, selectedColor.g, selectedColor.b, textMesh.color.a);
		}

		public void DeHighlightText()
		{
			textMesh.font = defaultFont.FontAsset;
			textMesh.fontSharedMaterial = defaultFont.FontMaterial;
			textMesh.color = new Color(deselectedColor.r, deselectedColor.g, deselectedColor.b, textMesh.color.a);
			currentFontType = defaultGameFontType;
		}

		public void ShowPressedText()
		{
			if (!textMesh.font.Equals(highlightedFont.FontAsset))
			{
				textMesh.font = highlightedPressedFont.FontAsset;
			}
			if (highlightedPressedFont.FontMaterial != null && !textMesh.fontSharedMaterial.Equals(highlightedPressedFont.FontMaterial))
			{
				textMesh.fontSharedMaterial = highlightedPressedFont.FontMaterial;
			}
			currentFontType = highlightedPressedGameFontType;
			textMesh.color = new Color(selectedColor.r, selectedColor.g, selectedColor.b, textMesh.color.a);
		}

		public void HideText()
		{
			textMesh.gameObject.SetActive(value: false);
		}

		public void ShowText()
		{
			textMesh.gameObject.SetActive(value: true);
		}

		public void ShowTextAlpha()
		{
			textMesh.alpha = 1f;
		}

		public void ToggleTextVisibility()
		{
			textMesh.alpha = ~(int)textMesh.alpha & 1;
		}

		private void Awake()
		{
			localizer = Core.Instance.Localizer;
			currentFontType = defaultGameFontType;
			SetTextMeshProFonts();
		}

		private void OnEnable()
		{
			localizer.OnLanguageChanged += OnLanguageChanged;
			if (currentLanguage != localizer.Language)
			{
				UpdateTextMeshProFonts(localizer.Language);
				currentLanguage = localizer.Language;
			}
		}

		private void SetTextMeshProFonts()
		{
			highlightedFont = highlightedGameFontType.DefaultFont;
			defaultFont = defaultGameFontType.DefaultFont;
			highlightedPressedFont = highlightedPressedGameFontType.DefaultFont;
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

		private void OnLanguageChanged(IGameTextLocalizer localizer)
		{
			UpdateTextMeshProFonts(localizer.Language);
			currentLanguage = localizer.Language;
		}

		internal void UpdateTextMeshProFonts(SystemLanguage language)
		{
			highlightedFont = highlightedGameFontType.GetFont(language);
			defaultFont = defaultGameFontType.GetFont(language);
			highlightedPressedFont = highlightedPressedGameFontType.GetFont(language);
			TextMeshProFont font = currentFontType.GetFont(language);
			UpdateTextMesh(font);
		}

		private void UpdateTextMesh(TextMeshProFont font)
		{
			textMesh.font = font.FontAsset;
			textMesh.fontMaterial = font.FontMaterial;
			textMesh.ForceMeshUpdate();
		}
	}
}
