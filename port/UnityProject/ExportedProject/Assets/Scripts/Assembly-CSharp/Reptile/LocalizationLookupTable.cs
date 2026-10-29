using System;
using System.Collections.Generic;

namespace Reptile
{
	public class LocalizationLookupTable : IDisposable
	{
		private Dictionary<string, LocalizationGroup> localizationGroupsDict;

		public int AmountOfLocalizationGroups => localizationGroupsDict.Count;

		public LocalizationLookupTable(LocalizationGroup[] localizationGroups)
		{
			if (localizationGroups == null)
			{
				throw new Exception("No localization groups are provided, was the localization file provided correctly?");
			}
			CreateDictionary(localizationGroups);
		}

		public void Dispose()
		{
			localizationGroupsDict = null;
		}

		private void CreateDictionary(LocalizationGroup[] localizationGroups)
		{
			localizationGroupsDict = new Dictionary<string, LocalizationGroup>();
			for (int i = 0; i < localizationGroups.Length; i++)
			{
				localizationGroupsDict.Add(localizationGroups[i].GroupName, localizationGroups[i]);
			}
		}

		public string GetLocalizationValueFromSubgroup(string groupName, string localizationKey)
		{
			LocalizationGroup localizationGroup = GetLocalizationGroup(groupName);
			return GetLocalizationValueFromGroup(localizationGroup, localizationKey);
		}

		public LocalizationGroup GetLocalizationGroup(string groupName)
		{
			localizationGroupsDict.TryGetValue(groupName, out var value);
			return value;
		}

		public string GetLocalizationValue(string localizationKey)
		{
			string text = localizationKey;
			foreach (KeyValuePair<string, LocalizationGroup> item in localizationGroupsDict)
			{
				text = GetLocalizationValueFromGroup(item.Value, localizationKey);
				if (!text.Equals(string.Empty))
				{
					break;
				}
			}
			return text;
		}

		private string GetLocalizationValueFromGroup(LocalizationGroup localizationGroup, string localizationKey)
		{
			if (localizationGroup == null)
			{
				return localizationKey;
			}
			return localizationGroup.GetLocalizationValue(localizationKey);
		}
	}
}
