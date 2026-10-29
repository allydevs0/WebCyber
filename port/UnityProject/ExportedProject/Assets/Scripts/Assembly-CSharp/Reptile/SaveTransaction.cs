namespace Reptile
{
	public class SaveTransaction<T> : AFileTransaction where T : class
	{
		private T data;

		private bool isBackupTransaction;

		public T Data => data;

		public bool IsBackupTransaction => isBackupTransaction;

		public SaveTransaction(T data, string fileName, string directoryName, bool isLocal, DataSerializerFactory.FormatType formatType)
			: base(fileName, directoryName, isLocal, formatType)
		{
			this.data = data;
		}

		public override void StartTransaction(AStorage storage)
		{
			storage.PlatformSaveData(this);
		}

		public override string ToString()
		{
			return "SaveTransaction\n" + base.ToString();
		}

		public void SetTransactionAsBackup()
		{
			isBackupTransaction = true;
		}
	}
}
