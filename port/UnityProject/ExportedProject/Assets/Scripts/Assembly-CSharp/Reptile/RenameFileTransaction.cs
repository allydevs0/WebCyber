using System.IO;

namespace Reptile
{
	public class RenameFileTransaction : ATransaction
	{
		private string newFileName;

		private string oldFileName;

		private string newFilePath;

		private string oldFilePath;

		public string OldFilePath => oldFilePath;

		public string NewFilePath => newFilePath;

		public string NewFileName => newFileName;

		public string OldFileName => oldFileName;

		public RenameFileTransaction(string oldFilePath, string newFilePath)
		{
			this.oldFilePath = oldFilePath;
			this.newFilePath = newFilePath;
			newFileName = Path.GetFileName(newFilePath);
			oldFileName = Path.GetFileName(oldFilePath);
		}

		public override void StartTransaction(AStorage storage)
		{
			storage.PlatformRenameFile(this);
		}
	}
}
