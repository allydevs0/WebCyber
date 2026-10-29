using System;

namespace Reptile
{
	public interface IGameTextFormatter : IDisposable
	{
		string FormatGameText(string textToFormat, string[] textsToInsert);
	}
}
