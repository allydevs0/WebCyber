using Steamworks;

namespace Reptile
{
	public struct FileWriteASyncCommand : ISteamCommand
	{
		public string fileName;

		public byte[] buffer;

		private bool isExecuted;

		private bool isDone;

		public bool ioFailure;

		public RemoteStorageFileWriteAsyncComplete_t callback;

		public bool IsDone
		{
			get
			{
				return isDone;
			}
			set
			{
				isDone = value;
			}
		}

		public bool IsExecuted
		{
			get
			{
				return isExecuted;
			}
			set
			{
				isExecuted = value;
			}
		}

		public bool IsValid
		{
			get
			{
				if (fileName != null)
				{
					return buffer != null;
				}
				return false;
			}
		}
	}
}
