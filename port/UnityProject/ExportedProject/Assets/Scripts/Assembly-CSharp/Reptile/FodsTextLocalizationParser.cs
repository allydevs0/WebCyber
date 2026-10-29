using System;
using System.Xml;
using UnityEngine;

namespace Reptile
{
	public class FodsTextLocalizationParser : AFodsLocalizationFileParser
	{
		private const int TRANSLATION_COLUMN_INDEX = 1;

		public FodsTextLocalizationParser(XmlDocument xmlDocument, string fodsTableName)
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
			string localizationValue = GetLocalizationValue(rowIndex, 1);
			return CreateLocalizationKeyValuePair(localizationKeyToRowEntries[keyIndex].localizationKey, localizationValue);
		}
	}
}
