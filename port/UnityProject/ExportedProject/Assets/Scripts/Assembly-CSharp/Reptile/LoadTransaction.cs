namespace Reptile
{
	public class LoadTransaction<T> : AFileTransaction where T : class
	{
		private T data;

		public T Data => data;

		public LoadTransaction(T data, string fileName, string directoryName, bool isLocal, DataSerializerFactory.FormatType formatType)
			: base(fileName, directoryName, isLocal, formatType)
		{
			this.data = data;
		}

		public override void StartTransaction(AStorage storage)
		{
			storage.PlatformLoadData(this);
		}

		public override string ToString()
		{
			return "LoadTransaction\n" + base.ToString();
		}
	}
}
