using UnityEngine;

namespace Reptile
{
	[RequireComponent(typeof(AudioSource))]
	public class AutoPlayLoop : MonoBehaviour
	{
		public SfxClip sfxToPlay;

		private void OnEnable()
		{
			AudioSource component = GetComponent<AudioSource>();
			Core.Instance.AudioManager.PlaySfxGameplayLooping(sfxToPlay, component);
		}
	}
}
