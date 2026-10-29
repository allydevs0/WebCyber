using UnityEngine;

namespace Reptile
{
	[CreateAssetMenu(menuName = "Reptile/Phone/Music/New Track")]
	public class MusicTrack : AUnlockable
	{
		public AudioClip AudioClip;

		public string Title = string.Empty;

		public string Artist = string.Empty;

		public bool isRepeatable;
	}
}
