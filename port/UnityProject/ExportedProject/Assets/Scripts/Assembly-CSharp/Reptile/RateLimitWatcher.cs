using System;
using System.Collections.Generic;
using UnityEngine;

namespace Reptile
{
	public class RateLimitWatcher
	{
		private int rateLimit;

		private float timeLimitInSeconds;

		private bool isLimiting;

		private List<DateTime> requestRegister;

		private bool IsWithinRateLimit => requestRegister.Count < rateLimit;

		public RateLimitWatcher(int rateLimit, float timeLimitInSeconds)
		{
			this.rateLimit = rateLimit;
			this.timeLimitInSeconds = timeLimitInSeconds;
			requestRegister = new List<DateTime>();
		}

		public void Dispose()
		{
			Stop();
			requestRegister.Clear();
			requestRegister = null;
		}

		public void Start()
		{
			isLimiting = true;
		}

		public void Stop()
		{
			isLimiting = false;
		}

		public bool CanExecuteRequest(DateTime timeOfRequest)
		{
			UpdateRegister();
			if (!isLimiting)
			{
				return true;
			}
			return AttemptRequest(timeOfRequest);
		}

		private bool AttemptRequest(DateTime timeOfRequest)
		{
			bool isWithinRateLimit = IsWithinRateLimit;
			if (isWithinRateLimit)
			{
				AddToRegister(timeOfRequest);
			}
			return isWithinRateLimit;
		}

		private void UpdateRegister()
		{
			requestRegister.RemoveAll((DateTime dateTime) => IsOutDated(dateTime, DateTime.Now));
		}

		private bool IsOutDated(DateTime timeToCheck, DateTime compareTo)
		{
			return GetDifferenceInSeconds(timeToCheck, compareTo) > timeLimitInSeconds;
		}

		private float GetDifferenceInSeconds(DateTime first, DateTime second)
		{
			return Mathf.Abs((float)(second - first).TotalSeconds);
		}

		private void AddToRegister(DateTime timeOfRequest)
		{
			requestRegister.Add(timeOfRequest);
		}
	}
}
