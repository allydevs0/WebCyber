using System;
using System.Collections.Generic;
using UnityEngine;

namespace Reptile
{
	[Serializable]
	public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
	{
		[SerializeField]
		private List<TKey> keys = new List<TKey>();

		[SerializeField]
		private List<TValue> values = new List<TValue>();

		public void OnBeforeSerialize()
		{
			keys.Clear();
			values.Clear();
			using Enumerator enumerator = GetEnumerator();
			while (enumerator.MoveNext())
			{
				KeyValuePair<TKey, TValue> current = enumerator.Current;
				keys.Add(current.Key);
				values.Add(current.Value);
			}
		}

		public void OnAfterDeserialize()
		{
			Clear();
			if (keys.Count != values.Count)
			{
				throw new Exception("Could not properly deserialize dictionary because there are not enough keys for values or visa versa.\nKeys: " + keys.Count + ", values: " + values.Count);
			}
			for (int i = 0; i < keys.Count; i++)
			{
				Add(keys[i], values[i]);
			}
		}
	}
}
