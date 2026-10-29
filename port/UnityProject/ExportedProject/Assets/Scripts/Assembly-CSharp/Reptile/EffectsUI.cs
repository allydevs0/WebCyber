using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Reptile
{
	public class EffectsUI : MonoBehaviour
	{
		public static readonly Color niceBlack = new Color(0.05f, 0.05f, 0.05f, 1f);

		public static readonly Color niceClear = new Color(0.05f, 0.05f, 0.05f, 0f);

		public const float BARS_DEFAULT_HEIGHT = 150f;

		public const float BARS_OFFSET = 200f;

		private const float BARS_HIDDEN_HEIGHT = 0f;

		[SerializeField]
		internal Image fullScreenFade;

		[SerializeField]
		private RectTransform topBar;

		[SerializeField]
		private RectTransform bottomBar;

		[SerializeField]
		private GameObject leftBar;

		[SerializeField]
		private GameObject rightBar;

		[SerializeField]
		private TextMeshProUGUI skipText;

		[SerializeField]
		private AnimationCurve fadeCurve;

		internal Color currentFadeColor = niceClear;

		private Vector2 initialTopBarSizeDelta;

		private Vector2 initialBottomBarSizeDelta;

		private Coroutine fadeCoroutine;

		private Coroutine showBarsCoroutine;

		private Tween fadeTween;

		private Sequence lastBarSequence;

		public bool IsShowingFade => fadeCoroutine != null;

		public bool IsAnimatingBars => showBarsCoroutine != null;

		public void Init()
		{
			base.gameObject.SetActive(value: true);
			fullScreenFade.gameObject.SetActive(value: false);
			initialTopBarSizeDelta = topBar.sizeDelta;
			initialBottomBarSizeDelta = bottomBar.sizeDelta;
			topBar.sizeDelta = new Vector2(initialTopBarSizeDelta.x, -200f);
			bottomBar.sizeDelta = new Vector2(initialBottomBarSizeDelta.x, -200f);
			HideVerticalBars();
			skipText.gameObject.SetActive(value: false);
		}

		public void KillFadeTween()
		{
			if (fadeTween != null)
			{
				fadeTween.Kill();
			}
		}

		public Tween FadeToBlack(float duration)
		{
			if (fadeCoroutine != null)
			{
				StopCoroutine(fadeCoroutine);
			}
			if (fadeTween != null && fadeTween.IsActive())
			{
				fadeTween.Kill();
			}
			return FadeTo(niceBlack, duration);
		}

		public Tween FadeOpen(float duration)
		{
			if (fadeCoroutine != null)
			{
				StopAllCoroutines();
			}
			if (fadeTween != null && fadeTween.IsActive())
			{
				fadeTween.Kill();
			}
			return FadeTo(niceClear, duration);
		}

		public void FadeInAndOut(float durationIn, float durationStay, float durationOut, Color color, UpdateType updateType = UpdateType.Manual)
		{
			if (fadeCoroutine != null)
			{
				StopCoroutine(fadeCoroutine);
			}
			if (fadeTween != null && fadeTween.IsActive())
			{
				fadeTween.Kill();
			}
			fadeCoroutine = StartCoroutine(FadeInAndOutRoutine(durationIn, durationStay, durationOut, color, updateType));
		}

		public void FadeInAndOutBlack(float durationIn, float durationStay, float durationOut, UpdateType updateType = UpdateType.Manual)
		{
			if (fadeCoroutine != null)
			{
				StopCoroutine(fadeCoroutine);
			}
			if (fadeTween != null && fadeTween.IsActive())
			{
				fadeTween.Kill();
			}
			fadeCoroutine = StartCoroutine(FadeInAndOutRoutine(durationIn, durationStay, durationOut, niceBlack, updateType));
		}

		public void FullBlackToFadeOut(float durationStay, float durationOut, UpdateType updateType = UpdateType.Manual)
		{
			if (fadeCoroutine != null)
			{
				StopCoroutine(fadeCoroutine);
			}
			if (fadeTween != null && fadeTween.IsActive())
			{
				fadeTween.Kill();
			}
			fadeCoroutine = StartCoroutine(FullToFadeOutRoutine(durationStay, durationOut, niceBlack, updateType));
		}

		public bool IsFadedToColor(Color fadeColor)
		{
			if (fullScreenFade.gameObject.activeInHierarchy && currentFadeColor == fadeColor)
			{
				return fullScreenFade.color == fadeColor;
			}
			return false;
		}

		public Sequence HideBars(float duration, UpdateType updateType = UpdateType.Manual)
		{
			return ShowBars(duration, 0f, updateType);
		}

		public Sequence ShowBars(float comeInDuration, float size, UpdateType updateType = UpdateType.Manual)
		{
			Sequence result = ShowBarsOfColor(niceBlack, comeInDuration, size, updateType);
			if (lastBarSequence != null)
			{
				lastBarSequence.Kill();
			}
			lastBarSequence = result;
			return result;
		}

		public void ShowBarsInAndOut(float durationIn, float durationStay, float durationOut, UpdateType updateType = UpdateType.Manual)
		{
			if (showBarsCoroutine != null)
			{
				StopCoroutine(showBarsCoroutine);
			}
			showBarsCoroutine = StartCoroutine(ShowBarsInAndOutRoutine(durationIn, durationStay, durationOut, updateType));
		}

		public Sequence ShowBarsOfColor(Color color, float comeInDuration, float size, UpdateType updateType)
		{
			float num = 0f;
			if (size <= 0f)
			{
				num = 200f;
			}
			Image component = topBar.GetComponent<Image>();
			Image component2 = bottomBar.GetComponent<Image>();
			Sequence sequence = DOTween.Sequence();
			sequence.SetUpdate(updateType);
			float y = size - num;
			Vector2 endValue = new Vector2(initialTopBarSizeDelta.x, y);
			Vector2 endValue2 = new Vector2(initialBottomBarSizeDelta.x, y);
			TweenerCore<Vector2, Vector2, VectorOptions> t = topBar.DOSizeDelta(endValue, comeInDuration);
			TweenerCore<Vector2, Vector2, VectorOptions> t2 = bottomBar.DOSizeDelta(endValue2, comeInDuration);
			TweenerCore<Color, Color, ColorOptions> t3 = component.DOColor(color, comeInDuration);
			TweenerCore<Color, Color, ColorOptions> t4 = component2.DOColor(color, comeInDuration);
			sequence.Insert(0f, t);
			sequence.Insert(0f, t3);
			sequence.Insert(0f, t2);
			sequence.Insert(0f, t4);
			sequence.PlayForward();
			return sequence;
		}

		public void ShowSkipText()
		{
			skipText.gameObject.SetActive(value: true);
		}

		public Tween FadeSkipIn(float duration, UpdateType updateType = UpdateType.Manual)
		{
			skipText.gameObject.SetActive(value: true);
			duration *= 1f - skipText.color.a;
			TweenerCore<Color, Color, ColorOptions> tweenerCore = skipText.DOFade(1f, duration);
			tweenerCore.SetUpdate(updateType);
			tweenerCore.SetAutoKill(autoKillOnCompletion: true);
			tweenerCore.SetEase(fadeCurve);
			tweenerCore.PlayForward();
			return tweenerCore;
		}

		public Tween FadeSkipOut(float duration, UpdateType updateType = UpdateType.Manual)
		{
			duration *= skipText.color.a;
			TweenerCore<Color, Color, ColorOptions> tweenerCore = skipText.DOFade(0f, duration);
			tweenerCore.SetUpdate(updateType);
			tweenerCore.SetAutoKill(autoKillOnCompletion: true);
			tweenerCore.SetEase(fadeCurve);
			tweenerCore.PlayForward();
			tweenerCore.OnComplete(() =>
			{
				skipText.gameObject.SetActive(value: false);
			});
			return tweenerCore;
		}

		public void ShowVerticalBars()
		{
			leftBar.SetActive(value: true);
			rightBar.SetActive(value: true);
		}

		public void HideVerticalBars()
		{
			leftBar.SetActive(value: false);
			rightBar.SetActive(value: false);
		}

		private Tween FadeTo(Color endColor, float duration, UpdateType updateType = UpdateType.Manual)
		{
			fullScreenFade.gameObject.SetActive(value: true);
			fullScreenFade.color = currentFadeColor;
			fadeTween = fullScreenFade.DOColor(endColor, duration);
			fadeTween.SetUpdate(updateType);
			fadeTween.SetAutoKill(autoKillOnCompletion: true);
			fadeTween.SetEase(fadeCurve);
			fadeTween.PlayForward();
			currentFadeColor = endColor;
			return fadeTween;
		}

		private IEnumerator FadeInAndOutRoutine(float durationIn, float durationStay, float durationOut, Color color, UpdateType updateType = UpdateType.Manual)
		{
			FadeTo(color, durationIn, updateType);
			yield return new WaitForSeconds(durationIn + durationStay);
			FadeTo(niceClear, durationOut, updateType);
			fadeCoroutine = null;
		}

		private IEnumerator FullToFadeOutRoutine(float durationStay, float durationOut, Color color, UpdateType updateType = UpdateType.Manual)
		{
			fullScreenFade.gameObject.SetActive(value: true);
			fullScreenFade.color = (currentFadeColor = color);
			yield return new WaitForSeconds(durationStay);
			FadeTo(niceClear, durationOut, updateType);
			fadeCoroutine = null;
		}

		private IEnumerator ShowBarsInAndOutRoutine(float durationIn, float durationStay, float durationOut, UpdateType updateType = UpdateType.Manual)
		{
			ShowBarsOfColor(niceBlack, durationIn, 100f, updateType);
			yield return new WaitForSeconds(durationIn + durationStay);
			ShowBarsOfColor(niceBlack, durationOut, 0f, updateType);
			showBarsCoroutine = null;
		}
	}
}
