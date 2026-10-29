using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Reptile
{
	public class TextMeshProGameTextLocalizer : IGameTextLocalizer, IDisposable, IEditorGameTextLocalizer
	{
		private SystemLanguage localizationLanguage = SystemLanguage.Unknown;

		private SystemLanguage defaultLanguage = SystemLanguage.Unknown;

		private LocalizationData localizerData;

		private ButtonSpriteData buttonSpriteData;

		private GameInput gameInput;

		private LocalizationLookupTable localizationLookupTable;

		private IGameTextFormatter gameTextFormatter;

		private TextMeshProButtonGlyphFormatter buttonGlyphTextFormatter;

		private PlatformLanguages platformLanguages;

		private IGameTextLocalizer.LanguagesLocalizedOptionText allAvailableLanguages;

		private string[] stageNameLookup;

		private string[] characterNameLookup;

		private string[] objectiveTextLookup;

		public SystemLanguage Language => localizationLanguage;

		public SystemLanguage DefaultLanguage => defaultLanguage;

		public IGameTextLocalizer.LanguagesLocalizedOptionText AllAvailableLanguages => allAvailableLanguages;

		public event OnLanguageChangedHandler OnLanguageChanged;

		public TextMeshProGameTextLocalizer(SystemLanguage localizationLanguage, SystemLanguage defaultLanguage, LocalizationData localizerData, PlatformLanguages platformLanguages, ButtonSpriteData buttonSpriteData, GameInput gameInput)
		{
			this.defaultLanguage = defaultLanguage;
			this.localizerData = localizerData;
			this.buttonSpriteData = buttonSpriteData;
			this.gameInput = gameInput;
			this.platformLanguages = platformLanguages;
			allAvailableLanguages = FindAllAvailableLanguages();
			CreateTextFormatterForLocalizer();
			if (IsLanguageAvailable(localizationLanguage))
			{
				UpdateLocalization(localizationLanguage);
			}
			else
			{
				UpdateLocalization(defaultLanguage);
			}
			InitStageNameLookupTable();
			InitCharacterNameLookupTable();
			InitObjectiveTextLookupTable();
		}

		public void Dispose()
		{
			localizerData = null;
			gameTextFormatter.Dispose();
			gameTextFormatter = null;
			DisposeCurrentLocalizationLookupTable();
		}

		public TextMeshProButtonGlyphFormatter GetButtonGlyphTextFormatter()
		{
			return buttonGlyphTextFormatter;
		}

		private void DisposeCurrentLocalizationLookupTable()
		{
			if (localizationLookupTable != null)
			{
				localizationLookupTable.Dispose();
			}
			localizationLookupTable = null;
		}

		private void CreateTextFormatterForLocalizer()
		{
			ITextMatcher[] array = new ITextMatcher[4];
			buttonGlyphTextFormatter = new TextMeshProButtonGlyphFormatter(gameInput, new TextMeshProButtonSpriteHandler(buttonSpriteData, gameInput));
			array[0] = new DelimiterMatcher(new TextMeshProBoldFormatter(), localizerData.formatterData.boldTextMarkupMarkers);
			array[1] = new DelimiterMatcher(buttonGlyphTextFormatter, localizerData.formatterData.buttonTextMarkupMarkers);
			array[2] = new DelimiterMatcher(new TextMeshProNormalFormatter(), localizerData.formatterData.normalTextMarkupMarkers);
			array[3] = new DelimiterMatcher(new TextMeshProHighlightFormatter(), localizerData.formatterData.highlightTextMarkupMarkers);
			gameTextFormatter = new RegexGameTextFormatter(localizerData.formatterData, array);
		}

		public void UpdateLocalization(SystemLanguage localizationLanguage)
		{
			if (this.localizationLanguage != localizationLanguage)
			{
				DisposeCurrentLocalizationLookupTable();
				this.localizationLanguage = localizationLanguage;
				GenerateLocalization();
				if (OnLanguageChanged != null)
				{
					OnLanguageChanged(this);
				}
			}
		}

		public bool ApplyNewLocalizationLanguage(string languageName)
		{
			SystemLanguage systemLanguage = ConvertToLanguage(languageName);
			if (systemLanguage == SystemLanguage.Unknown)
			{
				return false;
			}
			UpdateLocalization(systemLanguage);
			return true;
		}

		public string ConvertToLanguageName(SystemLanguage systemLanguage)
		{
			for (int i = 0; i < platformLanguages.availableLanguages.Length; i++)
			{
				if (platformLanguages.availableLanguages[i] == systemLanguage)
				{
					return platformLanguages.availableLanguagesNames[i];
				}
			}
			return string.Empty;
		}

		public SystemLanguage ConvertToLanguage(string languageName)
		{
			for (int i = 0; i < platformLanguages.availableLanguagesNames.Length; i++)
			{
				if (platformLanguages.availableLanguagesNames[i].Equals(languageName))
				{
					return platformLanguages.availableLanguages[i];
				}
			}
			return SystemLanguage.Unknown;
		}

		private void GenerateLocalization()
		{
			localizationLookupTable = GenerateLocalizationTable(localizationLanguage);
			if (localizationLookupTable == null)
			{
				localizationLookupTable = HandleNoLocalizationFoundForLanguage(localizationLanguage);
			}
		}

		private LocalizationLookupTable GenerateLocalizationTable(SystemLanguage language)
		{
			string localizationFilePath = Path.Combine(Application.streamingAssetsPath, localizerData.localizationSubdirectory, language.ToString() + localizerData.localizationFileExtension);
			LocalizationTableGenerator localizationTableGenerator = new LocalizationTableGenerator();
			LocalizationLookupTable result = localizationTableGenerator.GetLocalizationLookupTable(localizationFilePath, localizerData, localizationLanguage);
			DisposeLocalizationTableGenerator(localizationTableGenerator);
			return result;
		}

		private void DisposeLocalizationTableGenerator(LocalizationTableGenerator localizationTableGenerator)
		{
			localizationTableGenerator = null;
		}

		private LocalizationLookupTable HandleNoLocalizationFoundForLanguage(SystemLanguage systemLanguage)
		{
			return CreateDefaultLocalizationTable();
		}

		private LocalizationLookupTable CreateEmptyLocalizationTable()
		{
			return new LocalizationLookupTable(new LocalizationGroup[0]);
		}

		private LocalizationLookupTable CreateDefaultLocalizationTable()
		{
			return GenerateLocalizationTable(defaultLanguage);
		}

		public int GetButtonSize(string localizationKey)
		{
			string localizationValueFromSubgroup = localizationLookupTable.GetLocalizationValueFromSubgroup(localizerData.localizationButtonSizeName, localizationKey);
			int result = 0;
			int.TryParse(localizationValueFromSubgroup, out result);
			return result;
		}

		public string GetRawUserInterfaceTextValue(string localizationKey)
		{
			return localizationLookupTable.GetLocalizationValueFromSubgroup(localizerData.localizationTextName, localizationKey);
		}

		public string GetUserInterfaceText(string localizationKey, params string[] textsToInsert)
		{
			string rawUserInterfaceTextValue = GetRawUserInterfaceTextValue(localizationKey);
			return ReplaceAllTextMarkersWithText(rawUserInterfaceTextValue, textsToInsert);
		}

		public string GetRawDialogueTextValue(string localizationKey)
		{
			return localizationLookupTable.GetLocalizationValueFromSubgroup(localizerData.localizationDialoguesName, localizationKey);
		}

		public string GetDialogueText(string localizationKey, params string[] textsToInsert)
		{
			string rawDialogueTextValue = GetRawDialogueTextValue(localizationKey);
			return ReplaceAllTextMarkersWithText(rawDialogueTextValue, textsToInsert);
		}

		public string GetRawEmailTextValue(string localizationKey)
		{
			return localizationLookupTable.GetLocalizationValueFromSubgroup(localizerData.localizationEmailTextName, localizationKey);
		}

		public string GetEmailText(string localizationKey, params string[] textsToInsert)
		{
			string rawEmailTextValue = GetRawEmailTextValue(localizationKey);
			return ReplaceAllTextMarkersWithText(rawEmailTextValue, textsToInsert);
		}

		public string GetNotificationText(string localizationKey, params string[] textsToInsert)
		{
			string localizationValueFromSubgroup = localizationLookupTable.GetLocalizationValueFromSubgroup(localizerData.localizationNotificationTextName, localizationKey);
			return ReplaceAllTextMarkersWithText(localizationValueFromSubgroup, textsToInsert);
		}

		public string GetSkinText(string localizationKey, params string[] textsToInsert)
		{
			string localizationValueFromSubgroup = localizationLookupTable.GetLocalizationValueFromSubgroup(localizerData.localizationSkinTextName, localizationKey);
			return ReplaceAllTextMarkersWithText(localizationValueFromSubgroup, textsToInsert);
		}

		public string GetCharacterName(string localizationKey, params string[] textsToInsert)
		{
			string localizationValueFromSubgroup = localizationLookupTable.GetLocalizationValueFromSubgroup(localizerData.localizationCharacterNamesName, localizationKey);
			return ReplaceAllTextMarkersWithText(localizationValueFromSubgroup, textsToInsert);
		}

		public string GetCharacterName(int localizationKeyID)
		{
			return GetCharacterName($"CHARACTER{localizationKeyID:000}");
		}

		public string GetCharacterName(Characters character)
		{
			string localizationKey = characterNameLookup[(int)(character + 1)];
			return GetCharacterName(localizationKey);
		}

		public bool CharacterEqualsCharacterID(Characters character, int characterID)
		{
			string obj = characterNameLookup[(int)(character + 1)];
			return Convert.ToInt32(obj.Substring(obj.Length - 3, 3)) == characterID;
		}

		private void InitCharacterNameLookupTable()
		{
			characterNameLookup = new string[27];
			characterNameLookup[0] = "CHARACTER000";
			characterNameLookup[1] = "CHARACTER006";
			characterNameLookup[2] = "CHARACTER010";
			characterNameLookup[3] = "CHARACTER024";
			characterNameLookup[4] = "CHARACTER004";
			characterNameLookup[5] = "CHARACTER003";
			characterNameLookup[6] = "CHARACTER005";
			characterNameLookup[7] = "CHARACTER026";
			characterNameLookup[8] = "CHARACTER012";
			characterNameLookup[9] = "CHARACTER015";
			characterNameLookup[10] = "CHARACTER007";
			characterNameLookup[11] = "CHARACTER011";
			characterNameLookup[12] = "CHARACTER013";
			characterNameLookup[13] = "CHARACTER002";
			characterNameLookup[14] = "CHARACTER008";
			characterNameLookup[15] = "CHARACTER016";
			characterNameLookup[16] = "CHARACTER021";
			characterNameLookup[17] = "CHARACTER020";
			characterNameLookup[18] = "CHARACTER030";
			characterNameLookup[19] = "CHARACTER031";
			characterNameLookup[20] = "CHARACTER028";
			characterNameLookup[21] = "CHARACTER014";
			characterNameLookup[22] = "CHARACTER025";
			characterNameLookup[23] = "CHARACTER027";
			characterNameLookup[24] = "CHARACTER002";
			characterNameLookup[25] = "CHARACTER012";
			characterNameLookup[26] = "CHARACTER022";
		}

		public string GetStageName(string localizationKey, params string[] textsToInsert)
		{
			string localizationValueFromSubgroup = localizationLookupTable.GetLocalizationValueFromSubgroup(localizerData.localizationStageNamesName, localizationKey);
			return ReplaceAllTextMarkersWithText(localizationValueFromSubgroup, textsToInsert);
		}

		public string GetStageName(Stage stageID)
		{
			string stageNameKey = GetStageNameKey(stageID);
			if (stageNameKey == null)
			{
				return "";
			}
			string localizationValueFromSubgroup = localizationLookupTable.GetLocalizationValueFromSubgroup(localizerData.localizationStageNamesName, stageNameKey);
			return ReplaceAllTextMarkersWithText(localizationValueFromSubgroup, new string[1] { "" });
		}

		public string GetStageNameKey(Stage stageID)
		{
			return stageNameLookup[(int)(stageID + 1)];
		}

		private void InitStageNameLookupTable()
		{
			stageNameLookup = new string[15];
			stageNameLookup[0] = "S00";
			stageNameLookup[1] = "S00";
			stageNameLookup[2] = "S00";
			stageNameLookup[3] = "S00";
			stageNameLookup[11] = "S00";
			stageNameLookup[5] = "S01";
			stageNameLookup[6] = "S02";
			stageNameLookup[7] = "S03";
			stageNameLookup[8] = "S04";
			stageNameLookup[9] = "S05";
			stageNameLookup[10] = "S06";
			stageNameLookup[12] = "S07";
			stageNameLookup[13] = "S08";
			stageNameLookup[4] = "S02";
		}

		public string GetObjectiveText(Story.ObjectiveID objectiveID)
		{
			string localizationKey = objectiveTextLookup[(int)(objectiveID + 1)];
			return localizationLookupTable.GetLocalizationValueFromSubgroup(localizerData.localizationObjectiveTextName, localizationKey);
		}

		private void InitObjectiveTextLookupTable()
		{
			objectiveTextLookup = new string[20];
			objectiveTextLookup[0] = "O00";
			objectiveTextLookup[1] = "O01";
			objectiveTextLookup[2] = "O02";
			objectiveTextLookup[3] = "O03";
			objectiveTextLookup[4] = "O04";
			objectiveTextLookup[5] = "O05";
			objectiveTextLookup[6] = "O06";
			objectiveTextLookup[7] = "O07";
			objectiveTextLookup[8] = "O08";
			objectiveTextLookup[9] = "O09";
			objectiveTextLookup[10] = "O10";
			objectiveTextLookup[11] = "O11";
			objectiveTextLookup[12] = "O12";
			objectiveTextLookup[13] = "O13";
			objectiveTextLookup[14] = "O14";
			objectiveTextLookup[15] = "O15";
			objectiveTextLookup[16] = "O16";
			objectiveTextLookup[17] = "O17";
			objectiveTextLookup[18] = "O18";
			objectiveTextLookup[19] = "O19";
		}

		public string GetText(string localizationKey)
		{
			return localizationLookupTable.GetLocalizationValue(localizationKey);
		}

		private string ReplaceAllTextMarkersWithText(string localizationValue, string[] textsToInsert)
		{
			return gameTextFormatter.FormatGameText(localizationValue, textsToInsert);
		}

		public LocalizationGroup GetDialogueLocalizationGroup(string sceneName)
		{
			return localizationLookupTable.GetLocalizationGroup(localizerData.localizationDialoguesName).GetSubSetOfGroupByKey(sceneName, sceneName + "_" + localizerData.localizationDialoguesName);
		}

		public LocalizationGroup GetTextLocalizationGroup()
		{
			return localizationLookupTable.GetLocalizationGroup(localizerData.localizationTextName);
		}

		public LocalizationGroup GetLocalizationGroup(string localizationGroupName)
		{
			return localizationLookupTable.GetLocalizationGroup(localizationGroupName);
		}

		public int GetCreditsGroupLength()
		{
			return localizationLookupTable.GetLocalizationGroup(localizerData.localizationCreditsName).GroupSize;
		}

		public string GetCreditsText(string localizationKey, int index)
		{
			string localizationValueFromSubgroup = localizationLookupTable.GetLocalizationValueFromSubgroup(localizerData.localizationCreditsName, localizationKey);
			return (index switch
			{
				0 => localizationValueFromSubgroup.Split(':')[0], 
				1 => localizationValueFromSubgroup.Split(':')[1], 
				2 => localizationValueFromSubgroup.Split(':')[2], 
				3 => localizationValueFromSubgroup.Split(':')[3], 
				_ => localizationValueFromSubgroup.Replace(':', '\n'), 
			}).Replace(';', '\n');
		}

		private IGameTextLocalizer.LanguagesLocalizedOptionText FindAllAvailableLanguages()
		{
			IGameTextLocalizer.LanguagesLocalizedOptionText result = default;
			if (platformLanguages == null)
			{
				result.languagesNames = GetAvailableLanguagesFromStreamingAssets();
				result.languagesApplyText = new string[result.languagesNames.Length];
			}
			else
			{
				List<string> list = new List<string>();
				List<string> list2 = new List<string>();
				SystemLanguage[] availableLanguages = platformLanguages.availableLanguages;
				for (int i = 0; i < availableLanguages.Length; i++)
				{
					list.Add(platformLanguages.availableLanguagesNames[i]);
					list2.Add(platformLanguages.availableLanguagesApplyText[i]);
				}
				result.languagesNames = list.ToArray();
				result.languagesApplyText = list2.ToArray();
			}
			return result;
		}

		private bool IsLanguageAvailable(SystemLanguage systemLanguage)
		{
			bool result = false;
			for (int i = 0; i < platformLanguages.availableLanguages.Length; i++)
			{
				if (platformLanguages.availableLanguages[i] == systemLanguage)
				{
					result = true;
				}
			}
			return result;
		}

		private string[] GetAvailableLanguagesFromStreamingAssets()
		{
			string[] files = Directory.GetFiles(Path.Combine(Application.streamingAssetsPath, Core.Instance.localizerData.localizationSubdirectory), "*");
			List<string> list = new List<string>(files.Length);
			for (int i = 0; i < files.Length; i++)
			{
				string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(files[i]);
				SystemLanguage result = SystemLanguage.Afrikaans;
				if (Enum.TryParse<SystemLanguage>(fileNameWithoutExtension, ignoreCase: false, out result))
				{
					list.Add(fileNameWithoutExtension);
				}
			}
			return list.ToArray();
		}
	}
}
