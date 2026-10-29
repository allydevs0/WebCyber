namespace Reptile
{
	public class DeleteTransaction : AFileTransaction
	{
		public DeleteTransaction(string fileName, string directoryName, bool isLocal)
			: base(fileName, directoryName, isLocal, DataSerializerFactory.FormatType.NONE)
		{
		}

		public override void StartTransaction(AStorage storage)
		{
			storage.PlatformDeleteData(this);
		}

		public override string ToString()
		{
			return "DeleteTransaction\n" + base.ToString();
		}
	}
}
