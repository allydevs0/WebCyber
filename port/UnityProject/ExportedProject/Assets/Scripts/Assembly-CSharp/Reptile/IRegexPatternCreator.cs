namespace Reptile
{
	public interface IRegexPatternCreator
	{
		string CreateRegexPattern(params string[] symbols);
	}
}
