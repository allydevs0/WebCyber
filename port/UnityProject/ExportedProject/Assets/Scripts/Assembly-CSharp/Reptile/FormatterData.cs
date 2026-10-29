using UnityEngine;

namespace Reptile
{
	[CreateAssetMenu(fileName = "FormatterData", menuName = "Reptile/Localization/Formatting data", order = 1)]
	public class FormatterData : ScriptableObject
	{
		[Tooltip("The text marker that indicates a replacement is needed")]
		public string replacementMarker = "";

		[Tooltip("The text markers that indicates a bold text is needed")]
		public string boldTextMarkupMarkers = "";

		[Tooltip("The text markers that indicates a button replacement is needed")]
		public string buttonTextMarkupMarkers = "";

		[Tooltip("The text markers that indicates a normal text is needed, mostly used with the replacement marker")]
		public string normalTextMarkupMarkers = "";

		[Tooltip("The text markers that indicates a highlighted text is needed")]
		public string highlightTextMarkupMarkers = "";

		public string[] GetAllTextMarkupMarkers()
		{
			return new string[4] { boldTextMarkupMarkers, buttonTextMarkupMarkers, normalTextMarkupMarkers, highlightTextMarkupMarkers };
		}
	}
}
