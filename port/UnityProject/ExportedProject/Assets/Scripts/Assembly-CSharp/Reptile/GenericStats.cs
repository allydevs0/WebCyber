using System;
using System.Collections.Generic;

namespace Reptile
{
	public abstract class GenericStats<T, A> where T : struct, IConvertible
	{
		public delegate void OnNotifyCallback(T statEvent, int updatedValue, int deltaValue, A extraInfo);

		public StatTracker[] statTrackers;

		protected List<List<OnNotifyCallback>> observers;

		public abstract void UpdateAdditionalStats(T statEvent, A extraInfo, int value = 1);

		public abstract void InitStatTrackers();

		public abstract int GetEventCount();

		public GenericStats()
		{
			InitStatTrackers();
			observers = new List<List<OnNotifyCallback>>();
			int eventCount = GetEventCount();
			for (int i = 0; i < eventCount; i++)
			{
				observers.Add(new List<OnNotifyCallback>());
			}
		}

		public virtual void Notify(T statEvent, A extraInfo, int value = 1)
		{
			int num = Convert.ToInt32(statEvent);
			int updatedValue = UpdateStatTracker(num, value);
			UpdateAdditionalStats(statEvent, extraInfo, value);
			List<OnNotifyCallback> list = observers[num];
			for (int i = 0; i < list.Count; i++)
			{
				list[i](statEvent, updatedValue, value, extraInfo);
			}
		}

		protected int UpdateStatTracker(int statEventIndex, int value)
		{
			return statTrackers[statEventIndex].UpdateValue(value);
		}

		public int RetrieveStatValue(T statEvent)
		{
			int num = Convert.ToInt32(statEvent);
			return statTrackers[num].ivalue;
		}

		public virtual void RegisterEvent(T statEvent, OnNotifyCallback onNotifyCallback)
		{
			int index = Convert.ToInt32(statEvent);
			List<OnNotifyCallback> list = observers[index];
			if (!list.Contains(onNotifyCallback))
			{
				list.Add(onNotifyCallback);
			}
		}

		public void UnregisterToEvent(T statEvent, OnNotifyCallback onNotifyCallback)
		{
			int index = Convert.ToInt32(statEvent);
			List<OnNotifyCallback> list = observers[index];
			if (list.Contains(onNotifyCallback))
			{
				list.Remove(onNotifyCallback);
			}
		}

		public void UnregisterAll()
		{
			if (observers != null)
			{
				int count = observers.Count;
				for (int i = 0; i < count; i++)
				{
					observers[i].Clear();
				}
				observers.Clear();
			}
		}
	}
}
