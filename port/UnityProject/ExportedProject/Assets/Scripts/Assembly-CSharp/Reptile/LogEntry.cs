using System;
using UnityEngine;

namespace Reptile
{
	public class LogEntry
	{
		private string text = string.Empty;

		private string stackTrace = string.Empty;

		private LogType logType = LogType.Log;

		private TimeSpan timeStamp = TimeSpan.Zero;

		public string Text => text;

		public string StackTrace => stackTrace;

		public LogType LogType => logType;

		public TimeSpan TimeStamp => timeStamp;

		public LogEntry(string text, string stackTrace, LogType logType)
		{
			timeStamp = TimeSpan.FromSeconds(Time.time);
			this.text = text;
			this.stackTrace = stackTrace;
			this.logType = logType;
		}
	}
}
