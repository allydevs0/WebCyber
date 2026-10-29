using System;
using DG.Tweening;
using UnityEngine;

namespace Reptile.Phone
{
	public class PhoneScrollButton : MonoBehaviour
	{
		private bool m_IsHidden;

		private bool m_IsSelected;

		public bool IsSelected
		{
			get
			{
				return m_IsSelected;
			}
			set
			{
				if (value != m_IsSelected)
				{
					if (value)
					{
						OnSelect();
					}
					else
					{
						OnDeselect();
					}
				}
				m_IsSelected = value;
			}
		}

		public bool IsHidden
		{
			get
			{
				return m_IsHidden;
			}
			set
			{
				if (value != m_IsHidden)
				{
					if (value)
					{
						OnHide();
					}
					else
					{
						OnUnhide();
					}
				}
				m_IsHidden = value;
			}
		}

		public void Initalize()
		{
			OnCreate();
		}

		private void Awake()
		{
			Core.OnUpdate += UpdateHandeling;
		}

		private void OnDestroy()
		{
			Core.OnUpdate -= UpdateHandeling;
		}

		private void UpdateHandeling()
		{
			if (!m_IsHidden && base.gameObject.activeInHierarchy)
			{
				if (m_IsSelected)
				{
					SelectedUpdate();
				}
				else
				{
					InactiveUpdate();
				}
				ConstantUpdate();
			}
		}

		protected virtual void OnCreate()
		{
			OnDeselect(skipAnimations: true);
		}

		protected virtual void OnSelect(bool skipAnimations = false)
		{
			(base.transform as RectTransform).DOAnchorPosX(0f, 0.1f).SetEase(Ease.Linear);
		}

		protected virtual void OnDeselect(bool skipAnimations = false)
		{
			(base.transform as RectTransform).DOAnchorPosX(70f, 0.1f * (float)Convert.ToInt32(!skipAnimations)).SetEase(Ease.Linear);
		}

		public virtual void OnActivate()
		{
			(base.transform as RectTransform).DOKill();
			(base.transform as RectTransform).DOAnchorPosX(0f, 0.1f).SetEase(Ease.OutExpo);
		}

		public virtual void OnHold()
		{
			(base.transform as RectTransform).DOAnchorPosX(50f, 0.05f).SetEase(Ease.Linear);
		}

		protected virtual void SelectedUpdate()
		{
		}

		protected virtual void InactiveUpdate()
		{
		}

		protected virtual void ConstantUpdate()
		{
		}

		protected virtual void OnHide()
		{
			base.gameObject.SetActive(value: false);
		}

		protected virtual void OnUnhide()
		{
			base.gameObject.SetActive(value: true);
		}
	}
}
