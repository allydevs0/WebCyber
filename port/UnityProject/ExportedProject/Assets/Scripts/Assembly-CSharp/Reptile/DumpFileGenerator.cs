using System;
using System.IO;
using UnityEngine;

namespace Reptile
{
	public class DumpFileGenerator
	{
		private const string fileName = "BRCCrashReport.txt";

		private string filePath = string.Empty;

		private int lastLogEntriesTimeInSeconds;

		public DumpFileGenerator(string directory, int lastLogEntriesTimeInSeconds)
		{
			filePath = Path.Combine(directory, "BRCCrashReport.txt");
			this.lastLogEntriesTimeInSeconds = lastLogEntriesTimeInSeconds;
		}

		public void Generate(Exception exception)
		{
			RemoveOldDumpFile();
			CreateDumpFile(exception);
		}

		private void RemoveOldDumpFile()
		{
			if (File.Exists(filePath))
			{
				File.Delete(filePath);
			}
		}

		private void CreateDumpFile(Exception exception)
		{
			using StreamWriter streamWriter = new StreamWriter(new FileStream(filePath, FileMode.Create));
			WriteDate(streamWriter);
			WriteGameVersionToStream(streamWriter);
			WritePlatformToStream(streamWriter);
			WriteEmptyLine(streamWriter);
			WriteDivider(streamWriter);
			WriteEmptyLine(streamWriter);
			WriteExceptionToStream(streamWriter, exception);
			WriteEmptyLine(streamWriter);
			WriteDivider(streamWriter);
			WriteEmptyLine(streamWriter);
			WriteLogToStream(streamWriter);
			WriteEmptyLine(streamWriter);
			WriteDivider(streamWriter);
			WriteEmptyLine(streamWriter);
		}

		private void WriteDate(StreamWriter streamWriter)
		{
			streamWriter.WriteLine("Generated at: " + DateTime.Now.ToString("G"));
		}

		private void WriteEmptyLine(StreamWriter streamWriter)
		{
			streamWriter.WriteLine(string.Empty);
		}

		private void WriteDivider(StreamWriter streamWriter)
		{
			streamWriter.WriteLine("##################################################");
		}

		private void WriteGameVersionToStream(StreamWriter streamWriter)
		{
			GameVersion gameVersion = Core.Instance.GameVersion;
			streamWriter.WriteLine("Game version: " + gameVersion.ToString());
		}

		private void WritePlatformToStream(StreamWriter streamWriter)
		{
			streamWriter.WriteLine("Platform: " + Application.platform);
		}

		private void WriteExceptionToStream(StreamWriter streamWriter, Exception exception)
		{
			streamWriter.WriteLine(exception.ToString());
		}

		private void WriteLogToStream(StreamWriter streamWriter)
		{
			LogEntry[] lastEntriesFromBuffer = Core.Instance.Logger.GetLastEntriesFromBuffer(lastLogEntriesTimeInSeconds);
			streamWriter.WriteLine("Latest log entries for the last " + lastLogEntriesTimeInSeconds + " seconds: ");
			WriteEmptyLine(streamWriter);
			WriteLogEntriesToStream(streamWriter, lastEntriesFromBuffer);
		}

		private void WriteLogEntriesToStream(StreamWriter streamWriter, LogEntry[] logEntries)
		{
			for (int i = 0; i < logEntries.Length; i++)
			{
				streamWriter.WriteLine(CreateReadableTimeStamp(logEntries[i].TimeStamp) + logEntries[i].Text);
			}
		}

		private string CreateReadableTimeStamp(TimeSpan timeStamp)
		{
			return "[" + timeStamp.ToString("hh\\:mm\\:ss\\.ffff") + "]: ";
		}
	}
}
