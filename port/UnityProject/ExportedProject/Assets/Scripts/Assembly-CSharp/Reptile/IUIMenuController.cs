using UnityEngine;

namespace Reptile
{
	public interface IUIMenuController : IUIElement
	{
		bool AlwaysShowOnTop { get; }

		bool AllowPauseMenu { get; }

		bool AllowPhone { get; }

		Menu MenuType { get; }

		GameObject GetLastSelectedObject();

		void SetLastSelectedObject(GameObject selectedObject);

		void OnPressedBackButton();

		IMenuAnimationTransitionController ShowMenu();

		IMenuAnimationTransitionController HideMenu();
	}
}
