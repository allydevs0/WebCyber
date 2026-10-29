using UnityEngine;

namespace Reptile
{
	[CreateAssetMenu(fileName = "GameFontType", menuName = "Reptile/Localization/Game font type", order = 3)]
	public class GameFontType : ScriptableObject
	{
		[SerializeField]
		private TextMeshProFont defaultFont;

		[SerializeField]
		private TextMeshProFont[] fonts;

		public TextMeshProFont DefaultFont => defaultFont;

		public TextMeshProFont GetFont(SystemLanguage language)
		{
			for (int i = 0; i < fonts.Length; i++)
			{
				if (fonts[i].Language == language)
				{
					return fonts[i];
				}
			}
			return defaultFont;
		}
	}
}
