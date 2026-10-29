using System.IO;

namespace Reptile
{
	public abstract class AFileTransaction : ATransaction
	{
		private string fileName = string.Empty;

		private string directoryName = string.Empty;

		private string fullPath = string.Empty;

		private bool isSaveSlotTransaction;

		private bool isLocal;

		private DataSerializerFactory.FormatType formatType;

		public string FileName => fileName;

		public string DirectoryName => directoryName;

		public string FullPath => fullPath;

		public bool IsLocal => isLocal;

		public DataSerializerFactory.FormatType FormatType => formatType;

		public bool IsSaveSlotTransaction => isSaveSlotTransaction;

		public AFileTransaction(string fileName, string directoryName, bool isLocal, DataSerializerFactory.FormatType formatType)
		{
			this.fileName = fileName;
			this.directoryName = directoryName;
			this.formatType = formatType;
			this.isLocal = isLocal;
			fullPath = Path.Combine(directoryName, fileName);
		}

		public override string ToString()
		{
			return $"AFileTransaction info: fileName: {fileName}, directoryName: {directoryName}, fullpath: {fullPath}\n" + base.ToString();
		}

		public void SetTransactionAsSaveSlot()
		{
			isSaveSlotTransaction = true;
		}
	}
}
