using System.Collections.Generic;
using UnityEngine;

namespace Reptile
{
	public class Logger
	{
		public delegate void OnLogEntriesAddedDelegate();

		private Buffer<LogEntry> textBuffer;

		private bool hasNewLogMessages;

		public int CurrentLogSize => textBuffer.BufferSize;

		public static event OnLogEntriesAddedDelegate OnLogEntriesAdded;

		public Logger(int maxBufferSize)
		{
			textBuffer = new Buffer<LogEntry>(maxBufferSize);
			Application.logMessageReceived += OnLogMessageReceived;
			Core.OnLateUpdate += OnLateUpdate;
		}

		public void OnLateUpdate()
		{
			if (hasNewLogMessages)
			{
				SendBufferCacheToListeners();
			}
		}

		private void SendBufferCacheToListeners()
		{
			if (OnLogEntriesAdded != null)
			{
				OnLogEntriesAdded();
			}
			hasNewLogMessages = false;
		}

		private void OnLogMessageReceived(string logString, string stackTrace, LogType type)
		{
			HandleNewLogReceived(logString, stackTrace, type);
		}

		private void HandleNewLogReceived(string logString, string stackTrace, LogType type)
		{
			LogEntry logEntry = new LogEntry(logString, stackTrace, type);
			LogTextEntry(logEntry);
		}

		public void Dispose()
		{
			textBuffer.Dispose();
			textBuffer = null;
			Core.OnLateUpdate -= OnLateUpdate;
			Application.logMessageReceived -= OnLogMessageReceived;
		}

		public void LogTextEntry(LogEntry logEntry)
		{
			hasNewLogMessages = true;
		}

		public bool GetFromTextBuffer(int index, out LogEntry logEntry)
		{
			logEntry = null;
			if (index < textBuffer.BufferSize)
			{
				logEntry = textBuffer.GetElementFromTextBuffer(index);
				return true;
			}
			return false;
		}

		public LogEntry[] GetLastEntriesFromBuffer(int secondsFromLastEntry)
		{
			List<LogEntry> list = new List<LogEntry>();
			double num = DetermineTimeLimit(secondsFromLastEntry);
			for (int num2 = textBuffer.BufferSize - 1; num2 >= 0; num2--)
			{
				LogEntry elementFromTextBuffer = textBuffer.GetElementFromTextBuffer(num2);
				if (!(elementFromTextBuffer.TimeStamp.TotalSeconds >= num))
				{
					break;
				}
				list.Add(elementFromTextBuffer);
			}
			return list.ToArray();
		}

		private double DetermineTimeLimit(int secondsFromLastEntry)
		{
			if (textBuffer.BufferSize <= 0)
			{
				return 0.0;
			}
			double num = textBuffer.GetElementFromTextBuffer(textBuffer.BufferSize - 1).TimeStamp.TotalSeconds - (double)secondsFromLastEntry;
			if (num >= 0.0)
			{
				return num;
			}
			return 0.0;
		}
	}
}
