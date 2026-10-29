using System;
using UnityEngine;

namespace Kaj.SimpleEasing
{
	public static class SimpleEasing
	{
		public enum EaseType
		{
			EaseInBack = 0,
			EaseInBounce = 1,
			EaseInCirc = 2,
			EaseInCubic = 3,
			EaseInElastic = 4,
			EaseInExpo = 5,
			EaseInQuad = 6,
			EaseInQuart = 7,
			EaseInQuint = 8,
			EaseInSine = 9,
			EaseInOutBack = 10,
			EaseInOutBounce = 11,
			EaseInOutCirc = 12,
			EaseInOutCubic = 13,
			EaseInOutElastic = 14,
			EaseInOutExpo = 15,
			EaseInOutQuad = 16,
			EaseInOutQuart = 17,
			EaseInOutQuint = 18,
			EaseInOutSine = 19,
			EaseLinear = 20,
			EaseOutBack = 21,
			EaseOutBounce = 22,
			EaseOutCirc = 23,
			EaseOutCubic = 24,
			EaseOutElastic = 25,
			EaseOutExpo = 26,
			EaseOutQuad = 27,
			EaseOutQuart = 28,
			EaseOutQuint = 29,
			EaseOutSine = 30
		}

		public static float SimpleEase(EaseType ease, float time)
		{
			float start = 0f;
			float change = 1f;
			float duration = 1f;
			return SimpleEase(ease, time, start, change, duration);
		}

		public static float SimpleEase(EaseType ease, float time, float duration)
		{
			float start = 0f;
			float change = 1f;
			return SimpleEase(ease, time, start, change, duration);
		}

		public static float SimpleEase(EaseType ease, float time, float start, float change, float duration)
		{
			time = Mathf.Clamp(time, time, duration);
			return SimpleEaseUnClamped(ease, time, start, change, duration);
		}

		public static float SimpleEaseUnClamped(EaseType ease, float time)
		{
			float start = 0f;
			float change = 1f;
			float duration = 1f;
			return SimpleEaseUnClamped(ease, time, start, change, duration);
		}

		public static float SimpleEaseUnClamped(EaseType ease, float time, float duration)
		{
			float start = 0f;
			float change = 1f;
			return SimpleEaseUnClamped(ease, time, start, change, duration);
		}

		public static float SimpleEaseUnClamped(EaseType ease, float time, float start, float change, float duration)
		{
			switch (ease)
			{
			case EaseType.EaseInBack:
				return easeInBack(time, start, change, duration);
			case EaseType.EaseInBounce:
				return easeInBounce(time, start, change, duration);
			case EaseType.EaseInCirc:
				return easeInCirc(time, start, change, duration);
			case EaseType.EaseInCubic:
				return easeInCubic(time, start, change, duration);
			case EaseType.EaseInElastic:
				return easeInElastic(time, start, change, duration);
			case EaseType.EaseInExpo:
				return easeInExpo(time, start, change, duration);
			case EaseType.EaseInQuad:
				return easeInQuad(time, start, change, duration);
			case EaseType.EaseInQuart:
				return easeInQuart(time, start, change, duration);
			case EaseType.EaseInQuint:
				return easeInQuint(time, start, change, duration);
			case EaseType.EaseInSine:
				return easeInSine(time, start, change, duration);
			case EaseType.EaseInOutBack:
				return easeInOutBack(time, start, change, duration);
			case EaseType.EaseInOutBounce:
				return easeInOutBounce(time, start, change, duration);
			case EaseType.EaseInOutCirc:
				return easeInOutCirc(time, start, change, duration);
			case EaseType.EaseInOutCubic:
				return easeInOutCubic(time, start, change, duration);
			case EaseType.EaseInOutElastic:
				return easeInOutElastic(time, start, change, duration);
			case EaseType.EaseInOutExpo:
				return easeInOutExpo(time, start, change, duration);
			case EaseType.EaseInOutQuad:
				return easeInOutQuad(time, start, change, duration);
			case EaseType.EaseInOutQuart:
				return easeInOutQuart(time, start, change, duration);
			case EaseType.EaseInOutQuint:
				return easeInOutQuint(time, start, change, duration);
			case EaseType.EaseInOutSine:
				return easeInOutSine(time, start, change, duration);
			case EaseType.EaseLinear:
				return easeLiniear(time, start, change, duration);
			case EaseType.EaseOutBack:
				return easeOutBack(time, start, change, duration);
			case EaseType.EaseOutBounce:
				return easeOutBounce(time, start, change, duration);
			case EaseType.EaseOutCirc:
				return easeOutCirc(time, start, change, duration);
			case EaseType.EaseOutCubic:
				return easeOutCubic(time, start, change, duration);
			case EaseType.EaseOutElastic:
				return easeOutElastic(time, start, change, duration);
			case EaseType.EaseOutExpo:
				return easeOutExpo(time, start, change, duration);
			case EaseType.EaseOutQuad:
				return easeOutQuad(time, start, change, duration);
			case EaseType.EaseOutQuart:
				return easeOutQuart(time, start, change, duration);
			case EaseType.EaseOutQuint:
				return easeOutQuint(time, start, change, duration);
			case EaseType.EaseOutSine:
				return easeOutSine(time, start, change, duration);
			default:
				Debug.LogError("Unkown Easing type");
				return 0f;
			}
		}

		private static float easeInBack(float time, float start, float change, float duration)
		{
			float num = 1.70158f;
			time /= duration;
			return change * time * time * ((num + 1f) * time - num) + start;
		}

		private static float easeInBounce(float time, float start, float change, float duration)
		{
			return change - easeOutBounce(duration - time, 0f, change, duration) + start;
		}

		private static float easeInCirc(float time, float start, float change, float duration)
		{
			time /= duration;
			return change * (1f - Mathf.Sqrt(1f - time * time)) + start;
		}

		private static float easeInCubic(float time, float start, float change, float duration)
		{
			return change * Mathf.Pow(time / duration, 3f) + start;
		}

		private static float easeInElastic(float time, float start, float change, float duration)
		{
			float num = 1.70158f;
			float num2 = 0f;
			float num3 = change;
			if (time == 0f || num3 == 0f)
			{
				return start;
			}
			time /= duration;
			if (time == 1f)
			{
				return start + change;
			}
			if (num2 == 0f)
			{
				num2 = duration * 0.3f;
			}
			if (num3 < Mathf.Abs(change))
			{
				num3 = change;
				num = num2 * 0.25f;
			}
			else
			{
				num = num2 / (MathF.PI * 2f) * Mathf.Asin(change / num3);
			}
			return 0f - num3 * Mathf.Pow(2f, 10f * --time) * Mathf.Sin((time * duration - num) * (MathF.PI * 2f) / num2) + start;
		}

		private static float easeInExpo(float time, float start, float change, float duration)
		{
			return change * Mathf.Pow(2f, 10f * (time / duration - 1f)) + start;
		}

		private static float easeInQuad(float time, float start, float change, float duration)
		{
			time /= duration;
			return change * time * time + start;
		}

		private static float easeInQuart(float time, float start, float change, float duration)
		{
			return change * Mathf.Pow(time / duration, 4f) + start;
		}

		private static float easeInQuint(float time, float start, float change, float duration)
		{
			return change * Mathf.Pow(time / duration, 5f) + start;
		}

		private static float easeInSine(float time, float start, float change, float duration)
		{
			return change * (1f - Mathf.Cos(time / duration * (MathF.PI / 2f))) + start;
		}

		private static float easeInOutBack(float time, float start, float change, float duration)
		{
			float num = 1.70158f;
			time /= duration;
			time *= 2f;
			if (time < 1f)
			{
				num *= 1.525f;
				return change * 0.5f * (time * time * ((num + 1f) * time - num)) + start;
			}
			time -= 2f;
			num *= 1.525f;
			return change * 0.5f * (time * time * ((num + 1f) * time + num) + 2f) + start;
		}

		private static float easeInOutBounce(float time, float start, float change, float duration)
		{
			if (time < duration * 0.5f)
			{
				return easeInBounce(time * 2f, 0f, change, duration) * 0.5f + start;
			}
			return easeOutBounce(time * 2f - duration, 0f, change, duration) * 0.5f + change * 0.5f + start;
		}

		private static float easeInOutCirc(float time, float start, float change, float duration)
		{
			time /= duration * 0.5f;
			if (time < 1f)
			{
				return change * 0.5f * (1f - Mathf.Sqrt(1f - time * time)) + start;
			}
			time -= 2f;
			return change * 0.5f * (Mathf.Sqrt(1f - time * time) + 1f) + start;
		}

		private static float easeInOutCubic(float time, float start, float change, float duration)
		{
			time /= duration * 0.5f;
			if (time < 1f)
			{
				return change * 0.5f * Mathf.Pow(time, 3f) + start;
			}
			return change * 0.5f * (Mathf.Pow(time - 2f, 3f) + 2f) + start;
		}

		private static float easeInOutElastic(float time, float start, float change, float duration)
		{
			float num = 1.70158f;
			float num2 = 0f;
			float num3 = change;
			if (time == 0f || num3 == 0f)
			{
				return start;
			}
			time /= duration * 0.5f;
			if (time == 2f)
			{
				return start + change;
			}
			if (num2 < 0.5f)
			{
				num2 = duration * 0.45000002f;
			}
			if (num3 < Mathf.Abs(change))
			{
				num3 = change;
				num = num2 * 0.25f;
			}
			else
			{
				num = num2 / (MathF.PI * 2f) * Mathf.Asin(change / num3);
			}
			if (time < 1f)
			{
				return -0.5f * (num3 * Mathf.Pow(2f, 10f * --time) * Mathf.Sin((time * duration - num) * (MathF.PI * 2f) / num2)) + start;
			}
			return num3 * Mathf.Pow(2f, -10f * --time) * Mathf.Sin((time * duration - num) * (MathF.PI * 2f) / num2) * 0.5f + change + start;
		}

		private static float easeInOutExpo(float time, float start, float change, float duration)
		{
			time /= duration * 0.5f;
			if (time < 1f)
			{
				return change * 0.5f * Mathf.Pow(2f, 10f * (time - 1f)) + start;
			}
			time--;
			return change * 0.5f * (0f - Mathf.Pow(2f, -10f * time) + 2f) + start;
		}

		private static float easeInOutQuad(float time, float start, float change, float duration)
		{
			time /= duration * 0.5f;
			if (time < 1f)
			{
				return change * 0.5f * (time * time) + start;
			}
			return (0f - change) * 0.5f * (--time * (time - 2f) - 1f) + start;
		}

		private static float easeInOutQuart(float time, float start, float change, float duration)
		{
			time /= duration * 0.5f;
			if (time < 1f)
			{
				return change * 0.5f * Mathf.Pow(time, 4f) + start;
			}
			return (0f - change) * 0.5f * (Mathf.Pow(time - 2f, 4f) - 2f) + start;
		}

		private static float easeInOutQuint(float time, float start, float change, float duration)
		{
			time /= duration * 0.5f;
			if (time < 1f)
			{
				return change * 0.5f * Mathf.Pow(time, 5f) + start;
			}
			return change * 0.5f * (Mathf.Pow(time - 2f, 5f) + 2f) + start;
		}

		private static float easeInOutSine(float time, float start, float change, float duration)
		{
			return change * 0.5f * (1f - Mathf.Cos(MathF.PI * time / duration)) + start;
		}

		private static float easeLiniear(float time, float start, float change, float duration)
		{
			return change * time / duration + start;
		}

		private static float easeOutBack(float time, float start, float change, float duration)
		{
			float num = 1.70158f;
			time = time / duration - 1f;
			return change * (time * time * ((num + 1f) * time + num) + 1f) + start;
		}

		private static float easeOutBounce(float time, float start, float change, float duration)
		{
			time /= duration;
			if (time < 372f / 1023f)
			{
				return change * (7.5625f * time * time) + start;
			}
			if (time < 744f / 1023f)
			{
				time -= 558f / 1023f;
				return change * (7.5625f * time * time + 0.75f) + start;
			}
			if (time < 930f / 1023f)
			{
				time -= 837f / 1023f;
				return change * (7.5625f * time * time + 0.9375f) + start;
			}
			time -= 21f / 22f;
			return change * (7.5625f * time * time + 63f / 64f) + start;
		}

		private static float easeOutCirc(float time, float start, float change, float duration)
		{
			time = time / duration - 1f;
			return change * Mathf.Sqrt(1f - time * time) + start;
		}

		private static float easeOutCubic(float time, float start, float change, float duration)
		{
			return change * (Mathf.Pow(time / duration - 1f, 3f) + 1f) + start;
		}

		private static float easeOutElastic(float time, float start, float change, float duration)
		{
			float num = 1.70158f;
			float num2 = 0f;
			float num3 = time;
			float num4 = change;
			if (num3 == 0f || num4 == 0f)
			{
				return start;
			}
			num3 /= duration;
			if (num3 == 1f)
			{
				return start + change;
			}
			if (num2 < 0.5f)
			{
				num2 = duration * 0.3f;
			}
			if (num4 < Mathf.Abs(change))
			{
				num4 = change;
				num = num2 * 0.25f;
			}
			else
			{
				num = num2 / (MathF.PI * 2f) * Mathf.Asin(change / num4);
			}
			return num4 * Mathf.Pow(2f, -10f * num3) * Mathf.Sin((num3 * duration - num) * (MathF.PI * 2f) / num2) + change + start;
		}

		private static float easeOutExpo(float time, float start, float change, float duration)
		{
			return change * (0f - Mathf.Pow(2f, -10f * time / duration) + 1f) + start;
		}

		private static float easeOutQuad(float time, float start, float change, float duration)
		{
			time /= duration;
			return (0f - change) * time * (time - 2f) + start;
		}

		private static float easeOutQuart(float time, float start, float change, float duration)
		{
			return (0f - change) * (Mathf.Pow(time / duration - 1f, 4f) - 1f) + start;
		}

		private static float easeOutQuint(float time, float start, float change, float duration)
		{
			return change * (Mathf.Pow(time / duration - 1f, 5f) + 1f) + start;
		}

		private static float easeOutSine(float time, float start, float change, float duration)
		{
			return change * Mathf.Sin(time / duration * (MathF.PI / 2f)) + start;
		}
	}
}
