using UnityEngine;
using UnityEngine.UI;

namespace Reptile
{
	[RequireComponent(typeof(Image))]
	public class DanceAbilityUIButton : MonoBehaviour
	{
		[SerializeField]
		private Sprite highlightedSprite;

		[SerializeField]
		private Sprite dehighlightedSprite;

		[SerializeField]
		private string danceNameLocalizationKey;

		[SerializeField]
		private int danceSaveId;

		[SerializeField]
		private float circleArcMin = 0.85f;

		[SerializeField]
		private float circleArcMax = 0.15f;

		private Image imageComponent;

		private IGameTextLocalizer localizer;

		private bool isActive;

		private string danceName;

		public bool IsActive => isActive;

		public string DanceName => danceName;

		public int DanceSaveId => danceSaveId;

		public void ShowButton()
		{
			base.gameObject.SetActive(value: true);
			isActive = true;
		}

		public void HideButton()
		{
			base.gameObject.SetActive(value: false);
			isActive = false;
		}

		public void Highlight()
		{
			Core.Instance.AudioManager.PlaySfxUI(SfxCollectionID.MenuSfx, AudioClipID.selectionMove);
			imageComponent.sprite = highlightedSprite;
		}

		public void Dehighlight()
		{
			imageComponent.sprite = dehighlightedSprite;
		}

		public bool IsInCircleArc(float circleArcPercentage)
		{
			if (circleArcMin > circleArcMax)
			{
				if (!(circleArcPercentage >= circleArcMin))
				{
					return circleArcPercentage < circleArcMax;
				}
				return true;
			}
			if (circleArcPercentage >= circleArcMin)
			{
				return circleArcPercentage < circleArcMax;
			}
			return false;
		}

		protected void Awake()
		{
			imageComponent = GetComponent<Image>();
			localizer = Core.Instance.Localizer;
			localizer.OnLanguageChanged += OnLanguageChanged;
			danceName = localizer.GetUserInterfaceText(danceNameLocalizationKey);
		}

		protected void OnDestroy()
		{
			if (localizer != null)
			{
				localizer.OnLanguageChanged -= OnLanguageChanged;
			}
		}

		private void OnLanguageChanged(IGameTextLocalizer localizer)
		{
			danceName = localizer.GetUserInterfaceText(danceNameLocalizationKey);
		}
	}
}
