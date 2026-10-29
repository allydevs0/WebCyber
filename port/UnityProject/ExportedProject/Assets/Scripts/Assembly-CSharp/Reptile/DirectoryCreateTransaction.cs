using System.Text;

namespace Reptile
{
	public class DirectoryCreateTransaction : ATransaction
	{
		private string directoryName = string.Empty;

		private bool isLocal;

		public string DirectoryName => directoryName;

		public bool IsLocal => isLocal;

		public DirectoryCreateTransaction(string directoryName, bool isLocal)
		{
			this.directoryName = directoryName;
			this.isLocal = isLocal;
		}

		public override void StartTransaction(AStorage storage)
		{
			storage.PlatformCreateDirectory(this);
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("DirectoryCreateTransaction for directory: " + directoryName);
			return stringBuilder.ToString();
		}
	}
}
