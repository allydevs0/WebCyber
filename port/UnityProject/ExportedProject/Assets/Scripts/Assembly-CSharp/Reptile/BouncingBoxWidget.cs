using System.Collections;
using UnityEngine;

namespace Reptile
{
	public class BouncingBoxWidget : MonoBehaviour
	{
		private enum NormalDirection
		{
			NONE = -1,
			UP = 0,
			RIGHT = 1,
			DOWN = 2,
			LEFT = 3
		}

		[SerializeField]
		private float speed = 1f;

		[SerializeField]
		private RectTransform bouncingBox;

		[SerializeField]
		private RectTransform containerBox;

		private Coroutine getNewRectSizesRoutine;

		private Vector2[] startDirections = new Vector2[4];

		private Vector2 currentDirection = Vector2.zero;

		private Rect bouncingBoxRect;

		private Rect containerRect;

		private NormalDirection lastBounceNormalDirection = NormalDirection.NONE;

		private bool isBouncing;

		private float bouncingBoxHalfWidth;

		private float bouncingBoxHalfHeight;

		public void StartBouncing()
		{
			int num = Random.Range(0, 3);
			currentDirection = startDirections[num];
			isBouncing = true;
			SetRectData();
		}

		public void StopBouncing()
		{
			isBouncing = false;
		}

		public void ResetBox()
		{
			bouncingBox.anchoredPosition = Vector2.zero;
		}

		private void Awake()
		{
			startDirections[0] = new Vector2(1f, 1f);
			startDirections[1] = new Vector2(1f, -1f);
			startDirections[2] = new Vector2(-1f, -1f);
			startDirections[3] = new Vector2(-1f, 1f);
			Core.OnScreenSizeChanged += OnScreenSizeChanged;
		}

		private void Update()
		{
			if (isBouncing)
			{
				UpdateBouncingBox();
			}
		}

		private void OnDestroy()
		{
			StopBouncing();
			startDirections = null;
			if (getNewRectSizesRoutine != null)
			{
				StopCoroutine(getNewRectSizesRoutine);
			}
			Core.OnScreenSizeChanged -= OnScreenSizeChanged;
		}

		private void SetRectData()
		{
			containerRect = containerBox.rect;
			bouncingBoxRect = bouncingBox.rect;
			bouncingBoxHalfWidth = bouncingBoxRect.width / 2f;
			bouncingBoxHalfHeight = bouncingBoxRect.height / 2f;
		}

		private void OnScreenSizeChanged(int newWidth, int newHeight)
		{
			ResetBox();
			getNewRectSizesRoutine = StartCoroutine(GetNewRectSizes());
		}

		private IEnumerator GetNewRectSizes()
		{
			yield return new WaitForEndOfFrame();
			SetRectData();
			getNewRectSizesRoutine = null;
		}

		private void UpdateBouncingBox()
		{
			UpdateBouncingBoxPosition();
			UpdateBounce();
		}

		private void UpdateBouncingBoxPosition()
		{
			bouncingBox.anchoredPosition += speed * currentDirection * Time.deltaTime;
		}

		private void UpdateBounce()
		{
			NormalDirection bounceNormalDirection = GetBounceNormalDirection(containerRect);
			if (bounceNormalDirection != NormalDirection.NONE && bounceNormalDirection != lastBounceNormalDirection)
			{
				Bounce(bounceNormalDirection);
			}
		}

		private NormalDirection GetBounceNormalDirection(Rect containerRect)
		{
			Vector2[] bouncingBoxExtends = GetBouncingBoxExtends();
			for (int i = 0; i < bouncingBoxExtends.Length; i++)
			{
				if (!containerRect.Contains(bouncingBoxExtends[i]))
				{
					return (NormalDirection)i;
				}
			}
			return NormalDirection.NONE;
		}

		private Vector2[] GetBouncingBoxExtends()
		{
			Vector2[] array = new Vector2[4];
			Vector2 vector = bouncingBox.localPosition;
			array[0] = new Vector2(vector.x, vector.y - bouncingBoxHalfHeight);
			array[1] = new Vector2(vector.x - bouncingBoxHalfWidth, vector.y);
			array[2] = new Vector2(vector.x, vector.y + bouncingBoxHalfHeight);
			array[3] = new Vector2(vector.x + bouncingBoxHalfWidth, vector.y);
			return array;
		}

		private void Bounce(NormalDirection bounceNormalDirection)
		{
			Vector2 normalDirectionVector = GetNormalDirectionVector(bounceNormalDirection);
			currentDirection = Vector2.Reflect(currentDirection, normalDirectionVector);
			lastBounceNormalDirection = bounceNormalDirection;
		}

		private Vector2 GetNormalDirectionVector(NormalDirection normalDirection)
		{
			return normalDirection switch
			{
				NormalDirection.UP => Vector2.up, 
				NormalDirection.RIGHT => Vector2.right, 
				NormalDirection.DOWN => Vector2.down, 
				NormalDirection.LEFT => Vector2.left, 
				_ => Vector2.zero, 
			};
		}
	}
}
