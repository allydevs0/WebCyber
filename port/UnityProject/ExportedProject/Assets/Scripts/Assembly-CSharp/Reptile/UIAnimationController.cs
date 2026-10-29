using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace Reptile
{
	public class UIAnimationController : MonoBehaviour
	{
		public delegate void AnimationStartedDelegate();

		public const string ANIMATION_TWEEN_ID = "Animation_";

		public Menu useFromMenu;

		private bool animationsInitialized;

		private bool isPlayingAnimation;

		private DOTweenAnimation longestForwardAnimation;

		private DOTweenAnimation longestBackwardAnimation;

		private DOTweenAnimation[] animations;

		public bool IsPlayingAnimation => isPlayingAnimation;

		public event AnimationStartedDelegate OnAnimationStarted;

		public event AnimationStartedDelegate OnAnimationEnded;

		public void PlayAnimation()
		{
			if (animations == null || animations.Length == 0)
			{
				TriggerAnimationEndedEvent();
			}
			else if (!animationsInitialized)
			{
				StartCoroutine(PlayAnimationOnInitialized());
			}
			else
			{
				PlayAllAnimationsForward();
			}
		}

		public void PlayAnimationBackward()
		{
			if (animations == null || animations.Length == 0)
			{
				TriggerAnimationEndedEvent();
			}
			else if (!animationsInitialized)
			{
				StartCoroutine(PlayAnimationBackwardOnInitialized());
			}
			else
			{
				PlayAllAnimationsBackward();
			}
		}

		public void RecreateAllTweens()
		{
			for (int i = 0; i < animations.Length; i++)
			{
				animations[i].DOKill();
				animations[i].CreateTween();
			}
		}

		public void CompleteAllTweens()
		{
			for (int i = 0; i < animations.Length; i++)
			{
				animations[i].tween.Goto(animations[i].duration, andPlay: true);
			}
		}

		private void Awake()
		{
			animations = GetComponentsInChildren<DOTweenAnimation>();
		}

		private void AssignUIIDToTweens()
		{
			for (int i = 0; i < animations.Length; i++)
			{
				if (!(animations[i] == null))
				{
					animations[i].id = "Animation_" + base.gameObject.name;
				}
			}
		}

		private void FindLongestAnimations()
		{
			float num = 0f;
			float num2 = 0f;
			for (int i = 0; i < animations.Length; i++)
			{
				float num3 = animations[i].duration + animations[i].delay;
				if (num3 > num)
				{
					num = num3;
					longestForwardAnimation = animations[i];
				}
				if (animations[i].duration > num2)
				{
					num2 = animations[i].duration;
					longestBackwardAnimation = animations[i];
				}
			}
		}

		private IEnumerator PlayAnimationOnInitialized()
		{
			isPlayingAnimation = true;
			yield return WaitForAnimationInitialization();
			AssignUIIDToTweens();
			FindLongestAnimations();
			PlayAllAnimationsForward();
		}

		private IEnumerator WaitForAnimationInitialization()
		{
			for (int i = 0; i < animations.Length; i++)
			{
				if (animations[i].gameObject.activeSelf)
				{
					while (!animations[i].TweenCreated)
					{
						yield return null;
					}
				}
			}
			animationsInitialized = true;
		}

		private IEnumerator PlayAnimationBackwardOnInitialized()
		{
			isPlayingAnimation = true;
			yield return WaitForAnimationInitialization();
			AssignUIIDToTweens();
			FindLongestAnimations();
			PlayAllAnimationsBackward();
		}

		private void PlayAllAnimationsForward()
		{
			isPlayingAnimation = true;
			for (int i = 0; i < animations.Length; i++)
			{
				animations[i].tween.Rewind();
				animations[i].tween.PlayForward();
			}
			SetupPlayForwardEvents();
		}

		private void PlayAllAnimationsBackward()
		{
			isPlayingAnimation = true;
			for (int i = 0; i < animations.Length; i++)
			{
				animations[i].tween.PlayBackwards();
			}
			SetupPlayBackwardEvents();
		}

		private void SetupPlayForwardEvents()
		{
			if (OnAnimationStarted != null)
			{
				OnAnimationStarted();
			}
			Tween tween = longestBackwardAnimation.tween;
			tween.onRewind = (TweenCallback)Delegate.Remove(tween.onRewind, new TweenCallback(OnAnimationRewinded));
			Tween tween2 = longestForwardAnimation.tween;
			tween2.onComplete = (TweenCallback)Delegate.Combine(tween2.onComplete, new TweenCallback(OnAnimationCompleted));
		}

		private void SetupPlayBackwardEvents()
		{
			if (OnAnimationStarted != null)
			{
				OnAnimationStarted();
			}
			Tween tween = longestForwardAnimation.tween;
			tween.onComplete = (TweenCallback)Delegate.Remove(tween.onComplete, new TweenCallback(OnAnimationCompleted));
			Tween tween2 = longestBackwardAnimation.tween;
			tween2.onRewind = (TweenCallback)Delegate.Combine(tween2.onRewind, new TweenCallback(OnAnimationRewinded));
		}

		private void OnAnimationCompleted()
		{
			isPlayingAnimation = false;
			Tween tween = longestForwardAnimation.tween;
			tween.onComplete = (TweenCallback)Delegate.Remove(tween.onComplete, new TweenCallback(OnAnimationCompleted));
			TriggerAnimationEndedEvent();
		}

		private void TriggerAnimationEndedEvent()
		{
			if (OnAnimationEnded != null)
			{
				OnAnimationEnded();
			}
		}

		private void OnAnimationRewinded()
		{
			isPlayingAnimation = false;
			Tween tween = longestBackwardAnimation.tween;
			tween.onRewind = (TweenCallback)Delegate.Remove(tween.onRewind, new TweenCallback(OnAnimationRewinded));
			if (OnAnimationEnded != null)
			{
				OnAnimationEnded();
			}
		}
	}
}
