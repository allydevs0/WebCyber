using UnityEngine;

namespace Reptile
{
	public class AudioAmbienceZone : MonoBehaviour
	{
		public const SfxCollectionID sfxCollection = SfxCollectionID.AmbienceSfx;

		[AudioClipFilter("sfxCollection")]
		public AudioClipID ambienceClip;

		[Range(1f, 5f)]
		public int priority = 1;

		public bool noneIsSilence;
	}
}
