using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Reptile
{
	[Serializable]
	public class BlackBarBehaviour : PlayableBehaviour
	{
		[Tooltip("Height of black bars.")]
		public float barHeight = 150f;

		[Tooltip("Duration of fade in")]
		public float fadeInTime = 0.25f;

		[Tooltip("Duration of fade out")]
		public float fadeOutTime = 0.25f;

		public override void OnBehaviourPlay(Playable playable, FrameData info)
		{
			if (Application.isPlaying)
			{
				EffectsUI effects = Core.Instance.UIManager.effects;
				if (effects != null)
				{
					effects.ShowBars(fadeInTime, barHeight);
				}
			}
		}

		public override void OnBehaviourPause(Playable playable, FrameData info)
		{
			if (Application.isPlaying)
			{
				EffectsUI effects = Core.Instance.UIManager.effects;
				if (effects != null)
				{
					effects.HideBars(fadeOutTime);
				}
			}
		}
	}
}
