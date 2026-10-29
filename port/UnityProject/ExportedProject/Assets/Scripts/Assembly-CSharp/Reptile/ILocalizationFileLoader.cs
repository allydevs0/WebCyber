namespace Reptile
{
	public interface ILocalizationFileLoader
	{
		ILocalizationFileReader Load();

		void Dispose();
	}
}
