using UnityEngine;

namespace Reptile
{
	[CreateAssetMenu(fileName = "MusicBundle", menuName = "Reptile/Audio/Music/Music Bundle")]
	public class MusicBundle : ScriptableObject
	{
		[SerializeField]
		public AudioClip[] musicClips;
	}
}
