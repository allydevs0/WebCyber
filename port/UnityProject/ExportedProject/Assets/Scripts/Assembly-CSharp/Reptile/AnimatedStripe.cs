using UnityEngine;
using UnityEngine.UI;

namespace Reptile
{
	public class AnimatedStripe : MonoBehaviour
	{
		public enum AnimatedStripeDirection
		{
			right = 0,
			left = 2,
			up = 1,
			down = 3
		}

		[Header("Note: sprite size is set automaticaly")]
		[SerializeField]
		private AnimatedStripeDirection m_MoveDirection;

		[SerializeField]
		private float m_MoveSpeed = 600f;

		public Image m_Stripe;

		public AnimatedStripeDirection MoveDirection
		{
			get
			{
				return m_MoveDirection;
			}
			set
			{
				m_MoveDirection = value;
			}
		}

		public float MoveSpeed
		{
			get
			{
				return m_MoveSpeed;
			}
			set
			{
				m_MoveSpeed = value;
			}
		}

		public Image Spripe => m_Stripe;

		private void Awake()
		{
			Vector2 sizeDelta = (base.transform as RectTransform).sizeDelta;
			UpdateStripeSize(sizeDelta);
		}

		private void Update()
		{
			UpdatePos();
		}

		public void UpdateStripeSize(Vector2 endSize)
		{
			float num = (((int)m_MoveDirection % 2 == 0) ? endSize.y : endSize.x);
			if ((int)m_MoveDirection % 2 == 0)
			{
				m_Stripe.rectTransform.sizeDelta = new Vector2
				{
					x = endSize.x + num,
					y = endSize.y
				};
				m_Stripe.pixelsPerUnitMultiplier = m_Stripe.sprite.rect.height / num;
			}
			else
			{
				m_Stripe.rectTransform.sizeDelta = new Vector2
				{
					x = endSize.x,
					y = endSize.y + num
				};
				m_Stripe.pixelsPerUnitMultiplier = m_Stripe.sprite.rect.width / num;
			}
		}

		private void UpdatePos()
		{
			int num = ((m_MoveDirection < AnimatedStripeDirection.left) ? 1 : (-1));
			float num2 = (((int)m_MoveDirection % 2 == 0) ? m_Stripe.rectTransform.sizeDelta.y : m_Stripe.rectTransform.sizeDelta.x);
			float num3 = Time.time * m_MoveSpeed % num2 * (float)num - num2 / 2f * (float)num;
			Vector2 anchoredPosition = default;
			if ((int)m_MoveDirection % 2 == 0)
			{
				anchoredPosition.x = num3;
			}
			else
			{
				anchoredPosition.y = num3;
			}
			m_Stripe.rectTransform.anchoredPosition = anchoredPosition;
		}

		public void Color(Color color)
		{
			m_Stripe.color = color;
		}
	}
}
