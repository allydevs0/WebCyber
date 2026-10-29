using System.Collections.Generic;

namespace Reptile
{
	public class LocalizationGroup
	{
		private LocalizationKeyValuePair[] localizationKeyValuePairs;

		private string groupName = string.Empty;

		public string GroupName => groupName;

		public int GroupSize => localizationKeyValuePairs.Length;

		public LocalizationGroup(LocalizationKeyValuePair[] localizationKeyValuePairs, string groupName)
		{
			this.localizationKeyValuePairs = localizationKeyValuePairs;
			this.groupName = groupName;
		}

		public LocalizationGroup GetSubSetOfGroupByKey(string keySubSetFilter, string subSetGroupName)
		{
			List<LocalizationKeyValuePair> list = new List<LocalizationKeyValuePair>();
			for (int i = 0; i < localizationKeyValuePairs.Length; i++)
			{
				if (localizationKeyValuePairs[i].key.Contains(keySubSetFilter))
				{
					list.Add(localizationKeyValuePairs[i]);
				}
			}
			return new LocalizationGroup(list.ToArray(), subSetGroupName);
		}

		public string GetLocalizationValue(string localizationKey)
		{
			if (localizationKeyValuePairs == null)
			{
				return localizationKey;
			}
			for (int i = 0; i < localizationKeyValuePairs.Length; i++)
			{
				LocalizationKeyValuePair localizationKeyValuePair = localizationKeyValuePairs[i];
				if (localizationKeyValuePair.key.Equals(localizationKey))
				{
					return localizationKeyValuePair.value;
				}
			}
			return localizationKey;
		}
	}
}
