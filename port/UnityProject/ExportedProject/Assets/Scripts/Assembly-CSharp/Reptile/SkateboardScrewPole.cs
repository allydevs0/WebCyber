using UnityEngine;

namespace Reptile
{
	public class SkateboardScrewPole : MonoBehaviour
	{
		internal bool reset;

		public Transform point;

		public Transform maxPoint;

		public Transform pole;

		public AudioSource audioSource;

		private Vector3 poleStartPos;

		private Quaternion startRot;

		private AudioManager audioManager;

		public void Awake()
		{
			poleStartPos = pole.position;
			startRot = pole.rotation;
			audioManager = Core.Instance.AudioManager;
		}

		public void Update()
		{
			if (reset)
			{
				Move(-1f);
			}
		}

		public void Move(float distance)
		{
			if (distance == 0f)
			{
				audioManager.StopLoopingSfx(audioSource);
			}
			else if (distance < 0f)
			{
				if (pole.position.y <= poleStartPos.y)
				{
					pole.position = poleStartPos;
					pole.rotation = startRot;
					reset = false;
					audioManager.StopLoopingSfx(audioSource);
					return;
				}
				pole.Rotate(new Vector3(0f, distance * -800f * Core.dt, 0f));
				pole.position += Vector3.up * distance * Core.dt;
				if (!audioSource.isPlaying)
				{
					audioManager.PlaySfxUILooping(SfxCollectionID.EnvironmentSfx, AudioClipID.ScrewPoleMoving, audioSource);
				}
			}
			else if (pole.position.y < maxPoint.position.y)
			{
				pole.Rotate(new Vector3(0f, distance * -800f * Core.dt, 0f));
				pole.position += Vector3.up * distance * Core.dt;
				if (!audioSource.isPlaying)
				{
					audioManager.PlaySfxUILooping(SfxCollectionID.EnvironmentSfx, AudioClipID.ScrewPoleMoving, audioSource);
				}
			}
		}

		public void StopPlayingSound()
		{
			audioManager.StopLoopingSfx(audioSource);
		}
	}
}
