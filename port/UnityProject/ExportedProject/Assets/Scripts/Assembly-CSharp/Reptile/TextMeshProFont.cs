using TMPro;
using UnityEngine;

namespace Reptile
{
	[CreateAssetMenu(fileName = "TextMeshProFont", menuName = "Reptile/Localization/TextMeshPro Font", order = 4)]
	public class TextMeshProFont : ScriptableObject
	{
		[SerializeField]
		private SystemLanguage language;

		[SerializeField]
		private TMP_FontAsset fontAsset;

		[SerializeField]
		private Material fontMaterial;

		public TMP_FontAsset FontAsset => fontAsset;

		public Material FontMaterial => fontMaterial;

		public SystemLanguage Language => language;
	}
}
