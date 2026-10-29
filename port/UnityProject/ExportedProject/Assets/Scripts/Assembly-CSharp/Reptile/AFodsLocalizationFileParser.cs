using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

namespace Reptile
{
	public abstract class AFodsLocalizationFileParser : ILocalizationFileParser, IDisposable
	{
		public struct LocalizationKeyToRowEntry
		{
			public string localizationKey;

			public int rowIndex;
		}

		private const int DESCRIPTION_ROW_INDEX = 0;

		private const int LOCALIZATION_KEYS_COLUMN_INDEX = 0;

		private XmlDocument xmlDocument;

		protected FodsTable fodsTable;

		protected string fodsTableName = string.Empty;

		private int amountOfLocalizationKeys;

		protected LocalizationKeyToRowEntry[] localizationKeyToRowEntries;

		public int AmountOfLocalizationKeys => amountOfLocalizationKeys;

		public string Name => fodsTableName;

		public AFodsLocalizationFileParser(XmlDocument xmlDocument, string fodsTableName)
		{
			this.xmlDocument = xmlDocument;
			this.fodsTableName = fodsTableName;
			CreateFodsTable();
			GenerateLocalizationKeyToRowEntries();
		}

		public void Dispose()
		{
			fodsTable = null;
		}

		private void CreateFodsTable()
		{
			XmlNode xmlNode = RetrieveXmlTableNodeByName();
			if (xmlNode == null)
			{
				throw new Exception("Could not find table in .fods file with name: " + fodsTableName);
			}
			fodsTable = new FodsTable(xmlNode);
		}

		private XmlNode RetrieveXmlTableNodeByName()
		{
			if (fodsTableName.Equals(string.Empty))
			{
				return XmlHelper.FindXmlNodeByNameRecursive(xmlDocument, "table:table");
			}
			return XmlHelper.FindXmlNodeByNameWithAttributeValue(xmlDocument, "table:table", "table:name", fodsTableName);
		}

		private void GenerateLocalizationKeyToRowEntries()
		{
			string[] allCellTextValuesForColumn = fodsTable.GetAllCellTextValuesForColumn(0);
			localizationKeyToRowEntries = CreateLocalizationKeyToRowEntries(allCellTextValuesForColumn);
			amountOfLocalizationKeys = localizationKeyToRowEntries.Length;
		}

		private LocalizationKeyToRowEntry[] CreateLocalizationKeyToRowEntries(string[] localizationColumnTextValues)
		{
			List<LocalizationKeyToRowEntry> list = new List<LocalizationKeyToRowEntry>();
			for (int i = 0; i < localizationColumnTextValues.Length; i++)
			{
				if (!IsRowDescriptor(i) && IsValidLocalizationKey(localizationColumnTextValues[i]))
				{
					AddEntryToLocalizationKeyToRowList(list, localizationColumnTextValues[i], i);
				}
			}
			return list.ToArray();
		}

		private bool IsRowDescriptor(int index)
		{
			return index == 0;
		}

		private bool IsValidLocalizationKey(string textValue)
		{
			bool num = textValue.Equals(string.Empty);
			bool flag = textValue.Contains("/");
			if (!num)
			{
				return !flag;
			}
			return false;
		}

		private void AddEntryToLocalizationKeyToRowList(List<LocalizationKeyToRowEntry> localizationKeyToRowEntryList, string localizationKey, int rowIndex)
		{
			LocalizationKeyToRowEntry item = CreateLocalizationKeyToRowEntry(localizationKey, rowIndex);
			localizationKeyToRowEntryList.Add(item);
		}

		private LocalizationKeyToRowEntry CreateLocalizationKeyToRowEntry(string localizationKey, int rowIndex)
		{
			LocalizationKeyToRowEntry result = default;
			result.localizationKey = localizationKey;
			result.rowIndex = rowIndex;
			return result;
		}

		protected LocalizationKeyValuePair CreateLocalizationKeyValuePair(string localizationKey, string localizationValue)
		{
			LocalizationKeyValuePair result = default;
			result.key = localizationKey;
			result.value = localizationValue;
			return result;
		}

		protected string GetLocalizationValue(int rowIndex, int columnIndex)
		{
			return fodsTable.GetTextValueForCell(rowIndex, columnIndex);
		}

		public abstract LocalizationKeyValuePair GetKeyValueForLanguage(int keyIndex, SystemLanguage systemLanguage);
	}
}
