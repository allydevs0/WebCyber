using System;
using System.Xml;
using UnityEngine;

namespace Reptile
{
	public class FodsTextSizeLocalizationParser : AFodsLocalizationFileParser
	{
		private const int TRANSLATION_COLUMN_INDEX = 1;

		public FodsTextSizeLocalizationParser(XmlDocument xmlDocument, string fodsTableName)
			: base(xmlDocument, fodsTableName)
		{
		}

		public override LocalizationKeyValuePair GetKeyValueForLanguage(int keyIndex, SystemLanguage systemLanguage)
		{
			int rowIndex = localizationKeyToRowEntries[keyIndex].rowIndex;
			if (rowIndex < 0)
			{
				throw new Exception("No valid key index was provided");
			}
			string localizationKey = localizationKeyToRowEntries[keyIndex].localizationKey;
			string localizationValue = GetLocalizationValue(rowIndex, 1);
			return CreateLocalizationKeyValuePair(localizationKey, localizationValue);
		}
	}
}
