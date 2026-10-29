using System;

namespace Reptile
{
	[Serializable]
	public enum AudioChannelID
	{
		NONE = -1,
		Master = 0,
		Sfx = 1,
		UI = 2,
		Gameplay = 3,
		Music = 4,
		Ambience = 6,
		Voices = 5,
		MAX = 7
	}
}
