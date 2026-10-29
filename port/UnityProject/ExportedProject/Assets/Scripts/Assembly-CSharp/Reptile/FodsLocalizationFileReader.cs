using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

namespace Reptile
{
	public class FodsLocalizationFileReader : ILocalizationFileReader, IDisposable
	{
		private XmlDocument xmlDocument;

		private AFodsLocalizationFileParser[] fodsLocalizationFileParsers;

		public FodsLocalizationFileReader(XmlDocument xmlDocument, LocalizationData localizerData)
		{
			this.xmlDocument = xmlDocument;
			CreateParsers(localizerData);
		}

		private void CreateParsers(LocalizationData localizerData)
		{
			fodsLocalizationFileParsers = new AFodsLocalizationFileParser[10]
			{
				new FodsTextLocalizationParser(xmlDocument, localizerData.localizationTextName),
				new FodsTextSizeLocalizationParser(xmlDocument, localizerData.localizationButtonSizeName),
				new FodsDialogueParser(xmlDocument, localizerData.localizationDialoguesName),
				new FodsCreditsParser(xmlDocument, localizerData.localizationCreditsName),
				new FodsCharacterNamesParser(xmlDocument, localizerData.localizationCharacterNamesName),
				new FodsStageNamesParser(xmlDocument, localizerData.localizationStageNamesName),
				new FodsObjectiveTextParser(xmlDocument, localizerData.localizationObjectiveTextName),
				new FodsTextLocalizationParser(xmlDocument, localizerData.localizationEmailTextName),
				new FodsTextLocalizationParser(xmlDocument, localizerData.localizationNotificationTextName),
				new FodsTextLocalizationParser(xmlDocument, localizerData.localizationSkinTextName)
			};
		}

		public void Dispose()
		{
			xmlDocument = null;
			DisposeParsers();
		}

		private void DisposeParsers()
		{
			for (int i = 0; i < fodsLocalizationFileParsers.Length; i++)
			{
				fodsLocalizationFileParsers[i].Dispose();
				fodsLocalizationFileParsers[i] = null;
			}
			fodsLocalizationFileParsers = null;
		}

		public LocalizationGroup[] GetAllLocalizationGroups(SystemLanguage systemLanguage)
		{
			List<LocalizationGroup> list = new List<LocalizationGroup>();
			for (int i = 0; i < fodsLocalizationFileParsers.Length; i++)
			{
				LocalizationGroup item = new LocalizationGroup(GetLocalizationPairsFromParser(fodsLocalizationFileParsers[i], systemLanguage), fodsLocalizationFileParsers[i].Name);
				list.Add(item);
			}
			return list.ToArray();
		}

		private LocalizationKeyValuePair[] GetLocalizationPairsFromParser(AFodsLocalizationFileParser fodsLocalizationParser, SystemLanguage systemLanguage)
		{
			List<LocalizationKeyValuePair> list = new List<LocalizationKeyValuePair>();
			for (int i = 0; i < fodsLocalizationParser.AmountOfLocalizationKeys; i++)
			{
				LocalizationKeyValuePair keyValueForLanguage = fodsLocalizationParser.GetKeyValueForLanguage(i, systemLanguage);
				list.Add(keyValueForLanguage);
			}
			return list.ToArray();
		}
	}
}
