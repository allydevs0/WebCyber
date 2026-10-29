using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Reptile.Phone
{
	public class AppCall : App
	{
		private Sequence pickupAnimation;

		[SerializeField]
		private RectTransform pickupScreen;

		[SerializeField]
		private float pickupScreenMoveDuration = 0.35f;

		[SerializeField]
		private Image npcImage;

		[SerializeField]
		private Image npcIcon;

		[SerializeField]
		private TextMeshProUGUI npcName;

		[SerializeField]
		private PhoneContacts phoneContacts;

		protected override void OnAppInit()
		{
			pickupAnimation = DOTween.Sequence();
			pickupAnimation.SetAutoKill(autoKillOnCompletion: false);
			pickupAnimation.Pause();
			pickupAnimation.Append(pickupScreen.DOAnchorPosX(1100f, pickupScreenMoveDuration).SetEase(Ease.OutExpo));
			pickupAnimation.AppendCallback(() =>
			{
				pickupScreen.gameObject.SetActive(value: false);
			});
			pickupAnimation.AppendCallback(() =>
			{
				pickupScreen.anchoredPosition = Vector3.zero;
			});
			if (pickupScreen.gameObject.activeInHierarchy)
			{
				pickupScreen.gameObject.SetActive(value: false);
			}
		}

		protected override void OnAppTerminate()
		{
			base.OnAppTerminate();
			pickupAnimation?.Kill();
			pickupScreen = null;
		}

		public override void OnReleaseLeft()
		{
		}

		public string SetNPCCalling(int characterNameIDOfCaller)
		{
			pickupScreen.gameObject.SetActive(value: false);
			Contact contact = phoneContacts.contacts[characterNameIDOfCaller];
			npcImage.sprite = contact.avatar;
			return Core.Instance.Localizer.GetCharacterName(contact.characterID);
		}

		public void ShowPickupScreen(int characterNameIDOfCaller)
		{
			pickupScreen.gameObject.SetActive(value: true);
			Contact contact = phoneContacts.contacts[characterNameIDOfCaller];
			npcIcon.sprite = contact.avatar;
			npcName.text = Core.Instance.Localizer.GetCharacterName(contact.characterID);
		}

		public IEnumerator HidePickupScreen()
		{
			pickupAnimation.Restart();
			yield return pickupAnimation.WaitForCompletion();
		}
	}
}
