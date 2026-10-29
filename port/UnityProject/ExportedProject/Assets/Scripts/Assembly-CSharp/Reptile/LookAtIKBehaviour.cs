using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Reptile
{
	[Serializable]
	public class LookAtIKBehaviour : PlayableBehaviour
	{
		public Transform target;

		public float IKWeight = 0.75f;

		private PlayableDirector director;

		public override void OnPlayableCreate(Playable playable)
		{
			director = playable.GetGraph().GetResolver() as PlayableDirector;
		}

		public override void ProcessFrame(Playable playable, FrameData info, object playerData)
		{
		}

		public override void OnBehaviourPlay(Playable playable, FrameData info)
		{
		}

		public override void OnBehaviourPause(Playable playable, FrameData info)
		{
		}
	}
}
