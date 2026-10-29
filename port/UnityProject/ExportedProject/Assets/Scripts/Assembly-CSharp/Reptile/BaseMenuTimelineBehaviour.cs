using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Reptile
{
	[Serializable]
	public class BaseMenuTimelineBehaviour : PlayableBehaviour
	{
		public enum ShowBarsType
		{
			NoBars = 0,
			OnStartOnly = 1,
			Always = 2
		}

		private const float BLACK_BARS_ANIMATION_DURATION = 0.25f;

		public bool onlyShowOnYesAnswer = true;

		public bool onlyShowOnNoAnswer;

		[Tooltip("Shows ui bars when clip starts. Always means the bars stay after clip ends")]
		public ShowBarsType showBars = ShowBarsType.OnStartOnly;

		protected PlayableDirector director;

		private bool pauseScheduled;

		private SequenceHandler.SkipState skipStateSequence;

		private bool hasStarted;

		~BaseMenuTimelineBehaviour()
		{
			director = null;
		}

		public override void OnPlayableCreate(Playable playable)
		{
			director = playable.GetGraph().GetResolver() as PlayableDirector;
		}

		public override void OnBehaviourPlay(Playable playable, FrameData info)
		{
			if (hasStarted)
			{
				return;
			}
			EffectsUI effects = Core.Instance.UIManager.effects;
			bool answeredYes = SequenceHandler.instance.answeredYes;
			if (answeredYes && onlyShowOnNoAnswer)
			{
				if (showBars == ShowBarsType.OnStartOnly)
				{
					effects.HideBars(0.25f);
				}
				return;
			}
			if (!answeredYes && onlyShowOnYesAnswer)
			{
				if (showBars == ShowBarsType.OnStartOnly)
				{
					effects.HideBars(0.25f);
				}
				return;
			}
			skipStateSequence = SequenceHandler.instance.skipTextActiveState;
			SequenceHandler.instance.skipTextActiveState = SequenceHandler.SkipState.NOT_SKIPPABLE;
			hasStarted = true;
			SequenceHandler.instance.disabledExit = true;
			PauseGraph(director);
			if (showBars != ShowBarsType.NoBars)
			{
				effects.ShowBars(0.25f, 150f);
			}
			StartMenu();
		}

		public virtual void StartMenu()
		{
			ExitMenu();
		}

		public void ExitMenu()
		{
			SequenceHandler.instance.skipTextActiveState = skipStateSequence;
			SequenceHandler.instance.disabledExit = false;
			ResumeGraph(director);
			Core.Instance.UIManager.effects.HideBars(0.25f);
			Core.Instance.UIManager.PopCurrentMenu();
		}

		public override void OnBehaviourPause(Playable playable, FrameData info)
		{
			if (pauseScheduled)
			{
				pauseScheduled = false;
				EffectsUI effects = Core.Instance.UIManager.effects;
				if (showBars == ShowBarsType.OnStartOnly && effects != null)
				{
					effects.HideBars(0.25f);
				}
			}
		}

		private static void PauseGraph(PlayableDirector director)
		{
			FixedFramerateSequence component = director.GetComponent<FixedFramerateSequence>();
			if (component != null)
			{
				component.Pause();
			}
			else
			{
				director.playableGraph.GetRootPlayable(0).SetSpeed(0.0);
			}
		}

		private static void ResumeGraph(PlayableDirector director)
		{
			FixedFramerateSequence component = director.GetComponent<FixedFramerateSequence>();
			if (component != null)
			{
				component.Resume();
			}
			else
			{
				director.playableGraph.GetRootPlayable(0).SetSpeed(1.0);
			}
		}

		public IEnumerator FlickerDelayedButtonPress(MenuTimelineButton clickedButton, UnityAction onEndFlicker)
		{
			float buttonFlickerIterations = 6f;
			float buttonFlickerInterval = 0.5f / buttonFlickerIterations;
			bool enableButton = false;
			float textAlpha = clickedButton.buttonText.alpha;
			for (int i = 0; (float)i < buttonFlickerIterations; i++)
			{
				MenuTimelineButton.EnableText(clickedButton, (float)Convert.ToInt32(enableButton) * textAlpha);
				yield return new WaitForSeconds(buttonFlickerInterval);
				enableButton = !enableButton;
			}
			yield return Core.Instance.UIManager.effects.FadeToBlack(0.4f).WaitForCompletion();
			onEndFlicker();
		}

		public static void LoopAnimationTracks(PlayableDirector director, FrameData info)
		{
			foreach (PlayableBinding output in director.playableAsset.outputs)
			{
				if (!(output.sourceObject is AnimationTrack))
				{
					continue;
				}
				AnimationTrack obj = output.sourceObject as AnimationTrack;
				Animator animator = (Animator)director.GetGenericBinding(output.sourceObject);
				foreach (TimelineClip clip in obj.GetClips())
				{
					AnimationClip animationClip = clip.animationClip;
					if (clip.end >= director.time - (double)info.deltaTime && clip.end <= director.time + (double)info.deltaTime && !clip.hasPostExtrapolation)
					{
						animator.PlayInFixedTime(animationClip.name, 0, (float)(director.time - clip.start + clip.clipIn));
						animator.speed = (float)clip.timeScale;
					}
				}
			}
		}
	}
}
