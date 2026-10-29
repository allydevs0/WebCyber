using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Reptile
{
	[Serializable]
	public class CameraShakeBehaviour : PlayableBehaviour
	{
		public ScreenShakeType curShakeType;

		public bool animatedCamera;

		public Vector3 shakeDir;

		private CameraShakeComponent cameraShakeComponent;

		private bool startedShaking;

		private PlayableDirector director;

		public override void OnPlayableCreate(Playable playable)
		{
			director = playable.GetGraph().GetResolver() as PlayableDirector;
		}

		public override void ProcessFrame(Playable playable, FrameData info, object playerData)
		{
			cameraShakeComponent = playerData as CameraShakeComponent;
			if (!(cameraShakeComponent == null))
			{
				if (!startedShaking)
				{
					cameraShakeComponent.StartShake(curShakeType, shakeDir, animatedCamera, (float)playable.GetDuration());
					startedShaking = true;
				}
				cameraShakeComponent.SetTime((float)playable.GetTime());
			}
		}

		public override void OnBehaviourPlay(Playable playable, FrameData info)
		{
			startedShaking = false;
		}

		public override void OnBehaviourPause(Playable playable, FrameData info)
		{
			if (!(cameraShakeComponent == null))
			{
				startedShaking = false;
				cameraShakeComponent.StopShake();
			}
		}
	}
}
