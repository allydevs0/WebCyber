using System;

namespace Reptile
{
	public class NoOutfitException : Exception
	{
		private Characters character = Characters.NONE;

		private int outfitIndex = -1;

		public NoOutfitException(Characters character, int outfitIndex)
		{
			this.character = character;
			this.outfitIndex = outfitIndex;
		}

		public override string ToString()
		{
			return "Could not set outfit with id: '" + outfitIndex + "' because there is no outfit with this id yet for character: '" + character.ToString() + "'\n" + StackTrace.ToString();
		}
	}
}
