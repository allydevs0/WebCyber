using UnityEngine;

namespace Reptile
{
	[CreateAssetMenu(fileName = "LocalizationData", menuName = "Reptile/Localization/Localization data", order = 0)]
	public class LocalizationData : ScriptableObject
	{
		[Tooltip("Object containing formatting data; how to format and interpret texts that are retrieved from the localization")]
		public FormatterData formatterData;

		[Tooltip("Localization name to retrieve button texts from")]
		public string localizationTextName = string.Empty;

		[Tooltip("Localization name to retrieve dialogues from")]
		public string localizationDialoguesName = string.Empty;

		[Tooltip("Localization name to retrieve button sizes from")]
		public string localizationButtonSizeName = string.Empty;

		[Tooltip("Localization name to retrieve credits from")]
		public string localizationCreditsName = string.Empty;

		[Tooltip("Localization file extension including '.'")]
		public string localizationFileExtension = string.Empty;

		[Tooltip("Localization name to retrieve characters names from")]
		public string localizationCharacterNamesName = string.Empty;

		[Tooltip("Localization name to retrieve stage names from")]
		public string localizationStageNamesName = string.Empty;

		[Tooltip("Localization name to retrieve objective text from")]
		public string localizationObjectiveTextName = string.Empty;

		[Tooltip("Localization name to retrieve email messages and subject text from")]
		public string localizationEmailTextName = string.Empty;

		[Tooltip("Localization name to retrieve notification text from")]
		public string localizationNotificationTextName = string.Empty;

		[Tooltip("Localization name to retrieve email messages and subject text from")]
		public string localizationSkinTextName = string.Empty;

		[Tooltip("Localization sub-directory")]
		public string localizationSubdirectory = string.Empty;
	}
}
