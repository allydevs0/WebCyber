using UnityEngine;

namespace Reptile
{
	public class AudioAmbienceTrigger : MonoBehaviour
	{
		public const SfxCollectionID sfxCollection = SfxCollectionID.AmbienceSfx;

		[AudioClipFilter("sfxCollection")]
		public AudioClipID from;

		[AudioClipFilter("sfxCollection")]
		public AudioClipID to;
	}
}
