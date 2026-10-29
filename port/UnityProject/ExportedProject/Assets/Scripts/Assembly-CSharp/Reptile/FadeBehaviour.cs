using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Reptile
{
	[Serializable]
	public class FadeBehaviour : PlayableBehaviour
	{
		[Tooltip("Fade in or out")]
		public bool fadeToBlack = true;

		[Tooltip("Can be used to instantly fade instead of using the duration of the clip")]
		public bool instantFade;

		private PlayableDirector director;

		private bool fadeOnStartHasHappened;

		private bool hasStarted;

		public override void OnPlayableCreate(Playable playable)
		{
			director = playable.GetGraph().GetResolver() as PlayableDirector;
			if ((!fadeToBlack || !instantFade) && (fadeToBlack || instantFade))
			{
				return;
			}
			foreach (PlayableBinding output in director.playableAsset.outputs)
			{
				if (!(output.sourceObject is FadeTrack))
				{
					continue;
				}
				foreach (TimelineClip clip in (output.sourceObject as FadeTrack).GetClips())
				{
					if (clip.start == 0.0)
					{
						FadeBehaviour template = (clip.asset as FadeClip).template;
						if (!fadeOnStartHasHappened && ((template.fadeToBlack && template.instantFade) || (!template.fadeToBlack && !template.instantFade)))
						{
							fadeOnStartHasHappened = true;
							Core.Instance.UIManager.effects.currentFadeColor = EffectsUI.niceBlack;
							Core.Instance.UIManager.effects.FadeToBlack(0f);
						}
					}
				}
			}
		}

		public override void OnBehaviourPlay(Playable playable, FrameData info)
		{
			if (Application.isPlaying && !hasStarted)
			{
				hasStarted = true;
				EffectsUI effects = Core.Instance.UIManager.effects;
				float duration = (float)playable.GetDuration();
				effects.KillFadeTween();
				if (instantFade)
				{
					Color color = EffectsUI.niceBlack * Convert.ToInt32(fadeToBlack);
					effects.ShowBarsOfColor(color, 0f, 0.25f, UpdateType.Manual);
				}
				else if (fadeToBlack)
				{
					effects.FadeToBlack(duration);
				}
				else
				{
					effects.FadeOpen(duration);
				}
			}
		}

		public override void OnBehaviourPause(Playable playable, FrameData info)
		{
			_ = Application.isPlaying;
		}
	}
}
