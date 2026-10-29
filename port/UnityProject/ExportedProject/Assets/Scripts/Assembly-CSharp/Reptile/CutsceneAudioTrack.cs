using UnityEngine;

namespace Reptile
{
	public class CutsceneAudioTrack : MonoBehaviour
	{
		[SerializeField]
		private AudioSource source;

		[SerializeField]
		private AudioClip track;

		[SerializeField]
		private FixedFramerateSequence fixedFramerateSequence;

		private void Start()
		{
			fixedFramerateSequence.onPlay.AddListener(Play);
			fixedFramerateSequence.onPause.AddListener(Pause);
			fixedFramerateSequence.onResume.AddListener(Resume);
			fixedFramerateSequence.onStop.AddListener(Stop);
		}

		private void Play()
		{
			source.clip = track;
			source.Play();
		}

		private void Pause()
		{
			source.Pause();
		}

		private void Resume()
		{
			source.Play();
		}

		private void Stop()
		{
			source.Stop();
		}
	}
}
