namespace Reptile
{
	public static class DataSerializerFactory
	{
		public enum FormatType
		{
			NONE = 0,
			RAW_DATA = 1,
			COMPRESSED_GAME_DATA = 2
		}

		public static IDataSerializer CreateFormatSerializer(FormatType formatType)
		{
			return formatType switch
			{
				FormatType.RAW_DATA => new RawDataSerializer(), 
				FormatType.COMPRESSED_GAME_DATA => new CompressedGameSaveDataSerializer(), 
				_ => null, 
			};
		}
	}
}
