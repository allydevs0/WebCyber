using System.Text;

namespace Reptile
{
	public class DirectoryInfoTransaction : ATransaction
	{
		private string baseDirectory = string.Empty;

		private string[] directoryFileNames;

		private string subDirectory = string.Empty;

		private bool isSaveSlotTransaction;

		public string BaseDirectory => baseDirectory;

		public string[] DirectoryFileNames
		{
			get
			{
				return directoryFileNames;
			}
			set
			{
				directoryFileNames = value;
			}
		}

		public string SubDirectory => subDirectory;

		public bool IsSaveSlotTransaction => isSaveSlotTransaction;

		public DirectoryInfoTransaction(string baseDirectory, string subDirectory)
		{
			this.baseDirectory = baseDirectory;
			this.subDirectory = subDirectory;
		}

		public override void StartTransaction(AStorage storage)
		{
			storage.PlatformRetrieveDirectoryInfo(this);
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("DirectoryInfoTransaction for directory: " + baseDirectory);
			stringBuilder.AppendLine("DirectoryInfoTransaction for subdirectory: " + subDirectory);
			if (directoryFileNames != null)
			{
				for (int i = 0; i < directoryFileNames.Length; i++)
				{
					stringBuilder.AppendLine("Diretory File name: " + directoryFileNames[i]);
				}
			}
			return stringBuilder.ToString();
		}

		public void SetSaveSlotTransaction()
		{
			isSaveSlotTransaction = true;
		}
	}
}
