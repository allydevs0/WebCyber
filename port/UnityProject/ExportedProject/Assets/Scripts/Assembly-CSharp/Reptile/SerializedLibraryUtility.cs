using System;
using System.Collections.Generic;

namespace Reptile
{
	public static class SerializedLibraryUtility
	{
		public static Dictionary<T, U> GenerateCollection<T, U>(IEqualityComparer<T> equalityComparer, Dictionary<string, U> copyFromDictionary) where T : struct
		{
			Dictionary<T, U> dictionary = new Dictionary<T, U>(equalityComparer);
			foreach (KeyValuePair<string, U> item in copyFromDictionary)
			{
				T result = default;
				Enum.TryParse<T>(item.Key, out result);
				dictionary.Add(result, item.Value);
				if (item.Value is ICollectionGenerator)
				{
					(item.Value as ICollectionGenerator).GenerateEnumDictionary();
				}
			}
			return dictionary;
		}
	}
}
