using UnityEngine;

namespace Reptile
{
	public class PlayAudioOnEnable : MonoBehaviour
	{
		[SerializeField]
		private SfxCollectionID sfxCollection = SfxCollectionID.EnvironmentSfx;

		[SerializeField]
		private AudioSource audioSource;

		[SerializeField]
		private AudioClipID audioClipID;

		[SerializeField]
		private bool loop;

		private AudioManager audioManager;

		private AudioManager AudioManager
		{
			get
			{
				if (audioManager == null)
				{
					audioManager = Core.Instance.AudioManager;
				}
				return audioManager;
			}
		}

		private void OnEnable()
		{
			if (audioSource != null && audioClipID != AudioClipID.NONE)
			{
				if (loop)
				{
					AudioManager.PlaySfxGameplayLooping(sfxCollection, audioClipID, audioSource, 0.05f);
				}
				else
				{
					AudioManager.PlaySfxGameplay(sfxCollection, audioClipID, audioSource, 0.05f);
				}
			}
		}

		private void OnDisable()
		{
			if (audioSource != null && audioClipID != AudioClipID.NONE && AudioManager != null && loop)
			{
				AudioManager.StopLoopingSfx(audioSource);
			}
		}
	}
}
