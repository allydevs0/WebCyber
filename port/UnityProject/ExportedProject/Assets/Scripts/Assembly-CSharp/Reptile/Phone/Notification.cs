using System;
using TMPro;
using UnityEngine;

namespace Reptile.Phone
{
	public class Notification : MonoBehaviour
	{
		private Type appType;

		[Header("Main Screen Properties")]
		[SerializeField]
		protected GameObject appNotificationPanel_Main;

		[SerializeField]
		protected TextMeshProUGUI topBarText_Main;

		[Space(10f)]
		[Header("Outside Screen Properties")]
		[SerializeField]
		protected GameObject appNotificationPanel_Outside;

		[SerializeField]
		protected TextMeshProUGUI topBarText_Outside;

		public void InitNotification(App app)
		{
			appType = app.GetType();
			appNotificationPanel_Main.SetActive(value: false);
			appNotificationPanel_Outside.SetActive(value: false);
		}

		public Type GetAppType()
		{
			return appType;
		}

		public void ShowNotification(Phone phone, string topBarText, bool showOnMainScreen, bool notificationScreenIsAlreadyActive = false, bool dontPopupOrNotify = false)
		{
			SetNotifyText(topBarText, showOnMainScreen);
			if (showOnMainScreen)
			{
				appNotificationPanel_Main.SetActive(value: true);
			}
			else
			{
				appNotificationPanel_Outside.SetActive(value: true);
			}
			if (!dontPopupOrNotify)
			{
				phone.PopUp(showOnMainScreen, notificationScreenIsAlreadyActive);
			}
		}

		public void HideNotification(bool hideOnMainscreen)
		{
			if (hideOnMainscreen)
			{
				appNotificationPanel_Main.SetActive(value: false);
			}
			else
			{
				appNotificationPanel_Outside.SetActive(value: false);
			}
		}

		public void SetNotifyText(string textToShow, bool isOnMainScreen = false)
		{
			if (isOnMainScreen)
			{
				topBarText_Main.text = textToShow;
			}
			else
			{
				topBarText_Outside.text = textToShow;
			}
		}
	}
}
