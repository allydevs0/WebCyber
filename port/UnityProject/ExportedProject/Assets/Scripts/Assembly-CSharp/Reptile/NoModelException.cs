using System;

namespace Reptile
{
	public class NoModelException : Exception
	{
		private Characters character = Characters.NONE;

		public NoModelException(Characters character)
		{
			this.character = character;
		}

		public override string ToString()
		{
			return "Could not find character model for character: " + character.ToString() + "\n" + StackTrace.ToString();
		}
	}
}
